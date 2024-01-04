using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Duplicator : Machine
{
    protected override int CalculateOutputResource()
    {
        if (_inputs[0].ConnectedOutput != null && _inputs[0].ValidConnection)
        {
            return _inputs[0].ConnectedOutput.Value;
        }
        return 0;
    }
}
