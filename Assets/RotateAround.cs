using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAround : MonoBehaviour {
    [SerializeField] Vector3 rotationPosition;
    [SerializeField] float rotationSpeed;
    void Update() {
        transform.RotateAround(rotationPosition, Vector3.up, rotationSpeed);
        transform.LookAt(rotationPosition);
    }
}
