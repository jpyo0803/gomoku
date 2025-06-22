using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    public GameObject gameManagerPrefab; // GameManager 프리팹

    private void Awake()
    {
        if (GameManager.instance == null)
        {
            // GameManager 프리팹을 인스턴스화
            Instantiate(gameManagerPrefab);
        }
        
        SceneManager.LoadScene("AuthScene"); // AuthScene 로드
    }
}
