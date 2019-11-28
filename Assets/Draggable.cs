using UnityEngine;

public class Draggable : MonoBehaviour {
    Vector2 mousePositionLastFrame;
    public float maxScale, minScale;
    public float scrollSpeed;
    public float resizeSpeed;
    public float delta;
    
    void Update() {
        var mousePos1 = Input.mousePosition;
        if (Input.GetMouseButton(1)) {
            var adjusted1 = Camera.main.ScreenToViewportPoint((Vector2) mousePos1 - mousePositionLastFrame);
            transform.position += new Vector3(adjusted1.x, adjusted1.y, 0) * scrollSpeed;
        }

        delta = Input.GetAxis("Mouse ScrollWheel");
        
        if (Mathf.Abs(delta) > 0 && (
                (transform.localScale.x < maxScale || delta < 0) &&
            (transform.localScale.x > minScale || delta > 0))) {
            var value = Time.deltaTime * resizeSpeed * delta;
            transform.localScale += Vector3.one * value;
        }
        
        mousePositionLastFrame = mousePos1;
    }

}
