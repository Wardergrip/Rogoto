using UnityEngine;

public class CHEATER : MonoBehaviour
{
#if UNITY_EDITOR
	[SerializeField] private KeyCode _input = KeyCode.M;
	[SerializeField] private int _goldToAdd = 100;

	private void Update()
	{
		if (Input.GetKeyDown(_input))
		{
			EconomyManager.Instance.AddGold(_goldToAdd);
		}
	}
#endif
}
