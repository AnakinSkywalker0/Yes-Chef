using UnityEngine;

namespace YesChef.Core
{
    /// <summary>
    /// Widens the fixed overhead camera until the whole kitchen fits the frame.
    /// <para>
    /// The design requires the entire kitchen to be visible and the camera never to move,
    /// but a hard-coded field of view only holds for one aspect ratio. This measures the
    /// kitchen's bounding box in camera space and derives the vertical FOV that contains
    /// it, so the framing survives any window shape or resolution.
    /// </para>
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class KitchenCameraFitter : MonoBehaviour
    {
        [Header("Area to keep in frame")]
        [SerializeField] private Vector3 _areaCenter = new(0f, 1f, 0f);
        [SerializeField] private Vector3 _areaSize = new(19f, 2.6f, 14f);

        [Header("Framing")]
        [Tooltip("Extra breathing room around the kitchen, as a fraction of the frame.")]
        [SerializeField, Range(0f, 0.4f)] private float _padding = 0.05f;
        [SerializeField, Range(5f, 120f)] private float _minFieldOfView = 20f;
        [SerializeField, Range(5f, 179f)] private float _maxFieldOfView = 90f;

        private Camera _camera;
        private float _lastAspect = -1f;

        private void Awake() => _camera = GetComponent<Camera>();

        private void OnEnable()
        {
            _camera = GetComponent<Camera>();
            Fit();
        }

        private void LateUpdate()
        {
            if (_camera == null)
            {
                return;
            }

            // Re-fit only when the viewport shape actually changes.
            if (Mathf.Approximately(_camera.aspect, _lastAspect))
            {
                return;
            }

            Fit();
        }

        private void Fit()
        {
            if (_camera == null || _camera.orthographic)
            {
                return;
            }

            _lastAspect = _camera.aspect;

            Vector3 extents = _areaSize * 0.5f;
            float tanVertical = 0f;
            float tanHorizontal = 0f;

            // Walk the eight corners of the kitchen volume in camera space.
            for (int corner = 0; corner < 8; corner++)
            {
                var offset = new Vector3(
                    (corner & 1) == 0 ? -extents.x : extents.x,
                    (corner & 2) == 0 ? -extents.y : extents.y,
                    (corner & 4) == 0 ? -extents.z : extents.z);

                Vector3 local = transform.InverseTransformPoint(_areaCenter + offset);

                // Anything at or behind the lens cannot constrain the frame.
                if (local.z <= 0.01f)
                {
                    continue;
                }

                tanVertical = Mathf.Max(tanVertical, Mathf.Abs(local.y) / local.z);
                tanHorizontal = Mathf.Max(tanHorizontal, Mathf.Abs(local.x) / local.z);
            }

            if (tanVertical <= 0f && tanHorizontal <= 0f)
            {
                return;
            }

            float aspect = Mathf.Max(0.1f, _camera.aspect);
            float requiredTanHalfFov = Mathf.Max(tanVertical, tanHorizontal / aspect) * (1f + _padding);

            _camera.fieldOfView = Mathf.Clamp(
                2f * Mathf.Atan(requiredTanHalfFov) * Mathf.Rad2Deg,
                _minFieldOfView,
                _maxFieldOfView);
        }
    }
}
