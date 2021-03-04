using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FreeFlyCamControls : MonoBehaviour {

    Vector3 targetPosition;
    Quaternion targetRotation;

    [SerializeField] private float translationSpeed = 30.0f;
    [SerializeField] private float rotationSpeed = 30.0f;
    [SerializeField] private float zoomSpeed = 50.0f;

    [SerializeField] private float translationLerpFactor = 0.3f;
    [SerializeField] private float rotationLerpFactor = 0.6f;

    private void Awake() {
        targetPosition = transform.position;
        targetRotation = transform.rotation;
    }

    void Update() {

        // zoom using scrool
        var zoomDelta = zoomSpeed * Input.GetAxis("Mouse ScrollWheel");
        if (zoomDelta != 0) targetPosition -= transform.forward * zoomDelta * Time.deltaTime;

        // move using keyboard
        // targetPosition += new Vector3(Input.GetAxis("Horizontal") * translationSpeed * Time.deltaTime, 0.0f, Input.GetAxis("Vertical") * translationSpeed * Time.deltaTime);
        targetPosition += Input.GetAxis("Horizontal") * transform.right * translationSpeed * Time.deltaTime;
        targetPosition += Input.GetAxis("Vertical") * transform.forward * translationSpeed * Time.deltaTime;
        targetPosition += (Input.GetKey(KeyCode.Space) ? 1.0f : 0.0f) * Vector3.up * translationSpeed * Time.deltaTime;
        targetPosition += (Input.GetKey(KeyCode.LeftShift) ? 1.0f : 0.0f) * -Vector3.up * translationSpeed * Time.deltaTime;


        float mouseX = Input.mousePosition.x / Screen.width * 2.0f - 1.0f;
        float mouseY = Input.mousePosition.y / Screen.height * 2.0f - 1.0f;

        // Move camera with mouse
        if (Input.GetMouseButton(2)) {
            targetRotation = Quaternion.AngleAxis(mouseX * rotationSpeed * 1.4f * Time.deltaTime, Vector3.up) * targetRotation;
            targetRotation = Quaternion.AngleAxis(mouseY * rotationSpeed * Time.deltaTime, -transform.right) * targetRotation;
        }

        // apply target transform
        transform.position = Vector3.Lerp(transform.position, targetPosition, translationLerpFactor);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationLerpFactor);
    }
}