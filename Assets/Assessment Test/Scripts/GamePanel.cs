using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GamePanel : MonoBehaviour
{
    // Temporary solution to store the levels configuration (should be stored in a ScriptableObject)
    public static readonly Vector2Int[] levelsConfig = new[] {
        new Vector2Int(2, 2),
        new Vector2Int(3, 2),
        new Vector2Int(4, 3),
        new Vector2Int(4, 4),
        new Vector2Int(5, 4),
        new Vector2Int(6, 4),
        new Vector2Int(6, 5),
        new Vector2Int(8, 5),
        new Vector2Int(8, 6),
        new Vector2Int(9, 6),
        new Vector2Int(10, 6),
    };

    [SerializeField] Board board;

    [SerializeField] Button backButton;
    [SerializeField] Text matchesText;
    [SerializeField] Text turnsText;
    [SerializeField] Text scoreText;
    [SerializeField] Text countdownText;
    [SerializeField] Image boardOverlay;
    [SerializeField] Text levelText;

    [SerializeField] GameObject gameOverPanel;
    [SerializeField] Button restartButton;
    [SerializeField] Button nextLevelButton;

    [SerializeField] AudioClip matchSound;
    [SerializeField] AudioClip mismatchSound;
    [SerializeField] AudioClip comboSound;
    [SerializeField] AudioClip gameOverSound;
    [SerializeField] AudioClip bestScoreSound;

    AudioSource audioSource;
    int currentLevel = 1;
    bool gotNewHighScore;

    public Action CallbackOnClickBack { get; set; }

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        //audioSource.spatialBlend = 0;

        backButton.onClick.AddListener(() => CallbackOnClickBack?.Invoke());

        board.CallbackOnMatch += () => audioSource.PlayOneShot(board.ComboCount > 1 ? comboSound : matchSound);
        board.CallbackOnMismatch += () => audioSource.PlayOneShot(mismatchSound);

        board.CallbackOnGameOver += OnGameOver;

        nextLevelButton.onClick.AddListener(GoToNextLevel);
        restartButton.onClick.AddListener(RestartLevel);
    }

    void RestartLevel()
    {
        StopAllCoroutines();
        board.UnInitialize();
        SetCurrentLevel();
        board.Initialize();
        OnEnable();
    }

    void GoToNextLevel()
    {
        currentLevel++;
        RestartLevel();
    }

    void OnGameOver()
    {
        Debug.Log("Game Over!");
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);
        if (board.Score > bestScore)
        {
            PlayerPrefs.SetInt("BestScore", board.Score);
            audioSource.PlayOneShot(bestScoreSound);

            gotNewHighScore = true;
        }
        else
            audioSource.PlayOneShot(gameOverSound);

        gameOverPanel.SetActive(true);
    }

    public void SetCurrentLevel(int level = 0)
    {
        if (level > 0) currentLevel = level;
        board.defaultSize = levelsConfig[Mathf.Min(currentLevel - 1, levelsConfig.Length - 1)];
    }

    private void OnEnable()
    {
        StartCoroutine(CountdownCoroutine());

        gameOverPanel.SetActive(false);

        gotNewHighScore = false;
    }

    private void Update()
    {
        matchesText.text = $"Matches\n<b><size=64>{board.MatchesCount}</size></b>";
        turnsText.text = $"Turns\n<b><size=64>{board.StepsCount}</size></b>";
        string strScoreTitle = gotNewHighScore ? $"\n<color=#FF00FF>New Highest Score</color>" : "Score";
        scoreText.text = $"{strScoreTitle}\n<b><size=64>{board.Score}</size></b>";

        if (!board.IsOver && board.ComboCount > 1)
            scoreText.text += $"\n<color=#FF00FF>Combo x{board.ComboCount}</color>";

        levelText.text = $"Level {currentLevel}";
    }

    void Refresh(bool isReady)
    {
        matchesText.gameObject.SetActive(isReady);
        turnsText.gameObject.SetActive(isReady);
        scoreText.gameObject.SetActive(isReady);
        boardOverlay.gameObject.SetActive(!isReady);
        countdownText.gameObject.SetActive(!isReady);
    }

    IEnumerator CountdownCoroutine()
    {
        Refresh(false);
        for (int i = 3; i > 0; i--)
        {
            countdownText.text = $"You have\n<b><size=64>{i}</size></b>s\nto remember";
            yield return new WaitForSeconds(1);
        }
        Refresh(true);

        board.HideAllCards();
    }
}
