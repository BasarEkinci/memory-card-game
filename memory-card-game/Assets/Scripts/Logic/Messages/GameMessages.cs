using CardMatch.Logic.Models;

namespace CardMatch.Logic.Messages
{
    public readonly struct CardFlippedMessage
    {
        public readonly int CardIndex;
        public readonly CardState NewState;

        public CardFlippedMessage(int cardIndex, CardState newState)
        {
            CardIndex = cardIndex;
            NewState = newState;
        }
    }

    public readonly struct MatchResultMessage
    {
        public readonly bool IsMatch;
        public readonly int CardIndex1;
        public readonly int CardIndex2;
        public readonly int ScoreDelta;
        public readonly int NewStrike;
        public readonly int NewFailCount;

        public MatchResultMessage(bool isMatch, int cardIndex1, int cardIndex2, int scoreDelta, int newStrike, int newFailCount)
        {
            IsMatch = isMatch;
            CardIndex1 = cardIndex1;
            CardIndex2 = cardIndex2;
            ScoreDelta = scoreDelta;
            NewStrike = newStrike;
            NewFailCount = newFailCount;
        }
    }

    public readonly struct PenaltyAppliedMessage
    {
        public readonly int PenaltyAmount;
        public readonly int NewScore;

        public PenaltyAppliedMessage(int penaltyAmount, int newScore)
        {
            PenaltyAmount = penaltyAmount;
            NewScore = newScore;
        }
    }

    public readonly struct GamePhaseChangedMessage
    {
        public readonly GamePhase NewPhase;

        public GamePhaseChangedMessage(GamePhase newPhase)
        {
            NewPhase = newPhase;
        }
    }

    public readonly struct GameWonMessage
    {
        public readonly int FinalScore;
        public readonly int BestScore;
        public readonly int MaxStrike;

        public GameWonMessage(int finalScore, int bestScore, int maxStrike)
        {
            FinalScore = finalScore;
            BestScore = bestScore;
            MaxStrike = maxStrike;
        }
    }

    public readonly struct SettingsChangedMessage
    {
        public readonly float MusicVolume;
        public readonly float SfxVolume;

        public SettingsChangedMessage(float musicVolume, float sfxVolume)
        {
            MusicVolume = musicVolume;
            SfxVolume = sfxVolume;
        }
    }

    public readonly struct ResetRequestedMessage
    {
    }

    public readonly struct OpenSettingsRequestedMessage
    {
    }

    public readonly struct CloseSettingsRequestedMessage
    {
    }

    public readonly struct OpenResetConfirmRequestedMessage
    {
    }

    public readonly struct ResetConfirmedMessage
    {
    }

    public readonly struct NewGameRequestedMessage
    {
    }
}
