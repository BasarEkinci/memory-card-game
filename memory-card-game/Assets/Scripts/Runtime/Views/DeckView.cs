using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;

namespace CardMatch.Runtime.Views
{
    public sealed class DeckView : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;

        private Transform _transform;

        private void Awake()
        {
            _transform = transform;
        }

        public Vector3 GetDeckPosition() => _transform.position;

        public async UniTask PlayShuffleAnimation(CancellationToken token)
        {
            Debug.Log("[DeckView] PlayShuffleAnimation started");

            if (_transform == null)
            {
                Debug.LogError("[DeckView] Transform is null!");
                return;
            }

            await UniTask.Delay(100, cancellationToken: token);

            Debug.Log("[DeckView] PlayShuffleAnimation completed");
        }
    }
}
