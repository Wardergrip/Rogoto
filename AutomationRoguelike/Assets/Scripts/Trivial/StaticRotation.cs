using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StaticRotation : MonoBehaviour
{
    
    void Update()
    {
        transform.rotation = Quaternion.identity;
    }
}
