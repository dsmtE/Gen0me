using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int index = 0;

    void Start()
    {
        
    }

    void OnTriggerEnter(Collider oCollider)
    {
        var kartBrain = oCollider.transform.parent.gameObject.GetComponentInChildren<KartBrain>();
        if (kartBrain != null)
        {
            kartBrain.ValidateCheckpoint(index);
        }
    }
}
