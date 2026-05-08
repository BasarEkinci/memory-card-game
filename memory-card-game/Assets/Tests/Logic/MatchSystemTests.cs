using NUnit.Framework;
using CardMatch.Logic.Models;
using CardMatch.Logic.Systems;

namespace CardMatch.Logic.Tests
{
    [TestFixture]
    public sealed class MatchSystemTests
    {
        private CardModel[] _cards;
        private CardSystem _cardSystem;
        private GameStateModel _gameState;
        private MatchSystem _sut;

        [SetUp]
        public void SetUp()
        {
            _cards = new CardModel[16];
            for (int cardIndex = 0; cardIndex < 16; cardIndex++)
            {
                _cards[cardIndex] = new CardModel
                {
                    GridIndex = cardIndex,
                    TypeId = cardIndex / 2,   // Pairs: (0,1), (2,3), (4,5), (6,7), (8,9), (10,11), (12,13), (14,15)
                    State = CardState.FaceDown
                };
            }
            _cardSystem = new CardSystem(_cards);
            _gameState = new GameStateModel();
            _sut = new MatchSystem(_cardSystem, _gameState);
        }

        // ─────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────

        // Simulates selecting a matched pair; both cards end up Matched state.
        private MatchEvaluationResult SimulateMatch(int card1, int card2)
        {
            _sut.SelectCard(card1);
            _sut.SelectCard(card2);
            return _sut.EvaluateMatch();
        }

        // Simulates selecting a mismatched pair; returns result, closes both cards afterward.
        private MatchEvaluationResult SimulateFail(int card1, int card2)
        {
            EnsureFaceDown(card1);
            EnsureFaceDown(card2);
            _sut.SelectCard(card1);
            _sut.SelectCard(card2);
            MatchEvaluationResult result = _sut.EvaluateMatch();
            // Cards remain FaceUp after a mismatch; close them for next use.
            EnsureFaceDown(card1);
            EnsureFaceDown(card2);
            return result;
        }

        private void EnsureFaceDown(int cardIndex)
        {
            if (_cards[cardIndex].State == CardState.FaceUp)
            {
                _cardSystem.CloseCard(cardIndex);
            }
        }

        // ─────────────────────────────────────────────────
        // Selection Tests
        // ─────────────────────────────────────────────────

        [Test]
        public void SelectCard_FirstCard_ReturnsWaitingForSecond()
        {
            SelectionResult result = _sut.SelectCard(0);

            Assert.That(result, Is.EqualTo(SelectionResult.WaitingForSecond));
        }

        [Test]
        public void SelectCard_SecondCard_ReturnsReadyToEvaluate()
        {
            _sut.SelectCard(0);

            SelectionResult result = _sut.SelectCard(2);

            Assert.That(result, Is.EqualTo(SelectionResult.ReadyToEvaluate));
        }

        [Test]
        public void SelectCard_AlreadyFaceUp_ReturnsIgnored()
        {
            _cards[0].State = CardState.FaceUp;

            SelectionResult result = _sut.SelectCard(0);

            Assert.That(result, Is.EqualTo(SelectionResult.Ignored));
        }

        [Test]
        public void SelectCard_AlreadyMatched_ReturnsIgnored()
        {
            _cards[0].State = CardState.Matched;

            SelectionResult result = _sut.SelectCard(0);

            Assert.That(result, Is.EqualTo(SelectionResult.Ignored));
        }

        [Test]
        public void CancelSelection_ClosesFirstCard()
        {
            _sut.SelectCard(0);

            _sut.CancelSelection();

            Assert.That(_cards[0].State, Is.EqualTo(CardState.FaceDown));
        }

        // ─────────────────────────────────────────────────
        // Match Detection Tests
        // ─────────────────────────────────────────────────

        [Test]
        public void EvaluateMatch_SameType_IsMatch()
        {
            // Cards 0 and 1 share TypeId 0
            MatchEvaluationResult result = SimulateMatch(0, 1);

            Assert.That(result.IsMatch, Is.True);
        }

