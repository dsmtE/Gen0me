using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RayCastSensors : MonoBehaviour {

    [Range(0, 50)]
    [Tooltip("Number of rays to the left and right of center.")]
    [SerializeField] private int raysPerDirection = 3;

    [Range(0, 180)]
    [Tooltip("Cone size for rays. Using 90 degrees will cast rays to the left and right. " +
    "Greater than 90 degrees will go backwards.")]
    [SerializeField] private float maxRayDegrees = 70;

    [Range(1, 1000)]
    [Tooltip("Length of the rays to cast.")]
    [SerializeField] private float rayLength = 20f;

    [Tooltip("Starting rays orientation")]
    [SerializeField] private Vector3 rayOrientation = Vector3.zero;

    [Tooltip("Starting offset position of the rays to cast.")]
    [SerializeField] private Vector3 startOffset = Vector3.zero;

    [Tooltip("Ending offset position of the rays to cast.")]
    [SerializeField] private Vector3 endOffset = Vector3.zero;

    [Tooltip("Controls which layers the rays can hit.")]
    [SerializeField] private LayerMask rayLayerMask = Physics.DefaultRaycastLayers;

    [Header("Debug Gizmos", order = 999)]
    [SerializeField] internal Color rayHitColor = Color.red;
    [SerializeField] internal Color rayMissColor = Color.white;
    [SerializeField] float displayHitSphereRadius = 0.5f;

    public struct HitInformation {
        public bool hasHit;
        public GameObject hitObject;
        public float distance;
        public Vector3 direction;
    }

    public int RaysNumber {
        get { return 2 * raysPerDirection + 1; }
        set {

            if(value >= 1) {
                if (value % 2 == 0) Debug.Log("RayNumber must be odd. (it has been rounded down)");
                raysPerDirection = (value - 1) / 2;
            }else {
                raysPerDirection = 0;
                Debug.Log("[Warning] RayNumber can't be less than one.");
            }
            Debug.LogFormat("raysPerDirection: {0} from value : {1}", raysPerDirection, value);
        }
    }

    private float[] GetRayAngles(int raysPerDirection, float maxRayDegrees) {
        // Example:
        // { 90, 90 - delta, 90 + delta, 90 - 2*delta, 90 + 2*delta }
        var anglesOut = new float[RaysNumber];
        var delta = maxRayDegrees / raysPerDirection;
        anglesOut[0] = 90f;
        for (var i = 0; i < raysPerDirection; i++) {
            anglesOut[2 * i + 1] = 90 - (i + 1) * delta;
            anglesOut[2 * i + 2] = 90 + (i + 1) * delta;
        }
        return anglesOut;
    }

    public HitInformation[] GetHitInformations() {
        HitInformation[] hitInformations = new HitInformation[RaysNumber];

        Vector3 startPos = transform.TransformPoint(startOffset);
        float[] raysAngles = GetRayAngles(raysPerDirection, maxRayDegrees);

        for (int i = 0; i < hitInformations.Length; ++i) {

            Vector3 endPos = startPos + transform.rotation * (Quaternion.Euler(rayOrientation) * Quaternion.AngleAxis(raysAngles[i], Vector3.up) * Vector3.forward) * rayLength + endOffset;

            Vector3 rayDirection = Vector3.Normalize(endPos - startPos);

            RaycastHit rayHit;
            bool hasHit = Physics.Raycast(startPos, rayDirection, out rayHit, rayLength, rayLayerMask);
            GameObject hitObject = hasHit ? rayHit.collider.gameObject : null;
            float distance = hasHit ? rayHit.distance : 10000f;

            hitInformations[i] = new HitInformation { hasHit = hasHit, hitObject = hitObject, distance = distance, direction = rayDirection };
        }

        return hitInformations;
    }

    void OnDrawGizmosSelected() {
        foreach (HitInformation hi in GetHitInformations()) {
            DrawRaycastGizmos(hi);
        }
    }

    void DrawRaycastGizmos(HitInformation hi, float alpha = 1.0f) {

        // Color color = Color.Lerp(rayHitColor, rayMissColor, Mathf.Pow(hi.distance / RayLength, 2f));
        Color color = hi.hasHit ? rayHitColor : rayMissColor;
        color.a *= alpha;
        Gizmos.color = color;
        Gizmos.DrawRay(transform.TransformPoint(startOffset), hi.direction * (hi.hasHit ? hi.distance : rayLength));

        // Draw the hit point as a sphere. If using rays to cast (0 radius), use a small sphere.
        if (hi.hasHit) {
            Gizmos.DrawWireSphere(transform.TransformPoint(startOffset) + hi.direction * hi.distance, displayHitSphereRadius);
        }
    }
}