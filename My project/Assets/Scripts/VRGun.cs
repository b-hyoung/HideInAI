using UnityEngine;
using UnityEngine.InputSystem;

public class VRGun : MonoBehaviour
{
    [Header("총 설정")]
    [SerializeField] private float fireRange = 50f;
    [SerializeField] private float fireCooldown = 0.5f;
    [SerializeField] private LayerMask hitLayer = ~0;

    [Header("VR 입력")]
    [SerializeField] private InputActionReference triggerAction;

    [Header("총알 시각 효과")]
    [SerializeField] private Color bulletColor = Color.yellow;

    private float lastFireTime;
    private AudioSource audioSource;
    private AudioClip gunShotClip;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;

        // 총소리 미리 만들어두기
        gunShotClip = CreateGunShotClip();
    }

    private void Update()
    {
        if (triggerAction == null) return;

        if (triggerAction.action.WasPressedThisFrame())
        {
            TryFire();
        }
    }

    private void TryFire()
    {
        if (Time.time - lastFireTime < fireCooldown) return;
        lastFireTime = Time.time;

        // 총소리
        audioSource.PlayOneShot(gunShotClip);

        Ray ray = new Ray(transform.position, transform.forward);
        Vector3 endPoint = transform.position + transform.forward * fireRange;

        if (Physics.Raycast(ray, out RaycastHit hit, fireRange, hitLayer))
        {
            endPoint = hit.point;

            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(1);
                Debug.Log($"[총] 에너미 명중: {hit.collider.name}");
            }

            // 맞은 자리에 임팩트 이펙트
            CreateImpactEffect(hit.point);
        }

        // 총알 궤적 라인 표시
        CreateBulletTrail(transform.position, endPoint);
    }

    private void CreateBulletTrail(Vector3 start, Vector3 end)
    {
        GameObject trail = new GameObject("BulletTrail");
        LineRenderer lr = trail.AddComponent<LineRenderer>();
        lr.startWidth = 0.02f;
        lr.endWidth = 0.02f;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.startColor = bulletColor;
        lr.endColor = bulletColor;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        Destroy(trail, 0.1f);
    }

    private void CreateImpactEffect(Vector3 point)
    {
        GameObject impact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        impact.transform.position = point;
        impact.transform.localScale = Vector3.one * 0.15f;
        impact.GetComponent<Renderer>().material.color = Color.red;
        Destroy(impact.GetComponent<Collider>());
        Destroy(impact, 0.3f);
    }

    private AudioClip CreateGunShotClip()
    {
        int sampleRate = 44100;
        int samples = sampleRate / 5; // 0.2초
        AudioClip clip = AudioClip.Create("GunShot", samples, 1, sampleRate, false);

        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float noise = Random.Range(-1f, 1f);
            float lowBoom = Mathf.Sin(2f * Mathf.PI * 80f * t); // 저음 쿵
            float envelope = Mathf.Exp(-t * 15f);
            data[i] = (noise * 0.6f + lowBoom * 0.4f) * envelope;
        }

        clip.SetData(data, 0);
        return clip;
    }
}
