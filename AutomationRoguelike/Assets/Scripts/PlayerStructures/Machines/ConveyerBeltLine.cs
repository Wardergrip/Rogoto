using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Trivial;
using System.Linq;

public class ConveyerBeltLine : MonoBehaviour
{
    private Vector3 _targetTile;
    private Vector3 _lastTargetTile;
    private GameObject _previousEndConnection;
    private List<ConveyerBelt> _belts = new();
    private List<ConveyerBelt> _obstructedBelts = new();
    [SerializeField] private ConveyerBelt _beltPrefab;
    private ConveyerBelt _lastSpawnedBelt;
    private bool _obstructed;
    private bool _connected=false;
    [SerializeField] Material _obstructedMat;
    [SerializeField] Material _notConnectedMat;
    [SerializeField] Material _connectedMat;
    [SerializeField] ConveyerMachine _machine;
    [SerializeField] LayerMask _mask;
    [SerializeField] LayerMask _maskMouse;
    private bool _lineHasBeenShortenedButStillObstructed;
    private ConveyerBelt _connectedEndBelt;
    [SerializeField] private GameObject _beltModelStraight;
    [SerializeField] private GameObject _beltModelCurveRight;
    [SerializeField] private Transform _gridCanvas;
    [SerializeField] private bool _logWarnings = false;
    private int _amountTriedFindNewSpot = 0;
    private int _stackOverflowPreventionLimit = 5;
    private bool _corrupted = false;
    private bool Connected
    {
        get => _connected;
        set
        {
            if (value != Connected)
            {
                _connected = value;
                SetMats();
            }
        }
    }
    private void UpdateConnectedModelUndo()
    {
      
        if (_connectedEndBelt!=null)
        {     
            Destroy(_connectedEndBelt.transform.GetChild(0).gameObject);
            GameObject model = Instantiate(_beltModelStraight, _connectedEndBelt.transform);
            _connectedEndBelt.SetUp(model);
            SetMats();
            _connectedEndBelt = null;
            _lastSpawnedBelt=_connectedEndBelt;
        }
    }
    
    private IEnumerator UpdateModelConnected()
    {
        yield return null;
        if (BuildHud.S_HoverConveyerEnd == null)
            yield break;
        Vector3 targetForward = BuildHud.S_HoverConveyerEnd.transform.up * -1;
        if (!IsParallel(targetForward))
        {
            SetCurvedModel(targetForward);
            SetMats();
            _connectedEndBelt = _lastSpawnedBelt;
        }
        
        
       
    }

    public void TryPlace()
    {
        if (Connected)
        {
            ConveyerMachine machine = Instantiate(_machine, transform.position, Quaternion.identity);
            foreach (ConveyerBelt belt in _belts)
            {
                belt.transform.parent = machine.transform;
                belt.ResetMaterial();
            }
            machine.SetUp(transform.position, BuildHud.S_HoverConveyerEnd.transform.position);
        }
        else
        {
            if (_logWarnings)
            {
                Debug.LogWarning("UnsuccesfulPlacement");
            }
        }
        Destroy(this.gameObject);
        return;
    }
    
