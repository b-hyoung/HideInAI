using UnityEngine;
using UnityEditor;

public static class VRPlayerSetup
{
    private const string LEFT_HAND_VISUAL = "HandVisual_Left";
    private const string RIGHT_HAND_VISUAL = "HandVisual_Right";
    private const string GUN_VISUAL = "GunVisual";

    [MenuItem("Tools/Setup VR Player Visuals")]
    public static void SetupVRPlayer()
    {
        // 씬에서 컨트롤러 찾기 (다양한 이름 패턴 지원)
        Transform leftHand = FindController(true);
        Transform rightHand = FindController(false);

        if (leftHand == null && rightHand == null)
        {
            EditorUtility.DisplayDialog(
                "컨트롤러 못 찾음",
                "씬에서 LeftHand/RightHand Controller를 찾을 수 없습니다.\n" +
                "XR Origin이 씬에 있는지 확인하세요.",
                "확인");
            return;
        }

        if (leftHand != null)
        {
            CreateHandVisual(leftHand, LEFT_HAND_VISUAL, new Color(0.9f, 0.7f, 0.5f));
            Debug.Log($"[VRPlayerSetup] 왼손 비주얼 추가: {leftHand.name}");
        }

        if (rightHand != null)
        {
            CreateHandVisual(rightHand, RIGHT_HAND_VISUAL, new Color(0.9f, 0.7f, 0.5f));
            CreateGunVisual(rightHand);
            Debug.Log($"[VRPlayerSetup] 오른손 비주얼 + 총 추가: {rightHand.name}");
        }

        Debug.Log("[VRPlayerSetup] 완료! Play 눌러서 확인하세요.");
    }

    [MenuItem("Tools/Clear VR Player Visuals")]
    public static void ClearVRPlayer()
    {
        DestroyByName(LEFT_HAND_VISUAL);
        DestroyByName(RIGHT_HAND_VISUAL);
        DestroyByName(GUN_VISUAL);
        Debug.Log("[VRPlayerSetup] 비주얼 제거 완료.");
    }

    private static Transform FindController(bool isLeft)
    {
        string side = isLeft ? "Left" : "Right";
        var allObjects = Object.FindObjectsOfType<Transform>();

        // 1순위: 정확히 "Left Controller" / "Right Controller" 또는 "LeftHand Controller" / "RightHand Controller"
        foreach (var t in allObjects)
        {
            if (t.name == $"{side} Controller" ||
                t.name == $"{side}Hand Controller" ||
                t.name == $"{side}HandController")
            {
                return t;
            }
        }

        // 2순위: side + "Controller" 들어가지만 제외 키워드 없는 것
        string[] excludeWords = { "Teleport", "Stabilized", "Visual", "Origin", "Interactor", "Affordance" };
        foreach (var t in allObjects)
        {
            if (!t.name.Contains(side)) continue;
            if (!t.name.Contains("Controller")) continue;

            bool excluded = false;
            foreach (var word in excludeWords)
            {
                if (t.name.Contains(word))
                {
                    excluded = true;
                    break;
                }
            }
            if (!excluded) return t;
        }

        return null;
    }

    private static void CreateHandVisual(Transform parent, string visualName, Color color)
    {
        // 기존 비주얼 제거
        Transform existing = parent.Find(visualName);
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
        }

        GameObject hand = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hand.name = visualName;
        hand.transform.SetParent(parent, false);
        hand.transform.localPosition = Vector3.zero;
        hand.transform.localRotation = Quaternion.identity;
        // 크기 키움 (안 보인다고 해서)
        hand.transform.localScale = new Vector3(0.12f, 0.08f, 0.2f);

        // 콜라이더 제거 (총알이 손에 안 맞도록)
        Object.DestroyImmediate(hand.GetComponent<Collider>());

        ApplyEmissiveColor(hand, color);
        Undo.RegisterCreatedObjectUndo(hand, "Create Hand Visual");
    }

    private static void CreateGunVisual(Transform rightHand)
    {
        // 기존 총 제거
        Transform existing = rightHand.Find(GUN_VISUAL);
        if (existing != null)
        {
            Undo.DestroyObjectImmediate(existing.gameObject);
        }

        // 총 본체 (컨트롤러 비주얼보다 앞쪽으로 + 자연 그립 각도)
        GameObject gun = new GameObject(GUN_VISUAL);
        gun.transform.SetParent(rightHand, false);
        gun.transform.localPosition = new Vector3(0, -0.02f, 0.1f);
        gun.transform.localRotation = Quaternion.Euler(-30f, 0f, 0f); // 자연스러운 그립 각도

        // 총 모양 (크게 키움)
        GameObject barrel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        barrel.name = "Barrel";
        barrel.transform.SetParent(gun.transform, false);
        barrel.transform.localPosition = new Vector3(0, 0, 0.15f);
        barrel.transform.localScale = new Vector3(0.06f, 0.06f, 0.3f);
        Object.DestroyImmediate(barrel.GetComponent<Collider>());
        ApplyColor(barrel, new Color(0.15f, 0.15f, 0.15f));

        GameObject grip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        grip.name = "Grip";
        grip.transform.SetParent(gun.transform, false);
        grip.transform.localPosition = new Vector3(0, -0.08f, 0.02f);
        grip.transform.localScale = new Vector3(0.05f, 0.14f, 0.07f);
        Object.DestroyImmediate(grip.GetComponent<Collider>());
        ApplyColor(grip, new Color(0.2f, 0.15f, 0.1f));

        // 머즐 (총구) - 총알 발사 위치
        GameObject muzzle = new GameObject("Muzzle");
        muzzle.transform.SetParent(gun.transform, false);
        muzzle.transform.localPosition = new Vector3(0, 0, 0.32f);
        muzzle.transform.localRotation = Quaternion.identity;

        // VRGun 스크립트 부착 (없으면)
        var vrGunType = System.Type.GetType("VRGun, Assembly-CSharp");
        if (vrGunType != null)
        {
            var existingGun = Object.FindObjectOfType(vrGunType);
            if (existingGun != null)
            {
                Debug.LogWarning($"[VRPlayerSetup] VRGun이 이미 씬에 있음: {((Component)existingGun).gameObject.name}. 새로 만든 GunVisual에 다시 붙이세요.");
            }
            else
            {
                gun.AddComponent(vrGunType);
                Debug.Log("[VRPlayerSetup] VRGun 스크립트 자동 부착됨. Inspector에서 Trigger Action을 'XRI RightHand Interaction/Activate'로 설정하세요.");
            }
        }

        Undo.RegisterCreatedObjectUndo(gun, "Create Gun Visual");
    }

    private static void DestroyByName(string name)
    {
        var found = GameObject.Find(name);
        while (found != null)
        {
            Undo.DestroyObjectImmediate(found);
            found = GameObject.Find(name);
        }
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

    private static void ApplyEmissiveColor(GameObject obj, Color color)
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
            mat.SetColor("_EmissionColor", color * 1.5f);
            mat.EnableKeyword("_EMISSION");
        }
        else
        {
            mat.SetColor("_EmissionColor", color * 1.5f);
            mat.EnableKeyword("_EMISSION");
        }
        renderer.sharedMaterial = mat;
    }
}
