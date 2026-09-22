using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheArchitect.TheArchitectCode.Enchantments.Framework;

public sealed class MultiEnchantProxy : EnchantmentModel
{
    public const string CompositeIconPath = "res://TheArchitect/images/powers/power.png";

    private const string ChildrenDescriptionKey = "ChildrenDescription";
    private const string ChildrenExtraKey = "ChildrenExtra";

    private readonly List<EnchantmentModel> _children = [];
    private bool _childrenInitialized;

    [SavedProperty]
    public string SerializedChildren { get; set; } = string.Empty;

    public override bool HasExtraCardText => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
    [
        new StringVar(ChildrenDescriptionKey),
        new StringVar(ChildrenExtraKey)
    ];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        GetChildren().SelectMany(static child => child.HoverTips).Skip(1).ToList();

    public IReadOnlyList<EnchantmentModel> GetChildren()
    {
        EnsureChildrenInitialized(false);
        return _children;
    }

    public EnchantmentModel? GetChildOfType(Type type)
    {
        return GetChildren().FirstOrDefault(child => child.GetType() == type);
    }

    public void AddChild(EnchantmentModel child, decimal amount)
    {
        EnsureChildrenInitialized(false);
        child.AssertMutable();
        child.Amount = (int)amount;
        child.ApplyInternal(Card, amount);
        _children.Add(child);
        SerializedChildren = MultiEnchantSerialization.SerializeChildren(_children);
        child.ModifyCard();
        RecalculateValues();
        Card.DynamicVars.RecalculateForUpgradeOrEnchant();
    }

    public override void RecalculateValues()
    {
        EnsureChildrenInitialized(false);
        ((StringVar)DynamicVars[ChildrenDescriptionKey]).StringValue = BuildChildrenDescription();
        ((StringVar)DynamicVars[ChildrenExtraKey]).StringValue = BuildChildrenExtraCardText();
    }

    protected override void OnEnchant()
    {
        SetCompositeIconPath();
        EnsureChildrenInitialized(true);
    }

    public override decimal EnchantDamageAdditive(decimal originalDamage, ValueProp props)
    {
        return GetChildren().Sum(child => child.EnchantDamageAdditive(originalDamage, props));
    }

    public override decimal EnchantDamageMultiplicative(decimal originalDamage, ValueProp props)
    {
        decimal total = 1m;
        foreach (EnchantmentModel child in GetChildren())
        {
            total *= child.EnchantDamageMultiplicative(originalDamage, props);
        }

        return total;
    }

    public override decimal EnchantBlockAdditive(decimal originalBlock)
    {
        return GetChildren().Sum(child => child.EnchantBlockAdditive(originalBlock));
    }

    public override decimal EnchantBlockMultiplicative(decimal originalBlock)
    {
        decimal total = 1m;
        foreach (EnchantmentModel child in GetChildren())
        {
            total *= child.EnchantBlockMultiplicative(originalBlock);
        }

        return total;
    }

    public override int EnchantPlayCount(int originalPlayCount)
    {
        int playCount = originalPlayCount;
        foreach (EnchantmentModel child in GetChildren())
        {
            playCount = child.EnchantPlayCount(playCount);
        }

        return playCount;
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
        foreach (EnchantmentModel child in GetChildren())
        {
            await child.OnPlay(choiceContext, cardPlay);
            child.InvokeExecutionFinished();
        }

        RecalculateValues();
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext context, CardPlay cardPlay)
    {
        foreach (EnchantmentModel child in GetChildren())
        {
            await child.AfterCardPlayed(context, cardPlay);
            child.InvokeExecutionFinished();
        }

        RecalculateValues();
    }

    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        foreach (EnchantmentModel child in GetChildren())
        {
            await child.AfterCardDrawn(choiceContext, card, fromHandDraw);
            child.InvokeExecutionFinished();
        }

        RecalculateValues();
    }

    public override async Task AfterAutoPrePlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
    {
        foreach (EnchantmentModel child in GetChildren())
        {
            await child.AfterAutoPrePlayPhaseEntered(choiceContext, player);
            child.InvokeExecutionFinished();
        }

        RecalculateValues();
    }

    public override void ModifyShuffleOrder(Player player, List<CardModel> cards, bool isInitialShuffle)
    {
        foreach (EnchantmentModel child in GetChildren())
        {
            child.ModifyShuffleOrder(player, cards, isInitialShuffle);
        }
    }

    public override bool ShouldGlowGold => GetChildren().Any(static child => child.ShouldGlowGold);

    public override bool ShouldGlowRed => GetChildren().Any(static child => child.ShouldGlowRed);

    private void EnsureChildrenInitialized(bool applyCardMutations)
    {
        if (_childrenInitialized)
        {
            if (applyCardMutations)
            {
                ReapplyChildMutations();
            }

            return;
        }

        _children.Clear();
        foreach (SerializableEnchantment childData in MultiEnchantSerialization.DeserializeChildren(SerializedChildren))
        {
            EnchantmentModel child = MultiEnchantSerialization.ToMutableChild(childData);
            child.ApplyInternal(Card, child.Amount);
            _children.Add(child);
        }

        _childrenInitialized = true;

        if (applyCardMutations)
        {
            ReapplyChildMutations();
        }
    }

    private void ReapplyChildMutations()
    {
        foreach (EnchantmentModel child in _children)
        {
            child.ModifyCard();
        }
    }

    private string BuildChildrenDescription()
    {
        return string.Join(
            "\n",
            GetChildren()
                .Select(child => $"[gold]{child.Title.GetFormattedText()}[/gold]: {child.DynamicDescription.GetFormattedText()}")
                .Where(static part => !string.IsNullOrWhiteSpace(part))
        );
    }

    private string BuildChildrenExtraCardText()
    {
        return string.Join(
            "\n",
            GetChildren()
                .Select(child => child.DynamicExtraCardText?.GetFormattedText())
                .Where(static extra => !string.IsNullOrWhiteSpace(extra))
        );
    }

    private void SetCompositeIconPath()
    {
        FieldInfo? iconPathField = typeof(EnchantmentModel).GetField("_iconPath", BindingFlags.Instance | BindingFlags.NonPublic);
        iconPathField?.SetValue(this, CompositeIconPath);
    }
}
