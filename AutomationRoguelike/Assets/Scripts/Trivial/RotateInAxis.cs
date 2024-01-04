using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Trivial 
{ 

    /*
     DISCLAIMER: THIS IS A STINKY SCRIPT BY A STINKY ARTIST AND DOESNT REFLECT PROGRAMMER WORK


     */

    public enum Axis
    { 
        X,
        Y,
        Z
    }

    public class RotateInAxis : MonoBehaviour
    {
        [SerializeField] private Axis _targetAxis;
        [SerializeField] private float rotateSpeed = 100;

        private void Update()
        {

           switch (_targetAxis)
            {
                case Axis.X:
                    transform.Rotate(Vector3.right * rotateSpeed * Time.deltaTime);
                    break;
                case Axis.Y:
                    transform.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
                    break;
                case Axis.Z:
                    transform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
                    break;


            }
        }

    }
}
