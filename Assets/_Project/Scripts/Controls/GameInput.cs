using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace YesChef.Controls
{
    /// <summary>
    /// The single seam between the Input System and gameplay code. Nothing else in the
    /// project references <c>UnityEngine.InputSystem</c>, so rebinding or swapping the
    /// input backend only touches this class and the bound action asset.
    /// </summary>
    [DisallowMultipleComponent]
    public class GameInput : MonoBehaviour
    {
        private const string PlayerMapName = "Player";
        private const string MoveActionName = "Move";
        private const string InteractActionName = "Interact";
        private const string PauseActionName = "Pause";

        public static GameInput Instance { get; private set; }

        [SerializeField] private InputActionAsset _inputActions;

        private InputActionMap _playerMap;
        private InputAction _moveAction;
        private InputAction _interactAction;
        private InputAction _pauseAction;

        public event Action OnInteractPerformed;
        public event Action OnPausePerformed;

        /// <summary>Normalised movement on the kitchen floor plane.</summary>
        public Vector2 MoveInput => _moveAction != null ? _moveAction.ReadValue<Vector2>() : Vector2.zero;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (_inputActions == null)
            {
                Debug.LogError($"{nameof(GameInput)} has no {nameof(InputActionAsset)} assigned.", this);
                return;
            }

            _playerMap = _inputActions.FindActionMap(PlayerMapName);
            if (_playerMap == null)
            {
                Debug.LogError($"Action map '{PlayerMapName}' was not found on {_inputActions.name}.", this);
                return;
            }

            _moveAction = _playerMap.FindAction(MoveActionName);
            _interactAction = _playerMap.FindAction(InteractActionName);
            _pauseAction = _playerMap.FindAction(PauseActionName);
        }

        private void OnEnable()
        {
            if (_playerMap == null)
            {
                return;
            }

            _playerMap.Enable();

            if (_interactAction != null)
            {
                _interactAction.performed += HandleInteractPerformed;
            }

            if (_pauseAction != null)
            {
                _pauseAction.performed += HandlePausePerformed;
            }
        }

        private void OnDisable()
        {
            if (_playerMap == null)
            {
                return;
            }

            if (_interactAction != null)
            {
                _interactAction.performed -= HandleInteractPerformed;
            }

            if (_pauseAction != null)
            {
                _pauseAction.performed -= HandlePausePerformed;
            }

            _playerMap.Disable();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        /// <summary>One entry per binding, composites collapsed (e.g. "W/A/S/D", "Left Stick").</summary>
        /// <summary>A binding's display text plus whether it comes from a gamepad (vs keyboard).</summary>
        public readonly struct BindingLabel
        {
            public BindingLabel(string text, bool isGamepad)
            {
                Text = text;
                IsGamepad = isGamepad;
            }

            public string Text { get; }
            public bool IsGamepad { get; }
        }

        public IReadOnlyList<BindingLabel> GetMoveBindings() => GetBindings(_moveAction);
        public IReadOnlyList<BindingLabel> GetInteractBindings() => GetBindings(_interactAction);
        public IReadOnlyList<BindingLabel> GetPauseBindings() => GetBindings(_pauseAction);

        public string GetMoveDisplayString() => GetDisplayString(_moveAction);
        public string GetInteractDisplayString() => GetDisplayString(_interactAction);
        public string GetPauseDisplayString() => GetDisplayString(_pauseAction);

        private void HandleInteractPerformed(InputAction.CallbackContext _) => OnInteractPerformed?.Invoke();

        private void HandlePausePerformed(InputAction.CallbackContext _) => OnPausePerformed?.Invoke();

        private static string GetDisplayString(InputAction action) =>
            action == null ? "-" : action.GetBindingDisplayString();

        private static IReadOnlyList<BindingLabel> GetBindings(InputAction action)
        {
            var result = new List<BindingLabel>();
            if (action == null)
            {
                return result;
            }

            var bindings = action.bindings;
            for (int i = 0; i < bindings.Count; i++)
            {
                if (bindings[i].isPartOfComposite)
                {
                    continue;
                }

                string label = action.GetBindingDisplayString(i);
                if (string.IsNullOrWhiteSpace(label))
                {
                    continue;
                }

                // A composite has no device of its own; its first part decides.
                string path = bindings[i].effectivePath;
                if (bindings[i].isComposite && i + 1 < bindings.Count)
                {
                    path = bindings[i + 1].effectivePath;
                }

                result.Add(new BindingLabel(label, IsGamepadPath(path)));
            }

            return result;
        }

        private static bool IsGamepadPath(string path)
        {
            string layout = InputControlPath.TryGetDeviceLayout(path);
            return !string.IsNullOrEmpty(layout) && InputSystem.IsFirstLayoutBasedOnSecond(layout, "Gamepad");
        }
    }
}
