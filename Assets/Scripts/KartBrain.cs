using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(KartController)), RequireComponent(typeof(RayCastSensors))]
public class KartBrain : MonoBehaviour {

    [HideInInspector]
    public KartController kartController;
    private RayCastSensors rayCastSensors;

    private AIModel aiModel;
    private AIFitness aiFitness;

    private void Awake() {
        kartController = GetComponent<KartController>();
        rayCastSensors = GetComponent<RayCastSensors>();
        rayCastSensors.RaysNumber = 3;
        int[] layersDim = new int[] { rayCastSensors.RaysNumber, 4, 2 };
        aiModel = new AIModel(layersDim);
        aiFitness = new AIFitness(CheckpointManager.nbCheckpoints);
    }

    private void Update() {
        var hits = rayCastSensors.GetHitInformations();
        var output = aiModel.eval(hits.Select(hit => hit.distance).ToArray());
        kartController.ApplyAcceleration(output[0]*0.5f +0.5f);
        kartController.Steer(output[1]);
    }

    public void ValidateCheckpoint(int checkpointIdx)
    {
        aiFitness.valideCheckpoint(checkpointIdx);
    }
    
}
