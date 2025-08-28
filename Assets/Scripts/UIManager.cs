using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timerText;
    public GameObject gameOverWindow;
    public TextMeshProUGUI overScoreText;
    public Button buttonUp;

    public GameObject tipWindow;
    public GameObject tipButtonView;
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        buttonUp.onClick.AddListener(() =>
        {
            //将棋盘最后一排往上顶起一格
            BoardManager.Instance.UpBoardSquares();
        });
    }

    public void UpdateScore(int score)
    {
        scoreText.text = score.ToString();
    }

    public void UpdateTimer(float time)
    {
        timerText.text = time.ToString("f0");
    }

    public void ShowGameOver(int finalScore)
    {
        gameOverWindow.SetActive(true);
        overScoreText.text = finalScore.ToString();
    }

    public void ReturnToMain()
    {
        SceneManager.LoadScene(0);
    }

    public void Replay()
    {
        SceneManager.LoadScene(1);
    }

    public void ShowSingleSquareTip(Vector3 pos)
    {
        tipWindow.SetActive(true);
        Vector3 screenPosition=Camera.main.WorldToScreenPoint(pos);
        Vector2 localUivVector2 = Vector2.zero;
         bool isSuccese=   RectTransformUtility.ScreenPointToLocalPointInRectangle(tipWindow.transform as RectTransform,
                screenPosition, null,out localUivVector2);
         tipButtonView.transform.localPosition = localUivVector2;
    }
    
    
    
    public void HideSingleSquareTip()
    {
        tipWindow.SetActive(false);
    }
}