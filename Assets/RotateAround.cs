using System;
using UnityEngine;

public class RotateAround : MonoBehaviour {
    [SerializeField] RotationDirection direction;
    [SerializeField] Vector3 rotationPosition;
    [SerializeField] float rotationSpeed;
    Vector3 rotationDirection;
    
    void OnEnable() {
        switch (direction) {
            case RotationDirection.X: rotationDirection = Vector3.right; break;
            case RotationDirection.Y: rotationDirection = Vector3.up; break;
            case RotationDirection.Z: rotationDirection = Vector3.forward; break;
        }
    }

    void Update() {
        transform.RotateAround(rotationPosition, rotationDirection, rotationSpeed);
       // transform.LookAt(rotationPosition);
    }
}

[Serializable]
public enum RotationDirection { X, Y, Z }