using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(KartController)), RequireComponent(typeof(RayCastSensors))]
public class KartBrain : MonoBehaviour {

    [HideInInspector]
    public KartController kartController;
    public RayCastSensors rayCastSensors;

    private void Awake() {
        kartController = GetComponent<KartController>();
        rayCastSensors = GetComponent<RayCastSensors>();
        aiModel = new AIModel();
    }

    private void Update() {
        var hits = rayCastSensors.GetHitInformations();
        var output = aiModel.eval(hits.Select(hit => hit.distance).ToArray());
        kartController.ApplyAcceleration(output[0]);
        kartController.Steer(output[1]);
    }

    private AIModel aiModel;
}
