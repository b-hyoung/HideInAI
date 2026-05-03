using UnityEngine;
using UnityEditor;

public static class VRPlayerSetup
{
    private const string LEFT_HAND_VISUAL = "HandVisual_Left";
    private const string RIGHT_HAND_VISUAL = "HandVisual_Right";

    [MenuItem("Tools/Setup VR Player Visuals")]
    public static void SetupVRPlayer()
    {
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
            Debug.Log($"[VRPlayerSetup] 오른손 비주얼 추가: {rightHand.name}");
        }

        Debug.Log("[VRPlayerSetup] 완료! Play 눌러서 확인하세요.");
    }

    [MenuItem("Tools/Clear VR Player Visuals")]
    public static void ClearVRPlayer()
    {
        DestroyByName(LEFT_HAND_VISUAL);
        DestroyByName(RIGHT_HAND_VISUAL);
        Debug.Log("[VRPlayerSetup] 비주얼 제거 완료.");
    }

    private static Transform FindController(bool isLeft)
    {
        string side = isLeft ? "Left" : "Right";
        var allObjects = Object.FindObjectsOfType<Transform>();

        foreach (var t in allObjects)
        {
            if (t.name == $"{side} Controller" ||
                t.name == $"{side}Hand Controller" ||
                t.name == $"{side}HandController")
            {
                return t;
            }
        }

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
        hand.transform.localScale = new Vector3(0.12f, 0.08f, 0.2f);

        Object.DestroyImmediate(hand.GetComponent<Collider>());

        ApplyEmissiveColor(hand, color);
        Undo.RegisterCreatedObjectUndo(hand, "Create Hand Visual");
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
