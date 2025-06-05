using UnityEngine;

public class Intersection : MonoBehaviour
{
    private float x, y; // Intersection의 실제 좌표 

    [SerializeField]
    private GameObject blackStonePrefab, blackStoneNewPrefab; // 교차점에 놓을 돌 프리팹
    [SerializeField]
    private GameObject whiteStonePrefab, whiteStoneNewPrefab; // 교차점에 놓을 돌 프리팹

    [SerializeField]
    private int row_index, col_index; // 15 x 15 보드상에서 교차점의 행과 열 인덱스

    private void Start()
    {
        x = transform.position.x;
        y = transform.position.y;
    }

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

    public void SetRowIndex(int row_index)
    {
        this.row_index = row_index;
    }

    public void SetColIndex(int col_index)
    {
        this.col_index = col_index;
    }

    public void SetStone(bool isBlackStone, bool isLastMove = false)
    {
        // Set the stone at this intersection
        GameObject stonePrefab;
        if (isLastMove) 
        {
            stonePrefab = isBlackStone ? blackStoneNewPrefab : whiteStoneNewPrefab;
        }
        else
        {
            stonePrefab = isBlackStone ? blackStonePrefab : whiteStonePrefab;
        }

        Instantiate(stonePrefab, transform.position, Quaternion.identity);
    }
}
