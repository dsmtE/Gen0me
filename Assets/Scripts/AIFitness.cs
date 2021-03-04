using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIFitness
{
    private int[] validatedCheckpoints;
    private float malus = 0;
    private int _nbCheckpoints;

    public AIFitness(int nbCheckpoints)
    {
        _nbCheckpoints = nbCheckpoints;
        Reset();
    }

    public void Reset()
    {
        validatedCheckpoints = new int[_nbCheckpoints]; // Initialized to 0
        malus = 0;
    }

    public void valideCheckpoint(int index)
    {
        if (index == 0)
        {
            if (validatedCheckpoints[validatedCheckpoints.Length - 1] == validatedCheckpoints[0])
                validatedCheckpoints[0]++;
            else
                malus += 1;
        }
        else
        {
            if (validatedCheckpoints[index-1] == validatedCheckpoints[index] + 1)
                validatedCheckpoints[index]++;
            else
                malus += 1;
        }
    }

   public float computeScore()
    {
        float score = 0;
        foreach (int nb_laps in validatedCheckpoints)
        {
            score += nb_laps;
        }
        return score - malus;
    }
}
