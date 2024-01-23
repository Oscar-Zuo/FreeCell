using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

struct GameStepData
{
    public int stepNumber;
    public CardHolderController fromCardHolder;
    public CardHolderController toCardHolder;
    public List<CardController> movedCards;

    public GameStepData(int i_stepNumber, CardHolderController i_fromCardHolder, CardHolderController i_toCardHolder, List<CardController> i_movedCards)
    {
        stepNumber = i_stepNumber;
        fromCardHolder = i_fromCardHolder;
        toCardHolder = i_toCardHolder;
        movedCards = i_movedCards;
    }
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    [SerializeField] private GameObject cardPrefab;
    private Stack<GameStepData> gameStepDatas = new();
    [SerializeField] private TMP_Text timerText;


    public CardHolderController[] cascades = new CardHolderController[8];
    public CardHolderController heartFoundation;
    public CardHolderController spadeFoundation;
    public CardHolderController diamondFoundation;
    public CardHolderController clubFoundation;
    public bool lockingCards = false;
    public int totalSecond = 0;

    public static GameManager Instance { get => instance;}

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        // Generate Cards
        List<GameObject> newCardList = new();
        foreach (ECardSymbol symbol in Enum.GetValues(typeof(ECardSymbol)))
        {
            for (ushort i = 1; i <= 13; i++)
            {
                var newCard = Instantiate(cardPrefab);
                var newCardController = newCard.GetComponent<CardController>();
                newCardController.cardSymbol = symbol;
                newCardController.cardNumber = i;
                newCardList.Add(newCard);
            }
        }

        // Randomly distribute cards
        ushort cnt = 0;
        while(newCardList.Count > 0)
        {
            var card = newCardList[UnityEngine.Random.Range(0, newCardList.Count)];
            newCardList.Remove(card);
            cascades[cnt++].DropCards(new List<CardController>() { card.GetComponent<CardController>() });
            cnt %= 8;
        }

        // Record every successful move
        CardHolderController.onSuccessfulMove += RecordStep;
        // Auto Check and move cards to the foundations
        CardHolderController.onSuccessfulMove += CheckAndMoveCardToFoundations;
        CardHolderController.onSuccessfulMove += CheckGameOver;

        // start timer
        StartCoroutine(RecordTime());
    }

    void CheckAndMoveCardToFoundations(CardHolderController fromCardHolder, CardHolderController toCardHolder, List<CardController> movedCards)
    {
        // if player is dragging the card out of a Foundation, let it be. 
        if (fromCardHolder == null || fromCardHolder.GetType() == typeof(GoalCardHolderController))
            return;

        foreach (CardHolderController cardHolder in cascades)
        {
            if (cardHolder.cards.Count <= 0)
                continue;

            var finalCard = cardHolder.cards[^1];


            var finalCardList = new List<CardController>() { finalCard } ;
            switch (finalCard.cardSymbol)
            {
                case ECardSymbol.Heart:
                    if (heartFoundation.CanBeDropped(finalCardList) && !lockingCards)
                    {
                        StartCoroutine(SendCardToFoundationAnimation(finalCard, heartFoundation));
                        return;
                    }
                    break;
                case ECardSymbol.Diamond:
                    if (diamondFoundation.CanBeDropped(finalCardList) && !lockingCards)
                    {
                        StartCoroutine(SendCardToFoundationAnimation(finalCard, diamondFoundation));
                        return;
                    }  
                    break;
                case ECardSymbol.Spade:
                    if (spadeFoundation.CanBeDropped(finalCardList) && !lockingCards)
                    {
                        StartCoroutine(SendCardToFoundationAnimation(finalCard, spadeFoundation));
                        return;
                    }
                    break;
                case ECardSymbol.Club:
                    if (clubFoundation.CanBeDropped(finalCardList) && !lockingCards)
                    {
                        StartCoroutine(SendCardToFoundationAnimation(finalCard, clubFoundation));
                    }
                    break;
            }
        }
    }

    IEnumerator SendCardToFoundationAnimation(CardController card, CardHolderController foundation, float time = 1)
    {
        lockingCards = true;
        float currentTime = 0;
        while (currentTime < time)
        {
            Vector3 originalPosition = card.transform.position;
            card.transform.position = Vector3.Lerp(originalPosition,
                new Vector3(foundation.transform.position.x, foundation.transform.position.y, originalPosition.z),
                currentTime / time);
            yield return new WaitForEndOfFrame();
            currentTime += Time.deltaTime;
        }
        lockingCards = false;
        foundation.DropCards(new List<CardController> { card });
    }

    void RecordStep(CardHolderController fromCardHolder, CardHolderController toCardHolder, List<CardController> movedCards)
    {
        GameStepData stepData = new(gameStepDatas.Count, fromCardHolder, toCardHolder, movedCards);
        gameStepDatas.Push(stepData);
    }

    void CheckGameOver(CardHolderController fromCardHolder, CardHolderController toCardHolder, List<CardController> movedCards)
    {
        foreach (CardHolderController cardHolder in cascades)
        {
            if (cardHolder.cards.Count > 0)
                return;
        }

        PlayerPrefs.SetInt("TotalTime", totalSecond);
        SceneManager.LoadScene("GameOverScene");
    }

    public void RevertOneStep()
    {
        if (gameStepDatas.Count <= 0 || lockingCards)
            return;
        var stepData = gameStepDatas.Pop();
        stepData.fromCardHolder?.DropCards(stepData.movedCards, true);
    }

    IEnumerator RecordTime()
    {
        while (true)
        {
            timerText.text = (totalSecond / 60).ToString() + "m " + (totalSecond % 60) + "s";
            yield return new WaitForSecondsRealtime(1);
            totalSecond++;
        }
    }
}
