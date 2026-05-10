using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitMotion;
using LitMotion.Extensions;
using VContainer;
using MessagePipe;
using CardMatch.Logic.Messages;

namespace CardMatch.Runtime.Views
{
    public sealed class ResetConfirmPopupView : MonoBehaviour
    {
        private const float POPUP_DURATION = 0.25f;
        private const float SCALE_ZERO = 0f;
        private const float SCALE_ONE = 1f;

        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private GameObject _popupRoot;
        [SerializeField] private RectTransform _popupTransform;

        private ISubscriber<OpenResetConfirmRequestedMessage> _openResetConfirmSubscriber;
        private IPublisher<ResetConfirmedMessage> _resetConfirmedPublisher;
        private IPublisher<NewGameRequestedMessage> _newGamePublisher;
        private IDisposable _openResetConfirmDisposable;
        private CancellationTokenSource _cts;

        [Inject]
        public void Construct(
            ISubscriber<OpenResetConfirmRequestedMessage> openResetConfirmSubscriber,
            IPublisher<ResetConfirmedMessage> resetConfirmedPublisher,
            IPublisher<NewGameRequestedMessage> newGamePublisher)
        {
            _openResetConfirmSubscriber = openResetConfirmSubscriber;
            _resetConfirmedPublisher = resetConfirmedPublisher;
            _newGamePublisher = newGamePublisher;
        }

        private void Awake()
        {
            _confirmButton.onClick.AddListener(OnConfirmClicked);
            _cancelButton.onClick.AddListener(OnCancelClicked);
            _cts = new CancellationTokenSource();
        }

        private void Start()
        {
            _openResetConfirmDisposable = _openResetConfirmSubscriber.Subscribe(OnOpenResetConfirmRequested);
        }

        private void OnOpenResetConfirmRequested(OpenResetConfirmRequestedMessage msg)
        {
            _popupRoot.SetActive(true);
            PlayOpenAnimation().Forget();
        }

        private void OnConfirmClicked()
        {
            PlayCloseAnimationThenConfirm().Forget();
        }

        private void OnCancelClicked()
        {
            PlayCloseAnimation().Forget();
        }

        private async UniTaskVoid PlayOpenAnimation()
        {
            _popupTransform.localScale = Vector3.zero;
            var animateX = LMotion.Create(SCALE_ZERO, SCALE_ONE, POPUP_DURATION)
                .WithEase(Ease.OutBack)
                .BindToLocalScaleX(_popupTransform)
                .ToUniTask(_cts.Token);
            var animateY = LMotion.Create(SCALE_ZERO, SCALE_ONE, POPUP_DURATION)
                .WithEase(Ease.OutBack)
                .BindToLocalScaleY(_popupTransform)
                .ToUniTask(_cts.Token);
            await UniTask.WhenAll(animateX, animateY);
        }

        private async UniTaskVoid PlayCloseAnimation()
        {
            var animateX = LMotion.Create(SCALE_ONE, SCALE_ZERO, POPUP_DURATION)
                .WithEase(Ease.InBack)
                .BindToLocalScaleX(_popupTransform)
                .ToUniTask(_cts.Token);
            var animateY = LMotion.Create(SCALE_ONE, SCALE_ZERO, POPUP_DURATION)
                .WithEase(Ease.InBack)
                .BindToLocalScaleY(_popupTransform)
                .ToUniTask(_cts.Token);
            await UniTask.WhenAll(animateX, animateY);
            _popupRoot.SetActive(false);
        }

        private async UniTaskVoid PlayCloseAnimationThenConfirm()
        {
            var animateX = LMotion.Create(SCALE_ONE, SCALE_ZERO, POPUP_DURATION)
                .WithEase(Ease.InBack)
                .BindToLocalScaleX(_popupTransform)
                .ToUniTask(_cts.Token);
            var animateY = LMotion.Create(SCALE_ONE, SCALE_ZERO, POPUP_DURATION)
                .WithEase(Ease.InBack)
                .BindToLocalScaleY(_popupTransform)
                .ToUniTask(_cts.Token);
            await UniTask.WhenAll(animateX, animateY);
            _popupRoot.SetActive(false);
            _resetConfirmedPublisher.Publish(new ResetConfirmedMessage());
            _newGamePublisher.Publish(new NewGameRequestedMessage());
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _openResetConfirmDisposable?.Dispose();
            _confirmButton.onClick.RemoveListener(OnConfirmClicked);
            _cancelButton.onClick.RemoveListener(OnCancelClicked);
        }
    }
}
