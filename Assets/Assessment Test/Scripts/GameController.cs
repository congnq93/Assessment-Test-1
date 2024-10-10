using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    [SerializeField] MenuPanel menuPanel;
    [SerializeField] GamePanel gamePanel;

    private void Awake()
    {
        PlayerPrefs.DeleteAll();

        Application.targetFrameRate = 60;

        menuPanel.gameObject.SetActive(true);
        gamePanel.gameObject.SetActive(false);

        menuPanel.CallbackOnClickPlay += () =>
        {
            Debug.Log("Play button clicked level " + menuPanel.SelectedLevel);
            gamePanel.SetCurrentLevel(menuPanel.SelectedLevel);
            menuPanel.gameObject.SetActive(false);
            gamePanel.gameObject.SetActive(true);
        };

        gamePanel.CallbackOnClickBack += () =>
        {
            gamePanel.gameObject.SetActive(false);
            menuPanel.gameObject.SetActive(true);
        };
    }
}
