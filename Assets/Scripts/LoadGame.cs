using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadGame : MonoBehaviour
{
    private Button startButton;
    private Button exitButton;

    private void Awake()
    {
        startButton = transform.Find("ButtonStart").GetComponent<Button>();
        exitButton = transform.Find("ButtonExit").GetComponent<Button>();
        startButton.onClick.AddListener(StartGame);
        exitButton.onClick.AddListener(ExitGame);
    }

    public void StartGame()
    {
        SceneManager.LoadScene(1);
    }
    
    public void ExitGame()
    {
        Application.Quit();
    }
}
