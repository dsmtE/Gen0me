using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastSensors : MonoBehaviour {

    [Range(0, 50)]
    [Tooltip("Number of rays to the left and right of center.")]
    [SerializeField] private int raysPerDirection = 3;
    public int RaysPerDirection {
        get => raysPerDirection;
        set { raysPerDirection = value; UpdateHitInformations(); }
    }

    [Range(0, 180)]
    [Tooltip("Cone size for rays. Using 90 degrees will cast rays to the left and right. " +
    "Greater than 90 degrees will go backwards.")]
    [SerializeField] private float maxRayDegrees = 70;
    public float MaxRayDegrees {
        get => maxRayDegrees;
        set { maxRayDegrees = value; UpdateHitInformations(); }
    }

    [Range(1, 1000)]
    [Tooltip("Length of the rays to cast.")]
    [SerializeField] float rayLength = 20f;
    public float RayLength {
        get => rayLength;
        set { rayLength = value; UpdateHitInformations(); }
    }

    [Tooltip("Starting rays orientation")]
    [SerializeField] Vector3 rayOrientation = Vector3.zero;
    public Vector3 RayOrientation {
        get => rayOrientation;
        set { rayOrientation = value; UpdateHitInformations(); }
    }

    [Tooltip("Starting offset position of the rays to cast.")]
    [SerializeField] Vector3 startOffset = Vector3.zero;
    public Vector3 StartOffset {
        get => startOffset;
        set { startOffset = value; UpdateHitInformations(); }
    }

    [Tooltip("Ending offset position of the rays to cast.")]
    [SerializeField] Vector3 endOffset = Vector3.zero;
    public Vector3 EndOffset {
        get => endOffset;
        set { endOffset = value; UpdateHitInformations(); }
    }

    [Tooltip("Controls which layers the rays can hit.")]
    [SerializeField] LayerMask rayLayerMask = Physics.DefaultRaycastLayers;
    public LayerMask RayLayerMask {
        get => rayLayerMask;
        set { rayLayerMask = value; UpdateHitInformations(); }
    }

    [Header("Debug Gizmos", order = 999)]
    [SerializeField] internal Color rayHitColor = Color.red;
    [SerializeField] internal Color rayMissColor = Color.white;
    [SerializeField] float displayHitSphereRadius = 0.5f;

    public struct HitInformation {
        public bool hasHit;
        public GameObject hitObject;
        public float distance;
        public Vector3 direction; // non normalized
    }

    HitInformation[] hitInformations = null;
    private bool updatedInformations = false;

    private float[] GetRayAngles(int raysPerDirection, float maxRayDegrees) {
        // Example:
        // { 90, 90 - delta, 90 + delta, 90 - 2*delta, 90 + 2*delta }
        var anglesOut = new float[2 * raysPerDirection + 1];
        var delta = maxRayDegrees / raysPerDirection;
        anglesOut[0] = 90f;
        for (var i = 0; i < raysPerDirection; i++) {
            anglesOut[2 * i + 1] = 90 - (i + 1) * delta;
            anglesOut[2 * i + 2] = 90 + (i + 1) * delta;
        }
        return anglesOut;
    }

    public HitInformation[] GetHitInformations() {
        if (updatedInformations) {
            return hitInformations;
        } else {
            return UpdateHitInformations();
        }
    }
    private HitInformation[] UpdateHitInformations() {
        // change size if needed
        if (hitInformations == null || hitInformations.Length != 2 * RaysPerDirection + 1) {
            hitInformations = new HitInformation[2 * RaysPerDirection + 1];
        }

        Vector3 startPos = transform.TransformPoint(StartOffset);
        float[] raysAngles = GetRayAngles(RaysPerDirection, MaxRayDegrees);

        for (int i = 0; i < hitInformations.Length; ++i) {

            Vector3 endPos = startPos + transform.rotation * (Quaternion.Euler(RayOrientation) * Quaternion.AngleAxis(raysAngles[i], Vector3.up) * Vector3.forward) * RayLength + EndOffset;

            Vector3 rayDirection = Vector3.Normalize(endPos - startPos);

            RaycastHit rayHit;
            bool hasHit = Physics.Raycast(startPos, rayDirection, out rayHit, RayLength, RayLayerMask);
            GameObject hitObject = hasHit ? rayHit.collider.gameObject : null;
            float distance = hasHit ? rayHit.distance : 10000f;

            hitInformations[i] = new HitInformation { hasHit = hasHit, hitObject = hitObject, distance = distance, direction = rayDirection };
        }
        updatedInformations = true;
        return hitInformations;
    }

    void OnDrawGizmosSelected() {
        foreach (HitInformation hi in UpdateHitInformations()) {
            DrawRaycastGizmos(hi);
        }
    }

    void DrawRaycastGizmos(HitInformation hi, float alpha = 1.0f) {

        // Color color = Color.Lerp(rayHitColor, rayMissColor, Mathf.Pow(hi.distance / RayLength, 2f));
        Color color = hi.hasHit ? rayHitColor : rayMissColor;
        color.a *= alpha;
        Gizmos.color = color;
        Gizmos.DrawRay(transform.TransformPoint(StartOffset), hi.direction * (hi.hasHit ? hi.distance : rayLength));

        // Draw the hit point as a sphere. If using rays to cast (0 radius), use a small sphere.
        if (hi.hasHit) {
            Gizmos.DrawWireSphere(transform.TransformPoint(StartOffset) + hi.direction * hi.distance, displayHitSphereRadius);
        }
    }
}