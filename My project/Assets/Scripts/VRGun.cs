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

    [Header("레이저 포인터 (조준선)")]
    [SerializeField] private bool showLaserPointer = true;
    [SerializeField] private Color laserColor = Color.red;
    [SerializeField] private float laserWidth = 0.005f;

    private float lastFireTime;
    private AudioSource audioSource;
    private AudioClip gunShotClip;
    private LineRenderer laserLine;
    private GameObject laserDot;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;

        gunShotClip = CreateGunShotClip();

        if (showLaserPointer)
        {
            SetupLaserPointer();
        }
    }

    private void SetupLaserPointer()
    {
        GameObject laserObj = new GameObject("LaserPointer");
        laserObj.transform.SetParent(transform, false);
        laserLine = laserObj.AddComponent<LineRenderer>();
        laserLine.startWidth = laserWidth;
        laserLine.endWidth = laserWidth;
        laserLine.useWorldSpace = true;

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (shader == null) shader = Shader.Find("Unlit/Color");
        if (shader == null) shader = Shader.Find("Sprites/Default");

        Material mat = new Material(shader);
        mat.color = laserColor;
        if (shader.name.Contains("Universal Render Pipeline"))
        {
            mat.SetColor("_BaseColor", laserColor);
        }
        laserLine.material = mat;
        laserLine.startColor = laserColor;
        laserLine.endColor = laserColor;

        // 레이저 끝 점 (조준점)
        laserDot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        laserDot.name = "LaserDot";
        laserDot.transform.localScale = Vector3.one * 0.04f;
        Destroy(laserDot.GetComponent<Collider>());

        Shader dotShader = Shader.Find("Universal Render Pipeline/Lit");
        if (dotShader == null) dotShader = Shader.Find("Standard");
        if (dotShader != null)
        {
            Material dotMat = new Material(dotShader);
            dotMat.color = laserColor;
            if (dotShader.name.Contains("Universal Render Pipeline"))
            {
                dotMat.SetColor("_BaseColor", laserColor);
                dotMat.SetColor("_EmissionColor", laserColor * 3f);
                dotMat.EnableKeyword("_EMISSION");
            }
            laserDot.GetComponent<Renderer>().sharedMaterial = dotMat;
        }
    }

    private void Update()
    {
        if (showLaserPointer && laserLine != null)
        {
            UpdateLaserPointer();
        }

        if (triggerAction == null) return;

        if (triggerAction.action.WasPressedThisFrame())
        {
            TryFire();
        }
    }

    private void UpdateLaserPointer()
    {
        Vector3 start = transform.position;
        Vector3 end = transform.position + transform.forward * fireRange;

        if (Physics.Raycast(start, transform.forward, out RaycastHit hit, fireRange, hitLayer))
        {
            end = hit.point;
        }

        laserLine.SetPosition(0, start);
        laserLine.SetPosition(1, end);

        if (laserDot != null)
        {
            laserDot.transform.position = end;
            laserDot.SetActive(true);
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
        lr.startWidth = 0.04f;
        lr.endWidth = 0.04f;

        // URP 호환 셰이더 우선 시도
        Shader lineShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (lineShader == null) lineShader = Shader.Find("Unlit/Color");
        if (lineShader == null) lineShader = Shader.Find("Sprites/Default");

        Material mat = new Material(lineShader);
        mat.color = bulletColor;
        if (lineShader.name.Contains("Universal Render Pipeline"))
        {
            mat.SetColor("_BaseColor", bulletColor);
        }
        lr.material = mat;
        lr.startColor = bulletColor;
        lr.endColor = bulletColor;
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        Destroy(trail, 0.3f);

        // 머즐 플래시 (총구에 노란 구체 잠깐)
        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.name = "MuzzleFlash";
        flash.transform.position = start;
        flash.transform.localScale = Vector3.one * 0.08f;
        Destroy(flash.GetComponent<Collider>());
        ApplyEmissive(flash, Color.yellow);
        Destroy(flash, 0.08f);

        Debug.Log($"[총] 발사! 시작:{start} 끝:{end}");
    }

    private void CreateImpactEffect(Vector3 point)
    {
        GameObject impact = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        impact.name = "ImpactEffect";
        impact.transform.position = point;
        impact.transform.localScale = Vector3.one * 0.2f;
        Destroy(impact.GetComponent<Collider>());
        ApplyEmissive(impact, Color.red);
        Destroy(impact, 0.4f);
    }

    private void ApplyEmissive(GameObject obj, Color color)
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
            mat.SetColor("_EmissionColor", color * 3f);
            mat.EnableKeyword("_EMISSION");
        }
        else
        {
            mat.SetColor("_EmissionColor", color * 3f);
            mat.EnableKeyword("_EMISSION");
        }
        renderer.material = mat;
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
