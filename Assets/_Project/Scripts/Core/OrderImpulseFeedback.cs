using Unity.Cinemachine;
using UnityEngine;
using YesChef.Scoring;
using YesChef.Stations;

namespace YesChef.Core
{
    /// <summary>
    /// A small camera nudge when an order settles - a touch heavier when it lost points -
    /// delivered through Cinemachine's impulse system so the gameplay camera reacts and
    /// the establishing camera does not.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(CinemachineImpulseSource))]
    public class OrderImpulseFeedback : MonoBehaviour
    {
        [SerializeField, Min(0f)] private float _positiveForce = 0.45f;
        [SerializeField, Min(0f)] private float _negativeForce = 0.9f;

        private CinemachineImpulseSource _impulseSource;

        private void Awake() => _impulseSource = GetComponent<CinemachineImpulseSource>();

        private void Start()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnOrderAwarded += HandleOrderAwarded;
            }
        }

        private void OnDestroy()
        {
            if (ScoreManager.Instance != null)
            {
                ScoreManager.Instance.OnOrderAwarded -= HandleOrderAwarded;
            }
        }

        private void HandleOrderAwarded(CustomerWindowStation window, ScoreAward award)
        {
            if (_impulseSource == null)
            {
                return;
            }

            _impulseSource.GenerateImpulseWithForce(award.Points >= 0 ? _positiveForce : _negativeForce);
        }
    }
}
