using UnityEngine;
using UnityEngine.InputSystem;

public class StartWaveProxy : MonoBehaviour
{
    public void Execute(InputAction.CallbackContext ctx)
    {
        if (ctx.performed)
            GameSystem.Instance.StartWave();
    }
}
