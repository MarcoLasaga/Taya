using UnityEngine;
using TMPro;

public class NameTag : MonoBehaviour
{
    public TMP_Text nameText;
    public Transform target;
    public Vector3 offset = new Vector3(0, 2f, 0); // height above capsule

    void LateUpdate()
    {
        if (target == null) return;

        // Follow the target
        transform.position = target.position + offset;

        // Always face the camera
        transform.LookAt(Camera.main.transform);
        transform.Rotate(0, 180f, 0);
    }

    public void SetName(string newName)
    {
        nameText.text = newName;
    }

    public void SetColor(Color color)
    {
        nameText.color = color;
    }
}