        [Test]
        public void EvaluateMatch_DifferentType_NotMatch()
        {
            // Card 0 has TypeId 0; card 2 has TypeId 1
            MatchEvaluationResult result = SimulateFail(0, 2);

            Assert.That(result.IsMatch, Is.False);
        }

        // ─────────────────────────────────────────────────
        // Scoring Tests — GDD Appendix B
        // ─────────────────────────────────────────────────

        [Test]
        public void Scoring_FirstMatch_AddsOnePoint()
        {
            // StrikeCount before = 0 → delta = 1 + 0 + 0 = 1
            MatchEvaluationResult result = SimulateMatch(0, 1);

            Assert.That(result.ScoreDelta, Is.EqualTo(1));
        }

        [Test]
        public void Scoring_FirstMatch_TotalScoreIsOne()
        {
            SimulateMatch(0, 1);

            Assert.That(_gameState.Score, Is.EqualTo(1));
        }

        [Test]
        public void Scoring_SecondConsecutiveMatch_AddsThreePoints()
        {
            // StrikeCount before 2nd match = 1 → delta = 1 + 1 + 1 = 3
            SimulateMatch(0, 1);
            MatchEvaluationResult result = SimulateMatch(2, 3);

            Assert.That(result.ScoreDelta, Is.EqualTo(3));
        }

        [Test]
        public void Scoring_SecondConsecutiveMatch_RunningTotalIsFour()
        {
            SimulateMatch(0, 1);
            SimulateMatch(2, 3);

            Assert.That(_gameState.Score, Is.EqualTo(4));
        }

        [Test]
        public void Scoring_ThirdConsecutiveMatch_AddsFourPoints()
        {
            // StrikeCount before 3rd match = 2 → delta = 1 + 2 + 1 = 4
            SimulateMatch(0, 1);
            SimulateMatch(2, 3);
            MatchEvaluationResult result = SimulateMatch(4, 5);

            Assert.That(result.ScoreDelta, Is.EqualTo(4));
        }

        [Test]
        public void Scoring_ThirdConsecutiveMatch_RunningTotalIsEight()
        {
            SimulateMatch(0, 1);
            SimulateMatch(2, 3);
            SimulateMatch(4, 5);

            Assert.That(_gameState.Score, Is.EqualTo(8));
        }

        [Test]
        public void Scoring_FourthConsecutiveMatch_AddsFivePoints()
        {
            // StrikeCount before = 3 → delta = 1 + 3 + 1 = 5
            SimulateMatch(0, 1);
            SimulateMatch(2, 3);
            SimulateMatch(4, 5);
            MatchEvaluationResult result = SimulateMatch(6, 7);

            Assert.That(result.ScoreDelta, Is.EqualTo(5));
        }

        [Test]
        public void Scoring_FifthConsecutiveMatch_AddsSixPoints()
        {
            // StrikeCount before = 4 → delta = 1 + 4 + 1 = 6
            SimulateMatch(0, 1);
            SimulateMatch(2, 3);
            SimulateMatch(4, 5);
            SimulateMatch(6, 7);
            MatchEvaluationResult result = SimulateMatch(8, 9);

            Assert.That(result.ScoreDelta, Is.EqualTo(6));
        }

        [Test]
        public void Scoring_SixthConsecutiveMatch_AddsSevenPoints()
        {
            // StrikeCount before = 5 → delta = 1 + 5 + 1 = 7
            SimulateMatch(0, 1);
            SimulateMatch(2, 3);
            SimulateMatch(4, 5);
            SimulateMatch(6, 7);
            SimulateMatch(8, 9);
            MatchEvaluationResult result = SimulateMatch(10, 11);

            Assert.That(result.ScoreDelta, Is.EqualTo(7));
        }

