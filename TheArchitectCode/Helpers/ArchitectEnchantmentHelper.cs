using BaseLib.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Powers.Architect;
using TheArchitect.TheArchitectCode.Relics;

namespace TheArchitect.TheArchitectCode.Helpers;

public enum ArchitectEnchantKind
{
    Adroit,
    Sharp,
    Nimble,
    Swift,
    Instinct,
    Vigorous,
    Momentum,
    Sown,
    Glam,
    PerfectFit,
    Steady,
    Slither,
    Corrupted,
    TezcatarasEmber,
    SoulsPower
}

public readonly record struct ArchitectEnchantOption(ArchitectEnchantKind Kind, int Amount);

public static class ArchitectEnchantmentHelper
{
    private static readonly ArchitectEnchantOption[] BasicEnchantPool =
    [
        new(ArchitectEnchantKind.Nimble, 3),
        new(ArchitectEnchantKind.Sharp, 3),
        new(ArchitectEnchantKind.Sown, 1),
        new(ArchitectEnchantKind.Swift, 2),
        new(ArchitectEnchantKind.Instinct, 1)
    ];

    public static IReadOnlyList<ArchitectEnchantOption> BasicEnchantOptionsFor(CardModel card)
    {
        return BasicEnchantPool.Where(option => CanApplyCanonicalEnchant(card, option.Kind)).ToArray();
    }

    public static bool CanTargetForRandomBasic(CardModel card)
    {
        return CanReceiveEnchantment(card) && BasicEnchantOptionsFor(card).Count > 0;
    }

    public static ArchitectEnchantOption? RandomBasicForCard(Player player, CardModel card)
    {
        IReadOnlyList<ArchitectEnchantOption> options = BasicEnchantOptionsFor(card);
        return options.Count == 0 ? null : player.RunState.Rng.CombatCardSelection.NextItem(options);
    }

    public static EnchantmentModel? AddRandomBasic(Player player, CardModel card)
    {
        ArchitectEnchantOption? option = RandomBasicForCard(player, card);
        return option == null ? null : Add(card, option.Value.Kind, option.Value.Amount);
    }

    public static IReadOnlyList<ArchitectEnchantKind> TemperingOptionsFor(CardModel card)
    {
        List<ArchitectEnchantKind> options = [];
        if (CanApplyTemperingOption(card, ArchitectEnchantKind.Sharp))
        {
            options.Add(ArchitectEnchantKind.Sharp);
        }

        if (CanApplyTemperingOption(card, ArchitectEnchantKind.Nimble))
        {
            options.Add(ArchitectEnchantKind.Nimble);
        }

        return options;
    }

    public static string DescribeTemperingTargets(IEnumerable<CardModel> cards)
    {
        int validTargets = 0;
        int dualTargets = 0;
        foreach (CardModel card in cards)
        {
            int options = TemperingOptionsFor(card).Count;
            if (options <= 0)
            {
                continue;
            }

            validTargets++;
            if (options > 1)
            {
                dualTargets++;
            }
        }

        if (validTargets == 0)
        {
            return "there are no valid targets";
        }

        string preview = $"{validTargets} valid {(validTargets == 1 ? "target" : "targets")}";
        if (dualTargets > 0)
        {
            preview += $"; {dualTargets} can choose either enchantment";
        }

        return preview;
    }

    public static bool CanTargetWithTempering(CardModel card)
    {
        if (TemperingOptionsFor(card).Count == 0)
        {
            return false;
        }

        return CanReceiveEnchantment(card);
    }

    public static bool CanTargetForSpecificEnchant(CardModel card, ArchitectEnchantKind kind)
    {
        if (!CanReceiveEnchantment(card))
        {
            return false;
        }

        return CanApplyCanonicalEnchant(card, kind);
    }

    public static bool CanReceiveTransferredEnchantment(CardModel target, CardModel source)
    {
        return source.Enchantment is { } enchantment && CanReceiveEnchantmentModel(target, enchantment);
    }

    public static string DescribeSigilbreakerDamage(CardModel card, IEnumerable<CardModel> handCards)
    {
        int enchantedInHand = CountOtherEnchantedCards(card, handCards);
        DamageVar damage = PreviewAttackDamageVar(card,
            card.DynamicVars.Damage.BaseValue + enchantedInHand * card.DynamicVars["BonusDamage"].BaseValue);
        LocString preview = new("cards", "THEARCHITECT-SIGILBREAKER.combatPreview");
        preview.Add(damage);
        return preview.GetFormattedText();
    }

