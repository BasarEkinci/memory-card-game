using System.Threading;
using UnityEngine;
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
        private readonly LifetimeScope _rootScope;

        public BootstrapEntryPoint(
            SaveSystem saveSystem,
            AudioSettingsModel audioSettings,
            AudioSystem audioSystem,
            LifetimeScope rootScope)
        {
            _saveSystem = saveSystem;
            _audioSettings = audioSettings;
            _audioSystem = audioSystem;
            _rootScope = rootScope;
        }

        public async Awaitable StartAsync(CancellationToken cancellation)
        {
            Debug.Log("[BootstrapEntryPoint] StartAsync called");

            var (musicVolume, sfxVolume) = _saveSystem.LoadSettings();
            _audioSettings.MusicVolume = musicVolume;
            _audioSettings.SfxVolume = sfxVolume;

            _audioSystem.Initialize();
            Debug.Log("[BootstrapEntryPoint] Audio initialized");

            LifetimeScope.EnqueueParent(_rootScope);
            await SceneManager.LoadSceneAsync("GameScene", LoadSceneMode.Additive);
            Debug.Log("[BootstrapEntryPoint] GameScene loaded");
        }
    }
}
