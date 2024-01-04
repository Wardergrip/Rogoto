using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class OutputConnection : MonoBehaviour
{
    [HideInInspector]  public Machine ParentMachine;
    private bool _validConnection;
    private InputConnection _connectedInput;
    [SerializeField] BoxCollider _collider;
    [SerializeField] GameObject _outputAttachment;
    
    private TextMeshProUGUI _valueCounter;
    private int _value;
    [SerializeField] private GameObject _valueCounterPreFab;
    private GameObject _valuecounterObject;
    public int Value { get => _value;  }
    public BoxCollider Collider { get => _collider;  }
    public InputConnection ConnectedInput { get => _connectedInput; set => _connectedInput = value; }

    public bool ValueCounterVisibility { get => _valuecounterObject.activeInHierarchy; set => _valuecounterObject.SetActive(value); }
    public bool ValidConnection { get => _validConnection; set => _validConnection = value; }

    [SerializeField] private UnityEvent _onConnectionMade;
    [SerializeField] private UnityEvent _onConnectionLost;
    public void SetValue(int value)
    {
        RecheckTags();
        if (_connectedInput != null && !_validConnection)
        {
            SetInvalidConnection();
        }
        else
        {
            _value = value;
            _valueCounter.text = _value.ToString();
        }
        
    }
    private void RecheckTags()
    {
        if (_connectedInput == null)
            return;
        if (!CheckTags(_connectedInput))
        {
            SetInvalidConnection();
            _validConnection = false;
        }
        else
        {
            _validConnection = true;
        }
    }
    public void ResetCounterPos()
    {
        _valuecounterObject.transform.position = transform.position;
    }
    private IEnumerator EnableCollider()
    {
        yield return null;
        Collider.enabled = true;
    }
    private void CheckIfShowAttachment()
    {
        if(ParentMachine.ProcessTag!= ProcessingTag.Conveyer &&
            _connectedInput.ParentMachine.ProcessTag != ProcessingTag.Conveyer)
        {
            _outputAttachment.SetActive(true);
        }
    }
    private void UnshowAttachment()
    {
        _outputAttachment.SetActive(false);
    }

    private void Awake()
    {
        ParentMachine = transform.parent.GetComponent<Machine>();
        _valuecounterObject = Instantiate(_valueCounterPreFab, transform.position, Quaternion.identity);
        _valueCounter = _valuecounterObject.GetComponentInChildren<TextMeshProUGUI>();
        StartCoroutine(EnableCollider());
    }
    private void OnDestroy()
    {
        Destroy(_valuecounterObject.gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<InputConnection>(out InputConnection connection))
        {
            _connectedInput = connection;

            if (!CheckTags(connection))
            {
                SetInvalidConnection();
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
    
    private void SetInvalidConnection()
    {
        _valueCounter.text = "X";
    }
    private bool CheckTags(InputConnection connection)
    {
            foreach (ProcessingTag tagSelf in ParentMachine.OutPutTags)
            {
                if (tagSelf == _connectedInput.ParentMachine.ProcessTag
                && _connectedInput.ParentMachine.ProcessTag != ProcessingTag.Passthrough
                && _connectedInput.ParentMachine.ProcessTag != ProcessingTag.Conveyer)
                    return false;
            }
        return true;
    }
    public bool UpdateConnectedMachine(Machine originUpdate)
    {
        if (_connectedInput != null)
        {
            _connectedInput.ParentMachine.UpdateMachineViaInput(originUpdate);
            return true;
        }
        _onConnectionLost.Invoke();
        _validConnection = false;
        return false;
    }
}
