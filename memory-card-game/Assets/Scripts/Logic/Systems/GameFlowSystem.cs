using CardMatch.Logic.Models;

namespace CardMatch.Logic.Systems
{
    public sealed class GameFlowSystem
    {
        private readonly GameStateModel _gameState;
        private readonly GridSystem _gridSystem;
        private readonly CardSystem _cardSystem;

        public GameFlowSystem(GameStateModel gameState, GridSystem gridSystem, CardSystem cardSystem)
        {
            _gameState = gameState;
            _gridSystem = gridSystem;
            _cardSystem = cardSystem;
        }

        public GamePhase SetPhase(GamePhase phase)
        {
            _gameState.Phase = phase;
            return _gameState.Phase;
        }

        public void StartNewGame()
        {
            _gridSystem.Shuffle();
            _cardSystem.ResetAllCards();

            _gameState.Score = 0;
            _gameState.StrikeCount = 0;
            _gameState.FailCount = 0;
            _gameState.MaxStrike = 0;

            SetPhase(GamePhase.Dealing);
        }

        public void OnDealingComplete()
        {
            SetPhase(GamePhase.Playing);
        }

        public (int finalScore, int bestScore, int maxStrike) OnAllCardsMatched(int finalScore, int bestScore)
        {
            SetPhase(GamePhase.Win);
            return (finalScore, bestScore, _gameState.MaxStrike);
        }

        public void Pause()
        {
            if (_gameState.Phase == GamePhase.Playing)
            {
                SetPhase(GamePhase.Paused);
            }
        }

        public void Resume()
        {
            if (_gameState.Phase == GamePhase.Paused)
            {
                SetPhase(GamePhase.Playing);
            }
        }

        public bool IsInputAllowed()
        {
            return _gameState.Phase == GamePhase.Playing;
        }

        public GamePhase GetCurrentPhase()
        {
            return _gameState.Phase;
        }
    }
}
