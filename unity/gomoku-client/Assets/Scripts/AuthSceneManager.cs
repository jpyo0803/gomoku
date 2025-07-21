using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

namespace jpyo0803
{
    public class AuthSceneManager : MonoBehaviour
    {
        [Header("Input Fields")]
        [SerializeField]
        private TMP_InputField usernameInput;
        [SerializeField]
        private TMP_InputField passwordInput;

        [Header("Buttons")]
        [SerializeField]
        private Button loginButton;
        [SerializeField]
        private Button signupButton;

        [Header("Output Displays")]
        [SerializeField]
        private TextMeshProUGUI responseDisplay;

        void Start()
        {
            loginButton.onClick.AddListener(() => OnLoginClickedAsync());
            signupButton.onClick.AddListener(() => OnSignupClickedAsync());
        }

        private async void OnSignupClickedAsync()
        {
            string username = usernameInput.text;
            string password = passwordInput.text;

            if (CheckUsernameAndPassword(username, password) == false)
            {
                return; // 유효성 검사 실패 시 조기 반환
            }

            Debug.Log($"[Log; Signup Attempt] Username: {username}, Password: {password}");

            int code = await GameManager.instance.SignUp(username, password);

            UpdateResponseDisplay(code);
        }

        private async void OnLoginClickedAsync()
        {
            string username = usernameInput.text;
            string password = passwordInput.text;

            if (CheckUsernameAndPassword(username, password) == false)
            {
                return; // 유효성 검사 실패 시 조기 반환
            }

            Debug.Log($"[Log; Login Attempt] Username: {username}, Password: {password}");

            int code = await GameManager.instance.Login(username, password);

            UpdateResponseDisplay(code);

            if (code == 200) // 로그인 성공 시
            {
                SceneManager.LoadScene("GameSettingScene"); // 게임 설정 씬으로 이동
            }
        }

        private bool CheckUsernameAndPassword(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                responseDisplay.text = "Username and password cannot be empty.";
                return false;
            }
            return true;
        }

        private void UpdateResponseDisplay(int code)
        {
            /*
                200: 로그인 성공
                201: 회원가입 성공
                400: 잘못된 요청
                401: 인증 실패
                409: 중복된 사용자 이름
                기타: 기타 오류
            */

            switch (code)
            {
                case 200:
                    responseDisplay.text = "Login successful!";
                    break;
                case 201:
                    responseDisplay.text = "SignUp successful! You can now log in.";
                    break;
                case 400:
                    responseDisplay.text = "Bad request. Please check your input.";
                    break;
                case 401:
                    responseDisplay.text = "Invalid username or password.";
                    break;
                case 409:
                    responseDisplay.text = "Username already exists. Please choose a different username.";
                    break;
                default:
                    responseDisplay.text = "Operation failed with code: " + code;
                    break;
            }
        }
    }
}