    public static DamageVar PreviewAttackDamageVar(CardModel card, decimal baseDamage, string name = "Damage")
    {
        DamageVar preview = new(name, baseDamage, card.DynamicVars.Damage.Props);
        preview.EnchantedValue = card.DynamicVars.Damage.EnchantedValue +
            (baseDamage - card.DynamicVars.Damage.BaseValue);
        bool runGlobalHooks = card.IsMutable && card.CombatState != null &&
            card.Pile?.Type is PileType.Hand or PileType.Play;
        preview.UpdateCardPreview(card, CardPreviewMode.Normal,
            ArchitectDamagePreviewTarget.Target ?? card.CurrentTarget, runGlobalHooks);
        return preview;
    }

    public static EnchantmentModel Create(ArchitectEnchantKind kind)
    {
        return kind switch
        {
            ArchitectEnchantKind.Sharp => ModelDb.Enchantment<Sharp>().ToMutable(),
            ArchitectEnchantKind.Nimble => ModelDb.Enchantment<Nimble>().ToMutable(),
            ArchitectEnchantKind.Swift => ModelDb.Enchantment<Swift>().ToMutable(),
            ArchitectEnchantKind.Instinct => ModelDb.Enchantment<Instinct>().ToMutable(),
            ArchitectEnchantKind.Adroit => ModelDb.Enchantment<Adroit>().ToMutable(),
            ArchitectEnchantKind.Vigorous => ModelDb.Enchantment<Vigorous>().ToMutable(),
            ArchitectEnchantKind.Momentum => ModelDb.Enchantment<Momentum>().ToMutable(),
            ArchitectEnchantKind.Sown => ModelDb.Enchantment<Sown>().ToMutable(),
            ArchitectEnchantKind.Glam => ModelDb.Enchantment<Glam>().ToMutable(),
            ArchitectEnchantKind.PerfectFit => ModelDb.Enchantment<PerfectFit>().ToMutable(),
            ArchitectEnchantKind.Steady => ModelDb.Enchantment<Steady>().ToMutable(),
            ArchitectEnchantKind.Slither => ModelDb.Enchantment<Slither>().ToMutable(),
            ArchitectEnchantKind.Corrupted => ModelDb.Enchantment<Corrupted>().ToMutable(),
            ArchitectEnchantKind.TezcatarasEmber => ModelDb.Enchantment<TezcatarasEmber>().ToMutable(),
            ArchitectEnchantKind.SoulsPower => ModelDb.Enchantment<SoulsPower>().ToMutable(),
            _ => ModelDb.Enchantment<Sharp>().ToMutable()
        };
    }

    public static bool IsStackless(ArchitectEnchantKind kind) => kind is
        ArchitectEnchantKind.Instinct or ArchitectEnchantKind.PerfectFit or ArchitectEnchantKind.Steady or
        ArchitectEnchantKind.Slither or ArchitectEnchantKind.Corrupted or
        ArchitectEnchantKind.TezcatarasEmber or ArchitectEnchantKind.SoulsPower;

    public static int AmountFor(ArchitectEnchantKind kind)
    {
        return kind switch
        {
            ArchitectEnchantKind.Nimble => 3,
            ArchitectEnchantKind.Sharp => 3,
            ArchitectEnchantKind.Adroit => 2,
            ArchitectEnchantKind.Vigorous => 4,
            ArchitectEnchantKind.Momentum => 3,
            ArchitectEnchantKind.Sown => 1,
            ArchitectEnchantKind.Swift => 2,
            ArchitectEnchantKind.Instinct => 1,
            // Stackless enchantments still require a positive application amount.
            _ => 1
        };
    }

    public static bool HasAny(CardModel card) => card.Enchantment != null;

    public static bool HasInactive(CardModel card) => card.Enchantment?.Status == EnchantmentStatus.Disabled;

    public static bool Has<T>(CardModel card) where T : EnchantmentModel => card.Enchantment is T;

    public static EnchantmentModel? Get(CardModel card) => card.Enchantment;

    public static int Count(CardModel card) => HasAny(card) ? 1 : 0;

    public static bool IsUnenchanted(CardModel card) => !HasAny(card);

    public static bool CanReceiveEnchantment(CardModel card)
    {
        // Drowsy cards persist in the deck and cannot receive enchantments.
        return card is not Drowsy && !HasAny(card);
    }

