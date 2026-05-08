using UnityEngine;

namespace CardMatch.Runtime.ScriptableObjects
{
    [CreateAssetMenu(menuName = "CardMatch/GameConfig", fileName = "GameConfig")]
    public sealed class GameConfig : ScriptableObject
    {
        [SerializeField] private int _cardCount = 16;
        [SerializeField] private int _gridColumns = 4;
        [SerializeField] private float _dealDuration = 3f;
        [SerializeField] private float _flipDuration = 0.3f;
        [SerializeField] private float _noMatchRevealTime = 2f;
        [SerializeField] private float _autoCloseTime = 10f;
        [SerializeField] private int[] _penaltyThresholds = { 4, 6, 8 };
        [SerializeField] private int[] _penaltyAmounts = { 1, 2, 3 };

        public int CardCount => _cardCount;
        public int GridColumns => _gridColumns;
        public float DealDuration => _dealDuration;
        public float DealCardDelay => _dealDuration / _cardCount;
        public float FlipDuration => _flipDuration;
        public float NoMatchRevealTime => _noMatchRevealTime;
        public float AutoCloseTime => _autoCloseTime;
        public ReadOnlySpan<int> PenaltyThresholds => _penaltyThresholds;
        public ReadOnlySpan<int> PenaltyAmounts => _penaltyAmounts;
    }
}
