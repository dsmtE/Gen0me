using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(KartController))]
public class KartKeyboardInput : MonoBehaviour {

	private float forward = 0;
	private float turn = 0;
	private bool brake = false;

    private KartController kart;

    void Awake() {
        kart = GetComponent<KartController>();
    }
	void Update() {
		forward = Input.GetAxis("Vertical");
		turn = Input.GetAxis("Horizontal");
		brake = Input.GetButton("Jump");

        kart.ApplyAcceleration(forward);
        kart.Steer(turn);
    }
}
