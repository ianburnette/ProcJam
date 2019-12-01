using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class NameOfParent : MonoBehaviour {
    [SerializeField] TMP_Text text;
    void Update() => text.text = transform.parent.name;

    void OnEnable() {
        text = GetComponent<TMP_Text>();
    }
}
