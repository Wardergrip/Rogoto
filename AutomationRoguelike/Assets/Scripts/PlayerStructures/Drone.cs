using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Drone : MonoBehaviour
{
    private Transform _target;

    public Transform Target { get => _target; set => _target = value; }
    private void Update()
    {
        if (Target != null)
        {

        }
    }
    private void MoveToTarget()
    {

    }
}
