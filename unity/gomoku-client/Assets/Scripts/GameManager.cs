using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static GameManager instance = null;

    [SerializeField]
    private Image resultImage;

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
    }

    // Update is called once per frame
    public void DisplayResultImage(bool isWin)
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
    }
}
