using UnityEngine;
using UnityEngine.SceneManagement;

public class BootLoader : MonoBehaviour
{
    private void Awake()
    {
        SceneManager.LoadScene("AuthScene"); // AuthScene 로드
    }
}
