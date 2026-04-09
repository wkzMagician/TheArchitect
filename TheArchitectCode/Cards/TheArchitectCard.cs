using System.Collections.Generic;
using System.Reflection;
using BaseLib.Abstracts;
using BaseLib.Extensions;
using BaseLib.Utils;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using TheArchitect.TheArchitectCode.Character;
using TheArchitect.TheArchitectCode.Extensions;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards;

[Pool(typeof(TheArchitectCardPool))]
public abstract class TheArchitectCard(int cost, CardType type, CardRarity rarity, TargetType target) :
    CustomCardModel(cost, type, rarity, target)
{
    private static readonly MethodInfo SetIsMutableMethod =
        typeof(AbstractModel).GetMethod("NeverEverCallThisOutsideOfTests_SetIsMutable", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private bool _startingEnchantmentsApplied;

    public override string CustomPortraitPath
    {
        get
        {
            string path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".BigCardImagePath();
            return ResourceLoader.Exists(path) ? path : "strike_architect.png".BigCardImagePath();
        }
    }

    public override string PortraitPath
    {
        get
        {
            string path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
            return ResourceLoader.Exists(path) ? path : "strike_architect.png".CardImagePath();
        }
    }

    public override string BetaPortraitPath
    {
        get
        {
            string path = $"beta/{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
            return ResourceLoader.Exists(path) ? path : PortraitPath;
        }
    }

    protected virtual IEnumerable<(ArchitectEnchantKind Kind, int Amount)> StartingEnchantments => [];

    protected int TimesPlayedThisCombat { get; private set; }

    protected bool WasEnchantedAtCombatStart { get; private set; }

    protected bool ShuffleIntoDrawPileThisCombat { get; set; }

    protected virtual string GetCombatPreviewText()
    {
        return string.Empty;
    }

    protected string FormatCombatPreview(string text)
    {
        return string.IsNullOrWhiteSpace(text) ? string.Empty : $" (Currently {text})";
    }

    protected static string CountNoun(int count, string singular, string? plural = null)
    {
        return $"{count} {(count == 1 ? singular : plural ?? $"{singular}s")}";
    }

    public void EnableShuffleIntoDrawPile()
    {
        ShuffleIntoDrawPileThisCombat = true;
    }

    public override Task AfterCardEnteredCombat(CardModel card)
    {
        if (card != this)
        {
            return Task.CompletedTask;
        }

        EnsureStartingEnchantmentsApplied();
        return Task.CompletedTask;
    }

    public override Task BeforeCombatStart()
    {
        EnsureStartingEnchantmentsApplied();
        return Task.CompletedTask;
    }

    public override Task AfterCardPlayed(MegaCrit.Sts2.Core.GameActions.Multiplayer.PlayerChoiceContext context, CardPlay cardPlay)
    {
        if (cardPlay.Card != this)
        {
            return Task.CompletedTask;
        }

        TimesPlayedThisCombat++;
        ArchitectCombatState.RecordPlayed(this);
        return Task.CompletedTask;
    }

    protected override PileType GetResultPileType()
    {
        if (ShuffleIntoDrawPileThisCombat)
        {
            return PileType.Draw;
        }

        return base.GetResultPileType();
    }

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        bool inCombat = CombatState != null || Owner?.PlayerCombatState != null;
        description.Add("CombatPreview", inCombat ? FormatCombatPreview(GetCombatPreviewText()) : string.Empty);
    }

    protected virtual void ApplyStartingEnchantments()
    {
        foreach ((ArchitectEnchantKind kind, int amount) in StartingEnchantments)
        {
            ApplyStartingEnchantment(kind, amount);
        }
    }

    protected void ApplyStartingEnchantment(ArchitectEnchantKind kind, int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        if (Enchantment != null)
        {
            return;
        }

        try
        {
            if (ArchitectEnchantmentHelper.Add(this, kind, amount) != null)
            {
                return;
            }
        }
        catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException or NullReferenceException)
        {
        }

        if (Enchantment != null)
        {
            return;
        }

        EnchantmentModel enchantment = CreateFallbackEnchantment(kind);
        EnchantInternal(enchantment, amount);
        enchantment.ModifyCard();
        FinalizeUpgradeInternal();
    }

    private void EnsureStartingEnchantmentsApplied()
    {
        if (_startingEnchantmentsApplied)
        {
            return;
        }

        _startingEnchantmentsApplied = true;
        ApplyStartingEnchantments();
        WasEnchantedAtCombatStart = ArchitectEnchantmentHelper.HasAny(this);
    }

    private static EnchantmentModel CreateFallbackEnchantment(ArchitectEnchantKind kind)
    {
        EnchantmentModel enchantment = kind switch
        {
            ArchitectEnchantKind.Sharp => new Sharp(),
            ArchitectEnchantKind.Nimble => new Nimble(),
            ArchitectEnchantKind.Swift => new Swift(),
            ArchitectEnchantKind.Instinct => new Instinct(),
            ArchitectEnchantKind.Vitality => new Vigorous(),
            ArchitectEnchantKind.Momentum => new Momentum(),
            ArchitectEnchantKind.Seed => new Sown(),
            ArchitectEnchantKind.Chromatic => new Glam(),
            ArchitectEnchantKind.PerfectFit => new PerfectFit(),
            ArchitectEnchantKind.Stable => new Steady(),
            ArchitectEnchantKind.Serpentine => new Slither(),
            ArchitectEnchantKind.Corruption => new Corrupted(),
            ArchitectEnchantKind.Ember => new TezcatarasEmber(),
            ArchitectEnchantKind.SoulPower => new SoulsPower(),
            _ => new Sharp()
        };

        SetIsMutableMethod.Invoke(enchantment, [true]);
        return enchantment;
    }
}
