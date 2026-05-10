using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using VContainer.Unity;
using CardMatch.Logic.Models;
using CardMatch.Logic.Systems;
using CardMatch.Runtime.Services;
using CardMatch.Runtime.Views;
using CardMatch.Runtime.ScriptableObjects;

namespace CardMatch.Runtime.EntryPoints
{
    public sealed class GameEntryPoint : IAsyncStartable
    {
        private const float DEAL_ANIMATION_RATIO = 0.8f;
        private const int MILLISECONDS_PER_SECOND = 1000;

        private readonly GameFlowSystem _gameFlowSystem;
        private readonly CardSystem _cardSystem;
        private readonly CardModel[] _cardModels;
        private readonly GridView _gridView;
        private readonly DeckView _deckView;
        private readonly GameConfig _gameConfig;
        private readonly AudioSystem _audioSystem;

        public GameEntryPoint(
            GameFlowSystem gameFlowSystem,
            CardSystem cardSystem,
            CardModel[] cardModels,
            GridView gridView,
            DeckView deckView,
            GameConfig gameConfig,
            AudioSystem audioSystem)
        {
            _gameFlowSystem = gameFlowSystem;
            _cardSystem = cardSystem;
            _cardModels = cardModels;
            _gridView = gridView;
            _deckView = deckView;
            _gameConfig = gameConfig;
            _audioSystem = audioSystem;
        }

        public async Awaitable StartAsync(CancellationToken cancellation)
        {
            InitializeCardViews();
            _gameFlowSystem.StartNewGame();
            await PlayDealingSequence(cancellation);
            _cardSystem.DealAllCards();
            _gameFlowSystem.OnDealingComplete();
        }

        private void InitializeCardViews()
        {
            CardView[] cardViews = _gridView.GetCardViews();

            for (int i = 0; i < cardViews.Length && i < _cardModels.Length; i++)
            {
                cardViews[i].Initialize(_cardModels[i], i);
            }
        }

        private async UniTask PlayDealingSequence(CancellationToken cancellation)
        {
            CardView[] cardViews = _gridView.GetCardViews();
            Vector3 deckPosition = _deckView.GetDeckPosition();

            for (int i = 0; i < cardViews.Length; i++)
            {
                cardViews[i].SetPosition(deckPosition);
            }

            await _deckView.PlayShuffleAnimation(cancellation);

            float dealDelay = _gameConfig.DealCardDelay;
            float dealAnimDuration = dealDelay * DEAL_ANIMATION_RATIO;

            for (int i = 0; i < cardViews.Length; i++)
            {
                _audioSystem.PlayDeal();
                Vector3 targetPosition = _gridView.CalculateGridPosition(i);
                cardViews[i].PlayDealAnimation(targetPosition, dealAnimDuration, cancellation).Forget();
                await UniTask.Delay((int)(dealDelay * MILLISECONDS_PER_SECOND), cancellationToken: cancellation);
            }
        }
    }
}
