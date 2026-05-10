using System;
using UnityEngine;

namespace CardMatch.Runtime.ScriptableObjects
{
    [CreateAssetMenu(menuName = "CardMatch/GameConfig", fileName = "GameConfig")]
    public sealed class GameConfig : ScriptableObject
    {
        [SerializeField] private int _cardCount = 16;
        [SerializeField] private int _gridColumns = 4;
        [SerializeField] private float _columnSpacing = 2.2f;
        [SerializeField] private float _rowSpacing = 2.8f;
        [SerializeField] private float _cardScale = 4f;
        [SerializeField] private float _dealDuration = 3f;
        [SerializeField] private float _flipDuration = 0.3f;
        [SerializeField] private float _noMatchRevealTime = 2f;
        [SerializeField] private float _autoCloseTime = 10f;
        [SerializeField] private int[] _penaltyThresholds = { 4, 6, 8 };
        [SerializeField] private int[] _penaltyAmounts = { 1, 2, 3 };

        public int CardCount => _cardCount;
        public int GridColumns => _gridColumns;
        public float ColumnSpacing => _columnSpacing;
        public float RowSpacing => _rowSpacing;
        public float CardScale => _cardScale;
        public float DealDuration => _dealDuration;
        public float DealCardDelay => _dealDuration / _cardCount;
        public float FlipDuration => _flipDuration;
        public float NoMatchRevealTime => _noMatchRevealTime;
        public float AutoCloseTime => _autoCloseTime;
        public ReadOnlySpan<int> PenaltyThresholds => _penaltyThresholds;
        public ReadOnlySpan<int> PenaltyAmounts => _penaltyAmounts;
    }
}