    public static EnchantmentModel? Add(CardModel card, ArchitectEnchantKind kind, decimal amount, Player? enchanter = null)
    {
        if (!CanReceiveEnchantment(card)) return null;
        if ((enchanter ?? card.Owner)?.Creature.GetPower<InfiniteBlueprintPower>() != null)
            amount *= 2;

        EnchantmentModel? result = CardCmd.Enchant(Create(kind), card, amount);
        if (result != null)
        {
            ArchitectCombatState.RecordEnchanted(card);
            ArchitectEnchantmentReactions.AfterAdded(card);
        }
        return result;
    }

    // Copies an existing enchantment at its current amount, without applying combat multipliers again.
    public static EnchantmentModel? AddRaw(CardModel card, EnchantmentModel enchantment, decimal amount)
    {
        if (!CanReceiveEnchantment(card)) return null;
        EnchantmentModel? result = CardCmd.Enchant(enchantment, card, amount);
        if (result != null)
        {
            ArchitectCombatState.RecordEnchanted(card);
            ArchitectEnchantmentReactions.AfterAdded(card);
        }
        return result;
    }

    public static bool Remove(CardModel card)
    {
        if (!HasAny(card)) return false;
        CardCmd.ClearEnchantment(card);
        ArchitectEnchantmentReactions.AfterRemoved(card);
        return true;
    }

    public static int RemoveAll(IEnumerable<CardModel> cards)
    {
        int removed = 0;
        foreach (CardModel card in cards)
            if (Remove(card)) removed++;
        return removed;
    }

    public static void Refresh(CardModel card, bool triggerAutomaton = true)
    {
        if (card.Enchantment is not { } enchantment) return;
        enchantment.Status = EnchantmentStatus.Normal;
        ArchitectEnchantmentReactions.AfterRefreshed(card, triggerAutomaton);
    }

    public static int RefreshAll(IEnumerable<CardModel> cards)
    {
        int refreshed = 0;
        foreach (CardModel card in cards)
        {
            if (!HasAny(card)) continue;
            Refresh(card);
            refreshed++;
        }
        return refreshed;
    }

    public static void Transfer(CardModel from, CardModel to)
    {
        if (!CanReceiveTransferredEnchantment(to, from)) return;
        EnchantmentModel original = from.Enchantment!;
        EnchantmentModel copy = EnchantmentModel.FromSerializable(original.ToSerializable());
        decimal amount = original.Amount;
        Remove(from);
        AddRaw(to, copy, amount);
    }

    public static async Task<CardModel?> ChooseFromHand(PlayerChoiceContext choiceContext, Player player, string promptKey, Func<CardModel, bool>? filter, AbstractModel source, int min = 1)
    {
        return await ArchitectCardSelectionHelper.ChooseFromHand(choiceContext, player, promptKey, filter, source, min);
    }

    public static async Task<CardModel?> ChooseFromDrawPile(PlayerChoiceContext choiceContext, Player player, string promptKey, Func<CardModel, bool>? filter)
    {
        return await ArchitectCardSelectionHelper.ChooseFromDrawPile(choiceContext, player, promptKey, filter);
    }

    public static async Task<CardModel?> ChooseFromDiscard(PlayerChoiceContext choiceContext, Player player, string promptKey, Func<CardModel, bool>? filter)
    {
        return await ArchitectCardSelectionHelper.ChooseFromDiscard(choiceContext, player, promptKey, filter);
    }

    public static async Task<List<CardModel>> ChooseManyFromHand(PlayerChoiceContext choiceContext, Player player, string promptKey, int min, int max, Func<CardModel, bool>? filter, AbstractModel source)
    {
        return await ArchitectCardSelectionHelper.ChooseManyFromHand(choiceContext, player, promptKey, min, max, filter, source);
    }

    public static async Task<List<CardModel>> ChooseManyFromDrawPile(PlayerChoiceContext choiceContext, Player player, string promptKey, int min, int max, Func<CardModel, bool>? filter)
    {
        return await ArchitectCardSelectionHelper.ChooseManyFromDrawPile(choiceContext, player, promptKey, min, max, filter);
    }

    public static async Task<List<CardModel>> ChooseManyFromDeck(PlayerChoiceContext choiceContext, Player player, string promptKey, int min, int max, Func<CardModel, bool>? filter)
    {
        return await ArchitectCardSelectionHelper.ChooseManyFromDeck(choiceContext, player, promptKey, min, max, filter);
    }

