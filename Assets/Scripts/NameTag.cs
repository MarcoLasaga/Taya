using UnityEngine;
using TMPro;

public class NameTag : MonoBehaviour
{
    public TMP_Text label;
    public Vector3 offset = new Vector3(0, 2f, 0);

    void LateUpdate()
    {
        if (label != null)
        {
            label.transform.position = Camera.main.WorldToScreenPoint(transform.position + offset);
        }
    }

    public void SetText(string newText)
    {
        if (label != null) label.text = newText;
    }

    public void SetColor(Color color)
    {
        if (label != null) label.color = color;
    }
}
