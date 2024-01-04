using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Refiner : Machine
{
    protected override int CalculateOutputResource()
    {
        if(_inputs[0].ConnectedOutput!=null && _inputs[0].ValidConnection)
             return _inputs[0].ConnectedOutput.Value * 3;
        return 0;
    }
}
