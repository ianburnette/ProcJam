using TMPro;
using UnityEngine;

public class PalettesDropdown : MonoBehaviour
{
    [SerializeField] TMP_Dropdown presetsDropdown;
    [SerializeField] Controls controls;

    void OnEnable() {
        presetsDropdown.options.Clear();
        foreach (var palette in controls.spriteGeneration.Recoloring.palettes)
            presetsDropdown.options.Add(new TMP_Dropdown.OptionData(palette.name));
    }
}
