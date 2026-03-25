using UnityEngine;
using UnityEngine.InputSystem;

namespace SoulRift.Core
{
    /// <summary>
    /// Input Actions wrapper. Unity Editor'de .inputactions dosyasindan
    /// auto-generate edildikten sonra bu dosya silinip yerine gecebilir.
    /// </summary>
    public class PlayerInputActions
    {
        private readonly InputActionMap _gameplayMap;
        private readonly InputActionMap _uiMap;

        public GameplayActions Gameplay { get; }
        public UIActions UI { get; }

        public PlayerInputActions()
        {
            var asset = InputActionAsset.FromJson(Resources.Load<TextAsset>("PlayerInputActions")?.text
                ?? CreateDefaultJson());

            _gameplayMap = asset.FindActionMap("Gameplay");
            _uiMap = asset.FindActionMap("UI");

            Gameplay = new GameplayActions(_gameplayMap);
            UI = new UIActions(_uiMap);
        }

        private static string CreateDefaultJson()
        {
            // Fallback: programmatic tanimlama
            var asset = new InputActionAsset();

            var gameplayMap = asset.AddActionMap("Gameplay");
            var move = gameplayMap.AddAction("Move", type: InputActionType.Value,
                expectedControlLayout: "Vector2");
            move.AddCompositeBinding("2DVector")
                .With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s")
                .With("Left", "<Keyboard>/a")
                .With("Right", "<Keyboard>/d");
            move.AddBinding("<Gamepad>/leftStick");

            var aim = gameplayMap.AddAction("Aim", type: InputActionType.Value,
                expectedControlLayout: "Vector2");
            aim.AddBinding("<Mouse>/position");
            aim.AddBinding("<Gamepad>/rightStick");

            var fire = gameplayMap.AddAction("Fire", type: InputActionType.Button);
            fire.AddBinding("<Mouse>/leftButton");
            fire.AddBinding("<Gamepad>/rightTrigger");

            var uiMap = asset.AddActionMap("UI");
            var navigate = uiMap.AddAction("Navigate", type: InputActionType.Value,
                expectedControlLayout: "Vector2");
            navigate.AddBinding("<Keyboard>/upArrow");

            var confirm = uiMap.AddAction("Confirm", type: InputActionType.Button);
            confirm.AddBinding("<Keyboard>/enter");
            confirm.AddBinding("<Mouse>/leftButton");

            var cancel = uiMap.AddAction("Cancel", type: InputActionType.Button);
            cancel.AddBinding("<Keyboard>/escape");

            return asset.ToJson();
        }

        public class GameplayActions
        {
            private readonly InputActionMap _map;
            public InputAction Move { get; }
            public InputAction Aim { get; }
            public InputAction Fire { get; }

            public GameplayActions(InputActionMap map)
            {
                _map = map;
                Move = map.FindAction("Move");
                Aim = map.FindAction("Aim");
                Fire = map.FindAction("Fire");
            }

            public void Enable() => _map.Enable();
            public void Disable() => _map.Disable();
        }

        public class UIActions
        {
            private readonly InputActionMap _map;
            public InputAction Navigate { get; }
            public InputAction Confirm { get; }
            public InputAction Cancel { get; }

            public UIActions(InputActionMap map)
            {
                _map = map;
                Navigate = map.FindAction("Navigate");
                Confirm = map.FindAction("Confirm");
                Cancel = map.FindAction("Cancel");
            }

            public void Enable() => _map.Enable();
            public void Disable() => _map.Disable();
        }
    }
}
