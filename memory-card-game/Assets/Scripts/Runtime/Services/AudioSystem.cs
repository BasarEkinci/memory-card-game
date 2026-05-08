using System;
using CardMatch.Runtime.ScriptableObjects;
using UnityEngine;

namespace CardMatch.Runtime.Services
{
    public sealed class AudioSystem : IDisposable
    {
        private readonly AudioConfig _config;
        private readonly AudioSettingsModel _settings;
        private readonly AudioSource _bgmSource;
        private readonly AudioSource _sfxSource;

        public AudioSystem(AudioConfig config, AudioSettingsModel settings, AudioSource bgmSource, AudioSource sfxSource)
        {
            _config = config;
            _settings = settings;
            _bgmSource = bgmSource;
            _sfxSource = sfxSource;
        }

        public void Initialize()
        {
            _bgmSource.clip = _config.BgmClip;
            _bgmSource.loop = true;
            _bgmSource.volume = _settings.MusicVolume;
            _bgmSource.Play();
        }

        public void PlayFlip() => PlaySfx(_config.FlipClip);
        public void PlayMatch() => PlaySfx(_config.MatchClip);
        public void PlayStrike() => PlaySfx(_config.StrikeClip);
        public void PlayPenalty() => PlaySfx(_config.PenaltyClip);
        public void PlayWin() => PlaySfx(_config.WinClip);

        private void PlaySfx(AudioClip clip)
        {
            if (clip != null)
            {
                _sfxSource.PlayOneShot(clip, _settings.SfxVolume);
            }
        }

        public void SetMusicVolume(float volume)
        {
            _settings.MusicVolume = Mathf.Clamp01(volume);
            _bgmSource.volume = _settings.MusicVolume;
        }

        public void SetSfxVolume(float volume)
        {
            _settings.SfxVolume = Mathf.Clamp01(volume);
        }

        public void PauseBgm() => _bgmSource.Pause();
        public void ResumeBgm() => _bgmSource.UnPause();

        public void Dispose()
        {
            _bgmSource.Stop();
        }
    }
}
