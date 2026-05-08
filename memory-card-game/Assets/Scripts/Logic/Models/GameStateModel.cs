namespace CardMatch.Logic.Models
{
    public sealed class GameStateModel
    {
        public int Score { get; set; }

        public int StrikeCount { get; set; }

        public int FailCount { get; set; }

        public int MaxStrike { get; set; }

        public GamePhase Phase { get; set; }
    }
}
