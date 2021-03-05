using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;
using System.Linq;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public delegate float ActivationFunct(float x);

[System.Serializable]
public class Layer {

    public ActivationFunct activationFunct;

    public static ActivationFunct Sigmoid = (float value) => 1.0f / (1.0f + Mathf.Exp(-value));
    public static ActivationFunct Relu = (float value) => Mathf.Max(0, value);

    public Layer(int inputDimension, int outputDimension, ActivationFunct activationFunct = null) {
        
        // default activation function
        activationFunct = activationFunct ?? Layer.Sigmoid;

        weights = new float[inputDimension];
        biases = new float[outputDimension];
        this.activationFunct = activationFunct;

        for (int i = 0; i < weights.Length; ++i) {
            weights[i] = Random.Range(-0.1f, 0.1f);
        }
        for (int i = 0; i < biases.Length; ++i) {
            biases[i] = Random.Range(-0.1f, 0.1f);
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


    public void Crossover(Layer other, float keepeingRate = 0.5f) {
        Assert.AreEqual(weights.Length, other.weights.Length, "Both layers must share the same weights size to be mixed.");
        Assert.AreEqual(biases.Length, other.biases.Length, "Both layers must share the same biases size to be mixed.");

        for (int i = 0; i < weights.Length; ++i) {
            if (Random.value < keepeingRate) {
                weights[i] = other.weights[i];
            }
        }

        for (int i = 0; i < biases.Length; ++i) {
            if (Random.value < keepeingRate) {
                biases[i] = other.biases[i];
            }
        }
    }

    public void Mutate(float mutationRate = 0.1f, float mutationStrength = 0.1f) {
        for (int i = 0; i < weights.Length; ++i) {
            if (Random.value < mutationRate) {
                weights[i] += Random.value * mutationStrength;
            }
        }

        for (int i = 0; i < biases.Length; ++i) {
            if (Random.value < mutationRate) {
                biases[i] += Random.value * mutationStrength;
            }
        }
    }

    private float[] weights;
    private float[] biases;
}

[System.Serializable]
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
        if(!a.layersDim.SequenceEqual(b.layersDim)) {
            Debug.LogError("Both models must share the same architecture to be mixed.");
        }
        AIModel res = new AIModel(a.layersDim);
        for (int i = 0; i < res.layers.Count; ++i) {
            res.layers[i] = Layer.Crossover(a.layers[i], b.layers[i]);
        }
        return res;
    }
    public void Mutate(float mutationRate = 0.1f, float mutationStrength = 0.1f) {
        for (int i = 0; i < layers.Count; ++i) {
            layers[i].Mutate(mutationRate, mutationStrength);
        }
    }

    private List<Layer> layers;
}
