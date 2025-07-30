using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Mino : MonoBehaviour
{
    public bool isNumber;
    public string Value;

    private TextMeshProUGUI textMesh;

    void Awake()
    {
        // Canvas ���� TextMeshProUGUI ���擾�i��A�N�e�B�u���܂ށj
        textMesh = GetComponentInChildren<TextMeshProUGUI>(true);

        if (textMesh == null)
        {
            Debug.LogError($"TextMeshProUGUI ��������܂���: {name}");
        }
    }

    public void SetText(string value)
    {
        Value = value;
        isNumber = IsNumber(value);

        if (textMesh != null)
        {
            textMesh.text = value;
        }
    }

    private bool IsNumber(string val)
    {
        return int.TryParse(val, out _);
    }
}