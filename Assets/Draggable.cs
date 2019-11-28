using UnityEngine;

public class Draggable : MonoBehaviour {
    Vector2 mousePositionLastFrame;
    public float maxScale, minScale;
    public float scrollSpeed;
    public float resizeSpeed;
    
    void Update() {
        var mousePos1 = Input.mousePosition;
        if (Input.GetMouseButton(1)) {
            var adjusted1 = Camera.main.ScreenToViewportPoint((Vector2) mousePos1 - mousePositionLastFrame);
            transform.position += new Vector3(adjusted1.x, adjusted1.y, 0) * scrollSpeed;
        }

        if (Mathf.Abs(Input.mouseScrollDelta.magnitude) > 1 && (
                (transform.localScale.x < maxScale || Input.mouseScrollDelta.y < 0) &&
            (transform.localScale.x > minScale || Input.mouseScrollDelta.y > 0))) {
            var value = Time.deltaTime * resizeSpeed * (Vector3)Input.mouseScrollDelta;
            transform.localScale += Vector3.one * value.y;
        }
        
        mousePositionLastFrame = mousePos1;
    }

}
