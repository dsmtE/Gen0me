using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static int nbCheckpoints = 20;
    public GameObject checkpoint;

    void Start()
    {
        for (int i = 0; i < nbCheckpoints; ++i)
        {
            var obj = Instantiate(checkpoint);
            obj.GetComponent<Checkpoint>().index = i;
        }
    }
}
