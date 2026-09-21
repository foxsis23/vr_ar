using System.Collections.Generic;
using System.Linq;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Генератор сцени для лабораторної роботи.
/// Меню: Lab1 -> Build Scene
/// </summary>
public static class Lab1SceneBuilder
{
    private const string ScenePath = "Assets/Scenes/Lab1.unity";
    private const string PrefabPath = "Assets/Prefabs/Coin.prefab";
    private const string MaterialsFolder = "Assets/Materials";

    [MenuItem("Lab1/Build Scene")]
    public static void Build()
    {
        EnsureFolders();
        EnsureTags("Coin", "Obstacle", "Bouncer");

        Material groundMaterial = CreateMaterial("Ground", new Color(0.30f, 0.55f, 0.32f));
        Material playerMaterial = CreateMaterial("Player", new Color(0.20f, 0.55f, 0.95f));
        Material coinMaterial = CreateMaterial("Coin", new Color(1.00f, 0.82f, 0.20f));
        Material obstacleMaterial = CreateMaterial("Obstacle", new Color(0.85f, 0.25f, 0.20f));
        Material bouncerMaterial = CreateMaterial("Bouncer", new Color(0.60f, 0.25f, 0.90f));
        Material finishMaterial = CreateMaterial("Finish", new Color(0.20f, 0.90f, 0.45f));

        GameObject coinPrefab = CreateCoinPrefab(coinMaterial);

        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        CreateLights();
        CreateGround(groundMaterial);
        GameObject player = CreatePlayer(playerMaterial);
        CreateObstacles(obstacleMaterial);
        CreateBouncer(bouncerMaterial);
        CreateFinish(finishMaterial);
        CreateMovingObstacle(obstacleMaterial);
        PlaceCoins(coinPrefab);
        CreateCamera(player.transform);

        new GameObject("GameManager", typeof(GameManager));

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        AddSceneToBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Lab1: сцену створено -> {ScenePath}");
    }

    private static void EnsureFolders()
    {
        foreach (string folder in new[] { "Assets/Scenes", "Assets/Prefabs", MaterialsFolder })
        {
            if (!AssetDatabase.IsValidFolder(folder))
            {
                AssetDatabase.CreateFolder("Assets", folder.Substring("Assets/".Length));
            }
        }
    }

    private static void EnsureTags(params string[] tags)
    {
        foreach (string tag in tags)
        {
            if (!InternalEditorUtility.tags.Contains(tag))
            {
                InternalEditorUtility.AddTag(tag);
            }
        }
    }

    private static Material CreateMaterial(string name, Color color)
    {
        string path = $"{MaterialsFolder}/{name}.mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        Material material = new Material(shader);
        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) material.SetColor("_Color", color);

