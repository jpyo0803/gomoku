using UnityEngine;

public class IntersectionClickHandler : MonoBehaviour
{
    public float x, y;
    private bool alreadyClicked = false;

    void Start()
    {
        x = transform.position.x;
        y = transform.position.y;
        Debug.Log($"Intersection created at ({x}, {y})");
    }

    // Update is called once per frame
    private void OnMouseDown()
    {
        // Check if the mouse button is pressed
        Debug.Log($"Intersection clicked at ({x}, {y})");
        if (alreadyClicked)
        {
            Debug.Log("This intersection has already been clicked.");
            return;
        }

        alreadyClicked = true;
        GameManager.Instance.PlaceStone(x, y);
    }
}
