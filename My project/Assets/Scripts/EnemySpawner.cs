using UnityEngine;

/// <summary>
/// 간단한 에너미 스포너. 게임 시작 시 주변에 Capsule 에너미 생성.
/// 사용법: 빈 오브젝트에 붙이면 주변에 에너미 자동 스폰
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private int enemyCount = 5;
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private Material enemyMaterial;

    private void Start()
    {
        SpawnEnemies();
    }

    private void SpawnEnemies()
    {
        for (int i = 0; i < enemyCount; i++)
        {
            // 랜덤 위치 계산
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 1f, randomCircle.y);

            // Capsule 에너미 생성
            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            enemy.name = $"Enemy_{i + 1}";
            enemy.transform.position = spawnPos;
            // enemy.tag = "Enemy"; // Tags and Layers에서 Enemy 태그 추가 후 주석 해제

            // 에너미 컬러 (구분되게 초록색)
            if (enemyMaterial != null)
            {
                enemy.GetComponent<Renderer>().material = enemyMaterial;
            }
            else
            {
                enemy.GetComponent<Renderer>().material.color = Color.green;
            }

            // EnemyHealth 붙이기
            enemy.AddComponent<EnemyHealth>();
        }
    }
}
