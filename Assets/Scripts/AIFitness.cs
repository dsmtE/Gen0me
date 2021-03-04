using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIFitness
{
    private bool[] validatedCheckpoints;

    public AIFitness(int nbCheckpoints)
    {
        validatedCheckpoints = new bool[nbCheckpoints]; // Initialized to false
    }

    public void valideCheckpoint(int index)
    {
        validatedCheckpoints[index] = true;
    }

   public float computeScore()
    {
        float score = 0;
        foreach (bool b in validatedCheckpoints)
        {
            if (b)
            {
                score += 1;
            }
        }
        return score;
    }
}
