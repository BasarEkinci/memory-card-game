using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using MessagePipe;
using CardMatch.Logic.Models;
using CardMatch.Logic.Messages;
using CardMatch.Logic.Systems;
using CardMatch.Runtime.Services;

namespace CardMatch.Runtime.Views
{
    public sealed class InputView : MonoBehaviour
    {
        private const int EVALUATION_DELAY_MS = 500;

        private Camera _mainCamera;
        private MatchSystem _matchSystem;
        private CardSystem _cardSystem;
        private GameFlowSystem _gameFlowSystem;
        private GameStateModel _gameState;
        private GridView _gridView;
        private DeckView _deckView;
        private SaveSystem _saveSystem;
        private IPublisher<CardFlippedMessage> _cardFlippedPublisher;
        private IPublisher<MatchResultMessage> _matchPublisher;
        private IPublisher<GameWonMessage> _gameWonPublisher;
        private IPublisher<PenaltyAppliedMessage> _penaltyPublisher;
        private CancellationTokenSource _cts;
        private bool _isProcessing;
        private CardView _hoveredCard;

        [Inject]
        public void Construct(
            MatchSystem matchSystem,
            CardSystem cardSystem,
            GameFlowSystem gameFlowSystem,
            GameStateModel gameState,
            GridView gridView,
            DeckView deckView,
            SaveSystem saveSystem,
            IPublisher<CardFlippedMessage> cardFlippedPublisher,
            IPublisher<MatchResultMessage> matchPublisher,
            IPublisher<GameWonMessage> gameWonPublisher,
            IPublisher<PenaltyAppliedMessage> penaltyPublisher)
        {
            _matchSystem = matchSystem;
            _cardSystem = cardSystem;
            _gameFlowSystem = gameFlowSystem;
            _gameState = gameState;
            _gridView = gridView;
            _deckView = deckView;
            _saveSystem = saveSystem;
            _cardFlippedPublisher = cardFlippedPublisher;
            _matchPublisher = matchPublisher;
            _gameWonPublisher = gameWonPublisher;
            _penaltyPublisher = penaltyPublisher;
        }

        private void Awake()
        {
            _mainCamera = Camera.main;
            _cts = new CancellationTokenSource();
        }

        private void OnDestroy()
        {
            if (_hoveredCard != null)
            {
                _hoveredCard.OnHoverExit();
                _hoveredCard = null;
            }
            _cts?.Cancel();
            _cts?.Dispose();
        }

        private void Update()
        {
            if (_matchSystem == null || _gameFlowSystem == null)
            {
                return;
            }

            UpdateHover();

            bool mouseClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
            bool touchPressed = Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame;

            if (!_gameFlowSystem.IsInputAllowed())
            {
                return;
            }

            if (mouseClicked)
            {
                HandleClick(Mouse.current.position.ReadValue());
                return;
            }

            if (touchPressed)
            {
                HandleClick(Touchscreen.current.primaryTouch.position.ReadValue());
            }
        }

        private void UpdateHover()
        {
            if (Mouse.current == null)
            {
                return;
            }

            if (!_gameFlowSystem.IsInputAllowed())
            {
                if (_hoveredCard != null)
                {
                    _hoveredCard.OnHoverExit();
                    _hoveredCard = null;
                }
                return;
            }

            Vector2 screenPosition = Mouse.current.position.ReadValue();
            Vector2 worldPoint = _mainCamera.ScreenToWorldPoint(screenPosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            CardView newHoveredCard = null;
            if (hit.collider != null && hit.collider.TryGetComponent<CardView>(out var cardView))
            {
                if (cardView.gameObject.activeInHierarchy)
                {
                    newHoveredCard = cardView;
                }
            }

            if (newHoveredCard != _hoveredCard)
            {
                if (_hoveredCard != null)
                {
                    _hoveredCard.OnHoverExit();
                }

                _hoveredCard = newHoveredCard;

                if (_hoveredCard != null)
                {
                    _hoveredCard.OnHoverEnter();
                }
            }
        }

        private void HandleClick(Vector2 screenPosition)
        {
            if (_isProcessing)
            {
                return;
            }

            Vector2 worldPoint = _mainCamera.ScreenToWorldPoint(screenPosition);
            RaycastHit2D hit = Physics2D.Raycast(worldPoint, Vector2.zero);

            if (hit.collider != null && hit.collider.TryGetComponent<CardView>(out var cardView))
            {
                ProcessCardSelection(cardView).Forget();
            }
        }

        private async UniTaskVoid ProcessCardSelection(CardView cardView)
        {
            _isProcessing = true;

            SelectionResult result = _matchSystem.SelectCard(cardView.GridIndex);

            if (result == SelectionResult.Ignored)
            {
                _isProcessing = false;
                return;
            }

            if (result == SelectionResult.Deselected)
            {
                _cardFlippedPublisher.Publish(new CardFlippedMessage(cardView.GridIndex, CardState.FaceDown));
                await cardView.PlayFlipAnimation(CardState.FaceDown, _cts.Token);
                _isProcessing = false;
                return;
            }

            _cardFlippedPublisher.Publish(new CardFlippedMessage(cardView.GridIndex, CardState.FaceUp));
            await cardView.PlayFlipAnimation(CardState.FaceUp, _cts.Token);

            if (result == SelectionResult.ReadyToEvaluate)
            {
                await UniTask.Delay(EVALUATION_DELAY_MS, cancellationToken: _cts.Token);

                MatchEvaluationResult evalResult = _matchSystem.EvaluateMatch();

                _matchPublisher.Publish(new MatchResultMessage(
                    evalResult.IsMatch,
                    evalResult.CardIndex1,
                    evalResult.CardIndex2,
                    evalResult.ScoreDelta,
                    evalResult.NewStrike,
                    evalResult.NewFailCount));

                CardView[] cards = _gridView.GetCardViews();
                CardView card1 = cards[evalResult.CardIndex1];
                CardView card2 = cards[evalResult.CardIndex2];

                if (evalResult.IsMatch)
                {
                    Vector3 deckPos = _deckView.GetDeckPosition();
                    await UniTask.WhenAll(
                        card1.PlayMatchedAnimation(deckPos, _cts.Token),
                        card2.PlayMatchedAnimation(deckPos, _cts.Token));

                    if (evalResult.AllMatched)
                    {
                        int finalScore = _gameState.Score;
                        int bestScore = _saveSystem.LoadBestScore();
                        _saveSystem.SaveBestScore(finalScore);
                        int newBestScore = _saveSystem.LoadBestScore();

                        var (_, _, maxStrike) = _gameFlowSystem.OnAllCardsMatched(finalScore, newBestScore);
                        _gameWonPublisher.Publish(new GameWonMessage(finalScore, newBestScore, maxStrike));
                    }
                }
                else
                {
                    if (evalResult.PenaltyApplied > 0)
                    {
                        _penaltyPublisher.Publish(new PenaltyAppliedMessage(evalResult.PenaltyApplied, _gameState.Score));
                    }

                    await UniTask.WhenAll(
                        card1.PlayFlipAnimation(CardState.FaceDown, _cts.Token),
                        card2.PlayFlipAnimation(CardState.FaceDown, _cts.Token));

                    _cardSystem.CloseCard(evalResult.CardIndex1);
                    _cardSystem.CloseCard(evalResult.CardIndex2);
                }
            }

            _isProcessing = false;
        }
    }
}
