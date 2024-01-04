using UnityEngine;

public class LookAtMousePos : MonoBehaviour
{
	[SerializeField] int _lookAtDistance = 500;
	void Update()
	{
		Vector3 mousePos = (Input.mousePosition -transform.position - transform.position) * -1;
		transform.LookAt(new Vector3(mousePos.x,mousePos.y,_lookAtDistance));
	}
}
