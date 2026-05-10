using System;
using VContainer.Unity;
using MessagePipe;
using CardMatch.Logic.Messages;
using CardMatch.Runtime.Services;

namespace CardMatch.Runtime.Coordinators
{
    public sealed class AudioBridge : IStartable, IDisposable
    {
        private const int MIN_STRIKE_FOR_SOUND = 1;

        private readonly AudioSystem _audioSystem;
        private readonly ISubscriber<CardFlippedMessage> _cardFlippedSubscriber;
        private readonly ISubscriber<MatchResultMessage> _matchResultSubscriber;
        private readonly ISubscriber<PenaltyAppliedMessage> _penaltySubscriber;
        private readonly ISubscriber<GameWonMessage> _gameWonSubscriber;

        private IDisposable _cardFlippedDisposable;
        private IDisposable _matchResultDisposable;
        private IDisposable _penaltyDisposable;
        private IDisposable _gameWonDisposable;

        public AudioBridge(
            AudioSystem audioSystem,
            ISubscriber<CardFlippedMessage> cardFlippedSubscriber,
            ISubscriber<MatchResultMessage> matchResultSubscriber,
            ISubscriber<PenaltyAppliedMessage> penaltySubscriber,
            ISubscriber<GameWonMessage> gameWonSubscriber)
        {
            _audioSystem = audioSystem;
            _cardFlippedSubscriber = cardFlippedSubscriber;
            _matchResultSubscriber = matchResultSubscriber;
            _penaltySubscriber = penaltySubscriber;
            _gameWonSubscriber = gameWonSubscriber;
        }

        public void Start()
        {
            _cardFlippedDisposable = _cardFlippedSubscriber.Subscribe(OnCardFlipped);
            _matchResultDisposable = _matchResultSubscriber.Subscribe(OnMatchResult);
            _penaltyDisposable = _penaltySubscriber.Subscribe(OnPenalty);
            _gameWonDisposable = _gameWonSubscriber.Subscribe(OnGameWon);
        }

        private void OnCardFlipped(CardFlippedMessage msg)
        {
            _audioSystem.PlayFlip();
        }

        private void OnMatchResult(MatchResultMessage msg)
        {
            if (msg.IsMatch)
            {
                _audioSystem.PlayMatch();

                if (msg.NewStrike > MIN_STRIKE_FOR_SOUND)
                {
                    _audioSystem.PlayStrike();
                }
            }
        }

        private void OnPenalty(PenaltyAppliedMessage msg)
        {
            _audioSystem.PlayPenalty();
        }

        private void OnGameWon(GameWonMessage msg)
        {
            _audioSystem.PlayWin();
        }

        public void Dispose()
        {
            _cardFlippedDisposable?.Dispose();
            _matchResultDisposable?.Dispose();
            _penaltyDisposable?.Dispose();
            _gameWonDisposable?.Dispose();
        }
    }
}