    public static async Task<List<CardModel>> ChooseManyFromDiscard(PlayerChoiceContext choiceContext, Player player, string promptKey, int min, int max, Func<CardModel, bool>? filter)
    {
        return await ArchitectCardSelectionHelper.ChooseManyFromDiscard(choiceContext, player, promptKey, min, max, filter);
    }

    public static List<CardModel> Hand(Player player)
    {
        return PileType.Hand.GetPile(player).Cards.ToList();
    }

    public static List<CardModel> DrawPile(Player player)
    {
        return PileType.Draw.GetPile(player).Cards.ToList();
    }

    public static List<CardModel> DiscardPile(Player player)
    {
        return PileType.Discard.GetPile(player).Cards.ToList();
    }

    public static List<CardModel> Deck(Player player)
    {
        return PileType.Deck.GetPile(player).Cards.ToList();
    }

    public static IEnumerable<CardModel> AllPlayerCards(Player player)
    {
        IEnumerable<CardModel> combatCards = player.PlayerCombatState == null
            ? []
            : Hand(player)
                .Concat(DrawPile(player))
                .Concat(DiscardPile(player))
                .Concat(player.PlayerCombatState.ExhaustPile.Cards)
                .Concat(player.PlayerCombatState.PlayPile.Cards);

        return Deck(player).Concat(combatCards);
    }

    public static int CountEnchantedCardsInCombatPiles(Player player)
    {
        return Hand(player).Concat(DrawPile(player)).Concat(DiscardPile(player))
            .Concat(player.PlayerCombatState!.ExhaustPile.Cards).Distinct().Count(HasAny);
    }

    public static IReadOnlyList<ArchitectEnchantOption> CompatibleEnchantOptionsFor(CardModel card)
    {
        return Enum.GetValues<ArchitectEnchantKind>()
            .Where(kind => CanTargetForSpecificEnchant(card, kind))
            .Select(kind => new ArchitectEnchantOption(kind, AmountFor(kind))).ToArray();
    }

    public static bool CanTargetForRandomEnchant(CardModel card) => CompatibleEnchantOptionsFor(card).Count > 0;

    public static EnchantmentModel? AddRandomCompatible(Player enchanter, CardModel card)
    {
        IReadOnlyList<ArchitectEnchantOption> options = CompatibleEnchantOptionsFor(card);
        if (options.Count == 0) return null;
        ArchitectEnchantOption option = enchanter.RunState.Rng.CombatCardSelection.NextItem(options);
        return Add(card, option.Kind, option.Amount, enchanter);
    }

    public static int CountEnchantedCardsInHand(Player player)
    {
        return Hand(player).Count(HasAny);
    }

    public static int CountOtherEnchantedCards(CardModel card, IEnumerable<CardModel> cards)
    {
        return cards.Count(other => !ReferenceEquals(other, card) && HasAny(other));
    }

    public static async Task EnchantRandomCardsInHand(Player player, int count, Func<Player, ArchitectEnchantKind> selector, decimal amount)
    {
        List<CardModel> cards = Hand(player).ToList();
        for (int i = 0; i < count && cards.Count > 0; i++)
        {
            CardModel chosen = player.RunState.Rng.CombatCardSelection.NextItem(cards)!;
            cards.Remove(chosen);
            Add(chosen, selector(player), amount);
        }

        await Task.CompletedTask;
    }

    public static int EnchantAll(IEnumerable<CardModel> cards, ArchitectEnchantKind kind, decimal amount, Func<CardModel, bool>? filter = null)
    {
        int total = 0;
        foreach (CardModel card in cards.Where(filter ?? (_ => true)).Where(card => CanTargetForSpecificEnchant(card, kind)))
        {
            if (Add(card, kind, amount) != null)
            {
                total++;
            }
        }

        return total;
    }

    public static int EnchantAllRandom(IEnumerable<CardModel> cards, Player player, Func<Player, ArchitectEnchantKind> selector, decimal amount, Func<CardModel, bool>? filter = null)
    {
        int total = 0;
        foreach (CardModel card in cards.Where(filter ?? (_ => true)))
        {
            ArchitectEnchantKind kind = selector(player);
            if (!CanTargetForSpecificEnchant(card, kind))
            {
                continue;
            }

            if (Add(card, kind, amount) != null)
            {
                total++;
            }
        }

        return total;
    }

