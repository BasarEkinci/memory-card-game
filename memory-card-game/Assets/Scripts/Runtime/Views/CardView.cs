using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using VContainer;
using CardMatch.Logic.Models;
using CardMatch.Logic.Systems;
using CardMatch.Runtime.ScriptableObjects;

namespace CardMatch.Runtime.Views
{
    public sealed class CardView : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private Image _cardImage;
        [SerializeField] private CardDefinitions _cardDefinitions;

        private CardModel _model;
        private MatchSystem _matchSystem;
        private int _gridIndex;
        private CancellationTokenSource _cts;
        private RectTransform _rectTransform;

        [Inject]
        public void Construct(MatchSystem matchSystem)
        {
            _matchSystem = matchSystem;
        }

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _cts = new CancellationTokenSource();
        }

        public void Initialize(CardModel model, int gridIndex)
        {
            _model = model;
            _gridIndex = gridIndex;
            SetFaceDown();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_model == null || _matchSystem == null)
            {
                return;
            }

            _matchSystem.SelectCard(_gridIndex);
        }

        public async UniTask PlayFlipAnimation(CardState newState, CancellationToken token)
        {
            await LMotion.Create(1f, 0f, 0.15f)
                .WithEase(Ease.InQuad)
                .BindToLocalScaleX(_rectTransform)
                .ToUniTask(token);

            if (newState == CardState.FaceUp)
            {
                SetCardFace(_model.TypeId);
            }
            else
            {
                SetFaceDown();
            }

            await LMotion.Create(0f, 1f, 0.15f)
                .WithEase(Ease.OutQuad)
                .BindToLocalScaleX(_rectTransform)
                .ToUniTask(token);
        }

        public async UniTask PlayMatchedAnimation(Vector2 targetPosition, CancellationToken token)
        {
            Vector2 startPos = _rectTransform.anchoredPosition;

            await LMotion.Create(startPos, targetPosition, 0.4f)
                .WithEase(Ease.InBack)
                .BindToAnchoredPosition(_rectTransform)
                .ToUniTask(token);

            gameObject.SetActive(false);
        }

        public void SetCardFace(int typeId)
        {
            if (_cardDefinitions != null)
            {
                _cardImage.sprite = _cardDefinitions.GetFaceSprite(typeId);
            }
        }

        public void SetFaceDown()
        {
            if (_cardDefinitions != null)
            {
                _cardImage.sprite = _cardDefinitions.BackSprite;
            }
        }

        public void ResetCard()
        {
            gameObject.SetActive(true);
            _rectTransform.localScale = Vector3.one;
            SetFaceDown();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
