using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;

namespace CardMatch.Runtime.Views
{
    public sealed class DeckView : MonoBehaviour
    {
        [SerializeField] private RectTransform _deckTransform;

        private CancellationTokenSource _cts;

        private void Awake()
        {
            _cts = new CancellationTokenSource();
            if (_deckTransform == null)
                _deckTransform = GetComponent<RectTransform>();
        }

        public Vector2 GetDeckPosition() => _deckTransform.anchoredPosition;

        public async UniTask PlayShuffleAnimation(CancellationToken token)
        {
            Vector2 originalPos = _deckTransform.anchoredPosition;

            await LMotion.Create(originalPos, originalPos + new Vector2(5f, 0f), 0.05f)
                .WithEase(Ease.OutQuad)
                .BindToAnchoredPosition(_deckTransform)
                .ToUniTask(token);

            await LMotion.Create(originalPos + new Vector2(5f, 0f), originalPos - new Vector2(5f, 0f), 0.1f)
                .WithEase(Ease.InOutQuad)
                .BindToAnchoredPosition(_deckTransform)
                .ToUniTask(token);

            await LMotion.Create(originalPos - new Vector2(5f, 0f), originalPos, 0.05f)
                .WithEase(Ease.InQuad)
                .BindToAnchoredPosition(_deckTransform)
                .ToUniTask(token);
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
