using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace KayakSimulator.UI
{
    /// <summary>
    /// Settings panel manager.
    /// Controls audio volume, graphics quality, and control sensitivity sliders.
    /// Persists values to PlayerPrefs so they survive sessions.
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        // ---------------------------------------------------------------
        // PlayerPrefs keys
        // ---------------------------------------------------------------
        private const string KeyMasterVolume   = "MasterVolume";
        private const string KeyMusicVolume    = "MusicVolume";
        private const string KeySFXVolume      = "SFXVolume";
        private const string KeyQualityLevel   = "QualityLevel";
        private const string KeySensitivity    = "Sensitivity";
        private const string KeyFullscreen     = "Fullscreen";

        // ---------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------
        [Header("Audio")]
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;

        [Header("Graphics")]
        [SerializeField] private TMP_Dropdown qualityDropdown;
        [SerializeField] private Toggle       fullscreenToggle;

        [Header("Controls")]
        [SerializeField] private Slider sensitivitySlider;

        [Header("Labels")]
        [SerializeField] private TMP_Text masterVolumeLabel;
        [SerializeField] private TMP_Text musicVolumeLabel;
        [SerializeField] private TMP_Text sfxVolumeLabel;
        [SerializeField] private TMP_Text sensitivityLabel;

        // ---------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------
        private void OnEnable()
        {
            LoadSettings();
        }

        private void Start()
        {
            // Wire up listeners
            if (masterVolumeSlider != null)
                masterVolumeSlider.onValueChanged.AddListener(v => { UpdateLabel(masterVolumeLabel, v); SaveFloat(KeyMasterVolume, v); });
            if (musicVolumeSlider  != null)
                musicVolumeSlider.onValueChanged.AddListener(v  => { UpdateLabel(musicVolumeLabel, v);  SaveFloat(KeyMusicVolume, v);  });
            if (sfxVolumeSlider    != null)
                sfxVolumeSlider.onValueChanged.AddListener(v    => { UpdateLabel(sfxVolumeLabel, v);    SaveFloat(KeySFXVolume, v);    });
            if (sensitivitySlider  != null)
                sensitivitySlider.onValueChanged.AddListener(v  => { UpdateLabel(sensitivityLabel, v);  SaveFloat(KeySensitivity, v);  });

            if (qualityDropdown != null)
            {
                qualityDropdown.ClearOptions();
                qualityDropdown.AddOptions(new System.Collections.Generic.List<string>(QualitySettings.names));
                qualityDropdown.onValueChanged.AddListener(OnQualityChanged);
            }

            if (fullscreenToggle != null)
                fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);
        }

        // ---------------------------------------------------------------
        // Load / Save helpers
        // ---------------------------------------------------------------
        private void LoadSettings()
        {
            SetSlider(masterVolumeSlider, masterVolumeLabel, PlayerPrefs.GetFloat(KeyMasterVolume, 1f));
            SetSlider(musicVolumeSlider,  musicVolumeLabel,  PlayerPrefs.GetFloat(KeyMusicVolume,  0.7f));
            SetSlider(sfxVolumeSlider,    sfxVolumeLabel,    PlayerPrefs.GetFloat(KeySFXVolume,    0.8f));
            SetSlider(sensitivitySlider,  sensitivityLabel,  PlayerPrefs.GetFloat(KeySensitivity,  1f));

            if (qualityDropdown  != null) qualityDropdown.value  = PlayerPrefs.GetInt(KeyQualityLevel, QualitySettings.GetQualityLevel());
            if (fullscreenToggle != null) fullscreenToggle.isOn   = PlayerPrefs.GetInt(KeyFullscreen, Screen.fullScreen ? 1 : 0) == 1;
        }

        private static void SaveFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
            PlayerPrefs.Save();
        }

        // ---------------------------------------------------------------
        // Event handlers
        // ---------------------------------------------------------------
        private void OnQualityChanged(int level)
        {
            QualitySettings.SetQualityLevel(level, true);
            PlayerPrefs.SetInt(KeyQualityLevel, level);
            PlayerPrefs.Save();
        }

        private void OnFullscreenChanged(bool fullscreen)
        {
            Screen.fullScreen = fullscreen;
            PlayerPrefs.SetInt(KeyFullscreen, fullscreen ? 1 : 0);
            PlayerPrefs.Save();
        }

        // ---------------------------------------------------------------
        // Utility
        // ---------------------------------------------------------------
        private static void SetSlider(Slider slider, TMP_Text label, float value)
        {
            if (slider != null) slider.value = value;
            UpdateLabel(label, value);
        }

        private static void UpdateLabel(TMP_Text label, float value)
        {
            if (label != null) label.text = $"{value * 100f:0}%";
        }
    }
}
