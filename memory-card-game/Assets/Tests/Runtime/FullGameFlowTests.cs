using NUnit.Framework;
using CardMatch.Logic.Models;
using CardMatch.Logic.Systems;

namespace CardMatch.Runtime.Tests
{
    [TestFixture]
    public sealed class FullGameFlowTests
    {
        private GameStateModel _gameState;
        private GridModel _gridModel;
        private CardModel[] _cards;
        private CardSystem _cardSystem;
        private GridSystem _gridSystem;
        private MatchSystem _matchSystem;
        private GameFlowSystem _gameFlowSystem;

        [SetUp]
        public void SetUp()
        {
            _gameState = new GameStateModel();
            _gridModel = new GridModel();
            _cards = new CardModel[16];
            for (int cardIndex = 0; cardIndex < 16; cardIndex++)
            {
                _cards[cardIndex] = new CardModel
                {
                    GridIndex = cardIndex,
                    TypeId = cardIndex / 2,   // Pairs: (0,1), (2,3), ..., (14,15)
                    State = CardState.InDeck
                };
            }
            _cardSystem = new CardSystem(_cards);
            _gridSystem = new GridSystem(_gridModel, _cards, availableTypeCount: 8);
            _matchSystem = new MatchSystem(_cardSystem, _gameState);
            _gameFlowSystem = new GameFlowSystem(_gameState, _gridSystem, _cardSystem);
        }

        // ─────────────────────────────────────────────────
        // New Game Phase
        // ─────────────────────────────────────────────────

        [Test]
        public void NewGame_StartsInDealingPhase()
        {
            _gameFlowSystem.StartNewGame();

            Assert.That(_gameState.Phase, Is.EqualTo(GamePhase.Dealing));
        }

        [Test]
        public void NewGame_ScoreIsZero()
        {
            _gameState.Score = 99;

            _gameFlowSystem.StartNewGame();

            Assert.That(_gameState.Score, Is.EqualTo(0));
        }

        [Test]
        public void NewGame_AllCardsResetToInDeck()
        {
            // Manually put a card in Matched state before starting
            _cards[0].State = CardState.Matched;

            _gameFlowSystem.StartNewGame();

            for (int cardIndex = 0; cardIndex < 16; cardIndex++)
            {
                Assert.That(_cards[cardIndex].State, Is.EqualTo(CardState.InDeck),
                    $"Card at index {cardIndex} should be InDeck after StartNewGame");
            }
        }

        // ─────────────────────────────────────────────────
        // Dealing → Playing Transition
        // ─────────────────────────────────────────────────

        [Test]
        public void OnDealingComplete_TransitionsToPlayingPhase()
        {
            _gameFlowSystem.StartNewGame();

            _gameFlowSystem.OnDealingComplete();

            Assert.That(_gameState.Phase, Is.EqualTo(GamePhase.Playing));
        }

        [Test]
        public void OnDealingComplete_InputBecomesAllowed()
        {
            _gameFlowSystem.StartNewGame();
            _gameFlowSystem.OnDealingComplete();

            Assert.That(_gameFlowSystem.IsInputAllowed(), Is.True);
        }

        // ─────────────────────────────────────────────────
        // Full Match → Win Flow
        // ─────────────────────────────────────────────────

        [Test]
        public void MatchAllPairs_EntersWinPhase()
        {
            _gameFlowSystem.StartNewGame();
            _gameFlowSystem.OnDealingComplete();

            // Simulate dealing: put all cards face-down so MatchSystem can select them
            PrepareCardsForMatching();

            MatchEvaluationResult finalResult = MatchAllPairs();

            Assert.That(finalResult.AllMatched, Is.True);

            _gameFlowSystem.OnAllCardsMatched(_gameState.Score, _gameState.Score);

            Assert.That(_gameState.Phase, Is.EqualTo(GamePhase.Win));
        }

        [Test]
        public void MatchAllPairs_FinalResultReportsAllMatched()
        {
            _gameFlowSystem.StartNewGame();
            _gameFlowSystem.OnDealingComplete();
            PrepareCardsForMatching();

            MatchEvaluationResult finalResult = MatchAllPairs();

            Assert.That(finalResult.AllMatched, Is.True);
        }

        [Test]
        public void MatchAllPairs_PerfectGame_ScoreIsNonZero()
        {
            _gameFlowSystem.StartNewGame();
            _gameFlowSystem.OnDealingComplete();
            PrepareCardsForMatching();

            MatchAllPairs();

            Assert.That(_gameState.Score, Is.GreaterThan(0));
        }

