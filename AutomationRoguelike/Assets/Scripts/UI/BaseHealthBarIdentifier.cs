using UnityEngine;

public class BaseHealthBarIdentifier : MonoBehaviour
{
    [SerializeField] private HealthBar _healthBar;
    public HealthBar HealthBar { get => _healthBar; }

    public static BaseHealthBarIdentifier Instance { get; private set; }

    private void Awake()
    {
        if (Instance !=  null)
        {
            Debug.LogError($"More than one  instances detected in BaseHealthBarIdentifier");
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        Instance = null;    
    }
}