        [Test]
        public void Scoring_SeventhConsecutiveMatch_AddsEightPoints()
        {
            // StrikeCount before = 6 → delta = 1 + 6 + 1 = 8
            SimulateMatch(0, 1);
            SimulateMatch(2, 3);
            SimulateMatch(4, 5);
            SimulateMatch(6, 7);
            SimulateMatch(8, 9);
            SimulateMatch(10, 11);
            MatchEvaluationResult result = SimulateMatch(12, 13);

            Assert.That(result.ScoreDelta, Is.EqualTo(8));
        }

        [Test]
        public void Scoring_EighthConsecutiveMatch_AddsNinePoints()
        {
            // StrikeCount before = 7 → delta = 1 + 7 + 1 = 9
            SimulateMatch(0, 1);
            SimulateMatch(2, 3);
            SimulateMatch(4, 5);
            SimulateMatch(6, 7);
            SimulateMatch(8, 9);
            SimulateMatch(10, 11);
            SimulateMatch(12, 13);
            MatchEvaluationResult result = SimulateMatch(14, 15);

            Assert.That(result.ScoreDelta, Is.EqualTo(9));
        }

        [Test]
        public void PerfectGame_Scores43Points()
        {
            // GDD Appendix B: 1 + 3 + 4 + 5 + 6 + 7 + 8 + 9 = 43
            SimulateMatch(0, 1);
            SimulateMatch(2, 3);
            SimulateMatch(4, 5);
            SimulateMatch(6, 7);
            SimulateMatch(8, 9);
            SimulateMatch(10, 11);
            SimulateMatch(12, 13);
            SimulateMatch(14, 15);

            Assert.That(_gameState.Score, Is.EqualTo(43));
        }

        [Test]
        public void PerfectGame_AllMatchedFlagTrueOnLastPair()
        {
            SimulateMatch(0, 1);
            SimulateMatch(2, 3);
            SimulateMatch(4, 5);
            SimulateMatch(6, 7);
            SimulateMatch(8, 9);
            SimulateMatch(10, 11);
            SimulateMatch(12, 13);
            MatchEvaluationResult finalResult = SimulateMatch(14, 15);

            Assert.That(finalResult.AllMatched, Is.True);
        }

        [Test]
        public void AllMatched_NotFinalPair_ReturnsFalse()
        {
            MatchEvaluationResult result = SimulateMatch(0, 1);

            Assert.That(result.AllMatched, Is.False);
        }

        // ─────────────────────────────────────────────────
        // Strike Tests
        // ─────────────────────────────────────────────────

        [Test]
        public void Strike_AfterFirstMatch_IncrementsToOne()
        {
            MatchEvaluationResult result = SimulateMatch(0, 1);

            Assert.That(result.NewStrike, Is.EqualTo(1));
        }

        [Test]
        public void Strike_AfterTwoConsecutiveMatches_IsTwo()
        {
            SimulateMatch(0, 1);
            MatchEvaluationResult result = SimulateMatch(2, 3);

            Assert.That(result.NewStrike, Is.EqualTo(2));
        }

        [Test]
        public void Strike_ResetOnFail_BecomesZero()
        {
            // Build a streak of 2, then fail to reset it
            SimulateMatch(0, 1);
            SimulateMatch(2, 3);
            // Mismatch: TypeId 2 (card 4) vs TypeId 3 (card 6)
            MatchEvaluationResult failResult = SimulateFail(4, 6);

            Assert.That(failResult.NewStrike, Is.EqualTo(0));
        }

        [Test]
        public void MaxStrike_TracksHighestStreak()
        {
            SimulateMatch(0, 1);
            SimulateMatch(2, 3);   // Strike = 2, MaxStrike = 2
            SimulateFail(4, 6);    // Strike drops to 0, MaxStrike stays 2
            SimulateMatch(4, 5);   // Strike goes back to 1

            Assert.That(_gameState.MaxStrike, Is.EqualTo(2));
        }

        [Test]
        public void MaxStrike_UpdatesWhenNewHighestReached()
        {
            SimulateMatch(0, 1);
            SimulateMatch(2, 3);
            SimulateMatch(4, 5);   // Strike = 3, MaxStrike = 3

            Assert.That(_gameState.MaxStrike, Is.EqualTo(3));
        }

