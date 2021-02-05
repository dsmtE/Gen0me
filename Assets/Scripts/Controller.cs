using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controller : MonoBehaviour {

	public float forward = 0;
	public float turn = 0;
	public bool brake = false;

	void Update() {
		forward = Input.GetAxis("Vertical");
		turn = Input.GetAxis("Horizontal");
		brake = Input.GetButton("Jump");
	}
}
