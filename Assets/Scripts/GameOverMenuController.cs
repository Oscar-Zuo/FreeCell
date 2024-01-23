using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverMenuController : MonoBehaviour
{
    [SerializeField] private TMP_Text timeUI;
    private void Start()
    {
        int totalTime = PlayerPrefs.GetInt("TotalTime");
        timeUI.text = (totalTime / 60).ToString() + "m " + (totalTime % 60) + "s";
    }

    public static void Restart()
    {
        SceneManager.LoadScene("MainScene");
    }
}
