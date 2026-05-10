using UnityEngine;
using VContainer;
using CardMatch.Runtime.ScriptableObjects;

namespace CardMatch.Runtime.Views
{
    public sealed class GridView : MonoBehaviour
    {
        private const float DEFAULT_COLUMN_SPACING = 2.2f;
        private const float DEFAULT_ROW_SPACING = 2.8f;
        private const int DEFAULT_GRID_COLUMNS = 4;
        private const float HALF_DIVISOR = 2f;
        private const float POSITION_Z = 0f;

        [SerializeField] private CardView[] _cardViews;

        private Transform _transform;
        private GameConfig _gameConfig;

        [Inject]
        public void Construct(GameConfig gameConfig)
        {
            _gameConfig = gameConfig;
        }

        private void Awake()
        {
            _transform = transform;
        }

        private float ColumnSpacing => _gameConfig != null ? _gameConfig.ColumnSpacing : DEFAULT_COLUMN_SPACING;
        private float RowSpacing => _gameConfig != null ? _gameConfig.RowSpacing : DEFAULT_ROW_SPACING;
        private int Columns => _gameConfig != null ? _gameConfig.GridColumns : DEFAULT_GRID_COLUMNS;

        public CardView[] GetCardViews() => _cardViews;

        public CardView GetCardView(int index) => _cardViews[index];

        public int CardCount => _cardViews?.Length ?? 0;

        public Vector3 GetCardPosition(int gridIndex)
        {
            if (gridIndex < 0 || gridIndex >= _cardViews.Length)
            {
                return _transform.position;
            }

            return CalculateGridPosition(gridIndex);
        }

        public Vector3 CalculateGridPosition(int gridIndex)
        {
            int row = gridIndex / Columns;
            int column = gridIndex % Columns;

            float gridWidth = (Columns - 1) * ColumnSpacing;
            float gridHeight = ((_cardViews.Length / Columns) - 1) * RowSpacing;

            float startX = _transform.position.x - (gridWidth / HALF_DIVISOR);
            float startY = _transform.position.y + (gridHeight / HALF_DIVISOR);

            float posX = startX + (column * ColumnSpacing);
            float posY = startY - (row * RowSpacing);

            return new Vector3(posX, posY, POSITION_Z);
        }

        public void PositionAllCards()
        {
            for (int cardIndex = 0; cardIndex < _cardViews.Length; cardIndex++)
            {
                Vector3 position = CalculateGridPosition(cardIndex);
                _cardViews[cardIndex].SetPosition(position);
            }
        }
    }
}
