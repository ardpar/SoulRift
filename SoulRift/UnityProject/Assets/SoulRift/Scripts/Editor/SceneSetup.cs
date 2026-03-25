using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Tek tikla oynanabilir sahne kurulumu.
/// Menu: SoulRift > Setup Game Scene
/// </summary>
public static class SceneSetup
{
    private const string DATA_ROOT = "Assets/SoulRift/Data";
    private const string PREFAB_ROOT = "Assets/SoulRift/Prefabs";

    [MenuItem("SoulRift/Setup Game Scene")]
    public static void SetupGameScene()
    {
        // --- TYPES (reflection, cross-assembly) ---
        var tPoolManager = FindType("SoulRift.Core.PoolManager");
        var tSoulSystem = FindType("SoulRift.Core.SoulSystem");
        var tPlayerController = FindType("SoulRift.Gameplay.PlayerController");
        var tWeaponSystem = FindType("SoulRift.Gameplay.WeaponSystem");
        var tWaveManager = FindType("SoulRift.Gameplay.WaveManager");
        var tGameManager = FindType("SoulRift.Gameplay.GameManager");
        var tSoulMeterHUD = FindType("SoulRift.UI.SoulMeterHUD");
        var tSoulVFXController = FindType("SoulRift.Gameplay.SoulVFXController");
        var tWarningVignette = FindType("SoulRift.UI.WarningVignette");
        var tEnemy = FindType("SoulRift.Gameplay.Enemy");
        var tProjectile = FindType("SoulRift.Gameplay.Projectile");
        var tSoulOrb = FindType("SoulRift.Gameplay.SoulOrb");
        var tSoulStateData = FindType("SoulRift.Core.SoulStateData");
        var tHungerData = FindType("SoulRift.Core.HungerData");
        var tEnemyData = FindType("SoulRift.Core.EnemyData");
        var tWaveData = FindType("SoulRift.Core.WaveData");

        // --- 1. SCRIPTABLEOBJECT DATA ASSETS ---
        EnsureDirectories();

        var soulStateData = CreateSOIfMissing(tSoulStateData, $"{DATA_ROOT}/Soul/SoulStateData.asset");
        var hungerData = CreateSOIfMissing(tHungerData, $"{DATA_ROOT}/Soul/HungerData.asset");
        var basicEnemyData = CreateEnemyData(tEnemyData, $"{DATA_ROOT}/Enemies/BasicSkeleton.asset",
            "Basic Skeleton", 20f, 2f, 10f, 5f);

        // Wave data — 3 wave
        var wave1 = CreateWaveData(tWaveData, $"{DATA_ROOT}/Waves/Wave_01.asset", 1, basicEnemyData, 5, 1.5f);
        var wave2 = CreateWaveData(tWaveData, $"{DATA_ROOT}/Waves/Wave_02.asset", 2, basicEnemyData, 8, 1.2f);
        var wave3 = CreateWaveData(tWaveData, $"{DATA_ROOT}/Waves/Wave_03.asset", 3, basicEnemyData, 12, 1.0f);

        Debug.Log("[SceneSetup] ScriptableObject data assets ready.");

        // --- 2. PREFABS ---
        var enemyPrefab = CreateEnemyPrefab(tEnemy);
        var projectilePrefab = CreateProjectilePrefab(tProjectile);
        var soulOrbPrefab = CreateSoulOrbPrefab(tSoulOrb);

        Debug.Log("[SceneSetup] Prefabs ready.");

        // --- 3. SCENE OBJECTS ---

        // Managers
        var managersObj = FindOrCreate("Managers");
        var poolManager = EnsureComponent(managersObj, tPoolManager);
        var waveManager = EnsureComponent(managersObj, tWaveManager);
        var gameManager = EnsureComponent(managersObj, tGameManager);

        // Player
        var playerObj = FindOrCreate("Player");
        playerObj.tag = "Player";
        playerObj.layer = LayerMask.NameToLayer("Default");

        var soulSystem = EnsureComponent(playerObj, tSoulSystem);
        var playerController = EnsureComponent(playerObj, tPlayerController);
        var weaponSystem = EnsureComponent(playerObj, tWeaponSystem);

        // Player visual/physics
        var playerSr = EnsureUnityComponent<SpriteRenderer>(playerObj);
        var playerSprite = CreatePlaceholderSprite("PlayerSprite", 16, new Color(0.6f, 0.8f, 1f));
        var srSO = new SerializedObject(playerSr);
        var spriteProp = srSO.FindProperty("m_Sprite");
        var colorProp = srSO.FindProperty("m_Color");
        if (spriteProp != null) spriteProp.objectReferenceValue = playerSprite;
        if (colorProp != null) { colorProp.colorValue = new Color(0.6f, 0.8f, 1f); }
        srSO.ApplyModifiedProperties();

        var playerRb = EnsureUnityComponent<Rigidbody2D>(playerObj);
        playerRb.gravityScale = 0f;
        playerRb.freezeRotation = true;

        var playerCol = EnsureUnityComponent<CircleCollider2D>(playerObj);
        playerCol.radius = 0.3f;

        // FirePoint child
        var firePoint = playerObj.transform.Find("FirePoint");
        if (firePoint == null)
        {
            var fpObj = new GameObject("FirePoint");
            fpObj.transform.SetParent(playerObj.transform);
            fpObj.transform.localPosition = new Vector3(0.5f, 0f, 0f);
            firePoint = fpObj.transform;
        }

        // SoulVFXController + Hunger particles
        var vfxController = EnsureComponent(playerObj, tSoulVFXController);
        var hungerParticlesObj = playerObj.transform.Find("HungerParticles");
        ParticleSystem hungerPS = null;
        if (hungerParticlesObj == null)
        {
            var hpObj = new GameObject("HungerParticles");
            hpObj.transform.SetParent(playerObj.transform);
            hpObj.transform.localPosition = Vector3.zero;
            hungerPS = hpObj.AddComponent<ParticleSystem>();
            var main = hungerPS.main;
            main.startLifetime = 1.2f;
            main.startSpeed = 0.8f;
            main.startSize = 0.15f;
            main.startColor = new Color(0.3f, 0.15f, 0.4f, 0.7f);
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 50;
            main.loop = true;

            var emission = hungerPS.emission;
            emission.rateOverTime = 0f;

            var shape = hungerPS.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.4f;

            var sizeOverLifetime = hungerPS.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
                AnimationCurve.Linear(0f, 1f, 1f, 0f));

            var renderer = hpObj.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Sprites/Default"));
            renderer.sortingOrder = 1;

            hungerPS.Stop();
        }
        else
        {
            hungerPS = hungerParticlesObj.GetComponent<ParticleSystem>();
        }

