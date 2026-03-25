# Unity 6.3 LTS — Current Best Practices

**Last verified:** 2026-03-25

Modern Unity 6 patterns that may not be in the LLM's training data.
These are production-ready recommendations as of Unity 6.3 LTS.

---

## SoulRift-Specific Recommendations (2D Roguelite)

### Render Pipeline
- **Use URP** with 2D Renderer — best supported pipeline for 2D games
- HDRP no longer receiving new features — all graphics work is on URP
- URP Render Graph is mandatory in 6.3 — no Compatibility Mode fallback
- New Bloom filters (Kawase, Dual) — useful for Soul aura visual effects

### 2D Physics
- Box2D v3 API is new in 6.3 — runs alongside existing API
- **Recommendation**: Start with existing 2D Physics API for stability
- Use object pooling for projectiles and enemies (50+ per wave)

### 2D Rendering (WEB VERIFIED)
- New Mesh Renderer 2D and Skinned Mesh Renderer 2D workflows in URP
- Sprite Atlas Analyzer tool available for optimization
- 2D lights can light 3D elements in Sorting Groups — useful for hybrid effects

### Performance for Wave-Based Gameplay
- **Object Pooling**: Essential (enemies, projectiles, soul orbs, particles)
- **Sprite Atlas**: Group related sprites into atlases to reduce draw calls
- **Shader Build Settings**: Configure without coding to reduce compile times
- Use `ParticleSystem.Play/Stop` with pooling for aura and Hunger particle effects
- Target: 60fps, 16.6ms frame budget

---

## Project Setup

### Render Pipeline Choice
- **URP (Universal)**: Mobile, cross-platform, best for 2D — RECOMMENDED
- **HDRP**: High-end PC/console only, no longer getting new features
- **Built-in**: Deprecated, avoid for new projects

### Assembly Definitions
- Use Assembly Definitions to reduce compile times
- Separate game code into logical assemblies (Core, Gameplay, UI, etc.)

---

## Scripting

### C# 9+ Features (Unity 6 Supports C# 9)

```csharp
// Record types for data
public record PlayerData(string Name, int Level, float Health);

// Pattern matching
var result = enemy switch {
    Boss boss => boss.Enrage(),
    Minion minion => minion.Flee(),
    _ => null
};
```

### Async/Await for Asset Loading

```csharp
public async Task<GameObject> LoadEnemyAsync(string key) {
    var handle = Addressables.LoadAssetAsync<GameObject>(key);
    return await handle.Task;
}
```

---

## Input — Use Input System Package

```csharp
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour {
    private PlayerControls controls;

    void Awake() {
        controls = new PlayerControls();
        controls.Gameplay.Jump.performed += ctx => Jump();
    }

    void OnEnable() => controls.Enable();
    void OnDisable() => controls.Disable();
}
```

Create Input Actions asset in editor, generate C# class via inspector.

---

## UI — UI Toolkit (Production-Ready in Unity 6)

```csharp
using UnityEngine.UIElements;

public class GameHUD : MonoBehaviour {
    void OnEnable() {
        var root = GetComponent<UIDocument>().rootVisualElement;
        var soulMeter = root.Q<ProgressBar>("soul-meter");
        soulMeter.value = currentSoulLevel;
    }
}
```

**UXML** (structure) + **USS** (styling) = HTML/CSS-like workflow.
Note: USS parser is stricter in 6.3 — write valid CSS-like syntax.

---

## Data-Driven Design with ScriptableObjects

```csharp
// Ideal for SoulRift's item system, enemy configs, soul state thresholds
[CreateAssetMenu(fileName = "NewItem", menuName = "SoulRift/Item")]
public class ItemData : ScriptableObject {
    public string ItemName;
    public ItemCategory Category;
    public float SoulCost;
    public List<ItemEffect> Effects;
}
```

---

## Asset Management — Use Addressables

```csharp
using UnityEngine.AddressableAssets;

public async Task SpawnEnemyAsync(string enemyKey) {
    var handle = Addressables.InstantiateAsync(enemyKey);
    var enemy = await handle.Task;
    Addressables.ReleaseInstance(enemy); // Cleanup when done
}
```

---

## Performance

### Burst Compiler + Jobs System

```csharp
[BurstCompile]
struct ParticleUpdateJob : IJobParallelFor {
    public NativeArray<float3> Positions;
    public NativeArray<float3> Velocities;
    public float DeltaTime;

    public void Execute(int index) {
        Positions[index] += Velocities[index] * DeltaTime;
    }
}
```

### Object Pooling Pattern

```csharp
// Unity 6 has built-in ObjectPool<T>
using UnityEngine.Pool;

private ObjectPool<GameObject> enemyPool;

void Awake() {
    enemyPool = new ObjectPool<GameObject>(
        createFunc: () => Instantiate(enemyPrefab),
        actionOnGet: obj => obj.SetActive(true),
        actionOnRelease: obj => obj.SetActive(false),
        defaultCapacity: 50
    );
}
```

---

## Testing — Unity Test Framework (NUnit)

```csharp
[UnityTest]
public IEnumerator SoulSystem_CollectSoul_IncreasesLevel() {
    var player = new GameObject().AddComponent<SoulSystem>();
    player.CurrentSoul = 50f;

    player.CollectSoul(25f);
    yield return null;

    Assert.AreEqual(75f, player.CurrentSoul);
}
```

---

## Summary: Unity 6 Tech Stack for SoulRift

| Feature | Use This (2026) | Avoid This (Legacy) |
|---------|------------------|----------------------|
| **Rendering** | URP + 2D Renderer | Built-in pipeline |
| **Input** | Input System package | `Input` class |
| **UI** | UI Toolkit | UGUI (Canvas) |
| **Assets** | Addressables | Resources.Load |
| **Physics** | 2D Physics (existing API) | — |
| **Pooling** | ObjectPool<T> | Manual pooling |
| **Data** | ScriptableObjects | Hardcoded values |
| **Testing** | Unity Test Framework | Manual testing only |

---

**Sources:**
- https://docs.unity3d.com/6000.3/Documentation/Manual/best-practice-guides.html
- https://docs.unity3d.com/6000.3/Documentation/Manual/WhatsNewUnity63.html
- https://unity.com/blog/unity-6-3-lts-is-now-available
