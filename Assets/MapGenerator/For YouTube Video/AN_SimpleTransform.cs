using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AN_SimpleTransform : MonoBehaviour
{
    public float RotateSpeed = 1f;

    void Update()
    {
        transform.Rotate(new Vector3(0, RotateSpeed * Time.deltaTime, 0));
    }
}
