using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public int index = 0;

    void Start()
    {
        
    }

    void OnCollisionEnter(Collision collision)
    {
        var kartBrain = collision.transform.parent.gameObject.GetComponentInChildren<KartBrain>();
        if (kartBrain != null)
        {
            kartBrain.ValidateCheckpoint(index);
        }
    }
}
