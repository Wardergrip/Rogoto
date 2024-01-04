using System.Collections.Generic;
using UnityEngine;
using Trivial;
using System.Linq;
using Unity.VisualScripting;

public class EffectImproveBaseStatsAllOfType : Effect
{
	private struct TypeCombo
	{
		public string StructureType { get; set; }
		public string StatName { get; set; }
		public float Multiplier { get; set; }
	}
	// STATIC
	private static bool s_isInitialized = false;
	private int _rarityMultiplier = 1;
	private static readonly List<TypeCombo> s_persistantBuffs = new();
	private static void NewTurretSpawned(Turret turret)
	{
		ApplyBuffsOnNewlySpawned(turret.PlayerStructure);
	}

	private static void ApplyBuffsOnNewlySpawned(PlayerStructure playerStructure)
	{
		foreach (TypeCombo buff in s_persistantBuffs)
		{
			if (buff.StructureType == playerStructure.Name)
			{
				playerStructure.Stats
					.Where(x => x.GetName() == buff.StatName)
					.ToList()
					.ForEach(stat => stat.RootBuff(buff.Multiplier));
			}
		}
	}

	// NON-STATIC
	private string _type;
	private readonly List<Stat> _statsToBuff = new();

	public override void Excecute()
	{
		if (!IsAvailable())
		{
			Debug.LogError($"Effect not available!");
			return;
		}
		if (string.IsNullOrEmpty(_type))
		{
			Init((EffectRarity)(_rarityMultiplier - 1));
		}
		if (_statsToBuff.Count <= 0)
		{
			Debug.LogError("No stat to buff found");
		}
		_statsToBuff.ForEach(stat => stat.RootBuff(_rarityMultiplier));
		s_persistantBuffs.Add(
			new()
			{
				StructureType = _type,
				StatName = _statsToBuff[0].GetName(),
				Multiplier = _rarityMultiplier
			});
	}

	public override void Init(EffectRarity rarity)
	{
		if (rarity == EffectRarity.Common)
			_rarityMultiplier = 1;
		else if (rarity == EffectRarity.Rare)
			_rarityMultiplier = 2;
		else if (rarity == EffectRarity.Legendary)
			_rarityMultiplier = 3;
		// STATIC
		if (!s_isInitialized)
		{
			s_isInitialized = true;
			Turret.OnTurretSpawned += NewTurretSpawned;
			Base.OnStaticGameOver += () => { s_persistantBuffs.Clear(); };
		}

		// NON STATIC
		_statsToBuff.Clear();
		if (!IsAvailable())
		{
			return;
		}
		FindRandomType();

		List<PlayerStructure> structures = GetAvailableStructures();
		List<PlayerStructure> chosenStructures = structures
			.Where(x => x.Name == _type)
			.ToList();
		Stat chosenStat = chosenStructures[0].Stats
			.Where(x => x.IsBuffable)
			.ToList()
			.GetRandomValue();

		chosenStructures.ForEach(chosen => _statsToBuff
			.AddRange(chosen.Stats
			.Where(stat => stat.GetName() == chosenStat.GetName())
			.ToList()));
	}

	public override string OverrideDescription()
	{
		if (string.IsNullOrEmpty(_type))
		{
			return "No stat to buff found";
		}
		return $"Permanently improves\n {_statsToBuff[0].GetName()} of all\n {_type}s by\n {_statsToBuff[0].BuffValue*_rarityMultiplier}%.";
	}

	public override bool IsAvailable()
	{
		return true;

		// SAFE MODE (redundant since we always have 1 turret in hotbar)

		//List<PlayerStructure> turrets = GetAvailableStructures();
		//if (turrets.Count <= 0) return false;
		//return turrets.Any(x => x.Stats.Any(stat => stat.IsBuffable));
	}

	private void FindRandomType()
	{
		if (!IsAvailable())
		{
			_type = null;
			return;
		}
		List<PlayerStructure> structures = GetAvailableStructures();
		_type = structures
			.Where(x => 
				x.Stats.Any(stat => stat.IsBuffable))					// Filter out the ones that have buffable stats
			.Select(t => t.Name)										// Make a new containers of all the names
			.Distinct()													// Filter out the duplicates
			.ToList()													// Convert to list
			.GetRandomValue();											// Get random element of that list
	}

	private List<PlayerStructure> GetAvailableStructures()
	{
		List<PlayerStructure> structures = new();
		structures.AddRange(
			Turret.s_Turrets
			.Select(x => x.PlayerStructure)
			);
		structures.AddRange(
			Hotbar.s_Hotbars
			.Find(hotbar => hotbar.HotbarType == HotbarType.Combat)
			.HotbarSlots
			.Where(slot => slot.Blueprint != null)
			.Select(slot => slot.Blueprint.Structure)
		);
		return structures;
	}
}
