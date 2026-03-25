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
            return @"{
                ""name"": ""PlayerInputActions"",
                ""maps"": [
                    {
                        ""name"": ""Gameplay"",
                        ""actions"": [
                            { ""name"": ""Move"", ""type"": ""Value"", ""expectedControlType"": ""Vector2"" },
                            { ""name"": ""Aim"", ""type"": ""Value"", ""expectedControlType"": ""Vector2"" },
                            { ""name"": ""Fire"", ""type"": ""Button"" }
                        ],
                        ""bindings"": [
                            { ""path"": ""2DVector"", ""isComposite"": true, ""action"": ""Move"" },
                            { ""path"": ""<Keyboard>/w"", ""isPartOfComposite"": true, ""name"": ""up"", ""action"": ""Move"" },
                            { ""path"": ""<Keyboard>/s"", ""isPartOfComposite"": true, ""name"": ""down"", ""action"": ""Move"" },
                            { ""path"": ""<Keyboard>/a"", ""isPartOfComposite"": true, ""name"": ""left"", ""action"": ""Move"" },
                            { ""path"": ""<Keyboard>/d"", ""isPartOfComposite"": true, ""name"": ""right"", ""action"": ""Move"" },
                            { ""path"": ""<Gamepad>/leftStick"", ""action"": ""Move"" },
                            { ""path"": ""<Mouse>/position"", ""action"": ""Aim"" },
                            { ""path"": ""<Gamepad>/rightStick"", ""action"": ""Aim"" },
                            { ""path"": ""<Mouse>/leftButton"", ""action"": ""Fire"" },
                            { ""path"": ""<Gamepad>/rightTrigger"", ""action"": ""Fire"" }
                        ]
                    },
                    {
                        ""name"": ""UI"",
                        ""actions"": [
                            { ""name"": ""Navigate"", ""type"": ""Value"", ""expectedControlType"": ""Vector2"" },
                            { ""name"": ""Confirm"", ""type"": ""Button"" },
                            { ""name"": ""Cancel"", ""type"": ""Button"" }
                        ],
                        ""bindings"": [
                            { ""path"": ""<Keyboard>/upArrow"", ""action"": ""Navigate"" },
                            { ""path"": ""<Keyboard>/enter"", ""action"": ""Confirm"" },
                            { ""path"": ""<Mouse>/leftButton"", ""action"": ""Confirm"" },
                            { ""path"": ""<Keyboard>/escape"", ""action"": ""Cancel"" }
                        ]
                    }
                ]
            }";
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
