using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalCardHolderController : CardHolderController
{
    public ECardSymbol cardSymbol;

    public override bool CanBeDropped(List<CardController> droppingCards)
    {
        // only accept same symbol and one card
        if (droppingCards.Count == 1 && droppingCards[0].cardSymbol == cardSymbol)
        {
            if (cards.Count == 0)
                return droppingCards[0].cardNumber == 1;
            else
                // topper card should have plus one number
                return droppingCards[0].cardNumber == cards[cards.Count - 1].cardNumber + 1;
        } 
        else
            return false;
    }
}