    private void SpawnNewBelt(Vector3 pos)
    {
        ConveyerBelt belt = Instantiate(_beltPrefab, pos, Quaternion.identity,transform);
        _belts.Add(belt);
        SetBeltModel(belt);
        
        _lastSpawnedBelt = belt;
    }
    private void SetBeltModel(ConveyerBelt parent)
    {
        GameObject model;
        if (parent.Animator == null)
            return;
        model = Instantiate(_beltModelStraight, parent.Animator.transform);
        parent.SetUp(model);
        if (_lastSpawnedBelt == null)
        {
            Vector3 directionStart = parent.transform.position - transform.position;
            parent.transform.LookAt(parent.transform.position + directionStart);
            return;
        }
        Vector3 direction = parent.transform.position - _lastSpawnedBelt.transform.position;
        parent.transform.LookAt(parent.transform.position + direction);
        if (!IsParallel(parent.transform.forward))
        {
            SetCurvedModel(parent.transform.forward);
        }

    }
    // Im sorry this is ultra super sloppy 4 gigamaxed :)
    private void SetCurvedModel(Vector3 targetForward)
    {
        if (_lastSpawnedBelt.Animator == null)
            return;
        GameObject model;
        bool newlyPlacedIsToRight = true;
        if (Vector3.Dot(_lastSpawnedBelt.transform.right, targetForward) < 0)
            newlyPlacedIsToRight = false;
        Destroy(_lastSpawnedBelt.Animator.transform.GetChild(0).gameObject);
        model = Instantiate(_beltModelCurveRight, _lastSpawnedBelt.Animator.transform);
        _lastSpawnedBelt.SetUp(model);
        if (!newlyPlacedIsToRight)
        {
            model.transform.localScale = new Vector3(-1, 1, 1);

        }
    }

    private bool IsParallel(Vector3 forward)
    {
        if (Vector3.Angle(forward,_lastSpawnedBelt.transform.forward) > Mathf.Epsilon)
        {
            return false;
        }
        else
            return true;
             
    }
    private void CheckNewPos()
    {
        Vector3? nextPos = GetNextPosToPlace();
        if (!nextPos.HasValue||_corrupted)
            return;
        if (_gridCanvas)
        {
            _gridCanvas.position = nextPos.Value;
        }
        _lineHasBeenShortenedButStillObstructed = false;
        bool isHitBack = Physics.Raycast(nextPos.Value+Vector3.up*2, transform.up * -1, out RaycastHit hit, 1.5f,_mask);
        bool isObstructed=false;
        if (isHitBack)
        {
           isObstructed= CheckHit(hit);
            Debug.DrawLine(Camera.main.transform.position, hit.point);
        }
        
        SpawnNewBelt(nextPos.Value);
        if (isObstructed||_lineHasBeenShortenedButStillObstructed)
        {
            _obstructedBelts.Add(_lastSpawnedBelt);
            _obstructed = true;
        }
        SetMats();
        if (nextPos != _targetTile)
        {
            CheckNewPos();
        }
        else
        {
            _amountTriedFindNewSpot = 0;
        }
        
    }
    private void SetMats()
    {
        if (Connected)
        {
            foreach (ConveyerBelt belt in _belts)
            {
                belt.SetMaterial(_connectedMat);
            }
        }
        else if (_obstructed)
        {
            foreach (ConveyerBelt belt in _belts)
            {
                belt.SetMaterial(_obstructedMat);
            }
        }
        else
        {
            foreach (ConveyerBelt belt in _belts)
            {
                belt.SetMaterial(_notConnectedMat);
            }
        }
    }
    private bool CheckHit(RaycastHit hit)
    {
        if (hit.transform.TryGetComponent(out ConveyerBelt belt))
        {
            if (_belts.Contains(belt))
            {
                int index = _belts.IndexOf(belt);
                if (_obstructedBelts.Contains(belt))
                {
                    _lineHasBeenShortenedButStillObstructed = true;
                }
                int startCount = _belts.Count;
                for (int i = 0; i < startCount - index; i++)
                {
                    int newI = startCount - i - 1;
                    Destroy(_belts[newI].gameObject);
                    _belts.Remove(_belts[newI]);
                }
                if (index != 0)
                    _lastSpawnedBelt = _belts[_belts.Count - 1];
                else
                {
                    _lastSpawnedBelt = null;

                }

                CheckIfRemainingObstructions();
                return false;
            }
            
        }
        return true;
    }

    private void CheckIfRemainingObstructions()
    {
        bool noMoreObstructions = false;
        foreach (ConveyerBelt obstrBelt in _obstructedBelts)
        {
            if (_belts.Contains(obstrBelt))
            {
                noMoreObstructions = false;
                break;
            }
            else
            {
                noMoreObstructions = true;
            }
        }
        if (noMoreObstructions)
        {
            _obstructedBelts.Clear();
            _obstructed = false;
            SetMats();
        }
    }

