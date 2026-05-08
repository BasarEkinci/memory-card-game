using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using CardMatch.Logic.Systems;

namespace CardMatch.Runtime.Views
{
    public sealed class WinPanelView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _bestScoreText;
        [SerializeField] private TextMeshProUGUI _maxStrikeText;
        [SerializeField] private Button _newGameButton;
        [SerializeField] private GameObject _panelRoot;

        private GameFlowSystem _gameFlowSystem;

        public event Action OnNewGameRequested;

        [Inject]
        public void Construct(GameFlowSystem gameFlowSystem)
        {
            _gameFlowSystem = gameFlowSystem;
        }

        private void Awake()
        {
            _newGameButton.onClick.AddListener(OnNewGameClicked);
        }

        public void Show(int score, int bestScore, int maxStrike)
        {
            _scoreText.text = score.ToString();
            _bestScoreText.text = bestScore.ToString();
            _maxStrikeText.text = maxStrike.ToString();
            _panelRoot.SetActive(true);
        }

        public void Hide()
        {
            _panelRoot.SetActive(false);
        }

        private void OnNewGameClicked()
        {
            Hide();
            OnNewGameRequested?.Invoke();
            _gameFlowSystem.StartNewGame();
        }

        private void OnDestroy()
        {
            _newGameButton.onClick.RemoveListener(OnNewGameClicked);
        }
    }
}
