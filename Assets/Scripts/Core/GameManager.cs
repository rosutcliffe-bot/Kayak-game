using UnityEngine;
using UnityEngine.SceneManagement;

namespace KayakSimulator.Core
{
    /// <summary>
    /// Central game manager that controls overall game state,
    /// mode transitions, and acts as the single source of truth
    /// for global settings at runtime.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ---------------------------------------------------------------
        // Singleton
        // ---------------------------------------------------------------
        public static GameManager Instance { get; private set; }

        // ---------------------------------------------------------------
        // Game state
        // ---------------------------------------------------------------
        public enum GameState { MainMenu, Playing, Paused, GameOver }
        public GameState CurrentState { get; private set; } = GameState.MainMenu;

        // ---------------------------------------------------------------
        // Simulation mode
        // ---------------------------------------------------------------
        public enum SimulationMode { Arcade, Simulation }
        [Header("Simulation Settings")]
        [Tooltip("Arcade = simplified steering; Simulation = realistic paddle strokes.")]
        [SerializeField] private SimulationMode simulationMode = SimulationMode.Simulation;
        public SimulationMode CurrentSimulationMode => simulationMode;

        // ---------------------------------------------------------------
        // Events
        // ---------------------------------------------------------------
        public event System.Action<GameState> OnGameStateChanged;
        public event System.Action<SimulationMode> OnSimulationModeChanged;

        // ---------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ---------------------------------------------------------------
        // Public API
        // ---------------------------------------------------------------

        /// <summary>Transition to the main menu scene.</summary>
        public void LoadMainMenu()
        {
            SetGameState(GameState.MainMenu);
            SceneManager.LoadScene("MainMenu");
        }

        /// <summary>Start or restart the gameplay scene.</summary>
        public void StartGame()
        {
            SetGameState(GameState.Playing);
            SceneManager.LoadScene("GameScene");
        }

        /// <summary>Toggle pause state.</summary>
        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
            {
                SetGameState(GameState.Paused);
                Time.timeScale = 0f;
            }
            else if (CurrentState == GameState.Paused)
            {
                SetGameState(GameState.Playing);
                Time.timeScale = 1f;
            }
        }

        /// <summary>End the current run.</summary>
        public void EndGame()
        {
            SetGameState(GameState.GameOver);
            Time.timeScale = 1f;
        }

        /// <summary>Quit the application.</summary>
        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>Switch between Arcade and Simulation modes.</summary>
        public void SetSimulationMode(SimulationMode mode)
        {
            simulationMode = mode;
            OnSimulationModeChanged?.Invoke(simulationMode);
        }

        // ---------------------------------------------------------------
        // Private helpers
        // ---------------------------------------------------------------
        private void SetGameState(GameState newState)
        {
            CurrentState = newState;
            OnGameStateChanged?.Invoke(CurrentState);
        }
    }
}