    private Vector3? GetNextPosToPlace()
    {
        SafetyCheckTarget();
        _amountTriedFindNewSpot++;
        if (_amountTriedFindNewSpot >= _stackOverflowPreventionLimit)
        {
            if (_logWarnings)
            {
                Debug.LogWarning("Conveyer belt stack overflow prevention activated");
            }
            BuildHud.ResetHoverConveyerConnections();
            Destroy(this.gameObject);
            _corrupted = true;
            this.enabled = false;
            return null;
        }
        Vector3 lastPlaced;
        if (_lastSpawnedBelt == null)
        {
            lastPlaced = SnapToGrid(transform.position);
            return lastPlaced;
        }
        else
        {
            lastPlaced = _lastSpawnedBelt.transform.position;

        }
        if (lastPlaced == _targetTile)
        {
            return _targetTile;
        }
        var neighbourTiles = new Vector3[]
        {
            lastPlaced + Vector3.back, lastPlaced+Vector3.forward,
            lastPlaced+ Vector3.left, lastPlaced + Vector3.right
        }.Where(tile => tile != SnapToGrid(transform.position) - transform.forward);
        if (BuildHud.S_HoverConveyerEnd != null
            && SnapToGrid((BuildHud.S_HoverConveyerEnd.transform.position) - BuildHud.S_HoverConveyerEnd.transform.up)!= _targetTile)
        {
           neighbourTiles=  neighbourTiles.Where(tile => tile !=
         SnapToGrid(BuildHud.S_HoverConveyerEnd.transform.position) - BuildHud.S_HoverConveyerEnd.transform.up );
            
        }
        return neighbourTiles.ToArray().FindClosest(_targetTile);
    }
    private void Update()
    {
        if (_corrupted)
            return;
        SetTarget();
        CheckConnection();
    }
    private void CheckConnection()
    {
        
        if (BuildHud.S_HoverConveyerEnd != null && !_obstructed )
        {
            if (_previousEndConnection != null&& BuildHud.S_HoverConveyerEnd != _previousEndConnection)
            {
                UpdateConnectedModelUndo();
            }
            if (Connected != true|| BuildHud.S_HoverConveyerEnd!= _previousEndConnection)
            {
               StartCoroutine(UpdateModelConnected());
            }
            Connected = true;
            _previousEndConnection = BuildHud.S_HoverConveyerEnd;
        }
        else 
        {
            Connected = false;
            if (_previousEndConnection != null)
            {
               UpdateConnectedModelUndo();
                _previousEndConnection = null;
            }
        }
    }
    private void SetTarget()
    {
        if (BuildHud.S_HoverConveyerEnd != null)
        {
            _targetTile = SnapToGrid(BuildHud.S_HoverConveyerEnd.transform.position);
            if (BuildHud.S_HoverConveyerEnd != _previousEndConnection)
                CheckNewPos();
            return;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100,_maskMouse))
        {
            _targetTile = SnapToGrid(hit.point);
            if (_targetTile != _lastTargetTile&&_targetTile!=SnapToGrid(transform.position)-transform.forward)
            {
                CheckNewPos();
            }
            _lastTargetTile = _targetTile;
        }
    }
    private void SafetyCheckTarget()
    {
        if (BuildHud.S_HoverConveyerEnd != null)
        {
            if(_targetTile == SnapToGrid(BuildHud.S_HoverConveyerEnd.transform.position) - BuildHud.S_HoverConveyerEnd.transform.up)
            {
                _targetTile = SnapToGrid(transform.position);
            }
        }
    }
    private Vector3 SnapToGrid(Vector3 position)
    {
        Vector3 snapped = new Vector3(Mathf.Round(position.x), Mathf.Round(position.y), Mathf.Round(position.z));
        Vector3 clamped = new Vector3(snapped.x, Mathf.Clamp(snapped.y, 0, 0), snapped.z);
        return clamped;
    }

}
