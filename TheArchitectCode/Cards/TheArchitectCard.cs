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
            return path;
        }
    }

    public override string PortraitPath
    {
        get
        {
            string path = $"{Id.Entry.RemovePrefix().ToLowerInvariant()}.png".CardImagePath();
            return path;
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
    protected virtual bool ShufflesAfterPlay => ShuffleIntoDrawPileThisCombat;
    protected virtual CardPilePosition ShufflePosition => CardPilePosition.Random;

    protected virtual string GetCombatPreviewText()
    {
        return string.Empty;
    }

    protected string FormatCombatPreview(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        LocString preview = new("cards", "THEARCHITECT-COMBAT_PREVIEW");
        preview.Add("Preview", text);
        return preview.GetFormattedText();
    }

    protected string GetLocalizedCombatPreview(string key, params (string Name, decimal Value)[] args)
    {
        LocString preview = new("cards", key);
        foreach ((string name, decimal value) in args)
        {
            preview.Add(name, value);
        }

        return preview.GetFormattedText();
    }

    protected string GetLocalizedCombatDamagePreview(string key, decimal baseDamage)
    {
        LocString preview = new("cards", key);
        preview.Add(ArchitectEnchantmentHelper.PreviewAttackDamageVar(this, baseDamage));
        return preview.GetFormattedText();
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
        return Task.CompletedTask;
    }

    protected override PileType GetResultPileTypeForCardPlay()
    {
        if (ShufflesAfterPlay || ArchitectCombatState.ShouldShuffle(this))
        {
            return PileType.Draw;
        }

        return base.GetResultPileTypeForCardPlay();
    }

    public override (PileType, CardPilePosition) ModifyCardPlayResultPileTypeAndPosition(CardModel card, bool isAutoPlay, ResourceInfo resources, PileType pileType, CardPilePosition position)
    {
        return card == this && (ShufflesAfterPlay || ArchitectCombatState.ShouldShuffle(this))
            ? (PileType.Draw, ShufflePosition) : (pileType, position);
    }

    protected override void AddExtraArgsToDescription(LocString description)

    {
        base.AddExtraArgsToDescription(description);
        // The compendium renders canonical models; their Owner getter asserts
        // mutability even when no combat is active.
        bool inCombatHandCard = IsMutable && Pile?.Type == PileType.Hand &&
            (CombatState != null || Owner?.PlayerCombatState != null);
        description.Add("CombatPreview", inCombatHandCard ? FormatCombatPreview(GetCombatPreviewText()) : string.Empty);
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
            ArchitectEnchantKind.Adroit => new Adroit(),
            ArchitectEnchantKind.Sharp => new Sharp(),
            ArchitectEnchantKind.Nimble => new Nimble(),
            ArchitectEnchantKind.Swift => new Swift(),
            ArchitectEnchantKind.Instinct => new Instinct(),
            ArchitectEnchantKind.Vigorous => new Vigorous(),
            ArchitectEnchantKind.Momentum => new Momentum(),
            ArchitectEnchantKind.Sown => new Sown(),
            ArchitectEnchantKind.Glam => new Glam(),
            ArchitectEnchantKind.PerfectFit => new PerfectFit(),
            ArchitectEnchantKind.Steady => new Steady(),
            ArchitectEnchantKind.Slither => new Slither(),
            ArchitectEnchantKind.Corrupted => new Corrupted(),
            ArchitectEnchantKind.TezcatarasEmber => new TezcatarasEmber(),
            ArchitectEnchantKind.SoulsPower => new SoulsPower(),
            _ => new Sharp()
        };

        SetIsMutableMethod.Invoke(enchantment, [true]);
        return enchantment;
    }
}
