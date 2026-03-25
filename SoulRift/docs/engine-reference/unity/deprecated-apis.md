# Unity 6.3 LTS — Deprecated APIs

**Last verified:** 2026-03-25

Quick lookup table for deprecated APIs and their replacements.
Format: **Don't use X** -> **Use Y instead**

---

## Render Pipeline (WEB VERIFIED)

| Deprecated | Replacement | Since |
|------------|-------------|-------|
| `SetupRenderPasses()` | `AddRenderPasses()` (Render Graph) | 6.0 |
| `CustomEditorForRenderPipelineAttribute` | `CustomEditor` | 6.0 |
| `VolumeComponentMenuForRenderPipelineAttribute` | `VolumeComponentMenu` | 6.0 |
| URP Compatibility Mode | Render Graph (mandatory) | 6.0 (removed 6.3) |
| `RenderGraphSettings.enableRenderCompatibilityMode` | Read-only, returns `false` | 6.3 |
| `CommandBuffer.DrawMesh()` in SRP | RenderGraph API | 6.0 |
| `OnPreRender()` / `OnPostRender()` | `RenderPipelineManager` callbacks | 6.0 |
| `Camera.SetReplacementShader()` | Custom render pass | 6.0 |

## Graphics Formats (WEB VERIFIED)

| Deprecated | Replacement | Since |
|------------|-------------|-------|
| `GraphicsFormat.DepthAuto` | `GraphicsFormat.None` (for depth-only) | 6.0 |
| Legacy graphics format enums | Current `GraphicsFormat` values | 6.0 |
| Legacy ETC compression | Modern compression formats | 6.3 (removed) |

## UI Toolkit (WEB VERIFIED)

| Deprecated | Replacement | Since |
|------------|-------------|-------|
| `ExecuteDefaultAction()` | `HandleEventBubbleUp()` | 6.0 |
| `ExecuteDefaultActionAtTarget()` | `HandleEventBubbleUp()` | 6.0 |
| `PreventDefault()` | `StopPropagation()` | 6.0 |
| AtTarget dispatching phase | Standard event flow | 6.0 |

## Input

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| `Input.GetKey()` | `Keyboard.current[Key.X].isPressed` | New Input System |
| `Input.GetKeyDown()` | `Keyboard.current[Key.X].wasPressedThisFrame` | New Input System |
| `Input.GetMouseButton()` | `Mouse.current.leftButton.isPressed` | New Input System |
| `Input.GetAxis()` | `InputAction` callbacks | New Input System |
| `Input.mousePosition` | `Mouse.current.position.ReadValue()` | New Input System |

## UI

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| `Canvas` (UGUI) | `UIDocument` (UI Toolkit) | UI Toolkit production-ready in Unity 6 |
| `Text` component | `TextMeshPro` or UI Toolkit `Label` | Better rendering |
| Built-in RP | URP or HDRP | Avoid for new projects |

## Accessibility (WEB VERIFIED)

| Deprecated | Replacement | Since |
|------------|-------------|-------|
| `AccessibilityRole` (flags enum) | `AccessibilityRole` (standard enum) | 6.3 |

## Other

| Deprecated | Replacement | Notes |
|------------|-------------|-------|
| `Resources.Load()` | Addressables | Better memory control, async |
| `WWW` class | `UnityWebRequest` | Modern async networking |
| `Application.LoadLevel()` | `SceneManager.LoadScene()` | Scene management |
| Social API | Third-party solutions | 6.0+ |
| Multiplay Hosting | Alternatives | 6.3 (shutdown Mar 2026) |
| Legacy Particle System | Visual Effect Graph (optional) | Still works, VFX Graph is alternative |

---

## Quick Migration Patterns

### Input
```csharp
// Deprecated
if (Input.GetKeyDown(KeyCode.Space)) { Jump(); }

// New Input System
using UnityEngine.InputSystem;
if (Keyboard.current.spaceKey.wasPressedThisFrame) { Jump(); }
```

### Asset Loading
```csharp
// Deprecated
var prefab = Resources.Load<GameObject>("Enemies/Goblin");

// Addressables
var handle = Addressables.LoadAssetAsync<GameObject>("Enemies/Goblin");
var prefab = await handle.Task;
```

### UI
```csharp
// Deprecated (UGUI)
GetComponent<Text>().text = "Score: 100";

// TextMeshPro
GetComponent<TextMeshProUGUI>().text = "Score: 100";

// UI Toolkit
rootVisualElement.Q<Label>("score-label").text = "Score: 100";
```

---

**Sources:**
- https://docs.unity3d.com/6000.3/Documentation/Manual/UpgradeGuideUnity6.html
- https://docs.unity3d.com/6000.3/Documentation/Manual/UpgradeGuideUnity63.html
