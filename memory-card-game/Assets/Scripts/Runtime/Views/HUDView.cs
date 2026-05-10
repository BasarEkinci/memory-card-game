using System;
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
    public sealed class HUDView : MonoBehaviour
    {
        private const float PULSE_SCALE_UP_DURATION = 0.1f;
        private const float PULSE_SCALE_DOWN_DURATION = 0.15f;
        private const float SCORE_PULSE_SCALE = 1.2f;
        private const float STRIKE_PULSE_SCALE = 1.3f;
        private const float PENALTY_FLASH_DURATION = 0.3f;
        private const float SCALE_DEFAULT = 1f;
        private const int SCORE_DEFAULT = 0;

        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _strikeText;
        [SerializeField] private GameObject _strikeContainer;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private RectTransform _scoreTransform;
        [SerializeField] private RectTransform _strikeTransform;

        private ISubscriber<MatchResultMessage> _matchSubscriber;
        private ISubscriber<PenaltyAppliedMessage> _penaltySubscriber;
        private IPublisher<OpenSettingsRequestedMessage> _openSettingsPublisher;
        private IDisposable _matchDisposable;
        private IDisposable _penaltyDisposable;
        private int _lastStrike;

        [Inject]
        public void Construct(
            ISubscriber<MatchResultMessage> matchSubscriber,
            ISubscriber<PenaltyAppliedMessage> penaltySubscriber,
            IPublisher<OpenSettingsRequestedMessage> openSettingsPublisher)
        {
            _matchSubscriber = matchSubscriber;
            _penaltySubscriber = penaltySubscriber;
            _openSettingsPublisher = openSettingsPublisher;
        }

        private void Awake()
        {
            _settingsButton.onClick.AddListener(HandleSettingsClick);
        }

        private void Start()
        {
            _matchDisposable = _matchSubscriber.Subscribe(OnMatchResult);
            _penaltyDisposable = _penaltySubscriber.Subscribe(OnPenalty);

            UpdateScore(SCORE_DEFAULT);
            UpdateStrike(SCORE_DEFAULT);
        }

        private void OnMatchResult(MatchResultMessage msg)
        {
            if (msg.IsMatch)
            {
                UpdateScore(msg.ScoreDelta + GetCurrentScore());
                UpdateStrike(msg.NewStrike);
            }
            else
            {
                UpdateStrike(SCORE_DEFAULT);
            }
        }

        private void OnPenalty(PenaltyAppliedMessage msg)
        {
            UpdateScore(msg.NewScore);
            PlayPenaltyFlash();
        }

        public void UpdateScore(int score)
        {
            _scoreText.SetText("{0}", score);
            PlayScorePulse();
        }

        public void UpdateStrike(int strike)
        {
            _strikeText.SetText("x{0}", strike);
            _strikeContainer.SetActive(strike > SCORE_DEFAULT);

            if (strike > _lastStrike && strike > SCORE_DEFAULT)
            {
                PlayStrikePulse();
            }
            _lastStrike = strike;
        }

        private void PlayScorePulse()
        {
            _scoreTransform.localScale = Vector3.one;

            LMotion.Create(SCALE_DEFAULT, SCORE_PULSE_SCALE, PULSE_SCALE_UP_DURATION)
                .WithEase(Ease.OutQuad)
                .BindToLocalScaleX(_scoreTransform)
                .AddTo(this);
            LMotion.Create(SCALE_DEFAULT, SCORE_PULSE_SCALE, PULSE_SCALE_UP_DURATION)
                .WithEase(Ease.OutQuad)
                .BindToLocalScaleY(_scoreTransform)
                .AddTo(this);
            LMotion.Create(SCORE_PULSE_SCALE, SCALE_DEFAULT, PULSE_SCALE_DOWN_DURATION)
                .WithDelay(PULSE_SCALE_UP_DURATION)
                .WithEase(Ease.InQuad)
                .BindToLocalScaleX(_scoreTransform)
                .AddTo(this);
            LMotion.Create(SCORE_PULSE_SCALE, SCALE_DEFAULT, PULSE_SCALE_DOWN_DURATION)
                .WithDelay(PULSE_SCALE_UP_DURATION)
                .WithEase(Ease.InQuad)
                .BindToLocalScaleY(_scoreTransform)
                .AddTo(this);
        }

        private void PlayStrikePulse()
        {
            _strikeTransform.localScale = Vector3.one;

            LMotion.Create(SCALE_DEFAULT, STRIKE_PULSE_SCALE, PULSE_SCALE_UP_DURATION)
                .WithEase(Ease.OutQuad)
                .BindToLocalScaleX(_strikeTransform)
                .AddTo(this);
            LMotion.Create(SCALE_DEFAULT, STRIKE_PULSE_SCALE, PULSE_SCALE_UP_DURATION)
                .WithEase(Ease.OutQuad)
                .BindToLocalScaleY(_strikeTransform)
                .AddTo(this);
            LMotion.Create(STRIKE_PULSE_SCALE, SCALE_DEFAULT, PULSE_SCALE_DOWN_DURATION)
                .WithDelay(PULSE_SCALE_UP_DURATION)
                .WithEase(Ease.InQuad)
                .BindToLocalScaleX(_strikeTransform)
                .AddTo(this);
            LMotion.Create(STRIKE_PULSE_SCALE, SCALE_DEFAULT, PULSE_SCALE_DOWN_DURATION)
                .WithDelay(PULSE_SCALE_UP_DURATION)
                .WithEase(Ease.InQuad)
                .BindToLocalScaleY(_strikeTransform)
                .AddTo(this);
        }

        private void PlayPenaltyFlash()
        {
            Color originalColor = _scoreText.color;
            LMotion.Create(Color.red, originalColor, PENALTY_FLASH_DURATION)
                .WithEase(Ease.OutQuad)
                .BindToColor(_scoreText)
                .AddTo(this);
        }

        private int GetCurrentScore()
        {
            if (int.TryParse(_scoreText.text, out int score))
                return score;
            return SCORE_DEFAULT;
        }

        private void HandleSettingsClick()
        {
            _openSettingsPublisher.Publish(new OpenSettingsRequestedMessage());
        }

        private void OnDestroy()
        {
            _matchDisposable?.Dispose();
            _penaltyDisposable?.Dispose();
            _settingsButton.onClick.RemoveListener(HandleSettingsClick);
        }
    }
}
