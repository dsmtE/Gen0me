using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIFitness
{
    private int prevCheckpoint = -1;
    private float score = 0;

    public void Reset()
    {
        prevCheckpoint = -1;
        score = 0;
    }

    public void valideCheckpoint(int index)
    {
        if (
            prevCheckpoint == -1 ||
            prevCheckpoint == (index - 1 + CheckpointManager.nbCheckpoints) % CheckpointManager.nbCheckpoints
         )
        {
            score += 1;
            prevCheckpoint = index;
        }
        else
        {
            score -= 1;
        }
    }

   public float computeScore()
    {
        return score;
    }
}
