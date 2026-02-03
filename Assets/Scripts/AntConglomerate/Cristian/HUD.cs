using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class HUD : MonoBehaviour
{
    public float timeLeft = 120.0f;
    public TMP_Text timerText;
    public GameObject restartButton;
    public GameObject motorBroke;
    public GameObject simulationOver;
    void Start()
    {
        Time.timeScale = 1.0f;
    }

    // Update is called once per frame
    void Update()
    {
        timeLeft -= Time.deltaTime;

        if (timeLeft < 0.0f)
        {
            timeLeft = 0.0f;
            GameOver();
        }
        
        timerText.text = "Time Left: " + (int)Mathf.Ceil(timeLeft) + "s";
    }

    public void MotorBroke()
    {
        motorBroke.SetActive(true);
        GameOver();
    }

    public void GameOver()
    {
        Time.timeScale = 0.0f;
        simulationOver.SetActive(true);
        restartButton.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
