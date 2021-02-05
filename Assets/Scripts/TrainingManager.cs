using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingManager : MonoBehaviour {

    public int entitiesNumber = 10;

    private KartBrain[] brainsList;

    [SerializeField]
    private GameObject kartPrefab;

    private GameObject[] spawnPoints;


    private void Awake() {
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        brainsList = new KartBrain[entitiesNumber];

        for(int i = 0; i < entitiesNumber; ++i) {
            GameObject go = Instantiate(kartPrefab, Vector3.zero, Quaternion.identity);
            go.transform.parent = gameObject.transform;
            brainsList[i] = go.GetComponentInChildren<KartBrain>();
        }
    }
    public void SpawnKart() {
        foreach (KartBrain kb in brainsList) {
            Transform sp = SelectRandomSpawnpoint();
            kb.kartController.Respawn(sp.position, sp.rotation);
        }  
    }

    private Transform SelectRandomSpawnpoint() {
        return spawnPoints[Random.Range(0, spawnPoints.Length)].transform;
    }

    private void Start() {
        SpawnKart();
    }
}
