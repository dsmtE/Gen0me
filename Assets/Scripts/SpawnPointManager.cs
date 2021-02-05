using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointManager : MonoBehaviour {
    private SpawnPoint[] spawnPoints;

    void Awake() {
        spawnPoints = FindObjectsOfType<SpawnPoint>();
    } 
    public Vector3 SelectRandomSpawnpoint() {
        int rnd = Random.Range(0, spawnPoints.Length);
        return spawnPoints[rnd].transform.position;
    }
}
