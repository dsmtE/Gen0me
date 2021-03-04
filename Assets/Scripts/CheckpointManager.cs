using UnityEngine;

public class CheckpointManager : MonoBehaviour {
    public static int nbCheckpoints = 100;
    public GameObject checkpointPrefab;
    public CircuitMesh circuitMesh;
    public PathCreation.PathCreator pathCreator;


    void Start() {
        for (int i = 0; i < nbCheckpoints; ++i) {
            GameObject obj = Instantiate(checkpointPrefab);
            obj.transform.parent = gameObject.transform;
            obj.GetComponent<Checkpoint>().index = i;
            obj.transform.localScale = new Vector3(circuitMesh.roadWidth * 3.0f, 1, 1);
            obj.transform.localPosition = pathCreator.path.GetPointAtTime(i / (float)nbCheckpoints);
            obj.transform.rotation = Quaternion.FromToRotation(Vector3.right, pathCreator.path.GetNormalAtDistance(pathCreator.path.length * i / (float)nbCheckpoints));
        }
    }
}