        [Test]
        public void MatchAllPairs_AllCardsAreMatchedState()
        {
            _gameFlowSystem.StartNewGame();
            _gameFlowSystem.OnDealingComplete();
            PrepareCardsForMatching();

            MatchAllPairs();

            for (int cardIndex = 0; cardIndex < 16; cardIndex++)
            {
                Assert.That(_cards[cardIndex].State, Is.EqualTo(CardState.Matched),
                    $"Card at index {cardIndex} should be Matched after all pairs found");
            }
        }

        // ─────────────────────────────────────────────────
        // Pause / Resume Flow
        // ─────────────────────────────────────────────────

        [Test]
        public void Pause_DuringPlaying_BlocksInput()
        {
            _gameFlowSystem.StartNewGame();
            _gameFlowSystem.OnDealingComplete();

            _gameFlowSystem.Pause();

            Assert.That(_gameFlowSystem.IsInputAllowed(), Is.False);
        }

        [Test]
        public void Resume_AfterPause_AllowsInput()
        {
            _gameFlowSystem.StartNewGame();
            _gameFlowSystem.OnDealingComplete();
            _gameFlowSystem.Pause();

            _gameFlowSystem.Resume();

            Assert.That(_gameFlowSystem.IsInputAllowed(), Is.True);
        }

        [Test]
        public void PauseResumeCycle_PhaseReturnsToPlaying()
        {
            _gameFlowSystem.StartNewGame();
            _gameFlowSystem.OnDealingComplete();

            _gameFlowSystem.Pause();
            _gameFlowSystem.Resume();

            Assert.That(_gameState.Phase, Is.EqualTo(GamePhase.Playing));
        }

        // ─────────────────────────────────────────────────
        // Win Phase
        // ─────────────────────────────────────────────────

        [Test]
        public void OnAllCardsMatched_ReturnsCorrectFinalScore()
        {
            _gameFlowSystem.StartNewGame();
            _gameFlowSystem.OnDealingComplete();
            PrepareCardsForMatching();
            MatchAllPairs();

            int expectedScore = _gameState.Score;
            (int finalScore, int bestScore, int maxStrike) = _gameFlowSystem.OnAllCardsMatched(expectedScore, expectedScore);

            Assert.That(finalScore, Is.EqualTo(expectedScore));
        }

        [Test]
        public void OnAllCardsMatched_ReturnsMaxStrike()
        {
            _gameFlowSystem.StartNewGame();
            _gameFlowSystem.OnDealingComplete();
            PrepareCardsForMatching();
            MatchAllPairs();

            int expectedMaxStrike = _gameState.MaxStrike;
            (int finalScore, int bestScore, int maxStrike) = _gameFlowSystem.OnAllCardsMatched(_gameState.Score, _gameState.Score);

            Assert.That(maxStrike, Is.EqualTo(expectedMaxStrike));
        }

        // ─────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────

        // After StartNewGame, cards are InDeck. The view would animate them to FaceDown.
        // Set them FaceDown manually so MatchSystem can select them.
        private void PrepareCardsForMatching()
        {
            for (int cardIndex = 0; cardIndex < _cards.Length; cardIndex++)
            {
                _cards[cardIndex].State = CardState.FaceDown;
            }
        }

        // Matches all 8 pairs in order (TypeId 0..7) and returns the final result.
        private MatchEvaluationResult MatchAllPairs()
        {
            MatchEvaluationResult lastResult = default;

            for (int typeId = 0; typeId < 8; typeId++)
            {
                int firstIndex = -1;
                int secondIndex = -1;

                for (int cardIndex = 0; cardIndex < _cards.Length; cardIndex++)
                {
                    if (_cards[cardIndex].TypeId == typeId && _cards[cardIndex].State == CardState.FaceDown)
                    {
                        if (firstIndex == -1)
                        {
                            firstIndex = cardIndex;
                        }
                        else
                        {
                            secondIndex = cardIndex;
                            break;
                        }
                    }
                }

                Assert.That(firstIndex, Is.GreaterThanOrEqualTo(0), $"Could not find first card for typeId {typeId}");
                Assert.That(secondIndex, Is.GreaterThanOrEqualTo(0), $"Could not find second card for typeId {typeId}");

                _matchSystem.SelectCard(firstIndex);
                _matchSystem.SelectCard(secondIndex);
                lastResult = _matchSystem.EvaluateMatch();
            }

            return lastResult;
        }
    }
}
