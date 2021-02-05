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

            Transform sp = SelectRandomSpawnpoint();

            GameObject go = Instantiate(kartPrefab, sp.position, sp.rotation);
            go.transform.parent = transform;

            go.transform.localScale = Vector3.one;
            brainsList[i] = go.GetComponent<KartBrain>();
        }
    }
    public void SpawnKart() {
        foreach (KartBrain kb in brainsList) {
            kb.kartController.Respawn(SelectRandomSpawnpoint());
        }  
    }

    private Transform SelectRandomSpawnpoint() {
        return spawnPoints[Random.Range(0, spawnPoints.Length)].transform;
    }

    private void Start() {
        SpawnKart();
    }
}
