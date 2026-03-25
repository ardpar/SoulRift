# Unity 6.3 LTS — Breaking Changes

**Last verified:** 2026-03-25

This document tracks breaking API changes and behavioral differences between Unity 2022 LTS
(likely in model training) and Unity 6.3 LTS (current version). Organized by version.

## Unity 6.0 (from 2022 LTS)

### Render Pipeline
- **URP Compatibility Mode deprecated** — Render Graph is now the standard path
- `SetupRenderPasses` deprecated — use `AddRenderPasses` with Render Graph system
- `CustomEditorForRenderPipelineAttribute` deprecated — use `CustomEditor`
- `VolumeComponentMenuForRenderPipelineAttribute` deprecated — use `VolumeComponentMenu`

### Graphics Formats
- Previously deprecated graphics formats now produce **compile errors**
- `GraphicsFormatUtility.GetGraphicsFormat` no longer returns obsolete formats
- `RenderTextureFormat.Depth` now maps to `GraphicsFormat.None` (was `DepthAuto`)

### UI Toolkit
- `ExecuteDefaultAction` / `ExecuteDefaultActionAtTarget` deprecated — use `HandleEventBubbleUp`
- `PreventDefault` deprecated — use `StopPropagation`
- AtTarget dispatching phase deprecated

### Editor
- Assets/Create menu reorganized — menu item priorities may need updating
- Built-in ScriptTemplate files renamed
- `EditorApplication.ExecuteMenuItem` paths may have changed

### Input System
- Legacy Input Manager deprecated, new Input System is default

```csharp
// OLD: Input class (deprecated)
if (Input.GetKeyDown(KeyCode.Space)) { }

// NEW: Input System package
using UnityEngine.InputSystem;
if (Keyboard.current.spaceKey.wasPressedThisFrame) { }
```

### Rendering API
```csharp
// OLD: ScriptableRenderPass.Execute
public override void Execute(ScriptableRenderContext context, ref RenderingData data)

// NEW: RenderGraph API
public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
```

---

## Unity 6.1 (from 6.0)

- Incremental changes, no major breaking API changes for 2D development

## Unity 6.2 (from 6.1)

- Minor URP enhancements and fixes
- Check official guide: https://docs.unity3d.com/6000.3/Documentation/Manual/UpgradeGuideUnity62.html

---

## Unity 6.3 LTS (from 6.2) — WEB VERIFIED

### URP — Compatibility Mode REMOVED
- **CRITICAL**: URP Compatibility Mode fully removed (was deprecated in 6.0)
- Code stripped by default — improves compilation and build size
- `RenderGraphSettings.enableRenderCompatibilityMode` now read-only (returns `false`)
- **Action**: All custom Scriptable Renderer Features must use Render Graph

### 2D Physics — Box2D v3
- New low-level 2D physics API based on Box2D v3 added
- Runs alongside existing API — old API still works
- **Action for SoulRift**: Use existing API for now, migrate later when v3 API stabilizes

### USS Parser (UI Toolkit) — Stricter
- USS parser upgraded with stricter validation
- Single dots (`.`) in selectors no longer silently become wildcards (`*`)
- Missing/extra brackets, excessive semicolons now detected as errors
- **Action**: Validate all USS files

### Accessibility API
- `AccessibilityRole` changed from flags enum to standard enum
- Bitwise operations on AccessibilityRole values may cause warnings

### Removed Features
- Legacy ETC compression mode removed
- Multiplay Hosting deprecated (shutdown March 31, 2026)

---

## SoulRift-Specific Impact Summary

| Change | Impact | Action |
|--------|--------|--------|
| URP Compat Mode removed | LOW (new project) | Use Render Graph from start |
| Box2D v3 alongside old API | LOW | Use existing 2D Physics API |
| USS parser stricter | LOW (new project) | Write valid USS from start |
| Input System default | MEDIUM | Use new Input System package |
| Render Graph mandatory | LOW (new project) | Use RenderGraph for any custom passes |

---

**Sources:**
- https://docs.unity3d.com/6000.3/Documentation/Manual/UpgradeGuideUnity6.html
- https://docs.unity3d.com/6000.3/Documentation/Manual/UpgradeGuideUnity63.html
