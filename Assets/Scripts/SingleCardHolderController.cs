using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleCardHolderController : CardHolderController
{
    public override bool CanBeDropped(List<CardController> droppingCards)
    {
        return cards.Count <= 0 && droppingCards.Count <= 1;
    }
}
