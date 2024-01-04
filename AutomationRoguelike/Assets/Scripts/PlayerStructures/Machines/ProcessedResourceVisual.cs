using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProcessedResourceVisual : MonoBehaviour
{
    private ConveyerMachine _conveyerBelt;
    private int _beltIndex = 0;
    private Vector3 _target;
    [SerializeField] private float _speed;
    // Start is called before the first frame update
    void Start()
    {
        _conveyerBelt = transform.parent.GetComponent<ConveyerMachine>();
        SetMoveTarget();
    }
    private void SetMoveTarget()
    {
        if (_beltIndex >= _conveyerBelt.LinePosToForResources.Count)
        {
            Destroy(this.gameObject);
            return;
        }
        _target = _conveyerBelt.LinePosToForResources[_beltIndex];
        _beltIndex++;
    }
    
    private void MoveToNextBelt()
    {
        transform.position = Vector3.MoveTowards(transform.position, _target, _speed * Time.deltaTime);
        if (Vector3.Distance(_target, transform.position) <= 0.05)
        {
            SetMoveTarget();
        }
    }
    // Update is called once per frame
    void Update()
    {
        MoveToNextBelt();
    }
}
