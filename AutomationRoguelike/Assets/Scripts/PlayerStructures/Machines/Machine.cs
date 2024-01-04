using System.Collections.Generic;
using UnityEngine;

public enum ProcessingTag
{
	Refined, Combined, Split, Depot, Passthrough, Multiplier, Cleanser, Duplicator, Conveyer
}
public class Machine : MonoBehaviour
{
	private static readonly List<Machine> s_machines = new();

	[SerializeField] protected InputConnection[] _inputs;
	[SerializeField] protected OutputConnection[] _outputs;
	[SerializeField] private ProcessingTag _processTag;
	[SerializeField] private GameObject _processedResourcePrefab;
	public GameObject ProcessedResource { get=> _processedResourcePrefab ; }
	protected readonly List<ProcessingTag> _outPutTags = new();

	public ProcessingTag ProcessTag { get => _processTag; }
	public List<ProcessingTag> OutPutTags { get => _outPutTags;  }
	public InputConnection[] Inputs { get => _inputs; }
	public OutputConnection[] Outputs { get => _outputs;  }

	protected virtual void Awake()
	{
		s_machines.Add(this);
		SetOutPutTags();
	}

	private void Start()
	{
		SetOutPutTags();
		//update colliders to trigger ontriggerenters
		//foreach (InputConnection input in _inputs)
		//{
		//    input.Collider.enabled = false;
		//    input.Collider.enabled = true;
		//}
		//foreach (OutputConnection output in _outputs)
		//{
		//    output.Collider.enabled = false;
		//    output.Collider.enabled = true;
		//}
		ProcessResource();

	}
	public void ConnectionMade()
	{
		SetOutPutTags();
		UpdateMachineViaOutput(null);
	}
	public static void SetConnectionVisibility(bool isVisible)
	{
		foreach (Machine machine in s_machines)
		{
			foreach (OutputConnection output in machine.Outputs)
			{
				output.ValueCounterVisibility = isVisible;
			}
		}
	}
	protected virtual void SetOutPutTags()
	{
		_outPutTags.Clear();

		foreach (InputConnection input in _inputs)
		{
			if (input.ConnectedOutput != null&&input.ValidConnection)
			{
				foreach (ProcessingTag tag in input.ConnectedOutput.ParentMachine.OutPutTags)
				{
					if (!_outPutTags.Contains(tag))
					{
						_outPutTags.Add(tag);
					}
				}
				
			}
		}
		_outPutTags.Add(ProcessTag);
	}
	public void UpdateMachineViaInput(Machine originUpdate)
	{
		SetOutPutTags();

		if (originUpdate == this)
			return;
		if (originUpdate == null)
		{
			originUpdate = this;
		}
		ProcessResource();
		foreach (OutputConnection output in _outputs)
		{
			output.UpdateConnectedMachine(originUpdate);
		}
	}
	public void UpdateMachineViaOutput(Machine originUpdate)
	{
		SetOutPutTags();
		if (originUpdate == this)
			return;
		if (originUpdate == null )
		{
			originUpdate = this;
		}
		
		bool anyInputConnected = false;
		foreach (InputConnection input in _inputs)
		{
			bool isConnected = input.UpdateConnectedMachine(originUpdate);
			if (isConnected)
				anyInputConnected = true;
		}
		if (!anyInputConnected)
		{
			UpdateMachineViaInput(null);
		}
	}
	protected virtual void ProcessResource()
	{
		foreach (OutputConnection output in _outputs)
		{
			
			output.SetValue(CalculateOutputResource());
		}
	}
	protected virtual int CalculateOutputResource()
	{
		return 0;
	}
	protected virtual void OnDestroy()
	{
		List<Machine> connectedMachines = new();
		foreach (OutputConnection output in _outputs)
		{
			if (output.ConnectedInput != null)
			{
				connectedMachines.Add(output.ConnectedInput.ParentMachine);
				output.ConnectedInput.ConnectedOutput = null;
				output.ValidConnection = false;
			}
			Destroy(output.gameObject);
		}
		foreach (InputConnection input in _inputs)
		{
			if (input.ConnectedOutput != null)
			{
				connectedMachines.Add(input.ConnectedOutput.ParentMachine);
				input.ConnectedOutput.ConnectedInput = null;
				input.ValidConnection = false;
			}
			
		}
		foreach (Machine machine in connectedMachines)
		{
			ConveyerMachine belt = machine as ConveyerMachine;
			if (belt != null)
			{
				belt.GetComponent<PlayerStructure>().StartDestroy();
			}
			else
			{
				machine.UpdateMachineViaOutput(this);
			}
		}
		s_machines.Remove(this);
	}
}
