using NUnit.Framework;
using CardMatch.Logic.Models;
using CardMatch.Logic.Systems;

namespace CardMatch.Logic.Tests
{
    [TestFixture]
    public sealed class GameFlowSystemTests
    {
        private GameStateModel _gameState;
        private GridModel _gridModel;
        private CardModel[] _cards;
        private GridSystem _gridSystem;
        private CardSystem _cardSystem;
        private GameFlowSystem _sut;

        [SetUp]
        public void SetUp()
        {
            _gameState = new GameStateModel();
            _gridModel = new GridModel();
            _cards = new CardModel[16];
            for (int cardIndex = 0; cardIndex < 16; cardIndex++)
            {
                _cards[cardIndex] = new CardModel { GridIndex = cardIndex };
            }
            _gridSystem = new GridSystem(_gridModel, _cards);
            _cardSystem = new CardSystem(_cards);
            _sut = new GameFlowSystem(_gameState, _gridSystem, _cardSystem);
        }

        [Test]
        public void StartNewGame_SetsPhaseToDealing()
        {
            _sut.StartNewGame();

            Assert.That(_gameState.Phase, Is.EqualTo(GamePhase.Dealing));
        }

        [Test]
        public void StartNewGame_ResetsScore()
        {
            _gameState.Score = 100;

            _sut.StartNewGame();

            Assert.That(_gameState.Score, Is.EqualTo(0));
        }

        [Test]
        public void StartNewGame_ResetsStrike()
        {
            _gameState.StrikeCount = 5;

            _sut.StartNewGame();

            Assert.That(_gameState.StrikeCount, Is.EqualTo(0));
        }

        [Test]
        public void StartNewGame_ResetsFail()
        {
            _gameState.FailCount = 3;

            _sut.StartNewGame();

            Assert.That(_gameState.FailCount, Is.EqualTo(0));
        }

        [Test]
        public void OnDealingComplete_SetsPhaseToPlaying()
        {
            _gameState.Phase = GamePhase.Dealing;

            _sut.OnDealingComplete();

            Assert.That(_gameState.Phase, Is.EqualTo(GamePhase.Playing));
        }

        [Test]
        public void Pause_FromPlaying_SetsPhaseToPaused()
        {
            _gameState.Phase = GamePhase.Playing;

            _sut.Pause();

            Assert.That(_gameState.Phase, Is.EqualTo(GamePhase.Paused));
        }

        [Test]
        public void Pause_FromOtherPhase_DoesNothing()
        {
            _gameState.Phase = GamePhase.Dealing;

            _sut.Pause();

            Assert.That(_gameState.Phase, Is.EqualTo(GamePhase.Dealing));
        }

        [Test]
        public void Resume_FromPaused_SetsPhaseToPlaying()
        {
            _gameState.Phase = GamePhase.Paused;

            _sut.Resume();

            Assert.That(_gameState.Phase, Is.EqualTo(GamePhase.Playing));
        }

        [Test]
        public void Resume_FromOtherPhase_DoesNothing()
        {
            _gameState.Phase = GamePhase.Dealing;

            _sut.Resume();

            Assert.That(_gameState.Phase, Is.EqualTo(GamePhase.Dealing));
        }

        [Test]
        public void IsInputAllowed_DuringPlaying_ReturnsTrue()
        {
            _gameState.Phase = GamePhase.Playing;

            bool result = _sut.IsInputAllowed();

            Assert.That(result, Is.True);
        }

        [Test]
        public void IsInputAllowed_DuringDealing_ReturnsFalse()
        {
            _gameState.Phase = GamePhase.Dealing;

            bool result = _sut.IsInputAllowed();

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsInputAllowed_DuringPaused_ReturnsFalse()
        {
            _gameState.Phase = GamePhase.Paused;

            bool result = _sut.IsInputAllowed();

            Assert.That(result, Is.False);
        }

        [Test]
        public void IsInputAllowed_DuringWin_ReturnsFalse()
        {
            _gameState.Phase = GamePhase.Win;

            bool result = _sut.IsInputAllowed();

            Assert.That(result, Is.False);
        }

        [Test]
        public void OnAllCardsMatched_SetsPhaseToWin()
        {
            _gameState.Phase = GamePhase.Playing;

            _sut.OnAllCardsMatched(100, 200);

            Assert.That(_gameState.Phase, Is.EqualTo(GamePhase.Win));
        }
    }
}
