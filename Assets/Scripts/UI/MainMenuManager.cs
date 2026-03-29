using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KayakSimulator.Core;

namespace KayakSimulator.UI
{
    /// <summary>
    /// Main menu UI manager.
    /// Wires up the start, settings, and quit buttons, and reflects the
    /// current simulation mode toggle on the menu screen.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        // ---------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------
        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Mode Toggle")]
        [SerializeField] private Toggle arcadeModeToggle;
        [SerializeField] private Toggle simulationModeToggle;

        [Header("Version Label")]
        [SerializeField] private TMP_Text versionLabel;

        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject settingsPanel;

        // ---------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------
        private void Start()
        {
            // Display version string
            if (versionLabel != null)
                versionLabel.text = $"v{Application.version}";

            // Button click listeners
            if (startButton    != null) startButton.onClick.AddListener(OnStartClicked);
            if (settingsButton != null) settingsButton.onClick.AddListener(OnSettingsClicked);
            if (quitButton     != null) quitButton.onClick.AddListener(OnQuitClicked);

            // Mode toggle listeners
            if (arcadeModeToggle     != null)
                arcadeModeToggle.onValueChanged.AddListener(OnArcadeToggleChanged);
            if (simulationModeToggle != null)
                simulationModeToggle.onValueChanged.AddListener(OnSimToggleChanged);

            // Reflect current mode
            SyncModeToggles();

            // Start with settings panel hidden
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (mainPanel     != null) mainPanel.SetActive(true);
        }

        // ---------------------------------------------------------------
        // Button handlers
        // ---------------------------------------------------------------
        private void OnStartClicked()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.StartGame();
        }

        private void OnSettingsClicked()
        {
            if (mainPanel     != null) mainPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);
        }

        private void OnQuitClicked()
        {
            if (GameManager.Instance != null)
                GameManager.Instance.QuitGame();
        }

        // ---------------------------------------------------------------
        // Settings panel
        // ---------------------------------------------------------------
        public void OnSettingsBackClicked()
        {
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (mainPanel     != null) mainPanel.SetActive(true);
        }

        // ---------------------------------------------------------------
        // Mode toggles
        // ---------------------------------------------------------------
        private void OnArcadeToggleChanged(bool isOn)
        {
            if (isOn && GameManager.Instance != null)
                GameManager.Instance.SetSimulationMode(GameManager.SimulationMode.Arcade);
        }

        private void OnSimToggleChanged(bool isOn)
        {
            if (isOn && GameManager.Instance != null)
                GameManager.Instance.SetSimulationMode(GameManager.SimulationMode.Simulation);
        }

        private void SyncModeToggles()
        {
            if (GameManager.Instance == null) return;
            bool isArcade = GameManager.Instance.CurrentSimulationMode == GameManager.SimulationMode.Arcade;
            if (arcadeModeToggle     != null) arcadeModeToggle.isOn     = isArcade;
            if (simulationModeToggle != null) simulationModeToggle.isOn = !isArcade;
        }
    }
}
