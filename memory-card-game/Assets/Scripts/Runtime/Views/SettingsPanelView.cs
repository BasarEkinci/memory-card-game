using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using LitMotion;
using LitMotion.Extensions;
using VContainer;
using MessagePipe;
using CardMatch.Runtime.Services;
using CardMatch.Logic.Systems;
using CardMatch.Logic.Messages;

namespace CardMatch.Runtime.Views
{
    public sealed class SettingsPanelView : MonoBehaviour
    {
        private const float POPUP_DURATION = 0.25f;
        private const float SCALE_ZERO = 0f;
        private const float SCALE_ONE = 1f;

        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _panelRoot;
        [SerializeField] private RectTransform _panelTransform;

        private AudioSystem _audioSystem;
        private GameFlowSystem _gameFlowSystem;
        private SaveSystem _saveSystem;
        private ISubscriber<OpenSettingsRequestedMessage> _openSettingsSubscriber;
        private ISubscriber<ResetConfirmedMessage> _resetConfirmedSubscriber;
        private IPublisher<OpenResetConfirmRequestedMessage> _openResetConfirmPublisher;
        private IDisposable _openSettingsDisposable;
        private IDisposable _resetConfirmedDisposable;
        private CancellationTokenSource _cts;

        [Inject]
        public void Construct(
            AudioSystem audioSystem,
            GameFlowSystem gameFlowSystem,
            SaveSystem saveSystem,
            ISubscriber<OpenSettingsRequestedMessage> openSettingsSubscriber,
            ISubscriber<ResetConfirmedMessage> resetConfirmedSubscriber,
            IPublisher<OpenResetConfirmRequestedMessage> openResetConfirmPublisher)
        {
            _audioSystem = audioSystem;
            _gameFlowSystem = gameFlowSystem;
            _saveSystem = saveSystem;
            _openSettingsSubscriber = openSettingsSubscriber;
            _resetConfirmedSubscriber = resetConfirmedSubscriber;
            _openResetConfirmPublisher = openResetConfirmPublisher;
        }

        private void Awake()
        {
            _musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            _sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            _resetButton.onClick.AddListener(OnResetClicked);
            _closeButton.onClick.AddListener(OnCloseClicked);
            _cts = new CancellationTokenSource();
        }

        private void Start()
        {
            _openSettingsDisposable = _openSettingsSubscriber.Subscribe(OnOpenSettingsRequested);
            _resetConfirmedDisposable = _resetConfirmedSubscriber.Subscribe(OnResetConfirmed);
        }

        private void OnOpenSettingsRequested(OpenSettingsRequestedMessage msg)
        {
            Open();
        }

        private void OnResetConfirmed(ResetConfirmedMessage msg)
        {
            CloseImmediate();
        }

        private void Open()
        {
            var (music, sfx) = _saveSystem.LoadSettings();
            _musicSlider.SetValueWithoutNotify(music);
            _sfxSlider.SetValueWithoutNotify(sfx);

            _panelRoot.SetActive(true);
            PlayOpenAnimation().Forget();
            _gameFlowSystem.Pause();
        }

        private void Close()
        {
            _saveSystem.SaveSettings(_musicSlider.value, _sfxSlider.value);
            PlayCloseAnimation().Forget();
        }

        private void CloseImmediate()
        {
            _panelRoot.SetActive(false);
        }

        private async UniTaskVoid PlayOpenAnimation()
        {
            _panelTransform.localScale = Vector3.zero;
            var animateX = LMotion.Create(SCALE_ZERO, SCALE_ONE, POPUP_DURATION)
                .WithEase(Ease.OutBack)
                .BindToLocalScaleX(_panelTransform)
                .ToUniTask(_cts.Token);
            var animateY = LMotion.Create(SCALE_ZERO, SCALE_ONE, POPUP_DURATION)
                .WithEase(Ease.OutBack)
                .BindToLocalScaleY(_panelTransform)
                .ToUniTask(_cts.Token);
            await UniTask.WhenAll(animateX, animateY);
        }

        private async UniTaskVoid PlayCloseAnimation()
        {
            var animateX = LMotion.Create(SCALE_ONE, SCALE_ZERO, POPUP_DURATION)
                .WithEase(Ease.InBack)
                .BindToLocalScaleX(_panelTransform)
                .ToUniTask(_cts.Token);
            var animateY = LMotion.Create(SCALE_ONE, SCALE_ZERO, POPUP_DURATION)
                .WithEase(Ease.InBack)
                .BindToLocalScaleY(_panelTransform)
                .ToUniTask(_cts.Token);
            await UniTask.WhenAll(animateX, animateY);
            _panelRoot.SetActive(false);
            _gameFlowSystem.Resume();
        }

        private void OnMusicVolumeChanged(float volume)
        {
            _audioSystem.SetMusicVolume(volume);
        }

        private void OnSfxVolumeChanged(float volume)
        {
            _audioSystem.SetSfxVolume(volume);
        }

        private void OnResetClicked()
        {
            _openResetConfirmPublisher.Publish(new OpenResetConfirmRequestedMessage());
        }

        private void OnCloseClicked()
        {
            Close();
        }

        private void OnDestroy()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _openSettingsDisposable?.Dispose();
            _resetConfirmedDisposable?.Dispose();
            _musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            _sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            _resetButton.onClick.RemoveListener(OnResetClicked);
            _closeButton.onClick.RemoveListener(OnCloseClicked);
        }
    }
}
