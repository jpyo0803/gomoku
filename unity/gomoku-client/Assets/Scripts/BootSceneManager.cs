/*
    BootScene에서는 게임 실행에 필요한 초기화 작업을 수행합니다.
    현재는 단순히 AuthScene을 로드하는 역할만 합니다.
*/

using UnityEngine;
using UnityEngine.SceneManagement;

namespace jpyo0803
{
    public class BootSceneManager : MonoBehaviour
    {
        private void Awake()
        {
            Application.runInBackground = true;

            ServiceLocator.Register<ILogger>(new UnityDebugLogger()); // ILogger 서비스 등록
            ServiceLocator.Register<HttpService>(new HttpService());

            SceneManager.LoadScene("AuthScene"); // AuthScene 로드
        }
    }
}
