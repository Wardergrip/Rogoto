using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Combiner : Machine
{
    protected override int CalculateOutputResource()
    {
        int combinedOutput = 0;
        foreach (InputConnection input in _inputs)
        {
            if (input.ConnectedOutput != null&&input.ValidConnection)
            {
                combinedOutput += input.ConnectedOutput.Value;
            }
        }
        return combinedOutput;
    }
}
