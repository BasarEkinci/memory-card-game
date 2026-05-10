using System;
using CardMatch.Logic.Models;

namespace CardMatch.Logic.Systems
{
    public enum SelectionResult
    {
        Ignored,
        WaitingForSecond,
        ReadyToEvaluate,
        Deselected
    }

    public readonly struct MatchEvaluationResult
    {
        public readonly bool IsMatch;
        public readonly int ScoreDelta;
        public readonly int PenaltyApplied;
        public readonly int NewStrike;
        public readonly int NewFailCount;
        public readonly bool AllMatched;
        public readonly int CardIndex1;
        public readonly int CardIndex2;

        public MatchEvaluationResult(
            bool isMatch,
            int scoreDelta,
            int penaltyApplied,
            int newStrike,
            int newFailCount,
            bool allMatched,
            int cardIndex1,
            int cardIndex2)
        {
            IsMatch = isMatch;
            ScoreDelta = scoreDelta;
            PenaltyApplied = penaltyApplied;
            NewStrike = newStrike;
            NewFailCount = newFailCount;
            AllMatched = allMatched;
            CardIndex1 = cardIndex1;
            CardIndex2 = cardIndex2;
        }
    }

    public sealed class MatchSystem
    {
        private readonly CardSystem _cardSystem;
        private readonly GameStateModel _gameState;

        private int? _firstSelectedIndex;
        private int? _secondSelectedIndex;

        private static readonly int[] PenaltyThresholds = { 4, 6, 8 };
        private static readonly int[] PenaltyAmounts = { 1, 2, 3 };

        public MatchSystem(CardSystem cardSystem, GameStateModel gameState)
        {
            _cardSystem = cardSystem;
            _gameState = gameState;
        }

        public SelectionResult SelectCard(int cardIndex)
        {
            if (_firstSelectedIndex.HasValue && _firstSelectedIndex.Value == cardIndex)
            {
                CancelSelection();
                return SelectionResult.Deselected;
            }

            CardModel card = _cardSystem.GetCard(cardIndex);

            if (card == null || card.State != CardState.FaceDown)
            {
                return SelectionResult.Ignored;
            }

            _cardSystem.FlipCard(cardIndex);

            if (_firstSelectedIndex == null)
            {
                _firstSelectedIndex = cardIndex;
                return SelectionResult.WaitingForSecond;
            }

            _secondSelectedIndex = cardIndex;
            return SelectionResult.ReadyToEvaluate;
        }

        public void CancelSelection()
        {
            if (_firstSelectedIndex.HasValue)
            {
                _cardSystem.CloseCard(_firstSelectedIndex.Value);
                _firstSelectedIndex = null;
            }
        }

        public MatchEvaluationResult EvaluateMatch()
        {
            int first = _firstSelectedIndex.Value;
            int second = _secondSelectedIndex.Value;

            CardModel firstCard = _cardSystem.GetCard(first);
            CardModel secondCard = _cardSystem.GetCard(second);

            bool isMatch = firstCard.TypeId == secondCard.TypeId;

            int scoreDelta = 0;
            int penaltyApplied = 0;
            bool allMatched = false;

            if (isMatch)
            {
                int strikeBeforeIncrement = _gameState.StrikeCount;
                scoreDelta = 1 + strikeBeforeIncrement + (strikeBeforeIncrement > 0 ? 1 : 0);

                _gameState.StrikeCount++;
                _gameState.MaxStrike = Math.Max(_gameState.MaxStrike, _gameState.StrikeCount);
                _gameState.FailCount = 0;
                _gameState.Score += scoreDelta;

                _cardSystem.MarkMatched(first, second);

                allMatched = _cardSystem.AreAllMatched();
            }
            else
            {
                _gameState.StrikeCount = 0;
                _gameState.FailCount++;

                for (int thresholdIndex = 0; thresholdIndex < PenaltyThresholds.Length; thresholdIndex++)
                {
                    if (_gameState.FailCount == PenaltyThresholds[thresholdIndex])
                    {
                        penaltyApplied = PenaltyAmounts[thresholdIndex];
                        _gameState.Score = Math.Max(0, _gameState.Score - penaltyApplied);
                        break;
                    }
                }
            }

            var result = new MatchEvaluationResult(
                isMatch: isMatch,
                scoreDelta: scoreDelta,
                penaltyApplied: penaltyApplied,
                newStrike: _gameState.StrikeCount,
                newFailCount: _gameState.FailCount,
                allMatched: allMatched,
                cardIndex1: first,
                cardIndex2: second);

            _firstSelectedIndex = null;
            _secondSelectedIndex = null;

            return result;
        }

        public void ResetGame()
        {
            _gameState.Score = 0;
            _gameState.StrikeCount = 0;
            _gameState.FailCount = 0;
            _gameState.MaxStrike = 0;

            _firstSelectedIndex = null;
            _secondSelectedIndex = null;
        }
    }
}
