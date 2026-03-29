using UnityEngine;

namespace KayakSimulator.Utilities
{
    /// <summary>
    /// Utility: smoothly moves a transform to follow a target with independent
    /// position and rotation smoothing. Useful for the kayak mesh to trail
    /// slightly behind the physics Rigidbody for a more cinematic feel.
    /// </summary>
    public class SmoothFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float     positionSmoothing  = 8f;
        [SerializeField] private float     rotationSmoothing  = 6f;

        private void LateUpdate()
        {
            if (target == null) return;
            transform.position = Vector3.Lerp(
                transform.position, target.position, Time.deltaTime * positionSmoothing);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, target.rotation, Time.deltaTime * rotationSmoothing);
        }
    }
}
