using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using LitMotion;
using VContainer;
using MessagePipe;
using CardMatch.Logic.Messages;

namespace CardMatch.Runtime.Views
{
    public sealed class HUDView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _strikeText;
        [SerializeField] private GameObject _strikeContainer;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private RectTransform _scoreTransform;
        [SerializeField] private RectTransform _strikeTransform;

        private ISubscriber<MatchResultMessage> _matchSubscriber;
        private ISubscriber<PenaltyAppliedMessage> _penaltySubscriber;
        private IDisposable _matchDisposable;
        private IDisposable _penaltyDisposable;
        private CancellationTokenSource _cts;
        private int _lastStrike;

        public event Action OnSettingsClicked;

        [Inject]
        public void Construct(
            ISubscriber<MatchResultMessage> matchSubscriber,
            ISubscriber<PenaltyAppliedMessage> penaltySubscriber)
        {
            _matchSubscriber = matchSubscriber;
            _penaltySubscriber = penaltySubscriber;
        }

        private void Awake()
        {
            _cts = new CancellationTokenSource();
            _settingsButton.onClick.AddListener(HandleSettingsClick);
        }

        private void Start()
        {
            _matchDisposable = _matchSubscriber.Subscribe(OnMatchResult);
            _penaltyDisposable = _penaltySubscriber.Subscribe(OnPenalty);

            UpdateScore(0);
            UpdateStrike(0);
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
                UpdateStrike(0);
            }
        }

        private void OnPenalty(PenaltyAppliedMessage msg)
        {
            UpdateScore(msg.NewScore);
            PlayPenaltyFlash();
        }

        public void UpdateScore(int score)
        {
            _scoreText.text = score.ToString();
            PlayScorePulse();
        }

        public void UpdateStrike(int strike)
        {
            _strikeText.text = $"x{strike}";
            _strikeContainer.SetActive(strike > 0);

            if (strike > _lastStrike && strike > 0)
            {
                PlayStrikePulse();
            }
            _lastStrike = strike;
        }

        private void PlayScorePulse()
        {
            if (_scoreTransform == null) return;

            _scoreTransform.localScale = Vector3.one;
            LMotion.Create(1f, 1.2f, 0.1f)
                .WithEase(Ease.OutQuad)
                .BindToLocalScaleX(_scoreTransform)
                .AddTo(_cts.Token);
            LMotion.Create(1f, 1.2f, 0.1f)
                .WithEase(Ease.OutQuad)
                .BindToLocalScaleY(_scoreTransform)
                .AddTo(_cts.Token);
            LMotion.Create(1.2f, 1f, 0.15f)
                .WithDelay(0.1f)
                .WithEase(Ease.InQuad)
                .BindToLocalScaleX(_scoreTransform)
                .AddTo(_cts.Token);
            LMotion.Create(1.2f, 1f, 0.15f)
                .WithDelay(0.1f)
                .WithEase(Ease.InQuad)
                .BindToLocalScaleY(_scoreTransform)
                .AddTo(_cts.Token);
        }

        private void PlayStrikePulse()
        {
            if (_strikeTransform == null) return;

            _strikeTransform.localScale = Vector3.one;
            LMotion.Create(1f, 1.3f, 0.1f)
                .WithEase(Ease.OutQuad)
                .BindToLocalScaleX(_strikeTransform)
                .AddTo(_cts.Token);
            LMotion.Create(1f, 1.3f, 0.1f)
                .WithEase(Ease.OutQuad)
                .BindToLocalScaleY(_strikeTransform)
                .AddTo(_cts.Token);
            LMotion.Create(1.3f, 1f, 0.15f)
                .WithDelay(0.1f)
                .WithEase(Ease.InQuad)
                .BindToLocalScaleX(_strikeTransform)
                .AddTo(_cts.Token);
            LMotion.Create(1.3f, 1f, 0.15f)
                .WithDelay(0.1f)
                .WithEase(Ease.InQuad)
                .BindToLocalScaleY(_strikeTransform)
                .AddTo(_cts.Token);
        }

        private void PlayPenaltyFlash()
        {
            if (_scoreText == null) return;

            Color originalColor = _scoreText.color;
            LMotion.Create(Color.red, originalColor, 0.3f)
                .WithEase(Ease.OutQuad)
                .Bind(c => _scoreText.color = c)
                .AddTo(_cts.Token);
        }

        private int GetCurrentScore()
        {
            if (int.TryParse(_scoreText.text, out int score))
                return score;
            return 0;
        }

        private void HandleSettingsClick()
        {
            OnSettingsClicked?.Invoke();
        }

        private void OnDestroy()
        {
            _matchDisposable?.Dispose();
            _penaltyDisposable?.Dispose();
            _settingsButton.onClick.RemoveListener(HandleSettingsClick);
            _cts?.Cancel();
            _cts?.Dispose();
        }
    }
}
