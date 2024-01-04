using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlueprintTester : MonoBehaviour
{
#if UNITY_EDITOR
    [SerializeField] private Blueprint _blueprint;
    [SerializeField] private MonoVariant _variant;
    [SerializeField] private KeyCode _key = KeyCode.L;
    void Update()
    {
        if (Input.GetKeyDown(_key))
        {
            StructurePlacer structurePlacer = FindObjectOfType<StructurePlacer>();
            Blueprint blueprint = _blueprint;
            if (_variant !=  null)
            {
                blueprint = _blueprint.Duplicate();
                blueprint.Variant = _variant;
            }
            structurePlacer.SetUp(blueprint);
        }
    }
#endif
}
