using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialProperties
{
    public float height = 0f;
    public float amount = 0f;
    public Vector2 direction = new Vector2(0f, 0f);
    public bool stable = false;

    public MaterialProperties(float _height, float _amount)
    {
        height = _height;
        amount = _amount;
    }
}