        if (vfxController != null)
        {
            SetRef(vfxController, "_soulSystem", soulSystem);
            SetRef(vfxController, "_playerSprite", playerSr);
            if (hungerPS != null) SetRef(vfxController, "_hungerParticles", hungerPS);
        }

        // --- 4. WIRE REFERENCES ---

        // SoulSystem data
        if (soulSystem != null)
        {
            SetRef(soulSystem, "_stateData", soulStateData);
            SetRef(soulSystem, "_hungerData", hungerData);
        }

        // PlayerController
        if (playerController != null)
            SetRef(playerController, "_soulSystem", soulSystem);

        // WeaponSystem
        if (weaponSystem != null)
        {
            SetRef(weaponSystem, "_soulSystem", soulSystem);
            SetRef(weaponSystem, "_playerController", playerController);
            SetRef(weaponSystem, "_firePoint", firePoint);
            SetRef(weaponSystem, "_projectilePrefab", projectilePrefab);
        }

        // WaveManager
        if (waveManager != null)
        {
            SetRef(waveManager, "_player", playerObj.transform);
            SetRef(waveManager, "_basicEnemyPrefab", enemyPrefab);
            SetRef(waveManager, "_soulOrbPrefab", soulOrbPrefab);

            // Wave array
            var so = new SerializedObject((Object)waveManager);
            var wavesProp = so.FindProperty("_waves");
            if (wavesProp != null)
            {
                wavesProp.arraySize = 3;
                wavesProp.GetArrayElementAtIndex(0).objectReferenceValue = wave1;
                wavesProp.GetArrayElementAtIndex(1).objectReferenceValue = wave2;
                wavesProp.GetArrayElementAtIndex(2).objectReferenceValue = wave3;
                so.ApplyModifiedProperties();
            }
        }

