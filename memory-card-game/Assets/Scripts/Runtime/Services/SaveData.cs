namespace CardMatch.Runtime.Services
{
    [System.Serializable]
    public sealed class SaveData
    {
        public int Score;
        public int StrikeCount;
        public int FailCount;
        public int MaxStrike;
        public int Phase;
        public int[] CardStates;
        public int[] CardTypeIds;
    }
}
