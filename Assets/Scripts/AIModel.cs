using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

public delegate float ActivationFunct(float x);


public class Layer {

    public ActivationFunct activationFunct;

    public static ActivationFunct Sigmoid = (float value) => 1.0f / (1.0f + Mathf.Exp(-value));

    public Layer(int inputDimension, int outputDimension, ActivationFunct activationFunct = null) {
        
        // default activation function
        activationFunct = activationFunct ?? Layer.Sigmoid;

        weights = new float[inputDimension];
        biases = new float[outputDimension];
        this.activationFunct = activationFunct;

        for (int i = 0; i < inputDimension; ++i) {
            weights[i] = Random.Range(-1, 1);
        }
        for (int i = 0; i < outputDimension; ++i) {
            biases[i] = Random.Range(-1, 1);
        }
    }

    public float[] eval(float[] x) {
        float[] res = new float[biases.Length];
        for (int j = 0; j < biases.Length; ++j) {
            res[j] = biases[j];
            for (int i = 0; i < weights.Length; ++i) {
                res[j] += x[i] * weights[i];
            }
            res[j] = activationFunct(res[j]);
        }
        return res;
    }

    public static Layer Crossover(Layer a, Layer b) {
        Assert.AreEqual(a.weights.Length, b.weights.Length, "Both layers must share the same weights size to be mixed.");
        Assert.AreEqual(a.biases.Length, b.biases.Length, "Both layers must share the same biases size to be mixed.");
        Layer res = new Layer(a.weights.Length, a.biases.Length, a.activationFunct);

        for (int i = 0; i < res.weights.Length; ++i) {
            res.weights[i] = Random.value < 0.5 ? a.weights[i] : b.weights[i];
        }

        for (int i = 0; i < res.biases.Length; ++i) {
            res.biases[i] = Random.value < 0.5 ? a.biases[i] : b.biases[i];
        }

        return res;
    }

    private float[] weights;
    private float[] biases;
} 

public class AIModel {
    public int[] layersDim;

    public AIModel(int[] layersDim) {
        this.layersDim = layersDim;
        layers = new List<Layer>();
        for (int i = 0; i < layersDim.Length-1; ++i) {
            layers.Add(new Layer(layersDim[i], layersDim[i + 1]));
        }
    }

    public float[] eval(float[] input) {
        Assert.AreEqual(input.Length, layersDim[0], "input size must match model input size");
        float[] res = input;
        foreach (var layer in layers) {
            res = layer.eval(res);
        }
        return res;
    }

    public static AIModel Crossover(AIModel a, AIModel b) {
        Assert.AreEqual(a.layersDim, b.layersDim, "Both models must share the same architecture to be mixed.");
        AIModel res = new AIModel(a.layersDim);
        for (int i = 0; i < res.layers.Count; ++i) {
            res.layers[i] = Layer.Crossover(a.layers[i], b.layers[i]);
        }
        return res;
    }

private List<Layer> layers;
}
