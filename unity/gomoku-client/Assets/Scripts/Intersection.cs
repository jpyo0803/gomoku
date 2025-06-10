using UnityEngine;

public class Intersection : MonoBehaviour
{
    [SerializeField]
    private int row_index, col_index; // 15 x 15 보드상에서 교차점의 행과 열 인덱스

    private GameObject stone = null; // 교차점에 놓인 돌 오브젝트

    // Update is called once per frame
    private void OnMouseDown()
    {
        // Check if the mouse button is pressed
        Debug.Log($"[Log] Intersection clicked at ({row_index}, {col_index})");
        // Find gomoku client and send the click event
        GomokuClient gomokuClient = FindFirstObjectByType<GomokuClient>();
        if (gomokuClient == null)
        {
            Debug.LogError("GomokuClient not found in the scene.");
            return;
        }

        gomokuClient.SendPlaceStone(row_index, col_index);
    }
    
    public int GetRowIndex()
    {
        return row_index;
    }

    public int GetColIndex()
    {
        return col_index;
    }

    public void SetStone(GameObject stonePrefab)
    {
        if (stone != null)
        {
            Destroy(stone);
        }
        stone = Instantiate(stonePrefab, transform.position, Quaternion.identity);
    }
}