    public static async Task MoveToPile(CardModel card, PileType pileType, CardPilePosition position = CardPilePosition.Bottom, AbstractModel? source = null)
    {
        await CardPileCmd.Add(card, pileType, position, source);
    }

    public static async Task<CardModel?> ReturnRandomEnchantedDiscardToHand(PlayerChoiceContext choiceContext, Player player)
    {
        List<CardModel> cards = DiscardPile(player).Where(HasAny).ToList();
        if (cards.Count == 0)
        {
            return null;
        }

        CardModel chosen = player.RunState.Rng.CombatCardSelection.NextItem(cards)!;
        await MoveToPile(chosen, PileType.Hand);
        return chosen;
    }

    public static async Task AddDrowsy(Player player, int count, PileType pileType)
    {
        for (int i = 0; i < count; i++)
        {
            CardModel drowsy = player.Creature.CombatState!.CreateCard(ModelDb.Card<Drowsy>(), player);
            var result = await CardPileCmd.AddGeneratedCardToCombat(drowsy, pileType, player, pileType == PileType.Draw ? CardPilePosition.Random : CardPilePosition.Bottom);
            if (pileType is PileType.Draw or PileType.Discard)
            {
                // Generated pile cards need the fly-in preview to update the UI count.
                CardCmd.PreviewCardPileAdd(result);
            }
        }
    }

    public static async Task Swap(CardModel first, CardModel second)
    {
        if (first.Pile == null || second.Pile == null)
        {
            return;
        }

        PileType firstPile = first.Pile.Type;
        PileType secondPile = second.Pile.Type;
        await MoveToPile(first, secondPile);
        await MoveToPile(second, firstPile);
    }

    public static async Task Attack(CardModel card, PlayerChoiceContext choiceContext, Creature? target, decimal damage, ValueProp props = ValueProp.Move)
    {
        if (target == null)
        {
            return;
        }

        await DamageCmd.Attack(damage).FromCard(card).Targeting(target).Execute(choiceContext);
    }

    public static async Task AttackAll(CardModel card, PlayerChoiceContext choiceContext, decimal damage, int hits = 1, ValueProp props = ValueProp.Move)
    {
        await DamageCmd.Attack(damage)
            .WithHitCount(hits)
            .FromCard(card)
            .TargetingAllOpponents(card.CombatState!)
            .Execute(choiceContext);
    }

    public static async Task DamageAll(CardModel card, PlayerChoiceContext choiceContext, decimal damage, ValueProp props = ValueProp.Unpowered)
    {
        if (card.CombatState == null)
        {
            return;
        }

        await CreatureCmd.Damage(
            choiceContext,
            card.CombatState.HittableEnemies,
            damage,
            props,
            card.Owner.Creature,
            null);
    }

    public static async Task AttackAll(CardModel card, PlayerChoiceContext choiceContext, decimal damage, int hits, IEnumerable<Creature> targets, ValueProp props = ValueProp.Move)
    {
        for (int i = 0; i < hits; i++)
        {
            foreach (Creature enemy in targets)
            {
                await DamageCmd.Attack(damage).FromCard(card).Targeting(enemy).Execute(choiceContext);
            }
        }
    }

    public static async Task GainBlock(CardModel card, CardPlay play, decimal amount)
    {
        await CreatureCmd.GainBlock(card.Owner.Creature, amount, ValueProp.Move, play);
    }

    public static async Task ApplyWeak(PlayerChoiceContext choiceContext, Creature target, decimal amount, Creature source, CardModel card)
    {
        await PowerCmd.Apply<WeakPower>(choiceContext, target, amount, source, card);
    }

    public static async Task ApplyVulnerable(PlayerChoiceContext choiceContext, Creature target, decimal amount, Creature source, CardModel card)
    {
        await PowerCmd.Apply<VulnerablePower>(choiceContext, target, amount, source, card);
    }

    public static async Task GainStrength(PlayerChoiceContext choiceContext, Creature target, decimal amount, Creature source, CardModel? card)
    {
        await PowerCmd.Apply<StrengthPower>(choiceContext, target, amount, source, card);
    }

    private static bool CanApplyTemperingOption(CardModel card, ArchitectEnchantKind kind)
    {
        if (!IsBaseEnchantable(card))
        {
            return false;
        }

        return kind switch
        {
            ArchitectEnchantKind.Sharp => card.Type == CardType.Attack,
            ArchitectEnchantKind.Nimble => card.GainsBlock,
            _ => false
        };
    }

