using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMouseUI : MonoBehaviour
{
   
    void Update()
    {
        transform.position = Input.mousePosition;
    }
}
