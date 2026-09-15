using UnityEngine;

namespace YesChef.Core
{
    /// <summary>
    /// Sways the establishing camera very slowly while the menu is up, so the kitchen behind
    /// the title feels alive. <see cref="KitchenCameraFitter"/> re-fits the lens every frame,
    /// so the drift never pushes the room out of frame.
    /// </summary>
    [DisallowMultipleComponent]
    public class MenuCameraDrift : MonoBehaviour
    {
        [SerializeField] private Vector3 _amplitude = new(1.6f, 0.5f, 0.8f);
        [SerializeField, Min(1f)] private float _period = 18f;

        private Vector3 _restPosition;

        private void Awake() => _restPosition = transform.localPosition;

        private void Update()
        {
            if (GameManager.Instance != null && GameManager.Instance.IsPlaying)
            {
                return;
            }

            float t = Time.unscaledTime / _period * Mathf.PI * 2f;
            transform.localPosition = _restPosition + new Vector3(
                Mathf.Sin(t) * _amplitude.x,
                Mathf.Sin(t * 0.5f) * _amplitude.y,
                Mathf.Cos(t * 0.75f) * _amplitude.z);
        }
    }
}
