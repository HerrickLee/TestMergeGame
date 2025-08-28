using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum SquareType
    {
        EMPTY, HEROEXP, ATTACK, REWARDBOX
        , BARRIER, BOOM, COUNT,
    }
    

    public static GameManager Instance;

    private float gameTime = 60;
    private int playerScore = 0;
    private bool gameOver = false;

    private ItemSquare pressSquare;
    private ItemSquare enterSquare;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }

    void Update()
    {
        if (gameOver) return;

        gameTime -= Time.deltaTime;
        if (gameTime <= 0)
        {
            gameTime = 0;
            gameOver = true;
            UIManager.Instance.ShowGameOver(playerScore);
        }
        UIManager.Instance.UpdateTimer(gameTime);
        if (!isReleasePress)
        {
            pressStartTime+= Time.deltaTime;
            if (pressStartTime >= 1f)
            {
                CheckCombineSquears();
            }
        }
       
    }

    public void AddScore(int addScore)
    {
        playerScore += addScore;
        UIManager.Instance.UpdateScore(playerScore);
    }

    private float pressStartTime = 0;
    private bool isReleasePress=true;
    public void PressSquare(ItemSquare square)
    {
        if (gameOver) return;
        pressSquare = square;
        isReleasePress=false;
        pressStartTime = 0;
    }

    public void EnterSquare(ItemSquare square)
    {
        if (gameOver) return;
        enterSquare = square;
    }

    // public void ReleaseSquare()
    // {
    //     isReleasePress = true;
    //     if (pressSquare != null && enterSquare != null)
    //     {
    //         if (pressSquare == enterSquare)
    //         {
    //             List<ItemSquare> matchList = BoardManager.Instance.MatchNeighborSquares(pressSquare);
    //          if (pressStartTime < 1f)
    //          {
    //              //点击
    //              //检查相邻的块
    //              //消除相邻的超过3个的块
    //            
    //             if (matchList.Count >= 3)
    //             {
    //                 BoardManager.Instance.ClearAllConnectedSquare(matchList);
    //             }
    //             else
    //             {
    //                 UIManager.Instance.ShowSingleSquareTip(pressSquare.transform.position);
    //             }
    //          }
    //          else
    //          {
    //           
    //              //长按
    //          }
    //          pressStartTime=0;
    //         }
    //         else
    //         {
    //            // BoardManager.Instance.ExchangeSquare(pressSquare, enterSquare);
    //         }
    //        
    //     }
    //     pressSquare = null;
    //     enterSquare = null;
    // }
    
    private void CheckCombineSquears()
    {
        isReleasePress = true;
        if (pressSquare != null)
        {
            List<ItemSquare> matchList = BoardManager.Instance.MatchNeighborSquares(pressSquare);
            if(matchList.Count>=3){
                BoardManager.Instance.CombineMatchSquares(matchList, pressSquare);
            }
            pressStartTime=0;
        }
        pressSquare = null;
    }
}