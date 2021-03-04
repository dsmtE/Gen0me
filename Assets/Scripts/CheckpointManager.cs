using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static int nbCheckpoints = 200;
    public GameObject checkpoint;
    public CircuitMesh circuitMesh;
    public PathCreation.PathCreator pathCreator;

    void Start()
    {
        for (int i = 0; i < nbCheckpoints; ++i)
        {
            var obj = Instantiate(checkpoint);
            obj.GetComponent<Checkpoint>().index = i;
            obj.transform.localScale = new Vector3(circuitMesh.roadWidth * 2.0f, 1, 1);
            obj.transform.localPosition = pathCreator.path.GetPointAtTime(i / (float)nbCheckpoints);
            obj.transform.rotation = Quaternion.FromToRotation(Vector3.right, pathCreator.path.GetNormalAtDistance(pathCreator.path.length * i / (float)nbCheckpoints));
        }
    }
}
