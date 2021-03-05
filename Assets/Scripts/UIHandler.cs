using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public class UIHandler : MonoBehaviour {

    [SerializeField] private Text fpsText;
    [SerializeField] private Text trainingText;

    [SerializeField] private Dropdown filesDropdown;
    [SerializeField] private InputField saveName;

    public TrainingManager trainingManager;

    private float fps;
    private float deltaTime;

    void Update() {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        fps = 1.0f / deltaTime;
        fpsText.text = "fps: " + Mathf.Ceil(fps).ToString();

        trainingText.text = trainingManager.getInfos();
    }

    private void UpdateFilesNames() {
        DirectoryInfo dir = new DirectoryInfo(Application.persistentDataPath);
        FileInfo[] info = dir.GetFiles("*.*");
        filesNames = info.Select(i => i.Name.Split('.')[0]).ToList();
    }

    private void UpdateDropDownList() {
        UpdateFilesNames();
        filesDropdown.ClearOptions();
        filesDropdown.AddOptions(filesNames);
    }

    private void Awake() {
        UpdateDropDownList();
    }
    public void Save() {
        trainingManager.Save(saveName.text);
        UpdateDropDownList();
    }

    public void Load() {
        trainingManager.Load(filesNames[filesDropdown.value]);
    }


    private List<string> filesNames;

}