using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivationProxy : MonoBehaviour
{
    public void SetActivationStateTrue()
    {
        gameObject.SetActive(true);
    }
    public void SetActivationStateFalse()
    {
        gameObject.SetActive(false);
    }
}
