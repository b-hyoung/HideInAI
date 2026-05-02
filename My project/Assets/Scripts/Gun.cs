using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Gun : MonoBehaviourPun
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
    [Range(0f, 1f)]
    [SerializeField] private float spatialBlend = 1f;
    [SerializeField] private float minDistance = 1f;
    [SerializeField] private float maxDistance = 50f;

    [Header("장전")]
    [SerializeField] private bool requireChambering = false;

    [Header("탄피 배출")]
    [SerializeField] private GameObject shellPrefab;
    [SerializeField] private Transform shellEjectPoint;
    [SerializeField] private float shellEjectForce = 1.5f;
    [SerializeField] private float shellEjectUpForce = 0.4f;
    [SerializeField] private float shellLifetime = 3f;

    private float lastFireTime;
    private AudioSource audioSource;
    private XRGrabInteractable grabInteractable;
    private GameObject hiddenHandVisual;
    private SlideRecoil[] recoilParts;
    private SlideRail[] slideRails;
    private RecoilKick[] kickParts;
    private TriggerPull[] triggerParts;
    private bool chambered;

    private void Awake()
    {
        if (muzzle == null) muzzle = transform;
        grabInteractable = GetComponent<XRGrabInteractable>();
        chambered = !requireChambering;
    }

    public void ChamberRound()
    {
        chambered = true;
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = spatialBlend;
        audioSource.rolloffMode = AudioRolloffMode.Logarithmic;
        audioSource.minDistance = minDistance;
        audioSource.maxDistance = maxDistance;
        audioSource.dopplerLevel = 0f;
        audioSource.volume = 1f;

        if (gunShotClip == null) gunShotClip = CreateGunShotClip();

        recoilParts = GetComponentsInChildren<SlideRecoil>(true);
        slideRails = GetComponentsInChildren<SlideRail>(true);
        kickParts = GetComponentsInChildren<RecoilKick>(true);
        triggerParts = GetComponentsInChildren<TriggerPull>(true);
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
        if (photonView != null && PhotonNetwork.IsConnected && !photonView.IsMine)
        {
            photonView.RequestOwnership();
        }

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
        if (grabInteractable == null || !grabInteractable.isSelected) return;
        if (Time.time - lastFireTime < fireCooldown) return;
        if (requireChambering && !chambered) return;
        if (photonView != null && PhotonNetwork.IsConnected && !photonView.IsMine) return;
        lastFireTime = Time.time;

        Vector3 origin = muzzle.position;
        Vector3 dir = muzzle.forward;

        FireLocal(origin, dir, applyDamage: true);

        if (photonView != null && PhotonNetwork.IsConnected)
        {
            photonView.RPC(nameof(RPC_FireRemote), RpcTarget.Others, origin, dir);
        }
    }

    [PunRPC]
    private void RPC_FireRemote(Vector3 origin, Vector3 dir)
    {
        FireLocal(origin, dir, applyDamage: false);
    }

    private void FireLocal(Vector3 origin, Vector3 dir, bool applyDamage)
    {
        PlayFireVisuals();

        Vector3 endPoint = origin + dir * fireRange;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, fireRange, hitLayer))
        {
            endPoint = hit.point;

            if (applyDamage)
            {
                EnemyHealth enemy = hit.collider.GetComponentInParent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(1);
                    Debug.Log($"[Gun] 명중: {hit.collider.name}");
                }
            }

            CreateImpactEffect(hit.point);
        }

        CreateBulletTrail(origin, endPoint);
    }

    private void PlayFireVisuals()
    {
        if (audioSource != null && gunShotClip != null) audioSource.PlayOneShot(gunShotClip);

        if (recoilParts != null)
        {
            for (int i = 0; i < recoilParts.Length; i++)
            {
                if (recoilParts[i] != null) recoilParts[i].Recoil();
            }
        }

        if (slideRails != null)
        {
            for (int i = 0; i < slideRails.Length; i++)
            {
                if (slideRails[i] != null) slideRails[i].Recoil();
            }
        }

        if (kickParts != null)
        {
            for (int i = 0; i < kickParts.Length; i++)
            {
                if (kickParts[i] != null) kickParts[i].Kick();
            }
        }

        if (triggerParts != null)
        {
            for (int i = 0; i < triggerParts.Length; i++)
            {
                if (triggerParts[i] != null) triggerParts[i].Pull();
            }
        }

        EjectShell();
    }

    private void EjectShell()
    {
        if (shellPrefab == null) return;
        Transform pt = shellEjectPoint != null ? shellEjectPoint : muzzle;
        if (pt == null) return;

        GameObject shell = Instantiate(shellPrefab, pt.position, pt.rotation);
        Rigidbody rb = shell.GetComponent<Rigidbody>();
        if (rb == null) rb = shell.AddComponent<Rigidbody>();

        Vector3 force = pt.right * shellEjectForce + pt.up * shellEjectUpForce;
        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 0.5f, ForceMode.Impulse);

        Destroy(shell, shellLifetime);
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
