using UnityEngine;
using UnityEditor;

public static class TestMapGenerator
{
    private const string MAP_ROOT_NAME = "TestMap";

    [MenuItem("Tools/Generate Test Map")]
    public static void GenerateTestMap()
    {
        GameObject existing = GameObject.Find(MAP_ROOT_NAME);
        if (existing != null)
        {
            if (!EditorUtility.DisplayDialog(
                "TestMap 이미 있음",
                "기존 TestMap을 지우고 새로 만들까요?",
                "예", "취소"))
            {
                return;
            }
            Undo.DestroyObjectImmediate(existing);
        }

        GameObject root = new GameObject(MAP_ROOT_NAME);
        Undo.RegisterCreatedObjectUndo(root, "Generate Test Map");

        // 바닥 (20x20)
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor";
        floor.transform.SetParent(root.transform);
        floor.transform.position = Vector3.zero;
        floor.transform.localScale = new Vector3(2, 1, 2);
        ApplyColor(floor, new Color(0.45f, 0.45f, 0.45f));

        // 벽 4개
        CreateWall(root.transform, "Wall_North", new Vector3(0, 1.5f, 10), new Vector3(20, 3, 0.3f));
        CreateWall(root.transform, "Wall_South", new Vector3(0, 1.5f, -10), new Vector3(20, 3, 0.3f));
        CreateWall(root.transform, "Wall_East", new Vector3(10, 1.5f, 0), new Vector3(0.3f, 3, 20));
        CreateWall(root.transform, "Wall_West", new Vector3(-10, 1.5f, 0), new Vector3(0.3f, 3, 20));

        // 장애물 큐브
        CreateObstacle(root.transform, "Obstacle_1", new Vector3(3, 0.75f, 3), new Vector3(1.5f, 1.5f, 1.5f));
        CreateObstacle(root.transform, "Obstacle_2", new Vector3(-4, 1f, 2), new Vector3(2, 2, 2));
        CreateObstacle(root.transform, "Obstacle_3", new Vector3(2, 0.5f, -5), new Vector3(1, 1, 1));
        CreateObstacle(root.transform, "Obstacle_4", new Vector3(-3, 0.75f, -4), new Vector3(2.5f, 1.5f, 1));
        CreateObstacle(root.transform, "Obstacle_5", new Vector3(0, 1.5f, 7), new Vector3(3, 3, 0.5f));

        // 에너미 스포너 (EnemySpawner 컴포넌트 자동 부착)
        GameObject spawner = new GameObject("EnemySpawner");
        spawner.transform.SetParent(root.transform);
        spawner.transform.position = new Vector3(0, 0.5f, 5);

        var spawnerType = System.Type.GetType("EnemySpawner, Assembly-CSharp");
        if (spawnerType != null)
        {
            spawner.AddComponent(spawnerType);
        }
        else
        {
            Debug.LogWarning("EnemySpawner 스크립트를 찾을 수 없습니다. 스폰포인트만 만들었습니다.");
        }

        // 플레이어 시작 위치 마커 (XR Origin 옮길 위치 참고용)
        GameObject playerSpawn = new GameObject("PlayerSpawnPoint");
        playerSpawn.transform.SetParent(root.transform);
        playerSpawn.transform.position = new Vector3(0, 0, -7);
        // 시각용 화살표 표시 (씬뷰에서만 보임)
        playerSpawn.AddComponent<TestMapMarker>();

        Selection.activeGameObject = root;
        SceneView.lastActiveSceneView?.FrameSelected();

        Debug.Log("[TestMapGenerator] 테스트 맵 생성 완료! XR Origin을 PlayerSpawnPoint 위치(0,0,-7)로 옮기고 Play 누르세요.");
    }

    [MenuItem("Tools/Clear Test Map")]
    public static void ClearTestMap()
    {
        GameObject existing = GameObject.Find(MAP_ROOT_NAME);
        if (existing == null)
        {
            Debug.Log("[TestMapGenerator] TestMap이 없습니다.");
            return;
        }
        Undo.DestroyObjectImmediate(existing);
        Debug.Log("[TestMapGenerator] TestMap 삭제 완료.");
    }

    private static void CreateWall(Transform parent, string name, Vector3 pos, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(parent);
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        ApplyColor(wall, new Color(0.55f, 0.4f, 0.25f));
    }

    private static void CreateObstacle(Transform parent, string name, Vector3 pos, Vector3 scale)
    {
        GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obstacle.name = name;
        obstacle.transform.SetParent(parent);
        obstacle.transform.position = pos;
        obstacle.transform.localScale = scale;
        ApplyColor(obstacle, new Color(0.3f, 0.55f, 0.75f));
    }

    private static void ApplyColor(GameObject obj, Color color)
    {
        var renderer = obj.GetComponent<Renderer>();
        if (renderer == null) return;

        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        if (shader == null) return;

        Material mat = new Material(shader);
        mat.color = color;
        if (shader.name.Contains("Universal Render Pipeline"))
        {
            mat.SetColor("_BaseColor", color);
        }
        renderer.sharedMaterial = mat;
    }
}
