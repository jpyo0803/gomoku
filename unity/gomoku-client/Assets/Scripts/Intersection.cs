using UnityEngine;

public class Intersection : MonoBehaviour
{
    private float x, y; // Actual coordinates of the intersection

    public GameObject blackStonePrefab;
    public GameObject whiteStonePrefab;

    [SerializeField]
    private int row_index, col_index; // Row and column indices for the intersection
    private bool alreadyClicked = false;

    void Start()
    {
        x = transform.position.x;
        y = transform.position.y;
        // Debug.Log($"Intersection created at ({x}, {y})");
    }

    // Update is called once per frame
    private void OnMouseDown()
    {
        // Check if the mouse button is pressed
        Debug.Log($"Intersection clicked at ({row_index}, {col_index})");
        // Find gomoku client and send the click event
        GomokuClient gomokuClient = FindFirstObjectByType<GomokuClient>();
        if (gomokuClient == null)
        {
            Debug.LogError("GomokuClient not found in the scene.");
            return;
        }
        if (alreadyClicked)
        {
            Debug.Log("This intersection has already been clicked.");
            return;
        }
        alreadyClicked = true;
        gomokuClient.SendPlaceStone(row_index, col_index);
        // GameManager.Instance.PlaceStone(row, col);
    }
    public int GetRowIndex()
    {
        return row_index;
    }

    public int GetColIndex()
    {
        return col_index;
    }

    public void SetRowIndex(int row_index)
    {
        this.row_index = row_index;
    }

    public void SetColIndex(int col_index)
    {
        this.col_index = col_index;
    }

    public float GetX()
    {
        return x;
    }

    public float GetY()
    {
        return y;
    }

    public void SetStone(bool isBlack)
    {
        // Set the stone at this intersection
        GameObject stonePrefab = isBlack ? blackStonePrefab : whiteStonePrefab;
        Instantiate(stonePrefab, transform.position, Quaternion.identity);
    }
}
