using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine.SceneManagement;
using VContainer.Unity;
using CardMatch.Runtime.Services;

namespace CardMatch.Runtime.EntryPoints
{
    public sealed class BootstrapEntryPoint : IAsyncStartable
    {
        private readonly SaveSystem _saveSystem;
        private readonly AudioSettingsModel _audioSettings;
        private readonly AudioSystem _audioSystem;

        public BootstrapEntryPoint(
            SaveSystem saveSystem,
            AudioSettingsModel audioSettings,
            AudioSystem audioSystem)
        {
            _saveSystem = saveSystem;
            _audioSettings = audioSettings;
            _audioSystem = audioSystem;
        }

        public async UniTask StartAsync(CancellationToken cancellation)
        {
            // Load saved settings
            var (musicVolume, sfxVolume) = _saveSystem.LoadSettings();
            _audioSettings.MusicVolume = musicVolume;
            _audioSettings.SfxVolume = sfxVolume;

            // Initialize audio with loaded settings
            _audioSystem.Initialize();

            // Load GameScene additively
            await SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive).ToUniTask(cancellationToken: cancellation);
        }
    }
}
