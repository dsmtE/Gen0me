using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingManager : MonoBehaviour {

    public int entitiesNumber = 20;
	[Tooltip("number of cars that we should consider as elite and keep for the next generation")]
	public int elitKeepCount = 4;

	public float mutationRate = 0.1f;
	public float mutationStrength = 0.1f;

	[Tooltip("in seconds")] public float trainingTime = 10.0f;

	[SerializeField] private GameObject kartPrefab;

    private void Awake() {
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        brainsList = new List<KartBrain>(entitiesNumber);

        for(int i = 0; i < entitiesNumber; ++i) {
            GameObject go = Instantiate(kartPrefab, Vector3.zero, Quaternion.identity);
            go.transform.parent = gameObject.transform;
            brainsList[i] = go.GetComponentInChildren<KartBrain>();
        }

		generation = 0;
	}

    private void Start() {
		genertionStartTime = Time.time;
        SpawnKart();

	}

    private void FixedUpdate() {
		if (Time.time - genertionStartTime > trainingTime) {
			NewGeneration();
		}
	}

    private void SpawnKart() {
		foreach (KartBrain kb in brainsList) {
			Transform sp = SelectRandomSpawnpoint();
			kb.kartController.Respawn(sp.position, sp.rotation);
		}
	}

	public void NewGeneration() {

		// sort by fitness
		brainsList.Sort(KartBrain.CompareFitness);

		for (int i = elitKeepCount; i < brainsList.Count; i++) {
	
			KartBrain parent1 = ChooseParent();
			KartBrain parent2 = ChooseParent();

			brainsList[i].aiModel = AIModel.Crossover(parent1.aiModel, parent2.aiModel);
		}

        // Mutate all
        foreach (KartBrain brain in brainsList) {
			brain.MutateModel(mutationRate, mutationStrength);
		}

		++generation;

		genertionStartTime = Time.time;
		SpawnKart();
	}

	private Transform SelectRandomSpawnpoint() => spawnPoints[Random.Range(0, spawnPoints.Length)].transform;
	private KartBrain ChooseParent() => brainsList[Random.Range(0, elitKeepCount)];

	private int generation;
	private List<KartBrain> brainsList;
    private GameObject[] spawnPoints;
	private float genertionStartTime;
}
