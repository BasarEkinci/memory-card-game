using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitMotion;
using LitMotion.Extensions;
using VContainer;
using MessagePipe;
using CardMatch.Logic.Messages;

namespace CardMatch.Runtime.Views
{
    public sealed class WinPanelView : MonoBehaviour
    {
        private const float POPUP_DURATION = 0.3f;
        private const float SCALE_ZERO = 0f;
        private const float SCALE_ONE = 1f;
        private const string SCORE_LABEL = "Score: {0}";
        private const string BEST_SCORE_LABEL = "Best: {0}";
        private const string MAX_STRIKE_LABEL = "Max Strike: {0}";

        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _bestScoreText;
        [SerializeField] private TextMeshProUGUI _maxStrikeText;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private RectTransform _panelTransform;

        private ISubscriber<GameWonMessage> _gameWonSubscriber;
        private IPublisher<NewGameRequestedMessage> _newGamePublisher;
        private IDisposable _gameWonDisposable;
        private CancellationTokenSource _cts;

        [Inject]
        public void Construct(
            ISubscriber<GameWonMessage> gameWonSubscriber,
            IPublisher<NewGameRequestedMessage> newGamePublisher)
        {
            _gameWonSubscriber = gameWonSubscriber;
            _newGamePublisher = newGamePublisher;
        }

        private void Awake()
        {
            _newGameButton.onClick.AddListener(OnNewGameClicked);
            _cts = new CancellationTokenSource();
        }

        private void Start()
        {
            _gameWonDisposable = _gameWonSubscriber.Subscribe(OnGameWon);
        }

        private void OnGameWon(GameWonMessage msg)
        {
            _scoreText.SetText(SCORE_LABEL, msg.FinalScore);
            _bestScoreText.SetText(BEST_SCORE_LABEL, msg.BestScore);
            _maxStrikeText.SetText(MAX_STRIKE_LABEL, msg.MaxStrike);
            _panelRoot.SetActive(true);
            PlayOpenAnimation().Forget();
        }

        private void OnNewGameClicked()
        {
            PlayCloseAnimation().Forget();
        }

        private async UniTaskVoid PlayOpenAnimation()
        {
            _panelTransform.localScale = Vector3.zero;
            var animateX = LMotion.Create(SCALE_ZERO, SCALE_ONE, POPUP_DURATION)
                .WithEase(Ease.OutBack)
                .BindToLocalScaleX(_panelTransform)
                .ToUniTask(_cts.Token);
            var animateY = LMotion.Create(SCALE_ZERO, SCALE_ONE, POPUP_DURATION)
                .WithEase(Ease.OutBack)
                .BindToLocalScaleY(_panelTransform)
                .ToUniTask(_cts.Token);
            await UniTask.WhenAll(animateX, animateY);
        }

        private async UniTaskVoid PlayCloseAnimation()
        {
            var animateX = LMotion.Create(SCALE_ONE, SCALE_ZERO, POPUP_DURATION)
                .WithEase(Ease.InBack)
                .BindToLocalScaleX(_panelTransform)
                .ToUniTask(_cts.Token);
            var animateY = LMotion.Create(SCALE_ONE, SCALE_ZERO, POPUP_DURATION)
                .WithEase(Ease.InBack)
                .BindToLocalScaleY(_panelTransform)
                .ToUniTask(_cts.Token);
            await UniTask.WhenAll(animateX, animateY);
            _panelRoot.SetActive(false);
            _newGamePublisher.Publish(new NewGameRequestedMessage());
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _gameWonDisposable?.Dispose();
            _newGameButton.onClick.RemoveListener(OnNewGameClicked);
        }
    }
}
