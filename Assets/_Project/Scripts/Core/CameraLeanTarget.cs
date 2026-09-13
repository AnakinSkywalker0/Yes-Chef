using UnityEngine;
using YesChef.Characters;

namespace YesChef.Core
{
    /// <summary>
    /// The point the gameplay camera composes on: the kitchen centre, leaned a little
    /// towards the chef. Cinemachine's position composer damps the follow, so this only
    /// has to say where the interest is - never how fast to get there.
    /// <para>
    /// The lean is deliberately small; the whole kitchen must stay in frame and the
    /// <see cref="KitchenCameraFitter"/> pads for exactly this much drift.
    /// </para>
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraLeanTarget : MonoBehaviour
    {
        [SerializeField] private Vector3 _restPosition = new(0f, 1f, -0.77f);
        [Tooltip("0 = fixed on the kitchen centre, 1 = glued to the chef.")]
        [SerializeField, Range(0f, 0.5f)] private float _leanTowardsPlayer = 0.12f;

        private void LateUpdate()
        {
            PlayerController player = PlayerController.Instance;
            bool inService = GameManager.Instance != null && GameManager.Instance.IsPlaying;

            if (player == null || !inService)
            {
                transform.position = _restPosition;
                return;
            }

            Vector3 toPlayer = player.transform.position - _restPosition;
            toPlayer.y = 0f;
            transform.position = _restPosition + toPlayer * _leanTowardsPlayer;
        }
    }
}
