using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KayakSimulator.Core;

namespace KayakSimulator.UI
{
    /// <summary>
    /// Pause menu UI manager.
    /// Handles resume, settings, and return-to-main-menu from the pause overlay.
    /// Subscribes to GameManager's state change event so it shows/hides automatically.
    /// </summary>
    public class PauseMenuManager : MonoBehaviour
    {
        // ---------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------
        [Header("Root Panel")]
        [SerializeField] private GameObject pausePanel;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button mainMenuButton;

        [Header("Panels")]
        [SerializeField] private GameObject settingsPanel;

        // ---------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------
        private void Start()
        {
            if (resumeButton   != null) resumeButton.onClick.AddListener(OnResumeClicked);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
            if (mainMenuButton != null) mainMenuButton.onClick.AddListener(OnMainMenuClicked);

            // Hide panels at start
            if (pausePanel    != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);

            // Subscribe to game state changes
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }

        // ---------------------------------------------------------------
        // State callbacks
        // ---------------------------------------------------------------
        private void HandleGameStateChanged(GameManager.GameState state)
        {
            bool showPause = state == GameManager.GameState.Paused;
            if (pausePanel != null) pausePanel.SetActive(showPause);
            // Always hide settings when returning from pause
            if (!showPause && settingsPanel != null) settingsPanel.SetActive(false);
        }

        // ---------------------------------------------------------------
        // Button handlers
        // ---------------------------------------------------------------
        private void OnResumeClicked()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.TogglePause();
        }

        private void OnSettingsClicked()
        {
            if (pausePanel    != null) pausePanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        private void OnMainMenuClicked()
        {
            Time.timeScale = 1f; // Ensure time is running before scene load
            if (GameManager.Instance != null)
                GameManager.Instance.LoadMainMenu();
        }

        public void OnSettingsBackClicked()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (pausePanel    != null) pausePanel.SetActive(true);
        }
    }
}
