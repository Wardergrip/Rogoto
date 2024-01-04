using UnityEngine;

//This exists because communicating with the terrain generator is more trouble than it's worth sorry
public class OreHighlight : MonoBehaviour
{
	private void OnEnable()
	{
		foreach (var resourceSpot in ResourceSpot.s_ResourceSpots)
		{
			resourceSpot.showCanvas(true);
		}
	}

	private void OnDisable()
	{
		foreach (var resourceSpot in ResourceSpot.s_ResourceSpots)
		{
			resourceSpot.showCanvas(false);
		}
	}
}
