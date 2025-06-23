using UnityEngine;

public class PlayManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.instance.InitBoard(); // 게임 보드 초기화
    }
}
