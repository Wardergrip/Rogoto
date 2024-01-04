using System;
using System.Collections;
using Trivial;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;


public class Base : MonoBehaviour
{
    public static Base Instance { get; private set; }

    [SerializeField] private Health _health;
    public Health Health { get => _health; }
    public UnityEvent OnBaseTakesDamage;
    public UnityEvent OnGameOver;

    public static event Action OnStaticGameOver;
    
    public static Vector3 Position { get; private set; }
    private void Awake()
    {
        Position = transform.position;
        if (Instance != null)
        {
            Debug.LogError($"Multiple instances of base detected");
        }
        Instance = this;
		CameraController.EnqueueCinematic(new(transform, 2f));
		StartCoroutine(WaitUntilNest(InitialCinematics));
	}

    private IEnumerator WaitUntilNest(Action nestAssigned)
    {
        yield return new WaitUntil(() => EnemyNest.s_CurrentTargetNest != null);
        nestAssigned.Invoke();
	}

    private void InitialCinematics()
    {
		MiningArea[] areas = FindObjectsOfType<MiningArea>();
        int index = 0;
        for (int i = 0; i < areas.Length; i++)
        {
			if (areas[i].transform.position != Vector3.zero)
			{
				index = i;
				break;
			}
		}
		GameObject go = new("[Base] CinematicHelper");
		go.transform.position = areas[index].MiddlePosition();
		DestroyTimer destroyTimer = go.AddComponent<DestroyTimer>();
		destroyTimer.Time = 20;
		destroyTimer.StartTimer();
		CameraController.EnqueueCinematic(new(go.transform, 1.5f));
		CameraController.EnqueueCinematic(new(go.transform, 1.5f, 5.0f));
	}

    private void NestCinematic()
    {
        if (EnemyNest.ActiveNests.Count <= 0)
        {
            return;
        }

        Debug.Assert(EnemyNest.s_PreviousNestPos.HasValue, $"no previous nestpos");
        Vector3 pos = EnemyNest.s_PreviousNestPos.Value;
		GameObject go = new("[Base] CinematicHelper");
        go.transform.position = pos;
		DestroyTimer destroyTimer = go.AddComponent<DestroyTimer>();
		destroyTimer.Time = 10;
		destroyTimer.StartTimer();

        CameraController.EnqueueCinematic(new(go.transform, 2.0f));
        CameraController.EnqueueCinematic(new(go.transform, 2.0f, 6.0f));
	}

    private void Start()
    {
        GameSystem.OnWaveStarted += SendOutDrones;
		GameSystem.OnNewCaveRevealed += NewCaveRevealed;
        _health.OnDied.AddListener(GameOver);
    }

	private void NewCaveRevealed()
	{
        StartCoroutine(WaitUntilNest(NestCinematic));
	}

	public void SendOutDrones()
    {
        //needs code here for when drones are implemented
        
        GameSystem.Instance.StartEnemySpawning();
    }
    private void OnTriggerEnter(Collider other)
    {
        CheckForEnemy(other);
    }
    private void GameOver()
    {
        OnGameOver.Invoke();
		OnStaticGameOver?.Invoke();
		Debug.LogWarning("YOU DIED (game over)");
        CameraController cam = Camera.main.transform.parent.parent.gameObject.GetComponent<CameraController>();
        cam.OnEndCinematicEvent.AddListener(() => 
        { 
            cam.DisableScript(); 
            GameSystem.Instance.ShowGameOverScreen();
        }
        );
        CameraController.EnqueueCinematic(new CinematicCommand(transform, 5.0f,4.0f));
        GameSystem.Instance.PauseGame();
    }
    private void CheckForEnemy(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();
            enemy.ReachDestroyablePlayerStructure();
            _health.Damage(enemy.Damage);
            OnBaseTakesDamage.Invoke();
        }
	}

    public void TakeDamage(int i)
    {
		_health.Damage(i);
		OnBaseTakesDamage.Invoke();
	}

    private void OnDestroy()
    {
        Instance = null;
		GameSystem.OnWaveStarted -= SendOutDrones;
		GameSystem.OnNewCaveRevealed -= NewCaveRevealed;
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(Base))]
public class Base_Editor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

		Base script = (Base)target;

        if (GUILayout.Button("Button that kills you"))
        {
            script.TakeDamage(100);
        }
    }
}
#endif