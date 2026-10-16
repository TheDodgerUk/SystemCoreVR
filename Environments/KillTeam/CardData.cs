
using UnityEngine;
using System.Collections.Generic;

public enum CardCategory
{
    CustomAction,
    Operative,
    StrategicPloy,
    FirefightPloy,
    Equipment,
    FactionRule,
    Mission,
    Objective
}

public enum CardEffectType
{
    None,
    AttackModifier,
    DefenceModifier,
    Damage,
    Heal,
    MovementModifier,
    Reroll,
    AreaDamage,
    ApplyStatus,
    SpecialRule
}

[System.Serializable]
public class CardEffect
{
    public CardEffectType effectType;
    public int value;
    public float rangeInches;
    public float radiusInches;
    public string statusId;
}

[CreateAssetMenu(
    fileName = "NewCard",
    menuName = "Kill Team/Card")]
public class CardData : ScriptableObject
{
    [Header("Identity")]
    public string cardId;
    public string cardName;
    public string faction;
    public CardCategory category;

    [Header("Display")]
    [TextArea(3, 8)]
    public string description;
    public Sprite cardImage;

    [Header("Costs")]
    public int actionPointCost;
    public int commandPointCost;

    [Header("Effects")]
    public List<CardEffect> effects =
        new List<CardEffect>();

    [Header("Rules")]
    [TextArea(3, 10)]
    public string ruleText;

    public bool requiresTarget;
    public bool oncePerTurningPoint;
}
