using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public enum ECardSymbol
{
    Heart,
    Diamond,
    Club,
    Spade
}

[RequireComponent(typeof(Collider2D))]
public class CardController : MonoBehaviour, IDragHandler, IEndDragHandler, IBeginDragHandler
{
    [NonSerialized] public CardController nextCard = null;
    public ECardSymbol cardSymbol;
    [Range(1,13)] public ushort cardNumber;

    [SerializeField] private SpriteRenderer sprite;
    [SerializeField] private Collider2D cardCollider;

    private CardHolderController holder = null;
    private Vector3 dragDeltaPosition;
    private Vector3 pickedUpPosition;
    private bool isVaildDragging = false;

    public CardHolderController Holder { get => holder; }

    void LoadSprite()
    {
        string sprintName = cardNumber.ToString();
        switch (cardSymbol)
        {
            case ECardSymbol.Heart:
                sprintName += 'H';
                break;
            case ECardSymbol.Spade:
                sprintName += 'S';
                break;
            case ECardSymbol.Diamond:
                sprintName += 'D';
                break;
            case ECardSymbol.Club:
                sprintName += 'C';
                break;
        }

        sprite.sprite = Resources.Load<Sprite>("Images/" + sprintName);
    }

    void OnValidate() 
    {
        LoadSprite();
    }

    // Start is called before the first frame update
    void Start()
    {
        LoadSprite();
    }

    public List<CardController> GetStackingCards()
    {
        var result = new List<CardController>() { this };

        var card = nextCard;

        while(card != null)
        {
            result.Add(card);
            card = card.nextCard;
        }

        return result;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isVaildDragging)
            return;

        Vector3 worldLocation = Camera.main.ScreenToWorldPoint(eventData.position);
        worldLocation.z = 0;

        var card = this;
        while (card != null)
        {
            var newPosition = worldLocation + card.dragDeltaPosition;
            // make sure the selecting cards show above everything
            card.transform.position = new Vector3(newPosition.x, newPosition.y, card.pickedUpPosition.z - 1);
            card = card.nextCard;
        }
        
    }

    virtual protected bool CanBeDragged()
    {
        if (GameManager.Instance.lockingCards)
            return false;

        var stackingCards = GetStackingCards();

        var card = this;
        foreach (var nextCard in stackingCards)
        {
            if (card == nextCard)
                continue;

            if (card.IsRed() == nextCard.IsRed() || card.cardNumber != nextCard.cardNumber + 1)
                return false;

            card = nextCard;
        }

        return true;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanBeDragged())
            return;

        isVaildDragging = true;

        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(eventData.pressPosition);

        var card = this;
        while (card != null)
        {
            // Set the offset position to mouse and original position for each card
            card.pickedUpPosition = card.transform.position;
            card.dragDeltaPosition = card.transform.position - mousePosition;
            card.dragDeltaPosition.z = 0;
            card = card.nextCard;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isVaildDragging)
            return;

        isVaildDragging = false;

        List<Collider2D> overlapingColliders = new();
        ContactFilter2D contactFilter = new()
        {
            layerMask = LayerMask.NameToLayer("Cards")
        };
        cardCollider.OverlapCollider(contactFilter, overlapingColliders);

        foreach (var collider in overlapingColliders)
        {
            if (!collider.TryGetComponent<IMoveCards>(out var holder))
                continue;

            if (holder.CanBeDropped(GetStackingCards()))
            {
                // dropping confirmed
                holder.DropCards(GetStackingCards());
                return;
            }
        }

        // failed, reset to their original position
        var card = this;
        while (card != null)
        {
            card.ResetToPickUpPosition();
            card = card.nextCard;
        }
    }

    public void ResetToPickUpPosition()
    {
        transform.position = pickedUpPosition;
    }

    public void DroppedOnHolder(CardHolderController newHolder)
    {
        holder = newHolder;

        if (holder.cards.Count <= 0)
            transform.position = holder.anchorPoint.position;
        else
        {
           transform.position = holder.cards[^1].transform.position - new Vector3(0, holder.spaceBetweenCards, 0);
           transform.position = new Vector3(transform.position.x, transform.position.y, transform.position.z - 0.01f);
        }
    }

    public void OnStackingCard(List<CardController> newCards)
    {
        if (newCards.Count <= 0) return;
        nextCard = newCards[0];
    }

    public bool IsBlack()
    {
        return cardSymbol == ECardSymbol.Club || cardSymbol == ECardSymbol.Spade;
    }

    public bool IsRed()
    {
        return cardSymbol == ECardSymbol.Heart || cardSymbol == ECardSymbol.Diamond;
    }
}
