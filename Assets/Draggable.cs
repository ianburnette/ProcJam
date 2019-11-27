using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Draggable : MonoBehaviour {
    public Vector2 mousePositionLastFrame;
    public float distance;
    
    void Update() {
        if (Input.GetMouseButtonDown(1)) {
            distance = Vector2.Distance(transform.position, Camera.main.ScreenToViewportPoint(Input.mousePosition));
        }
        if (Input.GetMouseButton(1)) {
            var mousePos =  Input.mousePosition;
            transform.position = Camera.main.ScreenToViewportPoint((Vector2)mousePos + mousePositionLastFrame);
            //Camera.main.ScreenToViewportPoint((Vector2) (transform.position) + dragStartPosition +
            //                                  ((Vector2) Input.mousePosition - dragStartPosition));
            mousePositionLastFrame = mousePos;
            //new Vector2(mousePos.x, mousePos.y);)
        }
    }

}
