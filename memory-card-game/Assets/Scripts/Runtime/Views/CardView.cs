using System.Threading;
using Cysharp.Threading.Tasks;
using LitMotion;
using LitMotion.Extensions;
using UnityEngine;
using VContainer;
using CardMatch.Logic.Models;
using CardMatch.Runtime.ScriptableObjects;

namespace CardMatch.Runtime.Views
{
    public sealed class CardView : MonoBehaviour
    {
        private const float FLIP_HALF_DURATION = 0.15f;
        private const float MATCHED_ANIMATION_DURATION = 0.4f;
        private const float SCALE_Z = 1f;
        private const float HOVER_SCALE_MULTIPLIER = 1.1f;
        private const float HOVER_DURATION = 0.15f;

        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private CardDefinitions _cardDefinitions;

        private CardModel _model;
        private Transform _transform;
        private float _originalScaleX;
        private float _hoverScaleX;
        private MotionHandle _hoverHandleX;
        private MotionHandle _hoverHandleY;

        public int GridIndex { get; private set; }

        private void Awake()
        {
            _transform = transform;
            _originalScaleX = _transform.localScale.x;
            _hoverScaleX = _originalScaleX * HOVER_SCALE_MULTIPLIER;
        }

        public void Initialize(CardModel model, int gridIndex)
        {
            _model = model;
            GridIndex = gridIndex;
            SetFaceDown();
        }

        public async UniTask PlayFlipAnimation(CardState newState, CancellationToken token)
        {
            await LMotion.Create(_originalScaleX, 0f, FLIP_HALF_DURATION)
                .WithEase(Ease.InQuad)
                .BindToLocalScaleX(_transform)
                .ToUniTask(token);

            if (newState == CardState.FaceUp)
            {
                SetCardFace(_model.TypeId);
            }
            else
            {
                SetFaceDown();
            }

            await LMotion.Create(0f, _originalScaleX, FLIP_HALF_DURATION)
                .WithEase(Ease.OutQuad)
                .BindToLocalScaleX(_transform)
                .ToUniTask(token);
        }

        public async UniTask PlayMatchedAnimation(Vector3 targetPosition, CancellationToken token)
        {
            await LMotion.Create(_transform.position, targetPosition, MATCHED_ANIMATION_DURATION)
                .WithEase(Ease.InBack)
                .BindToPosition(_transform)
                .ToUniTask(token);

            gameObject.SetActive(false);
        }

        public async UniTask PlayDealAnimation(Vector3 targetPosition, float duration, CancellationToken token)
        {
            await LMotion.Create(_transform.position, targetPosition, duration)
                .WithEase(Ease.OutBack)
                .BindToPosition(_transform)
                .ToUniTask(token);
        }

        public void SetCardFace(int typeId)
        {
            if (_cardDefinitions != null && _spriteRenderer != null)
            {
                _spriteRenderer.sprite = _cardDefinitions.GetFaceSprite(typeId);
            }
        }

        public void SetFaceDown()
        {
            if (_cardDefinitions != null && _spriteRenderer != null)
            {
                _spriteRenderer.sprite = _cardDefinitions.BackSprite;
            }
        }

        public void ResetCard()
        {
            gameObject.SetActive(true);
            _transform.localScale = new Vector3(_originalScaleX, _originalScaleX, SCALE_Z);
            SetFaceDown();
        }

        public void SetPosition(Vector3 worldPosition)
        {
            _transform.position = worldPosition;
        }

        public void OnHoverEnter()
        {
            CancelHoverAnimations();
            _hoverHandleX = LMotion.Create(_transform.localScale.x, _hoverScaleX, HOVER_DURATION)
                .WithEase(Ease.OutQuad)
                .BindToLocalScaleX(_transform);
            _hoverHandleY = LMotion.Create(_transform.localScale.y, _hoverScaleX, HOVER_DURATION)
                .WithEase(Ease.OutQuad)
                .BindToLocalScaleY(_transform);
        }

        public void OnHoverExit()
        {
            CancelHoverAnimations();
            _hoverHandleX = LMotion.Create(_transform.localScale.x, _originalScaleX, HOVER_DURATION)
                .WithEase(Ease.OutQuad)
                .BindToLocalScaleX(_transform);
            _hoverHandleY = LMotion.Create(_transform.localScale.y, _originalScaleX, HOVER_DURATION)
                .WithEase(Ease.OutQuad)
                .BindToLocalScaleY(_transform);
        }

        private void CancelHoverAnimations()
        {
            if (_hoverHandleX.IsActive())
            {
                _hoverHandleX.Cancel();
            }
            if (_hoverHandleY.IsActive())
            {
                _hoverHandleY.Cancel();
            }
        }

        private void OnDestroy()
        {
            CancelHoverAnimations();
        }
    }
}
