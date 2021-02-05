using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Layer {
    public Layer(int inputDimension, int outputDimension)
    {
        coefficients = new float[inputDimension];
        biases = new float[outputDimension];
        for (int i = 0; i < inputDimension; i++)
        {
            coefficients[i] = Random.Range(-1, 1);
        }
        for (int i = 0; i < outputDimension; i++)
        {
            biases[i] = Random.Range(-1, 1);
        }
    }

    public float[] eval(float[] x) {
        float[] res = new float[biases.Length];
        for (int j = 0; j < biases.Length; ++j)
        {
            res[j] = biases[j];
            for (int i = 0; i < coefficients.Length; ++i)
            {
                res[j] += x[i] * coefficients[i];
            }
        }
        return res;
    }

    private float[] coefficients;
    private float[] biases;
} 

public class AIModel
{
    public AIModel()
    {
        layer1 = new Layer(3, 4);
        layer2 = new Layer(4, 2);
    }

    public float[] eval(float[] raycastDistances) {
        return layer2.eval(layer1.eval(raycastDistances));
    }

    Layer layer1;
    Layer layer2;
}
