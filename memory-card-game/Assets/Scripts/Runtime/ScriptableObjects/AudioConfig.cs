using UnityEngine;

namespace CardMatch.Runtime.ScriptableObjects
{
    [CreateAssetMenu(menuName = "CardMatch/AudioConfig", fileName = "AudioConfig")]
    public sealed class AudioConfig : ScriptableObject
    {
        [SerializeField] private AudioClip _bgmClip;
        [SerializeField] private AudioClip _dealClip;
        [SerializeField] private AudioClip _flipClip;
        [SerializeField] private AudioClip _matchClip;
        [SerializeField] private AudioClip _strikeClip;
        [SerializeField] private AudioClip _penaltyClip;
        [SerializeField] private AudioClip _winClip;

        public AudioClip BgmClip => _bgmClip;
        public AudioClip DealClip => _dealClip;
        public AudioClip FlipClip => _flipClip;
        public AudioClip MatchClip => _matchClip;
        public AudioClip StrikeClip => _strikeClip;
        public AudioClip PenaltyClip => _penaltyClip;
        public AudioClip WinClip => _winClip;
    }
}