        AssetDatabase.CreateAsset(material, path);
        return material;
    }

    private static GameObject CreateCoinPrefab(Material material)
    {
        GameObject root = new GameObject("Coin");
        root.tag = "Coin";

        SphereCollider trigger = root.AddComponent<SphereCollider>();
        trigger.radius = 0.5f;
        trigger.isTrigger = true;

        root.AddComponent<Rotator>();

        GameObject model = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        model.name = "Model";
        Object.DestroyImmediate(model.GetComponent<Collider>());
        model.transform.SetParent(root.transform, false);
        model.transform.localScale = new Vector3(0.6f, 0.06f, 0.6f);
        model.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        model.GetComponent<Renderer>().sharedMaterial = material;

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        return prefab;
    }

    private static void CreateLights()
    {
        GameObject sun = new GameObject("Directional Light");
        Light sunLight = sun.AddComponent<Light>();
        sunLight.type = LightType.Directional;
        sunLight.intensity = 1.2f;
        sunLight.shadows = LightShadows.Soft;
        sun.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        GameObject point = new GameObject("Point Light");
        Light pointLight = point.AddComponent<Light>();
        pointLight.type = LightType.Point;
        pointLight.range = 18f;
        pointLight.intensity = 3f;
        pointLight.color = new Color(1f, 0.85f, 0.6f);
        point.transform.position = new Vector3(0f, 5f, 4f);
    }

    private static void CreateGround(Material material)
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "Ground";
        ground.transform.localScale = new Vector3(4f, 1f, 4f);
        ground.GetComponent<Renderer>().sharedMaterial = material;

        CreateWall(new Vector3(0f, 1f, 20f), new Vector3(40f, 2f, 1f), material);
        CreateWall(new Vector3(0f, 1f, -20f), new Vector3(40f, 2f, 1f), material);
        CreateWall(new Vector3(20f, 1f, 0f), new Vector3(1f, 2f, 40f), material);
        CreateWall(new Vector3(-20f, 1f, 0f), new Vector3(1f, 2f, 40f), material);
    }

    private static void CreateWall(Vector3 position, Vector3 scale, Material material)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = "Wall";
        wall.transform.position = position;
        wall.transform.localScale = scale;
        wall.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static GameObject CreatePlayer(Material material)
    {
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0f, 1.1f, -8f);
        player.GetComponent<Renderer>().sharedMaterial = material;

        Rigidbody body = player.AddComponent<Rigidbody>();
        body.mass = 1f;
        body.freezeRotation = true;
        body.collisionDetectionMode = CollisionDetectionMode.Continuous;

        player.AddComponent<PlayerController>();
        return player;
    }

    private static void CreateObstacles(Material material)
    {
        Vector3[] positions =
        {
            new Vector3(-4f, 1f, 0f),
            new Vector3(4f, 1f, 3f),
            new Vector3(0f, 1f, 8f),
            new Vector3(-7f, 1f, 6f)
        };

        foreach (Vector3 position in positions)
        {
            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = "Obstacle";
            obstacle.tag = "Obstacle";
            obstacle.transform.position = position;
            obstacle.transform.localScale = new Vector3(2f, 2f, 2f);
            obstacle.GetComponent<Renderer>().sharedMaterial = material;
        }
    }

    private static void CreateMovingObstacle(Material material)
    {
        GameObject mover = GameObject.CreatePrimitive(PrimitiveType.Cube);
        mover.name = "MovingObstacle";
        mover.tag = "Obstacle";
        mover.transform.position = new Vector3(8f, 1f, -6f);
        mover.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        mover.GetComponent<Renderer>().sharedMaterial = material;
        mover.AddComponent<PatrolMover>();
        Rotator rotator = mover.AddComponent<Rotator>();
        SerializedObject serialized = new SerializedObject(rotator);
        serialized.FindProperty("bobAmplitude").floatValue = 0f;
        serialized.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateBouncer(Material material)
    {
        GameObject bouncer = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bouncer.name = "Bouncer";
        bouncer.tag = "Bouncer";
        bouncer.transform.position = new Vector3(-8f, 0.25f, -4f);
        bouncer.transform.localScale = new Vector3(3f, 0.5f, 3f);
        bouncer.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void CreateFinish(Material material)
    {
        GameObject finish = GameObject.CreatePrimitive(PrimitiveType.Cube);
        finish.name = "Finish";
        finish.tag = "Finish";
        finish.transform.position = new Vector3(0f, 1.5f, 15f);
        finish.transform.localScale = new Vector3(4f, 3f, 0.5f);
        finish.GetComponent<Collider>().isTrigger = true;
        finish.GetComponent<Renderer>().sharedMaterial = material;
    }

    private static void PlaceCoins(GameObject prefab)
    {
        Vector3[] positions =
        {
            new Vector3(-2f, 1f, -4f),
            new Vector3(2f, 1f, -2f),
            new Vector3(6f, 1f, 1f),
            new Vector3(-6f, 1f, 2f),
            new Vector3(0f, 1f, 5f),
            new Vector3(5f, 1f, 10f)
        };

        GameObject parent = new GameObject("Coins");

        for (int i = 0; i < positions.Length; i++)
        {
            GameObject coin = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            coin.name = $"Coin_{i + 1}";
            coin.transform.position = positions[i];
            coin.transform.SetParent(parent.transform, true);
        }
    }

    private static void CreateCamera(Transform target)
    {
        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.transform.position = new Vector3(0f, 8f, -16f);
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<CinemachineBrain>();

        GameObject virtualCameraObject = new GameObject("CinemachineCamera");
        CinemachineCamera virtualCamera = virtualCameraObject.AddComponent<CinemachineCamera>();
        virtualCamera.Follow = target;
        virtualCamera.LookAt = target;

        CinemachineFollow follow = virtualCameraObject.AddComponent<CinemachineFollow>();
        follow.FollowOffset = new Vector3(0f, 6f, -9f);

        virtualCameraObject.AddComponent<CinemachineRotationComposer>();
    }

    private static void AddSceneToBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
        if (!scenes.Exists(s => s.path == ScenePath))
        {
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
