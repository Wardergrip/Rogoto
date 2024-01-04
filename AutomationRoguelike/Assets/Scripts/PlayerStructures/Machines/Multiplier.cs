using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multiplier : Machine
{
    protected override int CalculateOutputResource()
    {
        
        int value = 1;
        foreach (InputConnection input in _inputs)
        {
            if (input.ConnectedOutput != null && input.ValidConnection)
            {
                value *= input.ConnectedOutput.Value;
            }
            else
            {
                value = 0;
            }
        }
        // if no valid connections aka value still at 1 set value to 0;
        if (value == 1)
        {
            value = 0;
        }
        return value;
    }
}
