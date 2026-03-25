# Unity Engine — Version Reference

| Field | Value |
|-------|-------|
| **Engine Version** | Unity 6.3 LTS (6000.3.x) |
| **Release Date** | December 2025 |
| **Project Pinned** | 2026-03-25 |
| **Last Docs Verified** | 2026-03-25 |
| **LLM Knowledge Cutoff** | May 2025 |
| **LTS Support Until** | December 2027 |
| **Risk Level** | HIGH — version is beyond LLM training data |

## Knowledge Gap Warning

The LLM's training data likely covers Unity up to ~2022 LTS (2022.3) / early Unity 6 previews.
The entire Unity 6 release series introduced significant changes that the model may
NOT know about. Always cross-reference this directory before suggesting Unity API calls.

## Post-Cutoff Version Timeline

| Version | Release | Risk Level | Key Theme |
|---------|---------|------------|-----------|
| 6.0 LTS | Oct 2024 | MEDIUM | Unity 6 rebrand, Render Graph, URP overhaul, Graphics Format deprecations |
| 6.1 | Apr 2025 | HIGH | Multiplayer templates, incremental improvements |
| 6.2 | Aug 2025 | HIGH | URP enhancements, editor improvements |
| 6.3 LTS | Dec 2025 | HIGH | Box2D v3 physics, URP Compat Mode removed, USS parser strict, Accessibility API |

## Key Changes Relevant to SoulRift (2D Roguelite)

- **2D Physics**: New low-level 2D physics API based on Box2D v3 (runs alongside old API)
- **2D Rendering**: Mesh Renderer and Skinned Mesh Renderer 2D workflow for URP
- **URP**: Compatibility Mode fully removed in 6.3 — Render Graph mandatory
- **Bloom**: Kawase and Dual filtering options (Soul aura efektleri icin faydali)
- **HDRP**: No longer receiving new features — all graphics work is on URP
- **Particle System**: Still supported (VFX Graph is alternative, not mandatory replacement)
- **Editor**: Customizable toolbar, rebuilt grid/snapping, LMDB search database

## Verified Sources

- Official docs: https://docs.unity3d.com/6000.3/Documentation/Manual/
- Upgrade to 6.0: https://docs.unity3d.com/6000.3/Documentation/Manual/UpgradeGuideUnity6.html
- Upgrade to 6.3: https://docs.unity3d.com/6000.3/Documentation/Manual/UpgradeGuideUnity63.html
- What's new in 6.3: https://docs.unity3d.com/6000.3/Documentation/Manual/WhatsNewUnity63.html
- Release support: https://unity.com/releases/unity-6/support
