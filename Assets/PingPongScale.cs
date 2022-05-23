using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PingPongScale : MonoBehaviour
{
    public float minScale = -0.5f;
    public float maxScale = 0.5f;
    public float speed = 5f;

    void Update()
    {
        transform.localScale = Vector3.right * (Mathf.PingPong(Time.time * speed, Mathf.Abs(minScale) + Mathf.Abs(maxScale)) + minScale);
    }
}
