﻿using System;
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

    private Vector3 sphereOffsetPosition;
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

    public void Awake() {
        sphereOffsetPosition = sphere.transform.localPosition;
    }

    public void ApplyAcceleration(float input) {
        if(input > 0f) {
            currentSpeed = Mathf.SmoothStep(currentSpeed, input * maxSpeed, Time.deltaTime * acceleration);
        }else { // brake
            currentSpeed = Mathf.SmoothStep(currentSpeed, 0f, Time.deltaTime * 12f * (1 + bakingForce * Mathf.Abs(input)) );
        }
    }

    private void AnimateKart(float input) {
        kartModel.localEulerAngles = new Vector3(0f, (input * 20f), 0f);
        frontWheels.localEulerAngles = new Vector3(0f, (input * 15f), frontWheels.localEulerAngles.z);
        frontWheels.localEulerAngles += new Vector3(0f, 0f, sphere.velocity.magnitude / 2f);
        backWheels.localEulerAngles += new Vector3(0f, 0f, sphere.velocity.magnitude / 2f);

        steeringWheel.localEulerAngles = new Vector3(-25, 90, ((input * 45)));
    }

    public void Respawn(Vector3 position, Quaternion rotation) {
        sphere.transform.position = position;
        transform.rotation = rotation;
    }
    void Update() {
        //Animations 
        AnimateKart(currentRotate / 20f);
    }

    public void FixedUpdate() {

        sphere.AddForce(-kartNormal.transform.right * currentSpeed, ForceMode.Acceleration);
        
        //Gravity
        sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

        //Follow Collider
        transform.position = sphere.transform.position - sphereOffsetPosition;

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
        currentRotate = Mathf.Lerp(currentRotate, steering * CorrectedTurn(rotate), Time.deltaTime * 4f);
    }

    public float getFitness() {
        return 0f;
    }
}