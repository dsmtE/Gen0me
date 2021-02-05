using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Layer {
    public Layer()
    {
        c1 = Random.value;
        c2 = Random.value;
        c3 = Random.value;
        bias = Random.value;
    }

    public float eval(float x1, float x2, float x3) {
        return c1 * x1 + c2 * x2  + c3 * x3 + bias;
    }

    private float c1;
    private float c2;
    private float c3;
    private float bias;
} 

public class AIModel
{
    public AIModel()
    {
        layer1 = new Layer();
        layer2 = new Layer();
    }

    public float evalAcceleration(float x1, float x2, float x3) {
        return layer1.eval(x1, x2, x3);
    }

    public float evalSteer(float x1, float x2, float x3) {
        return layer2.eval(x1, x2, x3);
    }

    Layer layer1;
    Layer layer2;
}
