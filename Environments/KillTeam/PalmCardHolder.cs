
using Autohand;
using System.Collections.Generic;
using UnityEngine;

public class PalmCardHolder : MonoBehaviour
{
    [Header("Hands")]
    public Transform palm;
    public Hand selectionHand;

    [Header("Cards")]
    public GameObject cardPrefab;
    public List<CardData> cardData = new List<CardData>();

    [Header("Card Layout")]
    public float cardSpacing = 0.08f;
    public float fanAngle = 10f;
    public float fanCurve = 0.015f;
    public float heightAbovePalm = 0.04f;

    [Header("Palm Detection")]
    public Vector3 localPalmNormal = Vector3.up;

    [Range(-1f, 1f)]
    public float palmUpThreshold = 0.65f;

    [Header("Selection")]
    public PalmCard selectedCard;

    private readonly List<GameObject> cards =
        new List<GameObject>();

    private bool showingCards;

    private void Start()
    {
        if (palm == null)
            palm = transform;

        CreateCards();
        SetCardsVisible(false);
    }

    private void Update()
    {
        if (palm == null)
            return;

        Vector3 normal =
            palm.TransformDirection(localPalmNormal.normalized);

        bool palmUp =
            Vector3.Dot(normal, Vector3.up) >
            palmUpThreshold;

        if (palmUp != showingCards)
        {
            showingCards = palmUp;
            SetCardsVisible(showingCards);
        }
    }

    private void CreateCards()
    {
        if (cardPrefab == null)
        {
            Debug.LogError("Card prefab not assigned.");
            return;
        }

        int count = cardData.Count;
        float middle = (count - 1) * 0.5f;

        for (int i = 0; i < count; i++)
        {
            if (cardData[i] == null)
                continue;

            float offset = i - middle;

            Vector3 localPosition = new Vector3(
                offset * cardSpacing,
                heightAbovePalm,
                -Mathf.Abs(offset) * fanCurve);

            Quaternion localRotation = Quaternion.Euler(
                0f,
                offset * fanAngle,
                0f);

            GameObject card = Instantiate(
                cardPrefab,
                palm);

            card.transform.localPosition = localPosition;
            card.transform.localRotation = localRotation;

            PalmCard script = card.GetComponent<PalmCard>();

            if (script != null)
            {
                script.cardData = cardData[i];
                script.Initialise(this);
                script.UpdateDisplay();
            }

            Rigidbody rb = card.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            cards.Add(card);
        }
    }

    private void SetCardsVisible(bool visible)
    {
        foreach (GameObject card in cards)
        {
            if (card == null)
                continue;

            PalmCard script = card.GetComponent<PalmCard>();

            // Don't hide a card being held.
            if (script != null && script.IsSelected)
                continue;

            card.SetActive(visible);
        }
    }

    public bool IsSelectionHand(Hand hand)
    {
        return hand == selectionHand;
    }

    public void CardSelected(PalmCard card, Hand hand)
    {
        if (card == null || card.cardData == null)
            return;

        selectedCard = card;

        CardData data = card.cardData;

        Debug.Log("Selected: " + data.cardName);
        Debug.Log("Category: " + data.category);

        foreach (CardEffect effect in data.effects)
        {
            switch (effect.effectType)
            {
                case CardEffectType.AttackModifier:
                    Debug.Log("Attack bonus: +" + effect.value);
                    break;

                case CardEffectType.DefenceModifier:
                    Debug.Log("Defence bonus: +" + effect.value);
                    break;

                case CardEffectType.Damage:
                    Debug.Log("Damage: " + effect.value);
                    break;

                case CardEffectType.AreaDamage:
                    Debug.Log(
                        "Bomb damage: " + effect.value +
                        ", radius: " + effect.radiusInches + " inches");
                    break;

                case CardEffectType.Heal:
                    Debug.Log("Heal: " + effect.value);
                    break;

                case CardEffectType.MovementModifier:
                    Debug.Log("Movement bonus: " + effect.value);
                    break;

                case CardEffectType.Reroll:
                    Debug.Log("Reroll selected");
                    break;

                case CardEffectType.ApplyStatus:
                    Debug.Log("Apply status: " + effect.statusId);
                    break;

                case CardEffectType.SpecialRule:
                    Debug.Log("Special rule: " + data.ruleText);
                    break;
            }
        }
    }

    public void CardReturned(PalmCard card)
    {
        if (selectedCard == card)
            selectedCard = null;

        if (!showingCards && card != null)
            card.gameObject.SetActive(false);
    }
}
