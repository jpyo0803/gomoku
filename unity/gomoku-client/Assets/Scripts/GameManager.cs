using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static GameManager instance = null;

    [SerializeField]
    private Image resultImage;

    [SerializeField]
    private GameObject playAgainButton;

    [SerializeField]
    private Sprite winSprite;
    [SerializeField]
    private Sprite loseSprite;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Start()
    {
        // Initialize the result image to be inactive at the start
        resultImage.gameObject.SetActive(false);
        playAgainButton.SetActive(false);
    }

    public void SetGameResult(bool isWin)
    {
        // Display the result image based on the game outcome
        DisplayResultImage(isWin);
    }

    // Update is called once per frame
    private void DisplayResultImage(bool isWin)
    {

        if (isWin)
        {
            resultImage.sprite = winSprite;
        }
        else
        {
            resultImage.sprite = loseSprite;
        }

        resultImage.gameObject.SetActive(true);
        playAgainButton.SetActive(true);
    }

    public void PlayAgain()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
