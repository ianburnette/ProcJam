using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Voxel : MonoBehaviour
{
    public void SetColor(Color color) {
        if (color == Color.black) {
            gameObject.SetActive(false);
        } else {
            GetComponent<Renderer>().material.color = color;
        }
    }
}
