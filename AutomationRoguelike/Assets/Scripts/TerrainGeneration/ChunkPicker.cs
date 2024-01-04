using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(LevelGenerator))]
public class ChunkPicker : MonoBehaviour
{
	private LevelGenerator _levelGenerator;
	public bool _canChoose = false;

	void Start()
	{
		_levelGenerator = GetComponent<LevelGenerator>();
	}

	private void Update()
	{
		if (Mouse.current.leftButton.wasPressedThisFrame && _canChoose)
		{
			Vector2 mousePosition = Mouse.current.position.ReadValue();
			Ray ray = Camera.main.ScreenPointToRay(mousePosition);
			if (ray.origin != null)
			{
				if (_levelGenerator.Choose(ray)) 
				{
					_canChoose = false;
				}
			}
		}
	}

	public void SetCanChoose(bool c)
	{
		_canChoose = c;
	}
}
