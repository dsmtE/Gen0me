using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Linq;

[System.Serializable]
public struct SaveState {
	public AIModel aiModel;

    public SaveState(AIModel aiModel) {
        this.aiModel = aiModel;
    }
}

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


	public void Save(string fileName) {

		brainsList.Sort(KartBrain.CompareFitness);

		SaveState saveState = new SaveState(brainsList[0].aiModel);

		BinaryFormatter bf = new BinaryFormatter();
		string layerDimStr = string.Join("_", saveState.aiModel.layersDim.Select(i => i.ToString()).ToArray());
		string fileRelativePath = "/" + fileName + "_" + layerDimStr + ".aiModel";
		FileStream file = File.Create(Application.persistentDataPath + fileRelativePath);
		Debug.LogFormat("saved to {0}", Application.persistentDataPath + fileRelativePath);
		bf.Serialize(file, saveState);
		file.Close();
	}

	public void Load(string fileName) {

		string fileRelativePath = "/" + fileName + ".aiModel";
		if (File.Exists(Application.persistentDataPath + fileRelativePath)) {
			BinaryFormatter bf = new BinaryFormatter();
			FileStream file = File.Open(Application.persistentDataPath + fileRelativePath, FileMode.Open);
			SaveState loaded = (SaveState)bf.Deserialize(file);
			Debug.LogFormat("loaded from {0}", Application.persistentDataPath + fileRelativePath);
			file.Close();

			for (int i = 0; i < brainsList.Count; i++) {
				brainsList[i].SetModel(loaded.aiModel);
			}
			NewGeneration();
		}else {
			Debug.LogErrorFormat("the file {0} doesn't exist !", fileRelativePath);
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
