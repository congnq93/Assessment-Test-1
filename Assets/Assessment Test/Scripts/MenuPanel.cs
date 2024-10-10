using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPanel : MonoBehaviour
{
    [SerializeField] Text levelText;
    [SerializeField] Slider levelSlider;
    [SerializeField] Button playButton;
    [SerializeField] Text bestScoreText;

    public Action CallbackOnClickPlay { get; set; }
    public int SelectedLevel => (int)levelSlider.value;

    private void Awake()
    {
        playButton.onClick.AddListener(() => CallbackOnClickPlay?.Invoke());

        levelSlider.onValueChanged.AddListener((value) =>
        {
            levelText.text = $"Level {value}";
        });

        levelSlider.minValue = 1;
        levelSlider.maxValue = GamePanel.levelsConfig.Length;
    }

    void OnEnable()
    {
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        bestScoreText.text = $"Best Score: {bestScore}";
    }
}
