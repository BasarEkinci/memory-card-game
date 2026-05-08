using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using CardMatch.Logic.Systems;

namespace CardMatch.Runtime.Views
{
    public sealed class ResetConfirmPopupView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button _cancelButton;
        [SerializeField] private GameObject _popupRoot;

        private GameFlowSystem _gameFlowSystem;

        public event Action OnConfirmed;
        public event Action OnCancelled;

        [Inject]
        public void Construct(GameFlowSystem gameFlowSystem)
        {
            _gameFlowSystem = gameFlowSystem;
        }

        private void Awake()
        {
            _confirmButton.onClick.AddListener(OnConfirmClicked);
            _cancelButton.onClick.AddListener(OnCancelClicked);

            if (_messageText != null)
                _messageText.text = "Emin misiniz?";
        }

        public void Show()
        {
            _popupRoot.SetActive(true);
        }

        public void Hide()
        {
            _popupRoot.SetActive(false);
        }

        private void OnConfirmClicked()
        {
            Hide();
            OnConfirmed?.Invoke();
            _gameFlowSystem.StartNewGame();
        }

        private void OnCancelClicked()
        {
            Hide();
            OnCancelled?.Invoke();
        }

        private void OnDestroy()
        {
            _confirmButton.onClick.RemoveListener(OnConfirmClicked);
            _cancelButton.onClick.RemoveListener(OnCancelClicked);
        }
    }
}
