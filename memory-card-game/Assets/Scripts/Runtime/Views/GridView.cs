using UnityEngine;

namespace CardMatch.Runtime.Views
{
    public sealed class GridView : MonoBehaviour
    {
        [SerializeField] private CardView[] _cardViews;
        [SerializeField] private RectTransform _gridContainer;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
        }

        public CardView[] GetCardViews() => _cardViews;

        public CardView GetCardView(int index) => _cardViews[index];

        public Vector2 GetCardPosition(int gridIndex)
        {
            if (gridIndex < 0 || gridIndex >= _cardViews.Length)
                return Vector2.zero;

            return _cardViews[gridIndex].GetComponent<RectTransform>().anchoredPosition;
        }

        public int CardCount => _cardViews?.Length ?? 0;
    }
}
