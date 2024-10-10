using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

using Random = UnityEngine.Random;

public class Board : MonoBehaviour
{
    public Vector2Int defaultSize = new(4, 4);
    public AudioClip flipSound;

    RectTransform rectTransform;
    GridLayoutGroup gridLayoutGroup;

    Card cardPrefab;
    List<Card> cards, flippedCards;
    AudioSource audioSource;

    public int ComboCount { get; private set; }
    public int StepsCount { get; private set; }
    public int MatchesCount { get; private set; }
    public int Score { get; private set; }

    public Action CallbackOnMatch { get; set; }
    public Action CallbackOnMismatch { get; set; }
    public Action CallbackOnGameOver { get; set; }
    public bool IsOver { get { return cards.Count == 0; } }

    private void Awake()
    {
        rectTransform = transform as RectTransform;
        TryGetComponent(out gridLayoutGroup);

        cardPrefab = GetComponentInChildren<Card>();
        cardPrefab.gameObject.SetActive(false);

        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnEnable()
    {
        Initialize(defaultSize);
    }

    private void OnDisable()
    {
        UnInitialize();
    }

    public void Initialize(Vector2Int size = default)
    {
        if (size == default) size = defaultSize;

        // Set the cell size of the GridLayoutGroup
        float totalPaddingX = gridLayoutGroup.padding.left + gridLayoutGroup.padding.right;
        float totalPaddingY = gridLayoutGroup.padding.top + gridLayoutGroup.padding.bottom;

        float availableWidth = rectTransform.rect.width - totalPaddingX;
        float availableHeight = rectTransform.rect.height - totalPaddingY;

        float cellWidth = (availableWidth - gridLayoutGroup.spacing.x * (size.x - 1)) / size.x;
        float cellHeight = (availableHeight - gridLayoutGroup.spacing.y * (size.y - 1)) / size.y;

        gridLayoutGroup.cellSize = new Vector2(cellWidth, cellHeight);

        // Load icons
        Sprite[] allIcons = Resources.LoadAll<Sprite>("Icons");
        int totalCards = size.x * size.y;
        int numPairs = totalCards / 2;

        // Create and shuffle the array of card icons use LINQ
        Sprite[] cardIcons = Enumerable.Range(0, numPairs)
            .Select(i => allIcons[i % allIcons.Length]) // Select the icons
            .Concat(Enumerable.Range(0, numPairs) // Duplicate to create pairs
            .Select(i => allIcons[i % allIcons.Length]))
            .OrderBy(a => Random.value) // Shuffle the list
            .ToArray();

        // Create cards
        cards = new List<Card>(size.x * size.y);
        for (int i = 0; i < size.x * size.y; i++)
        {
            Card card = Instantiate(cardPrefab, transform);
            card.SetIcon(cardIcons[i]);
            card.gameObject.SetActive(true);
            cards.Add(card);

            card.CallbackOnFlip = () => OnFlipCard(card);
        }

        flippedCards = new List<Card>();
    }

    void OnFlipCard(Card card)
    {
        audioSource.PlayOneShot(flipSound);
        flippedCards.Add(card);
        if (flippedCards.Count == 2)
        {
            // Check match
            bool isMatched = flippedCards[0].IconSprite == flippedCards[1].IconSprite;
            foreach (var flippedCard in flippedCards)
            {
                var c = isMatched ? flippedCard.HighlightMatchCoroutine() : flippedCard.HighlightMismatchCoroutine();
                flippedCard.StartCoroutine(c);

                if (isMatched)
                    cards.Remove(flippedCard);
            }
            flippedCards.Clear();

            if (isMatched)
            {
                MatchesCount++;
                ComboCount++;
                Score += 100 + 10 * ComboCount;

                CallbackOnMatch?.Invoke();

                if (cards.Count == 0)
                    CallbackOnGameOver?.Invoke();
            }
            else
            {
                Score = Mathf.Max(0, Score - 10);
                ComboCount = 0;
                CallbackOnMismatch?.Invoke();
            }
            StepsCount++;
        }
    }

    public void HideAllCards()
    {
        foreach (var card in cards)
            card.SetFlip(false);
    }

    public void UnInitialize()
    {
        while (transform.childCount > 1)
            DestroyImmediate(transform.GetChild(1).gameObject);

        flippedCards.Clear();
        cards.Clear();
        cards = null;

        ComboCount = StepsCount = MatchesCount = Score = 0;
    }
}