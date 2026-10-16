
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(
    fileName = "CardDatabase",
    menuName = "Kill Team/Card Database")]
public class CardDatabase : ScriptableObject
{
    public List<CardData> cards = new List<CardData>();

    public CardData GetCard(string id)
    {
        return cards.Find(card =>
            card != null && card.cardId == id);
    }

    public List<CardData> GetFactionCards(string faction)
    {
        return cards.FindAll(card =>
            card != null && card.faction == faction);
    }

    public List<CardData> GetCards(CardCategory category)
    {
        return cards.FindAll(card =>
            card != null && card.category == category);
    }
}
