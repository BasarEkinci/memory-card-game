using System;
using System.Threading;
using UnityEngine;
using Cysharp.Threading.Tasks;
using MessagePipe;
using VContainer.Unity;
using CardMatch.Logic.Messages;
using CardMatch.Logic.Systems;
using CardMatch.Runtime.Services;
using CardMatch.Runtime.Views;
using CardMatch.Runtime.ScriptableObjects;

namespace CardMatch.Runtime.Coordinators
{
    public sealed class GameCoordinator : IAsyncStartable, IDisposable
    {
        private const float DEAL_ANIMATION_RATIO = 0.8f;
        private const int MILLISECONDS_PER_SECOND = 1000;

        private readonly GameFlowSystem _gameFlowSystem;
        private readonly CardSystem _cardSystem;
        private readonly GridView _gridView;
        private readonly DeckView _deckView;
        private readonly HUDView _hudView;
        private readonly GameConfig _gameConfig;
        private readonly AudioSystem _audioSystem;
        private readonly ISubscriber<NewGameRequestedMessage> _newGameSubscriber;
        private IDisposable _subscription;
        private CancellationTokenSource _cts;

        public GameCoordinator(
            GameFlowSystem gameFlowSystem,
            CardSystem cardSystem,
            GridView gridView,
            DeckView deckView,
            HUDView hudView,
            GameConfig gameConfig,
            AudioSystem audioSystem,
            ISubscriber<NewGameRequestedMessage> newGameSubscriber)
        {
            _gameFlowSystem = gameFlowSystem;
            _cardSystem = cardSystem;
            _gridView = gridView;
            _deckView = deckView;
            _hudView = hudView;
            _gameConfig = gameConfig;
            _audioSystem = audioSystem;
            _newGameSubscriber = newGameSubscriber;
        }

        public async Awaitable StartAsync(CancellationToken cancellation)
        {
            _cts = new CancellationTokenSource();
            _subscription = _newGameSubscriber.Subscribe(OnNewGameRequested);
            await Awaitable.NextFrameAsync(cancellation);
        }

        private void OnNewGameRequested(NewGameRequestedMessage msg)
        {
            StartNewGameSequence().Forget();
        }

        private async UniTaskVoid StartNewGameSequence()
        {
            _gameFlowSystem.StartNewGame();
            _hudView.UpdateScore(0);
            _hudView.UpdateStrike(0);

            CardView[] cardViews = _gridView.GetCardViews();
            Vector3 deckPosition = _deckView.GetDeckPosition();

            for (int i = 0; i < cardViews.Length; i++)
            {
                cardViews[i].ResetCard();
                cardViews[i].SetPosition(deckPosition);
            }

            await _deckView.PlayShuffleAnimation(_cts.Token);

            float dealDelay = _gameConfig.DealCardDelay;
            float dealAnimDuration = dealDelay * DEAL_ANIMATION_RATIO;

            for (int i = 0; i < cardViews.Length; i++)
            {
                _audioSystem.PlayDeal();
                Vector3 targetPosition = _gridView.CalculateGridPosition(i);
                cardViews[i].PlayDealAnimation(targetPosition, dealAnimDuration, _cts.Token).Forget();
                await UniTask.Delay((int)(dealDelay * MILLISECONDS_PER_SECOND), cancellationToken: _cts.Token);
            }

            _cardSystem.DealAllCards();
            _gameFlowSystem.OnDealingComplete();
        }

        public void Dispose()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _subscription?.Dispose();
        }
    }
}
