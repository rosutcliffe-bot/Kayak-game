using UnityEngine;
using KayakSimulator.Core;

namespace KayakSimulator.Paddle
{
    /// <summary>
    /// Paddle controller: manages the visual/animation of the double-bladed paddle
    /// and coordinates left/right blade activation based on player input.
    ///
    /// The paddle mesh rotates around the kayak's centre of mass each stroke,
    /// driven by an AnimationCurve so the motion looks natural regardless of
    /// physics frame rate.
    /// </summary>
    public class PaddleController : MonoBehaviour
    {
        // ---------------------------------------------------------------
        // Inspector
        // ---------------------------------------------------------------
        [Header("Blade References")]
        [SerializeField] private PaddleBlade leftBlade;
        [SerializeField] private PaddleBlade rightBlade;

        [Header("Paddle Mesh Transform")]
        [Tooltip("The root transform of the paddle mesh (not the blades themselves).")]
        [SerializeField] private Transform paddleTransform;

        [Header("Stroke Animation")]
        [Tooltip("How far the paddle dips down (local Y) during a stroke.")]
        [SerializeField] private float strokeDipDepth = 0.35f;

        [Tooltip("Angular sweep of the blade through the water (degrees).")]
        [SerializeField] private float strokeArcDegrees = 60f;

        [Tooltip("Animation curve for dip/sweep over the stroke duration [0–1].")]
        [SerializeField] private AnimationCurve strokeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        [Tooltip("Duration of one full paddle stroke (seconds).")]
        [SerializeField] private float strokeDuration = 0.6f;

        [Header("Feathering")]
        [Tooltip("Blade feather angle — alternate blades are offset by this angle (degrees).")]
        [SerializeField] private float featherAngle = 45f;

        // ---------------------------------------------------------------
        // Private state
        // ---------------------------------------------------------------
        private InputManager _input;

        private float _leftStrokeTimer;
        private float _rightStrokeTimer;

        // Starting local rotation of the paddle transform
        private Quaternion _paddleRestRotation;

        // ---------------------------------------------------------------
        // Unity lifecycle
        // ---------------------------------------------------------------
        private void Start()
        {
            _input = InputManager.Instance;
            if (paddleTransform != null)
                _paddleRestRotation = paddleTransform.localRotation;
        }

        private void Update()
        {
            if (_input == null) return;

            // Advance stroke timers
            if (_input.LeftPaddleInput > 0.01f)
                _leftStrokeTimer  = Mathf.Min(_leftStrokeTimer  + Time.deltaTime, strokeDuration);
            else
                _leftStrokeTimer  = Mathf.Max(_leftStrokeTimer  - Time.deltaTime * 2f, 0f);

            if (_input.RightPaddleInput > 0.01f)
                _rightStrokeTimer = Mathf.Min(_rightStrokeTimer + Time.deltaTime, strokeDuration);
            else
                _rightStrokeTimer = Mathf.Max(_rightStrokeTimer - Time.deltaTime * 2f, 0f);

            AnimatePaddle();
        }

        // ---------------------------------------------------------------
        // Animation
        // ---------------------------------------------------------------
        private void AnimatePaddle()
        {
            if (paddleTransform == null) return;

            // Determine dominant stroke side (the side currently being activated more)
            float leftT  = _leftStrokeTimer  / strokeDuration;
            float rightT = _rightStrokeTimer / strokeDuration;
            bool  leftActive  = leftT  > rightT;

            float t         = leftActive ? leftT : rightT;
            float curveVal  = strokeCurve.Evaluate(t);
            float sideSign  = leftActive ? -1f : 1f;  // left dips to port (-X), right to starboard

            // Rotate paddle around forward axis for the dip
            float dipAngle    = curveVal * strokeDipDepth * 100f; // convert depth to angle approx
            float sweepAngle  = curveVal * strokeArcDegrees * sideSign;

            Quaternion dipRot    = Quaternion.AngleAxis(dipAngle   * sideSign, transform.forward);
            Quaternion sweepRot  = Quaternion.AngleAxis(sweepAngle,             transform.up);

            paddleTransform.localRotation = _paddleRestRotation * dipRot * sweepRot;
        }

        // ---------------------------------------------------------------
        // Editor gizmos
        // ---------------------------------------------------------------
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (leftBlade  != null) { Gizmos.color = Color.blue;  Gizmos.DrawWireSphere(leftBlade.transform.position,  0.05f); }
            if (rightBlade != null) { Gizmos.color = Color.red;   Gizmos.DrawWireSphere(rightBlade.transform.position, 0.05f); }
        }
#endif
    }
}
