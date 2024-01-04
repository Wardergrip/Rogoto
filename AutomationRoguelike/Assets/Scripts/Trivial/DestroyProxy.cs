using UnityEngine;

public class DestroyProxy : MonoBehaviour
{
    public void Destroy()
    {
        Destroy(gameObject);
    }
}
