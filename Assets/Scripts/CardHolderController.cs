using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

interface IMoveCards
{
    bool CanBeDropped(List<CardController> droppingCards);
    void DropCards(List<CardController> droppedCards, bool reverting = false);
    void PickUpCards(List<CardController> pickedUpCards, bool reverting = false);
}

public class CardHolderController : MonoBehaviour, IMoveCards
{
    [NonSerialized] public List<CardController> cards = new();
    public Transform anchorPoint;

    public float spaceBetweenCards = 0.5f;

    public delegate void OnSuccessfulMove(CardHolderController fromCardHolder, CardHolderController toCardHolder, List<CardController> movedCards);
    public static event OnSuccessfulMove onSuccessfulMove;

    virtual public bool CanBeDropped(List<CardController> droppingCards)
    {
        if (droppingCards.Count <=0)
            return false;


        // always true if has no cards
        if (cards.Count <=0)
            return true;

        var lastCard = cards[^1];
        // We want the adjacent card to be different color and the lower card has -1 number
        return lastCard.IsRed() ^ droppingCards[0].IsRed() && lastCard.cardNumber == droppingCards[0].cardNumber + 1;
    }
    public void DropCards(List<CardController> droppedCards, bool reverting = false)
    {
        if (droppedCards.Count <=0)
            return;

        // reset cards to its starting point if player dropped the cards on the same cardholder
        if (droppedCards[0].Holder == this)
        {
            foreach (CardController card in droppedCards)
            {
                card.ResetToPickUpPosition();
            }
            return;
        }

        // Confirmed dropping action
        // Now we do real pick up action
        CardHolderController fromCardHolder = droppedCards[0].Holder;

        droppedCards[0].Holder?.PickUpCards(droppedCards);

        // Then drop the cards
        if (cards.Count > 0)
            cards[^1].OnStackingCard(droppedCards);

        foreach (CardController card in droppedCards)
        {
            card.DroppedOnHolder(this);
            cards.Add(card);
        }

        // Succeed, call event
        if (!reverting)
            onSuccessfulMove?.Invoke(fromCardHolder, this, droppedCards);
    }

    public void PickUpCards(List<CardController> pickedUpCards, bool reverting = false)
    {
        if (pickedUpCards.Count <= 0)
            return;

        int index = cards.IndexOf(pickedUpCards[0]);
        
        if (index < 0)
             return;

        cards.RemoveRange(index, pickedUpCards.Count);
        if (cards.Count > 0)
            cards[^1].nextCard = null;
    }

}