    private static bool IsBaseEnchantable(CardModel card)
    {
        CardType type = card.Type;
        if ((uint)(type - 4) <= 2u)
        {
            return false;
        }

        return card.Pile?.Type != PileType.Deck || !card.Keywords.Contains(CardKeyword.Unplayable);
    }

    public static IEnumerable<IHoverTip> HoverFor(ArchitectEnchantKind kind, int amount)
    {
        return kind switch
        {
            ArchitectEnchantKind.Sharp => HoverTipFactory.FromEnchantment<Sharp>(amount),
            ArchitectEnchantKind.Nimble => HoverTipFactory.FromEnchantment<Nimble>(amount),
            ArchitectEnchantKind.Swift => HoverTipFactory.FromEnchantment<Swift>(amount),
            ArchitectEnchantKind.Instinct => HoverTipFactory.FromEnchantment<Instinct>(amount),
            ArchitectEnchantKind.Adroit => HoverTipFactory.FromEnchantment<Adroit>(amount),
            ArchitectEnchantKind.Vigorous => HoverTipFactory.FromEnchantment<Vigorous>(amount),
            ArchitectEnchantKind.Momentum => HoverTipFactory.FromEnchantment<Momentum>(amount),
            ArchitectEnchantKind.Sown => HoverTipFactory.FromEnchantment<Sown>(amount),
            ArchitectEnchantKind.Glam => HoverTipFactory.FromEnchantment<Glam>(amount),
            ArchitectEnchantKind.PerfectFit => HoverTipFactory.FromEnchantment<PerfectFit>(amount),
            ArchitectEnchantKind.Steady => HoverTipFactory.FromEnchantment<Steady>(amount),
            ArchitectEnchantKind.Slither => HoverTipFactory.FromEnchantment<Slither>(amount),
            ArchitectEnchantKind.Corrupted => HoverTipFactory.FromEnchantment<Corrupted>(amount),
            ArchitectEnchantKind.TezcatarasEmber => HoverTipFactory.FromEnchantment<TezcatarasEmber>(amount),
            ArchitectEnchantKind.SoulsPower => HoverTipFactory.FromEnchantment<SoulsPower>(amount),
            _ => []
        };
    }

    private static EnchantmentModel Canonical(ArchitectEnchantKind kind)
    {
        return kind switch
        {
            ArchitectEnchantKind.Sharp => ModelDb.Enchantment<Sharp>(),
            ArchitectEnchantKind.Nimble => ModelDb.Enchantment<Nimble>(),
            ArchitectEnchantKind.Swift => ModelDb.Enchantment<Swift>(),
            ArchitectEnchantKind.Instinct => ModelDb.Enchantment<Instinct>(),
            ArchitectEnchantKind.Adroit => ModelDb.Enchantment<Adroit>(),
            ArchitectEnchantKind.Vigorous => ModelDb.Enchantment<Vigorous>(),
            ArchitectEnchantKind.Momentum => ModelDb.Enchantment<Momentum>(),
            ArchitectEnchantKind.Sown => ModelDb.Enchantment<Sown>(),
            ArchitectEnchantKind.Glam => ModelDb.Enchantment<Glam>(),
            ArchitectEnchantKind.PerfectFit => ModelDb.Enchantment<PerfectFit>(),
            ArchitectEnchantKind.Steady => ModelDb.Enchantment<Steady>(),
            ArchitectEnchantKind.Slither => ModelDb.Enchantment<Slither>(),
            ArchitectEnchantKind.Corrupted => ModelDb.Enchantment<Corrupted>(),
            ArchitectEnchantKind.TezcatarasEmber => ModelDb.Enchantment<TezcatarasEmber>(),
            ArchitectEnchantKind.SoulsPower => ModelDb.Enchantment<SoulsPower>(),
            _ => ModelDb.Enchantment<Sharp>()
        };
    }

    private static bool CanApplyCanonicalEnchant(CardModel card, ArchitectEnchantKind kind)
    {
        try
        {
            return Canonical(kind).CanEnchant(card);
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }

    private static bool CanReceiveEnchantmentModel(CardModel card, EnchantmentModel enchantment)
    {
        if (!CanReceiveEnchantment(card))
        {
            return false;
        }

        try
        {
            return enchantment.CanEnchant(card);
        }
        catch (KeyNotFoundException)
        {
            return false;
        }
    }
}
