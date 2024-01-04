using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConveyerMachine : Machine
{
    private List<Vector3> _linePosToForResources = new();
    [SerializeField] private float _timeBetweenResourceSpawn;
    private float _beltHeightOffset = 0.2f;
    [SerializeField] private ConveyerBelt[] _belts;

    public List<Vector3> LinePosToForResources { get => _linePosToForResources;}

    protected override void Awake()
    {
        base.Awake();
        GetComponent<PlayerStructure>().OnStartDestroy.AddListener(DestroyAnimations);
        StartCoroutine(SetBeltLineForResource());
    }
    public void SetUp(Vector3 startPos, Vector3 endPos)
    {
        _belts = transform.GetComponentsInChildren<ConveyerBelt>();
        _inputs[0].transform.position = startPos;
        StartCoroutine(SetOutputPos(endPos));
        StartCoroutine(AnimateBelts());
    }

    private IEnumerator AnimateBelts()
    {
        foreach (ConveyerBelt belt in _belts)
        {
            belt.Animator.SetTrigger("Place");
            yield return new WaitForSeconds(0.1f);
        }
    }
    private void DestroyAnimations()
    {
        foreach (ConveyerBelt belt in _belts)
        {
            belt.Animator.SetTrigger("Destroy");
        }
    }
    private IEnumerator SetOutputPos(Vector3 pos)
    {
     //https://docs.unity3d.com/Manual/ExecutionOrder.html 
     // wait for 2 frames so it can run the input triggers first before running the output 
    
        yield return null;
        yield return null;
        _outputs[0].transform.position = pos;
        
        _outputs[0].ResetCounterPos();
    }
    private IEnumerator SetBeltLineForResource()
    {
        yield return new WaitForSeconds(0.5f);
        _linePosToForResources.Add(_inputs[0].transform.position+ new Vector3(0,_beltHeightOffset,0));
        foreach (ConveyerBelt belt in _belts)
        {
            _linePosToForResources.Add(belt.transform.position + new Vector3(0, _beltHeightOffset, 0));
        }
        _linePosToForResources.Add(_outputs[0].transform.position + new Vector3(0, _beltHeightOffset, 0));

        StartCoroutine(SpawnProcessedResources());
        _inputs[0].transform.LookAt(_belts[0].transform);
        _outputs[0].transform.LookAt(_belts[_belts.Length-1].transform);

    }
    private IEnumerator SpawnProcessedResources()
    {
        while (true)
        {
            yield return new WaitForSeconds(_timeBetweenResourceSpawn);
            Instantiate(_inputs[0].ConnectedOutput.ParentMachine.ProcessedResource, _linePosToForResources[0], Quaternion.identity, transform);
        }
    }
    protected override int CalculateOutputResource()
    {
        if (_inputs[0].ConnectedOutput != null&&_inputs[0].ValidConnection)
        {
            return _inputs[0].ConnectedOutput.Value;
        }
        return 0;
    }

    
}
