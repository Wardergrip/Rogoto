using System.Collections;
using UnityEngine;

public class HotbarMimic : MonoBehaviour
{
    private Hotbar _hotbar;
    [SerializeField] private HotbarMimicSlot _slotPreFab;
    private Blueprint _blueprintToAdd;
    public Hotbar Hotbar { get => _hotbar;  }
    public Blueprint BlueprintToAdd { get => _blueprintToAdd;  }
    private float _timePerSlotSpawn = 0.05f;

    public void SetUp(Hotbar hotbar, Blueprint blueprint)
    {
        foreach(Transform transform in transform)
        {
            Destroy(transform.gameObject);
        }
        _blueprintToAdd = blueprint;
        _hotbar = hotbar;
        StartCoroutine(SpawnSlots());
    }
    private IEnumerator SpawnSlots()
    {
        for (int i = 0; i < _hotbar.CurrentActiveSlotsAmount; i++)
        {
            if (ShouldSkip(Hotbar.HotbarSlots[i]))
            {
                continue;
            }
            HotbarMimicSlot slot = Instantiate(_slotPreFab, transform);
            slot.SetUpSlot(Hotbar.HotbarSlots[i], _blueprintToAdd);
            yield return new WaitForSeconds(_timePerSlotSpawn);
        }
    }
    public void RemoveMimic()
    {
        Destroy(this.gameObject);
    }

    private bool ShouldSkip(HotbarSlot slot)
    {
        // Sloppy fix so that you can't remove depositbox and miner from your hotbar
        if (slot.Blueprint == null)
        {
            return false;
        }
        if (slot.Blueprint.Name == "Depot" || slot.Blueprint.Name == "Miner")
        {
            return true;
        }
        return false;
    }
}
