using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Trivial;

public class ConnectionRotationRandomizer : MonoBehaviour
{
    [SerializeField] private LockAxis _lockAxis;

    private void Awake()
    {
        transform.rotation = transform.rotation.MakeRandom(_lockAxis);
    }
}
