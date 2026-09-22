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
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Enchantments;
using TheArchitect.TheArchitectCode.Enchantments.Framework;
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
        new(ArchitectEnchantKind.Nimble, 2),
        new(ArchitectEnchantKind.Sharp, 2),
        new(ArchitectEnchantKind.Sown, 1),
        new(ArchitectEnchantKind.Swift, 2),
        new(ArchitectEnchantKind.Instinct, 2)
    ];

    public static IReadOnlyList<ArchitectEnchantOption> BasicEnchantOptionsFor(CardModel card)
    {
        return BasicEnchantPool.Where(option => CanApplyCanonicalEnchant(card, option.Kind)).ToArray();
    }

    public static bool CanTargetForRandomBasic(CardModel card)
    {
        return CanReceiveAnotherEnchant(card) && BasicEnchantOptionsFor(card).Count > 0;
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

        return CanReceiveAnotherEnchant(card);
    }

    public static bool CanTargetForSpecificEnchant(CardModel card, ArchitectEnchantKind kind)
    {
        if (!CanReceiveAnotherEnchant(card))
        {
            return false;
        }

        return CanApplyCanonicalEnchant(card, kind);
    }

    public static bool CanReceiveTransferredEnchantments(CardModel target, CardModel source)
    {
        IReadOnlyList<EnchantmentModel> sourceEnchantments = GetAll(source);
        if (sourceEnchantments.Count == 0)
        {
            return false;
        }

        foreach (EnchantmentModel enchantment in sourceEnchantments)
        {
            if (!CanReceiveEnchantmentModel(target, enchantment))
            {
                return false;
            }
        }

        return true;
    }

    public static string DescribeSigilbreakerDamage(CardModel card, IEnumerable<CardModel> handCards)
    {
        int enchantedInHand = CountOtherEnchantedCards(card, handCards);
        int damage = card.DynamicVars.Damage.IntValue + enchantedInHand * card.DynamicVars["BonusDamage"].IntValue;
        return $"deals {damage} damage";
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

    public static int AmountFor(ArchitectEnchantKind kind)
    {
        return kind switch
        {
            ArchitectEnchantKind.Nimble => 2,
            ArchitectEnchantKind.Sharp => 2,
            ArchitectEnchantKind.Sown => 1,
            ArchitectEnchantKind.Swift => 2,
            ArchitectEnchantKind.Instinct => 2,
            _ => 1
        };
    }

    public static bool HasAny(CardModel card)
    {
        return MultiEnchantHelper.HasAnyEnchantments(card);
    }

    public static bool Has<T>(CardModel card) where T : EnchantmentModel
    {
        return MultiEnchantHelper.GetEnchantments(card).Any(enchant => enchant is T);
    }

    public static IReadOnlyList<EnchantmentModel> GetAll(CardModel card)
    {
        return MultiEnchantHelper.GetEnchantments(card);
    }

    public static int Count(CardModel card)
    {
        return GetAll(card).Count;
    }

    public static bool IsUnenchanted(CardModel card)
    {
        return !HasAny(card);
    }

    public static bool CanReceiveAnotherEnchant(CardModel card)
    {
        return !HasAny(card) || MultiEnchantRegistry.SupportsMultiEnchant(card);
    }

    public static EnchantmentModel? Add(CardModel card, ArchitectEnchantKind kind, decimal amount, Player? enchanter = null)
    {
        if ((enchanter ?? card.Owner)?.Creature.GetPower<InfiniteBlueprintPower>() != null)
        {
            amount *= 2;
        }
        bool wasUnenchanted = !HasAny(card);
        EnchantmentModel? result = MultiEnchantHelper.TryAddEnchantment(card, Create(kind), amount);
        if (result != null)
        {
            ArchitectCombatState.RecordEnchanted(card);
            TriggerEnchantHooks(card, wasUnenchanted);
        }

        return result;
    }

    public static void AddRaw(CardModel card, EnchantmentModel enchantment, decimal amount)
    {
        bool wasUnenchanted = !HasAny(card);
        if (MultiEnchantHelper.TryAddEnchantment(card, enchantment, amount) != null)
        {
            ArchitectCombatState.RecordEnchanted(card);
            TriggerEnchantHooks(card, wasUnenchanted);
        }
    }

    public static int RemoveAll(CardModel card)
    {
        int removed = MultiEnchantHelper.RemoveAllEnchantments(card);
        if (removed > 0)
        {
            TriggerRemoveHooks(card);
        }

        return removed;
    }

    public static int RemoveWhere(CardModel card, Func<EnchantmentModel, bool> predicate)
    {
        List<EnchantmentModel> kept = GetAll(card)
            .Where(enchantment => !predicate(enchantment))
            .Select(enchantment => EnchantmentModel.FromSerializable(enchantment.ToSerializable()))
            .ToList();

        int removed = GetAll(card).Count - kept.Count;
        if (removed <= 0)
        {
            return 0;
        }

        MultiEnchantHelper.RemoveAllEnchantments(card);
        foreach (EnchantmentModel enchantment in kept)
        {
            MultiEnchantHelper.TryAddEnchantment(card, enchantment, enchantment.Amount);
        }
        TriggerRemoveHooks(card);

        return removed;
    }

    public static void Refresh(CardModel card)
    {
        foreach (EnchantmentModel enchantment in GetAll(card))
        {
            enchantment.Status = EnchantmentStatus.Normal;
        }
    }

    public static int RefreshAll(IEnumerable<CardModel> cards)
    {
        int refreshed = 0;
        foreach (CardModel card in cards.Where(HasAny))
        {
            Refresh(card);
            refreshed++;
        }

        return refreshed;
    }

    public static void Transfer(CardModel from, CardModel to)
    {
        if (!CanReceiveTransferredEnchantments(to, from))
        {
            return;
        }

        List<EnchantmentModel> moved = GetAll(from)
            .Select(enchantment => EnchantmentModel.FromSerializable(enchantment.ToSerializable()))
            .ToList();
        MultiEnchantHelper.RemoveAllEnchantments(from);
        foreach (EnchantmentModel enchantment in moved)
        {
            AddRaw(to, enchantment, enchantment.Amount);
        }
    }

    public static int RemoveAll(IEnumerable<CardModel> cards)
    {
        int removed = 0;
        foreach (CardModel card in cards)
        {
            if (RemoveAll(card) > 0)
            {
                removed++;
            }
        }

        return removed;
    }

    public static async Task<CardModel?> ChooseFromHand(PlayerChoiceContext choiceContext, Player player, string promptKey, Func<CardModel, bool>? filter, AbstractModel source)
    {
        return await ArchitectCardSelectionHelper.ChooseFromHand(choiceContext, player, promptKey, filter, source);
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
            await CardPileCmd.AddGeneratedCardToCombat(drowsy, pileType, player, pileType == PileType.Draw ? CardPilePosition.Random : CardPilePosition.Bottom);
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
        for (int i = 0; i < hits; i++)
        {
            foreach (Creature enemy in card.CombatState!.HittableEnemies)
            {
                await DamageCmd.Attack(damage).FromCard(card).Targeting(enemy).Execute(choiceContext);
            }
        }
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

    private static void TriggerEnchantHooks(CardModel card, bool wasUnenchanted)
    {
        if (card.Owner?.Creature == null)
        {
            return;
        }

        if (card.CombatState == null || card.Pile?.Type == PileType.Deck) return;
        if (wasUnenchanted)
        {
            foreach (CalibrationRuler relic in card.Owner.Relics.OfType<CalibrationRuler>())
                ArchitectEffectQueue.Track(card.Owner, relic.OnFirstEnchantment(card));
        }

        SanctuaryPower? sanctuary = card.Owner.Creature.GetPower<SanctuaryPower>();
        if (sanctuary != null)
        {
            ArchitectEffectQueue.Track(card.Owner, CreatureCmd.GainBlock(card.Owner.Creature, sanctuary.Amount, ValueProp.Move, null));
        }
    }

    private static void TriggerRemoveHooks(CardModel card)
    {
        if (card.Owner?.Creature == null)
        {
            return;
        }

        if (card.CombatState == null || card.Pile?.Type == PileType.Deck) return;
        foreach (DismantlingPliers relic in card.Owner.Relics.OfType<DismantlingPliers>())
            ArchitectEffectQueue.Track(card.Owner, relic.OnActiveRemoval(card));

        DestroyerPower? destroyer = card.Owner.Creature.GetPower<DestroyerPower>();
        if (destroyer != null)
        {
            ArchitectEffectQueue.Track(card.Owner, PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), card.Owner.Creature, destroyer.Amount, card.Owner.Creature, null));
        }
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
        if (!CanReceiveAnotherEnchant(card))
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
