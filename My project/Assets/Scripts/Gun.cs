using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Gun : MonoBehaviour
{
    [Header("발사 설정")]
    [SerializeField] private Transform muzzle;
    [SerializeField] private float fireRange = 50f;
    [SerializeField] private float fireCooldown = 0.2f;
    [SerializeField] private LayerMask hitLayer = ~0;

    [Header("VR 입력")]
    [SerializeField] private InputActionReference triggerAction;

    [Header("이펙트")]
    [SerializeField] private Color bulletColor = Color.yellow;

    [Header("사운드")]
    [SerializeField] private AudioClip gunShotClip;

    private float lastFireTime;
    private AudioSource audioSource;
    private XRGrabInteractable grabInteractable;
    private GameObject hiddenHandVisual;
    private SlideRecoil[] recoilParts;

    private void Awake()
    {
        if (muzzle == null) muzzle = transform;
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f;
        audioSource.volume = 1f;

        if (gunShotClip == null) gunShotClip = CreateGunShotClip();

        recoilParts = GetComponentsInChildren<SlideRecoil>(true);
    }

    private void OnEnable()
    {
        if (triggerAction != null) triggerAction.action.Enable();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrabbed);
            grabInteractable.selectExited.AddListener(OnReleased);
        }
    }

    private void OnDisable()
    {
        if (triggerAction != null) triggerAction.action.Disable();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrabbed);
            grabInteractable.selectExited.RemoveListener(OnReleased);
        }
        if (hiddenHandVisual != null)
        {
            hiddenHandVisual.SetActive(true);
            hiddenHandVisual = null;
        }
    }

    private void OnGrabbed(SelectEnterEventArgs args)
    {
        Transform t = args.interactorObject.transform;
        while (t != null)
        {
            Transform visual = t.Find("HandVisual_Right");
            if (visual == null) visual = t.Find("HandVisual_Left");
            if (visual != null)
            {
                hiddenHandVisual = visual.gameObject;
                hiddenHandVisual.SetActive(false);
                return;
            }
            t = t.parent;
        }
    }

    private void OnReleased(SelectExitEventArgs args)
    {
        if (hiddenHandVisual != null)
        {
            hiddenHandVisual.SetActive(true);
            hiddenHandVisual = null;
        }
    }

    private void Update()
    {
        if (triggerAction == null) return;
        if (triggerAction.action.WasPressedThisFrame()) TryFire();
    }

    private void TryFire()
    {
        if (Time.time - lastFireTime < fireCooldown) return;
        lastFireTime = Time.time;

        if (audioSource != null && gunShotClip != null) audioSource.PlayOneShot(gunShotClip);

        if (recoilParts != null)
        {
            for (int i = 0; i < recoilParts.Length; i++)
            {
                if (recoilParts[i] != null) recoilParts[i].Recoil();
            }
        }

        Vector3 origin = muzzle.position;
        Vector3 dir = muzzle.forward;
        Vector3 endPoint = origin + dir * fireRange;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, fireRange, hitLayer))
        {
            endPoint = hit.point;

            EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
            if (enemy != null)
            {
                enemy.TakeDamage(1);
                Debug.Log($"[Gun] 명중: {hit.collider.name}");
            }

            CreateImpactEffect(hit.point);
        }

        CreateBulletTrail(origin, endPoint);
    }

    private void CreateBulletTrail(Vector3 start, Vector3 end)
    {
        GameObject flash = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        flash.name = "MuzzleFlash";
        flash.transform.position = start;
        flash.transform.localScale = Vector3.one * 0.08f;
        Destroy(flash.GetComponent<Collider>());
        ApplyEmissive(flash, Color.yellow);
        Destroy(flash, 0.08f);
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
        int samples = sampleRate / 5;
        AudioClip clip = AudioClip.Create("GunShot", samples, 1, sampleRate, false);

        float[] data = new float[samples];
        for (int i = 0; i < samples; i++)
        {
            float t = (float)i / samples;
            float noise = Random.Range(-1f, 1f);
            float lowBoom = Mathf.Sin(2f * Mathf.PI * 80f * t);
            float envelope = Mathf.Exp(-t * 15f);
            data[i] = (noise * 0.6f + lowBoom * 0.4f) * envelope;
        }

        clip.SetData(data, 0);
        return clip;
    }
}
