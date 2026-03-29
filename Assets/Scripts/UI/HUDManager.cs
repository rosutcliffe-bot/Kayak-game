using UnityEngine;
using UnityEngine.UI;
using TMPro;
using KayakSimulator.Physics;

namespace KayakSimulator.UI
{
    /// <summary>
    /// In-game HUD: displays speed, stroke indicators, and depth below surface.
    /// Updates every frame using data from KayakPhysicsController.
    /// </summary>
    public class HUDManager : MonoBehaviour
    {
        // ---------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------
        [Header("References")]
        [SerializeField] private KayakPhysicsController kayakController;

        [Header("Speed")]
        [SerializeField] private TMP_Text speedLabel;

        [Header("Stroke Indicators")]
        [SerializeField] private Image leftStrokeBar;
        [SerializeField] private Image rightStrokeBar;

        [Header("Compass / Heading")]
        [SerializeField] private TMP_Text headingLabel;
        [SerializeField] private Transform kayakTransform;

        // ---------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------
        private void Update()
        {
            if (kayakController == null) return;

            UpdateSpeed();
            UpdateStrokeBars();
            UpdateHeading();
        }

        // ---------------------------------------------------------------
        // Update helpers
        // ---------------------------------------------------------------
        private void UpdateSpeed()
        {
            if (speedLabel == null) return;
            float kmh = kayakController.ForwardSpeed * 3.6f;
            speedLabel.text = $"{kmh:0.0} km/h";
        }

        private void UpdateStrokeBars()
        {
            if (leftStrokeBar  != null) leftStrokeBar.fillAmount  = kayakController.LeftStrokePower;
            if (rightStrokeBar != null) rightStrokeBar.fillAmount = kayakController.RightStrokePower;
        }

        private void UpdateHeading()
        {
            if (headingLabel == null || kayakTransform == null) return;
            float heading = kayakTransform.eulerAngles.y;
            headingLabel.text = $"{heading:000}°";
        }
    }
}
