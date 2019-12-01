using UnityEngine;

public class ControlPanel : MonoBehaviour {
    public void DisableChildren() {
        for (var i = 0; i < transform.childCount; i++) {
            transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
