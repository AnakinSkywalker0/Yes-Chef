using Unity.Cinemachine;
using UnityEngine;

namespace YesChef.Core
{
    /// <summary>
    /// Widens a Cinemachine camera's lens until the whole kitchen fits the frame.
    /// <para>
    /// The design requires the entire kitchen to be visible, but a hard-coded field of view
    /// only holds for one aspect ratio and one camera pose. This measures the kitchen's
    /// bounding box from wherever the camera currently is and derives the vertical FOV that
    /// contains it, so the guarantee survives window resizes, Cinemachine blends and the
    /// small follow offsets the gameplay camera makes.
    /// </para>
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CinemachineCamera))]
    public class KitchenCameraFitter : MonoBehaviour
    {
        [Header("Area to keep in frame")]
        [SerializeField] private Vector3 _areaCenter = new(0f, 1f, 0f);
        [SerializeField] private Vector3 _areaSize = new(19f, 2.6f, 14f);

        [Header("Framing")]
        [Tooltip("Extra breathing room around the kitchen, as a fraction of the frame.")]
        [SerializeField, Range(0f, 0.4f)] private float _padding = 0.07f;
        [SerializeField, Range(5f, 120f)] private float _minFieldOfView = 20f;
        [SerializeField, Range(5f, 179f)] private float _maxFieldOfView = 90f;

        private CinemachineCamera _virtualCamera;

        private void Awake() => _virtualCamera = GetComponent<CinemachineCamera>();

        private void OnEnable()
        {
            _virtualCamera = GetComponent<CinemachineCamera>();
            Fit();
        }

        private void LateUpdate() => Fit();

        private void Fit()
        {
            if (_virtualCamera == null)
            {
                return;
            }

            Camera output = Camera.main;
            float aspect = output != null ? Mathf.Max(0.1f, output.aspect) : 16f / 9f;

            Vector3 extents = _areaSize * 0.5f;
            float tanVertical = 0f;
            float tanHorizontal = 0f;

            // Walk the eight corners of the kitchen volume in this camera's space.
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

            float requiredTanHalfFov = Mathf.Max(tanVertical, tanHorizontal / aspect) * (1f + _padding);
            float fieldOfView = Mathf.Clamp(
                2f * Mathf.Atan(requiredTanHalfFov) * Mathf.Rad2Deg,
                _minFieldOfView,
                _maxFieldOfView);

            if (Mathf.Approximately(_virtualCamera.Lens.FieldOfView, fieldOfView))
            {
                return;
            }

            _virtualCamera.Lens.FieldOfView = fieldOfView;
        }
    }
}
