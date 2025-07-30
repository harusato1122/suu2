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
        // Canvas 内の TextMeshProUGUI を取得（非アクティブも含む）
        textMesh = GetComponentInChildren<TextMeshProUGUI>(true);

        if (textMesh == null)
        {
            Debug.LogError($"TextMeshProUGUI が見つかりません: {name}");
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