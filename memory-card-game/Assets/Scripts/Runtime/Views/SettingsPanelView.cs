using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using CardMatch.Runtime.Services;
using CardMatch.Logic.Systems;

namespace CardMatch.Runtime.Views
{
    public sealed class SettingsPanelView : MonoBehaviour
    {
        [SerializeField] private Slider _musicSlider;
        [SerializeField] private Slider _sfxSlider;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private GameObject _panelRoot;

        private AudioSystem _audioSystem;
        private GameFlowSystem _gameFlowSystem;
        private SaveSystem _saveSystem;

        public event Action OnResetRequested;
        public event Action OnClosed;

        [Inject]
        public void Construct(AudioSystem audioSystem, GameFlowSystem gameFlowSystem, SaveSystem saveSystem)
        {
            _audioSystem = audioSystem;
            _gameFlowSystem = gameFlowSystem;
            _saveSystem = saveSystem;
        }

        private void Awake()
        {
            _musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            _sfxSlider.onValueChanged.AddListener(OnSfxVolumeChanged);
            _resetButton.onClick.AddListener(OnResetClicked);
            _closeButton.onClick.AddListener(OnCloseClicked);
        }

        public void Open()
        {
            var (music, sfx) = _saveSystem.LoadSettings();
            _musicSlider.SetValueWithoutNotify(music);
            _sfxSlider.SetValueWithoutNotify(sfx);

            _panelRoot.SetActive(true);
            _gameFlowSystem.Pause();
        }

        public void Close()
        {
            _saveSystem.SaveSettings(_musicSlider.value, _sfxSlider.value);

            _panelRoot.SetActive(false);
            _gameFlowSystem.Resume();
            OnClosed?.Invoke();
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
            OnResetRequested?.Invoke();
        }

        private void OnCloseClicked()
        {
            Close();
        }

        private void OnDestroy()
        {
            _musicSlider.onValueChanged.RemoveListener(OnMusicVolumeChanged);
            _sfxSlider.onValueChanged.RemoveListener(OnSfxVolumeChanged);
            _resetButton.onClick.RemoveListener(OnResetClicked);
            _closeButton.onClick.RemoveListener(OnCloseClicked);
        }
    }
}
