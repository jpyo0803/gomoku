using System.Threading.Tasks;
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

        if (GameManager.instance != null)
        {
            var websocketClient = GameManager.instance.WebSocketClient;

            if (websocketClient == null)
            {
                Debug.LogError("[Log Error] WebSocketClient is not initialized properly.");
                return;
            }
           
            // Send the place stone request to the server
            websocketClient.SendPlaceStone(row_index, col_index);
        }
        else
        {
            Debug.LogError("[Log Error] GameManager instance is null. Cannot place stone.");
            return;
        }

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
