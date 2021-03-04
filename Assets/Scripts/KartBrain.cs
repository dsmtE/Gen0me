using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(KartController)), RequireComponent(typeof(RayCastSensors))]
public class KartBrain : MonoBehaviour {

    [HideInInspector] public KartController kartController;

    public AIModel aiModel;
    public float Fitness => aiFitness.computeScore();
    public void ResetFitness() => aiFitness.Reset();

    private void Awake() {
        kartController = GetComponent<KartController>();
        rayCastSensors = GetComponent<RayCastSensors>();
        rayCastSensors.RaysNumber = 2*6+1;
        int[] layersDim = new int[] { rayCastSensors.RaysNumber, 10, 5, 2 };
        aiModel = new AIModel(layersDim);
        aiFitness = new AIFitness(CheckpointManager.nbCheckpoints);
    }

    private void Update() {
        var hits = rayCastSensors.GetHitInformations();
        var output = aiModel.eval(hits.Select(hit => hit.distance).ToArray());
        kartController.ApplyAcceleration(output[0]);
        kartController.Steer(output[1] * 2.0f - 1.0f);
    }

    public void ValidateCheckpoint(int checkpointIdx) {
        aiFitness.valideCheckpoint(checkpointIdx);
    }

#if UNITY_EDITOR
    void OnDrawGizmos() {
        UnityEditor.Handles.color = Color.white;
        UnityEditor.Handles.Label(transform.position + 1f * Vector3.up, "F: " + Fitness.ToString());
    }
#endif

    public void MutateModel(float mutationRate = 0.1f, float mutationStrength = 0.1f) => aiModel.Mutate(mutationRate, mutationStrength);

    public static int CompareFitness(KartBrain a, KartBrain b) => (a.Fitness > b.Fitness) ? 1 : ((a.Fitness < b.Fitness) ? -1 : 0);

    private RayCastSensors rayCastSensors;
    private AIFitness aiFitness;
}