        [Test]
        public void MaxStrike_DoesNotDecreaseAfterFail()
        {
            SimulateMatch(0, 1);
            SimulateMatch(2, 3);   // MaxStrike = 2
            SimulateFail(4, 6);    // Strike drops to 0; MaxStrike must stay 2

            Assert.That(_gameState.MaxStrike, Is.EqualTo(2));
        }

        // ─────────────────────────────────────────────────
        // Penalty Tests
        // ─────────────────────────────────────────────────

        [Test]
        public void Penalty_NonThresholdFail_NoPenaltyApplied()
        {
            // 1st, 2nd, 3rd fails — all below the first threshold (4)
            MatchEvaluationResult result = SimulateFail(0, 2);

            Assert.That(result.PenaltyApplied, Is.EqualTo(0));
        }

        [Test]
        public void Penalty_FourthFail_SubtractsOnePoint()
        {
            SimulateMatch(0, 1);    // Score = 1
            SimulateFail(2, 4);
            SimulateFail(2, 4);
            SimulateFail(2, 4);
            MatchEvaluationResult result = SimulateFail(2, 4);  // 4th fail

            Assert.That(result.PenaltyApplied, Is.EqualTo(1));
        }

        [Test]
        public void Penalty_FourthFail_ScoreDecreasedByOne()
        {
            SimulateMatch(0, 1);    // Score = 1
            SimulateFail(2, 4);
            SimulateFail(2, 4);
            SimulateFail(2, 4);
            SimulateFail(2, 4);     // 4th fail → -1, Score = 0

            Assert.That(_gameState.Score, Is.EqualTo(0));
        }

        [Test]
        public void Penalty_SixthFail_SubtractsTwoPoints()
        {
            SimulateMatch(0, 1);    // Score = 1
            SimulateMatch(2, 3);    // Score = 4
            SimulateFail(4, 6);
            SimulateFail(4, 6);
            SimulateFail(4, 6);
            SimulateFail(4, 6);     // 4th fail → -1, Score = 3
            SimulateFail(4, 6);
            MatchEvaluationResult result = SimulateFail(4, 6);  // 6th fail

            Assert.That(result.PenaltyApplied, Is.EqualTo(2));
        }

        [Test]
        public void Penalty_SixthFail_ScoreDecreasedByTwo()
        {
            SimulateMatch(0, 1);    // Score = 1
            SimulateMatch(2, 3);    // Score = 4
            SimulateFail(4, 6);
            SimulateFail(4, 6);
            SimulateFail(4, 6);
            SimulateFail(4, 6);     // 4th fail → -1, Score = 3
            SimulateFail(4, 6);
            SimulateFail(4, 6);     // 6th fail → -2, Score = 1

            Assert.That(_gameState.Score, Is.EqualTo(1));
        }

        [Test]
        public void Penalty_EighthFail_SubtractsThreePoints()
        {
            SimulateMatch(0, 1);    // Score = 1
            SimulateMatch(2, 3);    // Score = 4
            SimulateMatch(4, 5);    // Score = 8
            SimulateFail(6, 8);
            SimulateFail(6, 8);
            SimulateFail(6, 8);
            SimulateFail(6, 8);     // 4th fail → -1, Score = 7
            SimulateFail(6, 8);
            SimulateFail(6, 8);     // 6th fail → -2, Score = 5
            SimulateFail(6, 8);
            MatchEvaluationResult result = SimulateFail(6, 8);  // 8th fail

            Assert.That(result.PenaltyApplied, Is.EqualTo(3));
        }

        [Test]
        public void Penalty_EighthFail_ScoreDecreasedByThree()
        {
            SimulateMatch(0, 1);    // Score = 1
            SimulateMatch(2, 3);    // Score = 4
            SimulateMatch(4, 5);    // Score = 8
            SimulateFail(6, 8);
            SimulateFail(6, 8);
            SimulateFail(6, 8);
            SimulateFail(6, 8);     // 4th fail → -1, Score = 7
            SimulateFail(6, 8);
            SimulateFail(6, 8);     // 6th fail → -2, Score = 5
            SimulateFail(6, 8);
            SimulateFail(6, 8);     // 8th fail → -3, Score = 2

            Assert.That(_gameState.Score, Is.EqualTo(2));
        }

