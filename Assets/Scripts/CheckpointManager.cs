using UnityEngine;

[RequireComponent(typeof(CircuitMesh))]
public class CheckpointManager : MonoBehaviour {
    public static int nbCheckpoints = 100;
    public GameObject checkpointPrefab;
    
    public Transform InstancesHolder;

    public void Awake() {
        circuitMesh = GetComponent<CircuitMesh>();
    }

    void Start() {
        for (int i = 0; i < nbCheckpoints; ++i) {
            GameObject obj = Instantiate(checkpointPrefab);
            obj.transform.parent = InstancesHolder;
            obj.GetComponent<Checkpoint>().index = i;
            obj.transform.localScale = new Vector3(circuitMesh.roadWidth * 3.0f, 1, 1);
            obj.transform.localPosition = circuitMesh.pathCreator.path.GetPointAtTime(i / (float)nbCheckpoints);
            obj.transform.rotation = Quaternion.FromToRotation(Vector3.right, circuitMesh.pathCreator.path.GetNormalAtDistance(circuitMesh.pathCreator.path.length * i / (float)nbCheckpoints));
        }
    }

    private CircuitMesh circuitMesh;
}
