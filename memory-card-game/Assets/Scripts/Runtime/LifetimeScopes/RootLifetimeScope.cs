using MessagePipe;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using CardMatch.Runtime.ScriptableObjects;
using CardMatch.Runtime.Services;
using CardMatch.Runtime.EntryPoints;

namespace CardMatch.Runtime.LifetimeScopes
{
    public sealed class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private AudioConfig _audioConfig;
        [SerializeField] private GameConfig _gameConfig;
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioSource _sfxSource;

        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterMessagePipe();

            builder.RegisterInstance(_audioConfig);
            builder.RegisterInstance(_gameConfig);

            builder.Register<AudioSettingsModel>(Lifetime.Singleton);
            builder.Register<SaveSystem>(Lifetime.Singleton);

            var bgmSource = _bgmSource;
            var sfxSource = _sfxSource;

            builder.Register<AudioSystem>(resolver =>
            {
                var config = resolver.Resolve<AudioConfig>();
                var settings = resolver.Resolve<AudioSettingsModel>();
                return new AudioSystem(config, settings, bgmSource, sfxSource);
            }, Lifetime.Singleton).AsImplementedInterfaces().AsSelf();

            builder.RegisterEntryPoint<BootstrapEntryPoint>();
        }
    }
}
