using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;


public class UIHandler : MonoBehaviour {

    [SerializeField] private Text fpsText;
    [SerializeField] private Text trainingText;

    public TrainingManager trainingManager;

    private float fps;
    private float deltaTime;

    void Update() {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        fps = 1.0f / deltaTime;
        fpsText.text = "fps: " + Mathf.Ceil(fps).ToString();

        trainingText.text = trainingManager.getInfos();
    }
}