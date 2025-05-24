using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject blackStonePrefab;
    public GameObject whiteStonePrefab;

    private bool isBlackTurn = true;

    private void Awake()
    {
        Instance = this;
    }

    public void PlaceStone(float x, float y)
    {

        GameObject prefab = isBlackTurn ? blackStonePrefab : whiteStonePrefab;
        Vector3 position = new Vector3(x, y, 0); // 좌표를 실제 위치에 맞게 변환 필요
        Instantiate(prefab, position, Quaternion.identity);

        isBlackTurn = !isBlackTurn;
    }
}