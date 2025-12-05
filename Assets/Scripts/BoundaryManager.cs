using UnityEngine;

public class BoundaryManager : MonoBehaviour
{
    public Collider[] walls;

    public void SetWallsActive(bool active)
    {
        foreach (Collider wall in walls)
            wall.enabled = active;
    }
}
