using UnityEngine;
using UnityEngine.UI;

public class HotbarMimicSlot : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    private HotbarSlot _hotbarSlot;
    private Blueprint _blueprintToAdd;
    public void SetUpSlot(HotbarSlot slot, Blueprint blueprint)
    {
        _blueprintToAdd = blueprint;
        _hotbarSlot = slot;
        if (_hotbarSlot.Blueprint != null)
        {
            _iconImage.sprite = _hotbarSlot.Blueprint.Icon;
            _iconImage.gameObject.SetActive(true);
        }
    }
    public void SetBlueprintToThisSlot()
    {
        _hotbarSlot.SetNewBlueprint(_blueprintToAdd);
        _iconImage.sprite = _hotbarSlot.Blueprint.Icon;
        FindObjectOfType<BlueprintScreen>().Skip();
    }

}
