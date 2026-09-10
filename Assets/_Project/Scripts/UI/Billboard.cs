using UnityEngine;

namespace YesChef.UI
{
    /// <summary>
    /// Keeps a world-space canvas square to the camera so order tickets and progress bars
    /// stay readable from the fixed overhead view.
    /// </summary>
    [DisallowMultipleComponent]
    public class Billboard : MonoBehaviour
    {
        private Transform _cameraTransform;

        private void Start()
        {
            if (Camera.main != null)
            {
                _cameraTransform = Camera.main.transform;
            }
        }

        private void LateUpdate()
        {
            if (_cameraTransform == null)
            {
                return;
            }

            transform.forward = _cameraTransform.forward;
        }
    }
}
