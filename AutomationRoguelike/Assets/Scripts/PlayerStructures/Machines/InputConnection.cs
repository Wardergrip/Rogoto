using UnityEngine;
using UnityEngine.Events;

public class InputConnection : MonoBehaviour
{
	private OutputConnection _connectedOutput;
	[HideInInspector] public Machine ParentMachine;
	[SerializeField] BoxCollider _collider;
	private bool _validConnection;
    [SerializeField] GameObject _inputAttachment;

    public BoxCollider Collider { get => _collider;  }
	public OutputConnection ConnectedOutput { get => _connectedOutput; set => _connectedOutput = value; }
	public bool ValidConnection { get => _validConnection; set => _validConnection = value; }

	[SerializeField] private UnityEvent _onConnectionMade;
	[SerializeField] private UnityEvent _onConnectionLost;
	private void Awake()
	{
		ParentMachine = transform.parent.GetComponent<Machine>();
    }
    private void CheckIfShowAttachment()
    {
        if (ParentMachine.ProcessTag != ProcessingTag.Conveyer &&
            _connectedOutput.ParentMachine.ProcessTag != ProcessingTag.Conveyer)
        {
            _inputAttachment.SetActive(true);
        }
    }
    private void UnshowAttachment()
    {
        _inputAttachment.SetActive(false);
    }
    private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent<OutputConnection>(out OutputConnection connection))
		{
			_connectedOutput = connection;

			if (!CheckTags(connection))
			{
				_validConnection = false;
			}
			else
			{
				_validConnection = true;
				ParentMachine.ConnectionMade();
				_onConnectionMade.Invoke();
			}
		}
	}
	private bool CheckTags(OutputConnection connection)
	{
		if (ParentMachine.ProcessTag == ProcessingTag.Passthrough)
			return true;
		foreach (ProcessingTag tag in connection.ParentMachine.OutPutTags)
		{
			if (tag == ParentMachine.ProcessTag
				&& tag != ProcessingTag.Passthrough
                && tag != ProcessingTag.Conveyer)
			{
				return false;
			}
		}
		return true;
	}
	public bool UpdateConnectedMachine(Machine originUpdate)
	{
		if (ConnectedOutput != null)
		{
			ConnectedOutput.ParentMachine.UpdateMachineViaOutput(originUpdate);
			return true;
		}
		_onConnectionLost.Invoke();
		_validConnection = false;
		return false;
	}
}
