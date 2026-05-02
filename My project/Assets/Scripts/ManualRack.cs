using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider))]
public class ManualRack : MonoBehaviour
{
    [SerializeField] private Gun gun;

    private XRSimpleInteractable simpleInteractable;

    private void Awake()
    {
        if (gun == null) gun = GetComponentInParent<Gun>();

        simpleInteractable = GetComponent<XRSimpleInteractable>();
        if (simpleInteractable == null)
        {
            simpleInteractable = gameObject.AddComponent<XRSimpleInteractable>();
        }
    }

    private void OnEnable()
    {
        if (simpleInteractable != null) simpleInteractable.selectEntered.AddListener(OnRackInput);
    }

    private void OnDisable()
    {
        if (simpleInteractable != null) simpleInteractable.selectEntered.RemoveListener(OnRackInput);
    }

    private void OnRackInput(SelectEnterEventArgs args)
    {
        if (gun != null) gun.ChamberRound();
    }
}
