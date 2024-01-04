using System.Linq;
using UnityEngine;

public class EffectResourceIncrease : Effect
{
	[SerializeField] private int _increaseAmount = 2;
	public override void Excecute()
	{
		foreach (var rs in ResourceSpot.s_ResourceSpots)
		{
			rs.ChangeValueUnoccupied(_increaseAmount);
		}
	}

	public override bool IsAvailable()
	{
		return ResourceSpot.s_ResourceSpots.Any( x=> !x.IsOccupied());
	}
}