        Debug.Log("[SceneSetup] All references wired.");

        // --- 5. 2D SCENE ENVIRONMENT ---
        Setup2DEnvironment();

        // Camera
        var mainCam = Camera.main;
        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(0f, 0f, -10f);
            mainCam.transform.rotation = Quaternion.identity;
            mainCam.orthographic = true;
            mainCam.orthographicSize = 8f;
            mainCam.backgroundColor = new Color(0.05f, 0.05f, 0.1f);
            mainCam.clearFlags = CameraClearFlags.SolidColor;
        }

        // --- 6. ARENA BOUNDARY ---
        CreateArenaBoundary();

        // --- 7. HUD ---
        SetupHUD(tSoulMeterHUD, tWarningVignette, soulSystem, waveManager);

        // --- 8. GAME MANAGER (HUD referanslari gerektirir) ---
        if (gameManager != null)
        {
            SetRef(gameManager, "_soulSystem", soulSystem);
            SetRef(gameManager, "_waveManager", waveManager);
            SetRef(gameManager, "_poolManager", poolManager);
            SetRef(gameManager, "_enemyPrefab", enemyPrefab);
            SetRef(gameManager, "_projectilePrefab", projectilePrefab);
            SetRef(gameManager, "_soulOrbPrefab", soulOrbPrefab);

            var goPanel = GameObject.Find("HUDCanvas")?.transform.Find("GameOverPanel");
            if (goPanel != null)
            {
                SetRef(gameManager, "_gameOverPanel", goPanel.gameObject);
                var goText = goPanel.Find("GameOverText");
                if (goText != null) SetRef(gameManager, "_gameOverText", goText.GetComponent<TextMeshProUGUI>());
                var restartBtn = goPanel.Find("RestartButton");
                if (restartBtn != null) SetRef(gameManager, "_restartButton", restartBtn.GetComponent<Button>());
            }
        }

        // --- 9. FINALIZE ---
        var activeScene = UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene();
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(activeScene);
        UnityEditor.SceneManagement.EditorSceneManager.SaveScene(activeScene);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[SceneSetup] === Scene setup complete! Press Play to test. ===");
    }

    // ======== 2D ENVIRONMENT ========

    private static void Setup2DEnvironment()
    {
        // Remove default Directional Light (3D leftover)
        var lights = Object.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var light in lights)
        {
            Debug.Log($"[SceneSetup] Removing 3D light: {light.gameObject.name}");
            Object.DestroyImmediate(light.gameObject);
        }

        // Clear skybox, set flat ambient
        RenderSettings.skybox = null;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.3f, 0.3f, 0.35f);
        RenderSettings.fog = false;
    }

    // ======== DATA ASSET CREATION ========

    private static void EnsureDirectories()
    {
        string[] dirs = {
            $"{DATA_ROOT}/Soul", $"{DATA_ROOT}/Enemies", $"{DATA_ROOT}/Waves",
            $"{PREFAB_ROOT}/Enemies", $"{PREFAB_ROOT}/Projectiles", $"{PREFAB_ROOT}/Player"
        };
        foreach (var dir in dirs)
        {
            if (!AssetDatabase.IsValidFolder(dir))
            {
                string parent = System.IO.Path.GetDirectoryName(dir).Replace('\\', '/');
                string folder = System.IO.Path.GetFileName(dir);
                if (!AssetDatabase.IsValidFolder(parent))
                {
                    string pp = System.IO.Path.GetDirectoryName(parent).Replace('\\', '/');
                    string pf = System.IO.Path.GetFileName(parent);
                    AssetDatabase.CreateFolder(pp, pf);
                }
                AssetDatabase.CreateFolder(parent, folder);
            }
        }
    }

    private static Object CreateSOIfMissing(System.Type type, string path)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (existing != null) return existing;
        if (type == null) { Debug.LogWarning($"[SceneSetup] Type null, cannot create SO at {path}"); return null; }

        var so = ScriptableObject.CreateInstance(type);
        AssetDatabase.CreateAsset(so, path);
        Debug.Log($"[SceneSetup] Created: {path}");
        return so;
    }

    private static Object CreateEnemyData(System.Type type, string path,
        string enemyName, float maxHealth, float moveSpeed, float contactDamage, float soulReward)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (existing != null) return existing;
        if (type == null) return null;

        var so = ScriptableObject.CreateInstance(type);
        var serialized = new SerializedObject(so);
        SetValue(serialized, "EnemyName", enemyName);
        SetFloatValue(serialized, "MaxHealth", maxHealth);
        SetFloatValue(serialized, "MoveSpeed", moveSpeed);
        SetFloatValue(serialized, "ContactDamage", contactDamage);
        SetFloatValue(serialized, "SoulReward", soulReward);
        serialized.ApplyModifiedPropertiesWithoutUndo();

        AssetDatabase.CreateAsset(so, path);
        Debug.Log($"[SceneSetup] Created: {path}");
        return so;
    }

    private static Object CreateWaveData(System.Type type, string path,
        int waveNumber, Object enemyData, int enemyCount, float spawnInterval)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (existing != null) return existing;
        if (type == null || enemyData == null) return null;

        var so = ScriptableObject.CreateInstance(type);
        var serialized = new SerializedObject(so);

        SetIntValue(serialized, "WaveNumber", waveNumber);
        SetFloatValue(serialized, "SpawnInterval", spawnInterval);

        var spawnsProp = serialized.FindProperty("Spawns");
        if (spawnsProp != null)
        {
            spawnsProp.arraySize = 1;
            var entry = spawnsProp.GetArrayElementAtIndex(0);
            var enemyTypeProp = entry.FindPropertyRelative("EnemyType");
            var countProp = entry.FindPropertyRelative("Count");
            if (enemyTypeProp != null) enemyTypeProp.objectReferenceValue = enemyData;
            if (countProp != null) countProp.intValue = enemyCount;
        }

        serialized.ApplyModifiedPropertiesWithoutUndo();
        AssetDatabase.CreateAsset(so, path);
        Debug.Log($"[SceneSetup] Created: {path}");
        return so;
    }

    // ======== PREFAB CREATION ========

    private static GameObject CreateEnemyPrefab(System.Type tEnemy)
    {
        string path = $"{PREFAB_ROOT}/Enemies/BasicEnemy.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = new GameObject("BasicEnemy");
        if (tEnemy != null) go.AddComponent(tEnemy);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.9f, 0.3f, 0.3f);
        sr.sprite = CreatePlaceholderSprite("EnemySprite", 16, new Color(0.9f, 0.3f, 0.3f));

        // Rigidbody2D is added via RequireComponent on Enemy
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null) rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.3f;

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log($"[SceneSetup] Created prefab: {path}");
        return prefab;
    }

    private static GameObject CreateProjectilePrefab(System.Type tProjectile)
    {
        string path = $"{PREFAB_ROOT}/Projectiles/BasicProjectile.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = new GameObject("BasicProjectile");
        if (tProjectile != null) go.AddComponent(tProjectile);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.color = new Color(1f, 0.95f, 0.6f);
        sr.sprite = CreatePlaceholderSprite("ProjectileSprite", 8, new Color(1f, 0.95f, 0.6f));
        sr.sortingOrder = 5;

        var rb = go.GetComponent<Rigidbody2D>();
        if (rb == null) rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        var col = go.AddComponent<CircleCollider2D>();
        col.radius = 0.15f;
        col.isTrigger = true;

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log($"[SceneSetup] Created prefab: {path}");
        return prefab;
    }

    private static GameObject CreateSoulOrbPrefab(System.Type tSoulOrb)
    {
        string path = $"{PREFAB_ROOT}/Enemies/SoulOrb.prefab";
        var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (existing != null) return existing;

        var go = new GameObject("SoulOrb");
        if (tSoulOrb != null) go.AddComponent(tSoulOrb);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.color = new Color(0.4f, 0.9f, 1f);
        sr.sprite = CreatePlaceholderSprite("SoulOrbSprite", 8, new Color(0.4f, 0.9f, 1f));
        sr.sortingOrder = 3;

        go.transform.localScale = Vector3.one * 0.5f;

        var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
        Object.DestroyImmediate(go);
        Debug.Log($"[SceneSetup] Created prefab: {path}");
        return prefab;
    }

    // ======== ARENA BOUNDARY ========

    private static void CreateArenaBoundary()
    {
        var arena = FindOrCreate("ArenaBoundary");
        float halfW = 14f;
        float halfH = 10f;
        float thickness = 1f;

        CreateWall(arena.transform, "WallTop", new Vector3(0, halfH + thickness / 2, 0), new Vector2(halfW * 2 + thickness * 2, thickness));
        CreateWall(arena.transform, "WallBottom", new Vector3(0, -(halfH + thickness / 2), 0), new Vector2(halfW * 2 + thickness * 2, thickness));
        CreateWall(arena.transform, "WallLeft", new Vector3(-(halfW + thickness / 2), 0, 0), new Vector2(thickness, halfH * 2));
        CreateWall(arena.transform, "WallRight", new Vector3(halfW + thickness / 2, 0, 0), new Vector2(thickness, halfH * 2));

        Debug.Log("[SceneSetup] Arena boundary created (28x20 play area).");
    }

    private static void CreateWall(Transform parent, string name, Vector3 localPos, Vector2 size)
    {
        var existing = parent.Find(name);
        if (existing != null) return;

        var wall = new GameObject(name);
        wall.transform.SetParent(parent);
        wall.transform.localPosition = localPos;

        var col = wall.AddComponent<BoxCollider2D>();
        col.size = size;
    }

    // ======== HUD ========

    private static void SetupHUD(System.Type tSoulMeterHUD, System.Type tWarningVignette, Object soulSystem, Object waveManager)
    {
        var canvasObj = GameObject.Find("HUDCanvas");
        if (canvasObj == null)
        {
            canvasObj = new GameObject("HUDCanvas");
            var canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
            var scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // HUD Panel
        var hudPanel = canvasObj.transform.Find("SoulMeterPanel");
        if (hudPanel == null)
        {
            var panelObj = new GameObject("SoulMeterPanel");
            panelObj.transform.SetParent(canvasObj.transform, false);
            var rt = panelObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0.3f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.anchoredPosition = new Vector2(10f, -10f);
            rt.sizeDelta = new Vector2(0f, 120f);
            hudPanel = panelObj.transform;
        }

        // Meter fill background
        var meterBg = hudPanel.Find("MeterBg");
        if (meterBg == null)
        {
            var bgObj = new GameObject("MeterBg");
            bgObj.transform.SetParent(hudPanel, false);
            var bgRt = bgObj.AddComponent<RectTransform>();
            bgRt.anchorMin = Vector2.zero;
            bgRt.anchorMax = new Vector2(1f, 0.3f);
            bgRt.offsetMin = Vector2.zero;
            bgRt.offsetMax = Vector2.zero;
            var bgImg = bgObj.AddComponent<Image>();
            bgImg.color = new Color(0.15f, 0.15f, 0.2f, 0.8f);
        }

        // Meter fill
        var meterFillObj = hudPanel.Find("MeterFill");
        if (meterFillObj == null)
        {
            var mfObj = new GameObject("MeterFill");
            mfObj.transform.SetParent(hudPanel, false);
            var mfRt = mfObj.AddComponent<RectTransform>();
            mfRt.anchorMin = Vector2.zero;
            mfRt.anchorMax = new Vector2(1f, 0.3f);
            mfRt.offsetMin = Vector2.zero;
            mfRt.offsetMax = Vector2.zero;
            var img = mfObj.AddComponent<Image>();
            img.color = Color.white;
            img.type = Image.Type.Filled;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillAmount = 0.5f;
            meterFillObj = mfObj.transform;
        }

        // TMP text elements
        var soulTextObj = CreateOrFindTMP(hudPanel, "SoulText", "50%", new Vector2(0f, 0.7f), new Vector2(0.5f, 1f));
        var stateTextObj = CreateOrFindTMP(hudPanel, "StateText", "Stable", new Vector2(0.5f, 0.7f), new Vector2(1f, 1f));
        var hungerTextObj = CreateOrFindTMP(hudPanel, "HungerText", "", new Vector2(0f, 0.35f), new Vector2(0.5f, 0.65f));
        var waveTextObj = CreateOrFindTMP(hudPanel, "WaveText", "Wave 1", new Vector2(0.5f, 0.35f), new Vector2(1f, 0.65f));

        // SoulMeterHUD component
        if (tSoulMeterHUD != null)
        {
            var hudComp = EnsureComponent(hudPanel.gameObject, tSoulMeterHUD);
            if (hudComp != null)
            {
                SetRef(hudComp, "_soulSystem", soulSystem);
                SetRef(hudComp, "_meterFill", meterFillObj.GetComponent<Image>());
                SetRef(hudComp, "_soulText", soulTextObj.GetComponent<TextMeshProUGUI>());
                SetRef(hudComp, "_stateText", stateTextObj.GetComponent<TextMeshProUGUI>());
                SetRef(hudComp, "_hungerText", hungerTextObj.GetComponent<TextMeshProUGUI>());
                SetRef(hudComp, "_waveText", waveTextObj.GetComponent<TextMeshProUGUI>());
                SetRef(hudComp, "_waveManager", waveManager);
            }
        }

        // Warning Vignette overlay (tam ekran)
        var vignetteObj = canvasObj.transform.Find("WarningVignette");
        if (vignetteObj == null)
        {
            var vObj = new GameObject("WarningVignette");
            vObj.transform.SetParent(canvasObj.transform, false);
            var vRt = vObj.AddComponent<RectTransform>();
            vRt.anchorMin = Vector2.zero;
            vRt.anchorMax = Vector2.one;
            vRt.offsetMin = Vector2.zero;
            vRt.offsetMax = Vector2.zero;

            var vImg = vObj.AddComponent<Image>();
            // Radyal vignette sprite yerine basit kenar gradient — solid color ile basliyoruz
            vImg.color = new Color(1f, 0.4f, 0.1f, 0f);
            vImg.raycastTarget = false;
            vignetteObj = vObj.transform;
        }

        if (tWarningVignette != null)
        {
            var vigComp = EnsureComponent(vignetteObj.gameObject, tWarningVignette);
            if (vigComp != null)
            {
                SetRef(vigComp, "_soulSystem", soulSystem);
                SetRef(vigComp, "_vignetteImage", vignetteObj.GetComponent<Image>());
            }
        }

        // Game Over Panel
        var gameOverPanel = canvasObj.transform.Find("GameOverPanel");
        if (gameOverPanel == null)
        {
            var goPanel = new GameObject("GameOverPanel");
            goPanel.transform.SetParent(canvasObj.transform, false);
            var goRt = goPanel.AddComponent<RectTransform>();
            goRt.anchorMin = Vector2.zero;
            goRt.anchorMax = Vector2.one;
            goRt.offsetMin = Vector2.zero;
            goRt.offsetMax = Vector2.zero;

            // Dark overlay
            var bgImg = goPanel.AddComponent<Image>();
            bgImg.color = new Color(0f, 0f, 0f, 0.7f);

            // Game Over text
            var goTextObj = new GameObject("GameOverText");
            goTextObj.transform.SetParent(goPanel.transform, false);
            var goTextRt = goTextObj.AddComponent<RectTransform>();
            goTextRt.anchorMin = new Vector2(0.2f, 0.5f);
            goTextRt.anchorMax = new Vector2(0.8f, 0.75f);
            goTextRt.offsetMin = Vector2.zero;
            goTextRt.offsetMax = Vector2.zero;
            var goTmp = goTextObj.AddComponent<TextMeshProUGUI>();
            goTmp.text = "GAME OVER";
            goTmp.fontSize = 48f;
            goTmp.alignment = TextAlignmentOptions.Center;
            goTmp.color = new Color(0.95f, 0.2f, 0.2f);
            var defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
            if (defaultFont != null) goTmp.font = defaultFont;

            // Restart button
            var btnObj = new GameObject("RestartButton");
            btnObj.transform.SetParent(goPanel.transform, false);
            var btnRt = btnObj.AddComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.35f, 0.3f);
            btnRt.anchorMax = new Vector2(0.65f, 0.42f);
            btnRt.offsetMin = Vector2.zero;
            btnRt.offsetMax = Vector2.zero;
            var btnImg = btnObj.AddComponent<Image>();
            btnImg.color = new Color(0.2f, 0.6f, 0.9f);
            var btn = btnObj.AddComponent<Button>();

            var btnTextObj = new GameObject("Text");
            btnTextObj.transform.SetParent(btnObj.transform, false);
            var btnTextRt = btnTextObj.AddComponent<RectTransform>();
            btnTextRt.anchorMin = Vector2.zero;
            btnTextRt.anchorMax = Vector2.one;
            btnTextRt.offsetMin = Vector2.zero;
            btnTextRt.offsetMax = Vector2.zero;
            var btnTmp = btnTextObj.AddComponent<TextMeshProUGUI>();
            btnTmp.text = "TEKRAR OYNA";
            btnTmp.fontSize = 24f;
            btnTmp.alignment = TextAlignmentOptions.Center;
            btnTmp.color = Color.white;
            if (defaultFont != null) btnTmp.font = defaultFont;

            goPanel.SetActive(false);
            gameOverPanel = goPanel.transform;
        }

        Debug.Log("[SceneSetup] HUD created.");
    }

    // ======== PLACEHOLDER SPRITE ========

    private static Sprite CreatePlaceholderSprite(string name, int size, Color color)
    {
        string path = $"Assets/SoulRift/Art/Placeholders/{name}.png";
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;

        // Ensure directory
        if (!AssetDatabase.IsValidFolder("Assets/SoulRift/Art"))
            AssetDatabase.CreateFolder("Assets/SoulRift", "Art");
        if (!AssetDatabase.IsValidFolder("Assets/SoulRift/Art/Placeholders"))
            AssetDatabase.CreateFolder("Assets/SoulRift/Art", "Placeholders");

        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        Color borderColor = color * 0.6f;
        borderColor.a = 1f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isBorder = x == 0 || y == 0 || x == size - 1 || y == size - 1;
                tex.SetPixel(x, y, isBorder ? borderColor : color);
            }
        }
        tex.Apply();

        byte[] pngData = tex.EncodeToPNG();
        System.IO.File.WriteAllBytes(path, pngData);
        Object.DestroyImmediate(tex);

        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

        // Configure as sprite
        var importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer != null)
        {
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (sprite == null)
            Debug.LogWarning($"[SceneSetup] Sprite could not be loaded: {path}");
        return sprite;
    }

    // ======== UTILITY ========

    private static GameObject FindOrCreate(string name)
    {
        return GameObject.Find(name) ?? new GameObject(name);
    }

    private static System.Type FindType(string fullName)
    {
        foreach (var asm in System.AppDomain.CurrentDomain.GetAssemblies())
        {
            var t = asm.GetType(fullName);
            if (t != null) return t;
        }
        Debug.LogWarning($"[SceneSetup] Type not found: {fullName}");
        return null;
    }

    private static Component EnsureComponent(GameObject go, System.Type type)
    {
        if (type == null) return null;
        var comp = go.GetComponent(type);
        if (comp == null) comp = go.AddComponent(type);
        return comp;
    }

    private static T EnsureUnityComponent<T>(GameObject go) where T : Component
    {
        var comp = go.GetComponent<T>();
        if (comp == null) comp = go.AddComponent<T>();
        return comp;
    }

    private static void SetRef(Object target, string propName, Object value)
    {
        var so = new SerializedObject(target);
        var prop = so.FindProperty(propName);
        if (prop != null)
        {
            prop.objectReferenceValue = value;
            so.ApplyModifiedProperties();
        }
    }

    private static void SetValue(SerializedObject so, string propName, string value)
    {
        var prop = so.FindProperty(propName);
        if (prop != null) prop.stringValue = value;
    }

    private static void SetFloatValue(SerializedObject so, string propName, float value)
    {
        var prop = so.FindProperty(propName);
        if (prop != null) prop.floatValue = value;
    }

    private static void SetIntValue(SerializedObject so, string propName, int value)
    {
        var prop = so.FindProperty(propName);
        if (prop != null) prop.intValue = value;
    }

    private static Transform CreateOrFindTMP(Transform parent, string name, string defaultText,
        Vector2 anchorMin, Vector2 anchorMax)
    {
        var existing = parent.Find(name);
        if (existing != null) return existing;

        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = defaultText;
        tmp.fontSize = 18f;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = Color.white;

        // Assign default TMP font
        var defaultFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        if (defaultFont != null)
            tmp.font = defaultFont;

        return obj.transform;
    }
}
