using UnityEngine;

/// <summary>
/// 캐릭터 비주얼 (placeholder). 모든 플레이어 + AI가 같은 모양 사용.
/// 나중에 실제 모델 오면:
/// - 1) 자식의 Placeholder GameObject 다 삭제
/// - 2) 실제 모델 메쉬를 자식으로 추가
/// - 3) Inspector에서 head/body/leftHand/rightHand 슬롯에 모델의 본을 드래그
/// 그러면 PlayerAvatar/EnemyAI 코드는 그대로 작동.
/// </summary>
public class CharacterModel : MonoBehaviour
{
    [Header("앵커 (실제 모델 본 또는 Placeholder)")]
    public Transform head;
    public Transform body;
    public Transform leftHand;
    public Transform rightHand;
    public Transform leftArm;  // 어깨~손 연결 (placeholder만 사용, 동적 스케일됨)
    public Transform rightArm;

    [Header("Placeholder 자동 생성")]
    [SerializeField] private bool autoGeneratePlaceholder = true;
    [SerializeField] private Color characterColor = new Color(0.45f, 0.55f, 0.65f);
    [SerializeField] private Color skinColor = new Color(0.95f, 0.8f, 0.7f);

    private void Awake()
    {
        if (autoGeneratePlaceholder)
        {
            GeneratePlaceholder();
        }
    }

    private void GeneratePlaceholder()
    {
        // 이미 슬롯이 채워져 있으면 (실제 모델 사용 중) 생성 안 함
        if (head != null || body != null) return;

        // 머리 (Sphere)
        GameObject headObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        headObj.name = "Head_Placeholder";
        headObj.transform.SetParent(transform, false);
        headObj.transform.localPosition = new Vector3(0, 1.7f, 0);
        headObj.transform.localScale = new Vector3(0.25f, 0.3f, 0.25f);
        ApplyColor(headObj, skinColor);
        Destroy(headObj.GetComponent<Collider>());
        head = headObj.transform;

        // 몸통 (Capsule) — PlayerAvatar에서 다리까지 늘어나게 동적으로 스케일됨
        GameObject bodyObj = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        bodyObj.name = "Body_Placeholder";
        bodyObj.transform.SetParent(transform, false);
        bodyObj.transform.localPosition = new Vector3(0, 0.85f, 0);
        bodyObj.transform.localScale = new Vector3(0.4f, 0.85f, 0.3f);
        ApplyColor(bodyObj, characterColor);
        body = bodyObj.transform;

        // 왼손 (Cube)
        GameObject leftHandObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftHandObj.name = "LeftHand_Placeholder";
        leftHandObj.transform.SetParent(transform, false);
        leftHandObj.transform.localPosition = new Vector3(-0.3f, 1.1f, 0.2f);
        leftHandObj.transform.localScale = new Vector3(0.1f, 0.07f, 0.18f);
        ApplyColor(leftHandObj, skinColor);
        Destroy(leftHandObj.GetComponent<Collider>());
        leftHand = leftHandObj.transform;

        // 오른손 (Cube)
        GameObject rightHandObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightHandObj.name = "RightHand_Placeholder";
        rightHandObj.transform.SetParent(transform, false);
        rightHandObj.transform.localPosition = new Vector3(0.3f, 1.1f, 0.2f);
        rightHandObj.transform.localScale = new Vector3(0.1f, 0.07f, 0.18f);
        ApplyColor(rightHandObj, skinColor);
        Destroy(rightHandObj.GetComponent<Collider>());
        rightHand = rightHandObj.transform;

        // 팔 (어깨~손 연결, 동적 스케일됨)
        leftArm = CreateArm("LeftArm_Placeholder", characterColor);
        rightArm = CreateArm("RightArm_Placeholder", characterColor);
    }

    private Transform CreateArm(string armName, Color color)
    {
        GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        arm.name = armName;
        arm.transform.SetParent(transform, false);
        arm.transform.localScale = new Vector3(0.07f, 0.2f, 0.07f);
        ApplyColor(arm, color);
        Destroy(arm.GetComponent<Collider>());
        return arm.transform;
    }

    /// <summary>
    /// 본인 시점에서는 머리 가리기 (안에 들어가있으면 화면 보임)
    /// </summary>
    public void HideHeadForLocalPlayer()
    {
        if (head != null)
        {
            var renderer = head.GetComponent<Renderer>();
            if (renderer != null) renderer.enabled = false;
        }
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
        renderer.sharedMaterial = mat;
    }
}
