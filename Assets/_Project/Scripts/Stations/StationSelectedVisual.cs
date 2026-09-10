using System.Collections.Generic;
using UnityEngine;
using YesChef.Characters;

namespace YesChef.Stations
{
    /// <summary>
    /// Purely presentational: lights up a station while the player is aiming at it.
    /// The station itself has no idea this component exists.
    /// </summary>
    [DisallowMultipleComponent]
    public class StationSelectedVisual : MonoBehaviour
    {
        [SerializeField] private BaseStation _station;
        [SerializeField] private List<GameObject> _highlightObjects = new();

        private PlayerController _player;

        private void Start()
        {
            _player = PlayerController.Instance;
            if (_player != null)
            {
                _player.OnSelectedStationChanged += HandleSelectedStationChanged;
            }

            SetHighlightVisible(false);
        }

        private void OnDestroy()
        {
            if (_player != null)
            {
                _player.OnSelectedStationChanged -= HandleSelectedStationChanged;
            }
        }

        private void HandleSelectedStationChanged(BaseStation selected) =>
            SetHighlightVisible(selected == _station);

        private void SetHighlightVisible(bool visible)
        {
            for (int i = 0; i < _highlightObjects.Count; i++)
            {
                if (_highlightObjects[i] != null)
                {
                    _highlightObjects[i].SetActive(visible);
                }
            }
        }
    }
}
