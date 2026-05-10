using UnityEngine;
using VContainer;
using VContainer.Unity;
using CardMatch.Runtime.Services;
using CardMatch.Runtime.EntryPoints;
using CardMatch.Runtime.ScriptableObjects;

namespace CardMatch.Runtime
{
    public sealed class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private AudioConfig _audioConfig;
        [SerializeField] private AudioSource _bgmSource;
        [SerializeField] private AudioSource _sfxSource;

        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            builder.RegisterInstance(_audioConfig);

            builder.Register<SaveSystem>(Lifetime.Singleton);
            builder.Register<AudioSettingsModel>(Lifetime.Singleton);
            builder.Register<AudioSystem>(Lifetime.Singleton)
                .WithParameter("bgmSource", _bgmSource)
                .WithParameter("sfxSource", _sfxSource);

            builder.RegisterEntryPoint<BootstrapEntryPoint>();
        }
    }
}
