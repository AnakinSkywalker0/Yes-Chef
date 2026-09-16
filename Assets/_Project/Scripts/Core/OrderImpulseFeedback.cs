using Unity.Cinemachine;
using UnityEngine;
using YesChef.Orders;
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
        [SerializeField] private OrderBoard _orderBoard;
        [SerializeField, Min(0f)] private float _positiveForce = 0.45f;
        [SerializeField, Min(0f)] private float _negativeForce = 0.9f;

        private CinemachineImpulseSource _impulseSource;

        private void Awake() => _impulseSource = GetComponent<CinemachineImpulseSource>();

        private void OnEnable()
        {
            if (_orderBoard != null)
            {
                _orderBoard.OnOrderScored += HandleOrderScored;
            }
        }

        private void OnDisable()
        {
            if (_orderBoard != null)
            {
                _orderBoard.OnOrderScored -= HandleOrderScored;
            }
        }

        private void HandleOrderScored(CustomerWindowStation window, int points)
        {
            if (_impulseSource == null)
            {
                return;
            }

            _impulseSource.GenerateImpulseWithForce(points >= 0 ? _positiveForce : _negativeForce);
        }
    }
}
