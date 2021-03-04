using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIFitness
{
    private int[] validatedCheckpoints;
    private float malus = 0;

    public AIFitness(int nbCheckpoints)
    {
        validatedCheckpoints = new int[nbCheckpoints]; // Initialized to 0
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
