﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {

    [SerializeField] private Transform target;

    [Tooltip("Use scene's current setup for offset.")]
    public bool useInitial = false;

    [SerializeField] private Vector3 offsetCamPosition = Vector3.zero;
    [SerializeField] private Quaternion offsetCamRotation = Quaternion.identity;

    [SerializeField] private float translateSpeed;
    [SerializeField] private float rotationSpeed;

    void Start() {
        if (useInitial) {
            offsetCamPosition = transform.position - target.transform.position;
            offsetCamRotation = transform.rotation;
        }
    }

    private void FixedUpdate() {
        HandleTranslation();
        HandleRotation();
    }

    private void HandleTranslation() {
        var targetPosition = target.position + target.rotation * offsetCamPosition;
        transform.position = Vector3.Lerp(transform.position, targetPosition, translateSpeed * Time.fixedDeltaTime);
    }
    private void HandleRotation() {
        var direction = target.position - transform.position;
        var rotation = Quaternion.LookRotation(direction, Vector3.up);
        transform.rotation = Quaternion.Lerp(transform.rotation, rotation, rotationSpeed * Time.fixedDeltaTime) * offsetCamRotation;
    }
}