        [Test]
        public void Penalty_ScoreCannotGoBelowZero()
        {
            // Score is 0; the 4th fail would subtract 1 — must clamp at 0
            SimulateFail(0, 2);
            SimulateFail(0, 2);
            SimulateFail(0, 2);
            SimulateFail(0, 2);     // 4th fail → penalty 1, clamped at 0

            Assert.That(_gameState.Score, Is.EqualTo(0));
        }

        [Test]
        public void Fail_IncrementsFailCount()
        {
            MatchEvaluationResult result = SimulateFail(0, 2);

            Assert.That(result.NewFailCount, Is.EqualTo(1));
        }

        [Test]
        public void Match_ResetsFailCount()
        {
            SimulateFail(0, 2);
            SimulateFail(0, 2);
            MatchEvaluationResult result = SimulateMatch(0, 1);

            Assert.That(result.NewFailCount, Is.EqualTo(0));
        }

        // ─────────────────────────────────────────────────
        // Result Fields Tests
        // ─────────────────────────────────────────────────

        [Test]
        public void EvaluateMatch_ResultContainsCorrectCardIndices()
        {
            _sut.SelectCard(3);
            _sut.SelectCard(7);
            MatchEvaluationResult result = _sut.EvaluateMatch();

            Assert.That(result.CardIndex1, Is.EqualTo(3));
            Assert.That(result.CardIndex2, Is.EqualTo(7));
        }

        // ─────────────────────────────────────────────────
        // Reset Tests
        // ─────────────────────────────────────────────────

        [Test]
        public void ResetGame_ScoreBecomesZero()
        {
            SimulateMatch(0, 1);
            SimulateMatch(2, 3);

            _sut.ResetGame();

            Assert.That(_gameState.Score, Is.EqualTo(0));
        }

        [Test]
        public void ResetGame_StrikeCountBecomesZero()
        {
            SimulateMatch(0, 1);
            SimulateMatch(2, 3);

            _sut.ResetGame();

            Assert.That(_gameState.StrikeCount, Is.EqualTo(0));
        }

        [Test]
        public void ResetGame_FailCountBecomesZero()
        {
            SimulateFail(0, 2);
            SimulateFail(0, 2);

            _sut.ResetGame();

            Assert.That(_gameState.FailCount, Is.EqualTo(0));
        }

        [Test]
        public void ResetGame_MaxStrikeBecomesZero()
        {
            SimulateMatch(0, 1);
            SimulateMatch(2, 3);

            _sut.ResetGame();

            Assert.That(_gameState.MaxStrike, Is.EqualTo(0));
        }

        [Test]
        public void ResetGame_PendingSelectionCleared_NextSelectReturnsWaitingForSecond()
        {
            // Start a selection, then reset — the pending first-card slot must be cleared
            _sut.SelectCard(0);
            _sut.ResetGame();

            // Card 0 is still FaceUp from the SelectCard; reset does not close cards
            // so use a different card that is FaceDown
            SelectionResult result = _sut.SelectCard(2);

            Assert.That(result, Is.EqualTo(SelectionResult.WaitingForSecond));
        }

        [Test]
        public void ResetGame_ClearsAllState()
        {
            SimulateMatch(0, 1);
            SimulateMatch(2, 3);
            SimulateFail(4, 6);
            SimulateFail(4, 6);

            _sut.ResetGame();

            Assert.That(_gameState.Score, Is.EqualTo(0));
            Assert.That(_gameState.StrikeCount, Is.EqualTo(0));
            Assert.That(_gameState.FailCount, Is.EqualTo(0));
            Assert.That(_gameState.MaxStrike, Is.EqualTo(0));
        }
    }
}
