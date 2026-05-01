using UnityEngine;

/// <summary>
/// 간단한 에너미 스포너. 게임 시작 시 주변에 사람 모양 에너미 생성.
/// 사용법: 빈 오브젝트에 붙이면 주변에 에너미 자동 스폰
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private int enemyCount = 20;
    [SerializeField] private float spawnRadius = 14f;
    [SerializeField] private Color enemyColor = new Color(0.2f, 0.7f, 0.3f);

    private void Start()
    {
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);

            CreateHumanoidEnemy($"Enemy_{i + 1}", spawnPos);
        }
    }

    private void CreateHumanoidEnemy(string enemyName, Vector3 position)
    {
        // 부모 오브젝트
        GameObject root = new GameObject(enemyName);
        root.transform.position = position;

        // 몸통 (Capsule)
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(root.transform);
        body.transform.localPosition = new Vector3(0, 0.9f, 0);
        body.transform.localScale = new Vector3(0.6f, 0.7f, 0.4f);
        ApplyColor(body, enemyColor);

        // 머리 (Sphere)
        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(root.transform);
        head.transform.localPosition = new Vector3(0, 1.75f, 0);
        head.transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        ApplyColor(head, new Color(0.95f, 0.8f, 0.7f));
        // 머리 콜라이더는 그대로 (헤드샷 가능)

        // 왼팔
        GameObject leftArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        leftArm.name = "LeftArm";
        leftArm.transform.SetParent(root.transform);
        leftArm.transform.localPosition = new Vector3(-0.4f, 1.0f, 0);
        leftArm.transform.localScale = new Vector3(0.18f, 0.5f, 0.18f);
        ApplyColor(leftArm, enemyColor);
        Destroy(leftArm.GetComponent<Collider>());

        // 오른팔
        GameObject rightArm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        rightArm.name = "RightArm";
        rightArm.transform.SetParent(root.transform);
        rightArm.transform.localPosition = new Vector3(0.4f, 1.0f, 0);
        rightArm.transform.localScale = new Vector3(0.18f, 0.5f, 0.18f);
        ApplyColor(rightArm, enemyColor);
        Destroy(rightArm.GetComponent<Collider>());

        // 몸통에 EnemyHealth + AI 부착
        root.AddComponent<EnemyHealth>();
        root.AddComponent<EnemyAI>();

        // 랜덤 방향 회전
        root.transform.rotation = Quaternion.Euler(0, Random.Range(0, 360), 0);
    }

    private void ApplyColor(GameObject obj, Color color)
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
        renderer.material = mat;
    }
}
