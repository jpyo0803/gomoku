using UnityEngine;
using UnityEngine.SceneManagement;

public class BootSceneManager : MonoBehaviour
{
    private void Awake()
    {
        SceneManager.LoadScene("AuthScene"); // AuthScene 로드
    }
}
