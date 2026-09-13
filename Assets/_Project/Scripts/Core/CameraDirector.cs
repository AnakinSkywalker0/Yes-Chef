using Unity.Cinemachine;
using UnityEngine;

namespace YesChef.Core
{
    /// <summary>
    /// Picks which Cinemachine camera is live for each game state and lets the brain
    /// blend between them: a high establishing shot while the kitchen is closed, the
    /// gameplay shot during service.
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraDirector : MonoBehaviour
    {
        private const int LivePriority = 20;
        private const int StandbyPriority = 10;

        [SerializeField] private CinemachineCamera _establishingCamera;
        [SerializeField] private CinemachineCamera _gameplayCamera;

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged += HandleStateChanged;
                HandleStateChanged(GameManager.Instance.State);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnStateChanged -= HandleStateChanged;
            }
        }

        private void HandleStateChanged(GameState state)
        {
            bool inService = state is GameState.Playing or GameState.Paused;
            SetPriority(_gameplayCamera, inService ? LivePriority : StandbyPriority);
            SetPriority(_establishingCamera, inService ? StandbyPriority : LivePriority);
        }

        private static void SetPriority(CinemachineCamera camera, int priority)
        {
            if (camera != null)
            {
                camera.Priority = priority;
            }
        }
    }
}
