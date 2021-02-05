using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KartController : MonoBehaviour {
    public Transform kartModel;
    public Transform kartNormal;
    public Rigidbody sphere;

    public float maxSpeed = 20f;
    private float currentSpeed = 0;
    private float rotate;
    private float currentRotate;
    [Range(0f, 10f)]
    public float bakingForce = 1f;

    [Header("Parameters")]
    public float acceleration = 10f;
    public float steering = 10f;
    public float gravity = 10f;
    public LayerMask layerMask;

    [Header("Model Parts")]
    public Transform frontWheels;
    public Transform backWheels;
    public Transform steeringWheel;

    private float CorrectedTurn(float turn) { return turn * (1f - Mathf.Exp(-sphere.velocity.magnitude / 4f)); }

    public void ApplyAcceleration(float input) {
        if(input > 0f) {
            currentSpeed = Mathf.SmoothStep(currentSpeed, input * maxSpeed, Time.deltaTime * acceleration);
        }else { // brake
            currentSpeed = Mathf.SmoothStep(currentSpeed, 0f, Time.deltaTime * 12f * (1 + bakingForce * Mathf.Abs(input)) );
        }

        currentRotate = Mathf.Lerp(currentRotate, steering * CorrectedTurn(rotate), Time.deltaTime * 4f);
        rotate *= 0.8f;
    }

    private void AnimateKart(float input) {
        kartModel.localEulerAngles = Vector3.Lerp(kartModel.localEulerAngles, new Vector3(0, 90 + (input * 15), kartModel.localEulerAngles.z), .2f);

        frontWheels.localEulerAngles = new Vector3(0, (input * 15), frontWheels.localEulerAngles.z);
        frontWheels.localEulerAngles += new Vector3(0, 0, sphere.velocity.magnitude / 20);
        backWheels.localEulerAngles += new Vector3(0, 0, sphere.velocity.magnitude / 20);

        steeringWheel.localEulerAngles = new Vector3(-25, 90, ((input * 45)));
    }

    public void Respawn(Transform spawnPointTransform) {
        sphere.MovePosition(spawnPointTransform.position);
        sphere.MoveRotation(spawnPointTransform.rotation);
        transform.position = sphere.transform.position;
    }
    void Update() {
  
        //Animations 
        AnimateKart(CorrectedTurn(rotate));
    }

    public void FixedUpdate() {
        sphere.AddForce(-kartModel.transform.right * currentSpeed, ForceMode.Acceleration);
        
        //Gravity
        sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

        //Follow Collider
        transform.position = sphere.transform.position;

        //Steering
        transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0, transform.eulerAngles.y + currentRotate, 0), Time.deltaTime * 5f);

        Physics.Raycast(transform.position + (transform.up * .1f), Vector3.down, out RaycastHit hitOn, 1.1f, layerMask);
        Physics.Raycast(transform.position + (transform.up * .1f), Vector3.down, out RaycastHit hitNear, 2.0f, layerMask);

        //Normal Rotation
        kartNormal.up = Vector3.Lerp(kartNormal.up, hitNear.normal, Time.deltaTime * 8.0f);
        kartNormal.Rotate(0, transform.eulerAngles.y, 0);
    }

    public void Steer(float steeringSignal) {
        rotate = steeringSignal;
    }

    public float getFitness() {
        return 0f;
    }
}