using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingManager : MonoBehaviour {

    public int entitiesNumber = 20;
	[Tooltip("number of cars that we should consider as elite and keep for the next generation")]
	public int elitKeepCount = 4;

	[SerializeField] private float mutationRate = 0.1f;
	[SerializeField] private float mutationStrength = 0.1f;
	[SerializeField, Tooltip("in seconds")] private float trainingTime = 10.0f;

	[SerializeField] private GameObject kartPrefab;

	private void Awake() {
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

        brainsList = new List<KartBrain>();

        for(int i = 0; i < entitiesNumber; ++i) {
            GameObject go = Instantiate(kartPrefab, Vector3.zero, Quaternion.identity);
            go.transform.parent = gameObject.transform;
            brainsList.Add(go.GetComponentInChildren<KartBrain>());
        }

		generation = 0;
	}

    private void Start() {
		genertionStartTime = Time.time;
        SpawnKart();
	}

    private void FixedUpdate() {
		if (enableTraining) {
			if (Time.time - genertionStartTime > trainingTime) {
				NewGeneration();
			}
		}
		ComputeBestFiness();
	}

	private void ComputeBestFiness() {
        foreach (KartBrain brain in brainsList) {
			bestFitness = Mathf.Max(bestFitness, brain.Fitness);
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

			brainsList[i].MutateModel(mutationRate, mutationStrength);
		}

		for (int i = 0; i < brainsList.Count; ++i) {
			brainsList[i].ResetFitness();
		}

		++generation;
		bestFitness = 0;
		genertionStartTime = Time.time;
		SpawnKart();
	}

	public string getInfos() {
		if (enableTraining) {
			return string.Format("Best Fitness: {0}\nGeneration: {1}\nTraining time: {2}/{3}",
			bestFitness.ToString("F2"), generation, (Time.time - genertionStartTime).ToString("F2"), trainingTime);
		}else {
			return string.Format("Best Fitness: {0}\nGeneration: {1}", bestFitness.ToString("F2"), generation);
        }
	}

	// setters
	public void EnableTraining(bool v) {
		enableTraining = v;
		NewGeneration();
	}
	public void SetMutationRate(float v) => mutationRate = v;
	public void SetMutationStrength(float v) => mutationStrength = v;
	public void SetTrainingTime(float v) => trainingTime = v;


	private int generation;
	private Transform SelectRandomSpawnpoint() => spawnPoints[Random.Range(0, spawnPoints.Length)].transform;
	private KartBrain ChooseParent() => brainsList[Random.Range(0, elitKeepCount)];

	private List<KartBrain> brainsList;
    private GameObject[] spawnPoints;
	private float genertionStartTime;

	private float bestFitness = 0;
	private bool enableTraining = true;
}
