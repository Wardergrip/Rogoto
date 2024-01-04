using UnityEngine;

public class EffectExtraHotbarSlot : Effect
{
    [SerializeField] private HotbarType _hotbarType;

    public override void Excecute()
    {
        if (!IsAvailable())
        {
            Debug.LogWarning($"Effect not available.");
            return;
        }
        Hotbar hotbar = Hotbar.s_Hotbars.Find(x => x.HotbarType == _hotbarType);
        hotbar.AddHotbarSlot();
    }

    public override bool IsAvailable()
    {
        switch (_hotbarType) 
        {
            case HotbarType.Machine:
            case HotbarType.Combat:
                Hotbar hotbar = Hotbar.s_Hotbars.Find(x => x.HotbarType == _hotbarType);
                if (hotbar == null) return false; // It happens that because of order of initialisation the hotbars are not there yet and this effect is initialised in the UI which gets override anyway
                return hotbar.CurrentActiveSlotsAmount < hotbar.MaxActiveSlotsAmount;
            default:
                Debug.LogWarning($"Hotbartype not implemented");
                return false;
        }
    }
}
