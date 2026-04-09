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
using TheArchitect.TheArchitectCode.Enchantments.Framework;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.TheArchitectCode.Helpers;

public enum ArchitectEnchantKind
{
    Sharp,
    Nimble,
    Swift,
    Instinct,
    Vitality,
    Momentum,
    Seed,
    Chromatic,
    PerfectFit,
    Stable,
    Serpentine,
    Corruption,
    Ember,
    SoulPower
}

public readonly record struct ArchitectEnchantOption(ArchitectEnchantKind Kind, int Amount);

public static class ArchitectEnchantmentHelper
{
    private static readonly ArchitectEnchantOption[] BasicEnchantPool =
    [
        new(ArchitectEnchantKind.Nimble, 2),
        new(ArchitectEnchantKind.Sharp, 2),
        new(ArchitectEnchantKind.Seed, 1),
        new(ArchitectEnchantKind.Swift, 2),
        new(ArchitectEnchantKind.Instinct, 2)
    ];

    public static IReadOnlyList<ArchitectEnchantOption> BasicEnchantOptionsFor(CardModel card)
    {
        return BasicEnchantPool.Where(option => Canonical(option.Kind).CanEnchant(card)).ToArray();
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

        if (kind == ArchitectEnchantKind.Momentum)
        {
            return true;
        }

        try
        {
            return Canonical(kind).CanEnchant(card);
        }
        catch (KeyNotFoundException)
        {
            return true;
        }
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
            ArchitectEnchantKind.Vitality => ModelDb.Enchantment<Vigorous>().ToMutable(),
            ArchitectEnchantKind.Momentum => ModelDb.Enchantment<Momentum>().ToMutable(),
            ArchitectEnchantKind.Seed => ModelDb.Enchantment<Sown>().ToMutable(),
            ArchitectEnchantKind.Chromatic => ModelDb.Enchantment<Glam>().ToMutable(),
            ArchitectEnchantKind.PerfectFit => ModelDb.Enchantment<PerfectFit>().ToMutable(),
            ArchitectEnchantKind.Stable => ModelDb.Enchantment<Steady>().ToMutable(),
            ArchitectEnchantKind.Serpentine => ModelDb.Enchantment<Slither>().ToMutable(),
            ArchitectEnchantKind.Corruption => ModelDb.Enchantment<Corrupted>().ToMutable(),
            ArchitectEnchantKind.Ember => ModelDb.Enchantment<TezcatarasEmber>().ToMutable(),
            ArchitectEnchantKind.SoulPower => ModelDb.Enchantment<SoulsPower>().ToMutable(),
            _ => ModelDb.Enchantment<Sharp>().ToMutable()
        };
    }

    public static int AmountFor(ArchitectEnchantKind kind)
    {
        return kind switch
        {
            ArchitectEnchantKind.Nimble => 2,
            ArchitectEnchantKind.Sharp => 2,
            ArchitectEnchantKind.Seed => 1,
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

    public static EnchantmentModel? Add(CardModel card, ArchitectEnchantKind kind, decimal amount)
    {
        EnchantmentModel? result = MultiEnchantHelper.TryAddEnchantment(card, Create(kind), amount);
        if (result != null)
        {
            ArchitectCombatState.RecordEnchanted(card);
            TriggerEnchantHooks(card);
        }

        return result;
    }

    public static void AddRaw(CardModel card, EnchantmentModel enchantment, decimal amount)
    {
        if (MultiEnchantHelper.TryAddEnchantment(card, enchantment, amount) != null)
        {
            ArchitectCombatState.RecordEnchanted(card);
            TriggerEnchantHooks(card);
        }
    }

    public static int RemoveAll(CardModel card)
    {
        int removed = MultiEnchantHelper.RemoveAllEnchantments(card);
        if (removed > 0)
        {
            TriggerRemoveHooks(card, removed);
        }

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
        List<EnchantmentModel> moved = GetAll(from)
            .Select(enchantment => EnchantmentModel.FromSerializable(enchantment.ToSerializable()))
            .ToList();
        RemoveAll(from);
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
        return (await CardSelectCmd.FromHand(
            choiceContext,
            player,
            new CardSelectorPrefs(new LocString("cards", promptKey), 1),
            filter,
            source)).FirstOrDefault();
    }

    public static async Task<CardModel?> ChooseFromDrawPile(PlayerChoiceContext choiceContext, Player player, string promptKey, Func<CardModel, bool>? filter)
    {
        List<CardModel> cards = PileType.Draw.GetPile(player).Cards.Where(filter ?? (_ => true)).ToList();
        return (await CardSelectCmd.FromSimpleGrid(choiceContext, cards, player, new CardSelectorPrefs(new LocString("cards", promptKey), 1))).FirstOrDefault();
    }

    public static async Task<CardModel?> ChooseFromDiscard(PlayerChoiceContext choiceContext, Player player, string promptKey, Func<CardModel, bool>? filter)
    {
        List<CardModel> cards = PileType.Discard.GetPile(player).Cards.Where(filter ?? (_ => true)).ToList();
        return (await CardSelectCmd.FromSimpleGrid(choiceContext, cards, player, new CardSelectorPrefs(new LocString("cards", promptKey), 1))).FirstOrDefault();
    }

    public static async Task<List<CardModel>> ChooseManyFromHand(PlayerChoiceContext choiceContext, Player player, string promptKey, int min, int max, Func<CardModel, bool>? filter, AbstractModel source)
    {
        return (await CardSelectCmd.FromHand(
            choiceContext,
            player,
            new CardSelectorPrefs(new LocString("cards", promptKey), min, max),
            filter,
            source)).ToList();
    }

    public static async Task<List<CardModel>> ChooseManyFromDrawPile(PlayerChoiceContext choiceContext, Player player, string promptKey, int min, int max, Func<CardModel, bool>? filter)
    {
        List<CardModel> cards = PileType.Draw.GetPile(player).Cards.Where(filter ?? (_ => true)).ToList();
        return (await CardSelectCmd.FromSimpleGrid(choiceContext, cards, player, new CardSelectorPrefs(new LocString("cards", promptKey), min, max))).ToList();
    }

    public static async Task<List<CardModel>> ChooseManyFromDeck(PlayerChoiceContext choiceContext, Player player, string promptKey, int min, int max, Func<CardModel, bool>? filter)
    {
        List<CardModel> cards = PileType.Deck.GetPile(player).Cards.Where(filter ?? (_ => true)).ToList();
        return (await CardSelectCmd.FromSimpleGrid(choiceContext, cards, player, new CardSelectorPrefs(new LocString("cards", promptKey), min, max))).ToList();
    }

    public static async Task<List<CardModel>> ChooseManyFromDiscard(PlayerChoiceContext choiceContext, Player player, string promptKey, int min, int max, Func<CardModel, bool>? filter)
    {
        List<CardModel> cards = PileType.Discard.GetPile(player).Cards.Where(filter ?? (_ => true)).ToList();
        return (await CardSelectCmd.FromSimpleGrid(choiceContext, cards, player, new CardSelectorPrefs(new LocString("cards", promptKey), min, max))).ToList();
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
        foreach (CardModel card in cards.Where(filter ?? (_ => true)))
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
            if (Add(card, selector(player), amount) != null)
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
            CardModel drowsy = player.Creature.CombatState!.CreateCard<Drowsy>(player);
            await CardPileCmd.AddGeneratedCardToCombat(drowsy, pileType, addedByPlayer: true, pileType == PileType.Draw ? CardPilePosition.Random : CardPilePosition.Bottom);
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

    public static async Task GainBlock(CardModel card, CardPlay play, decimal amount)
    {
        await CreatureCmd.GainBlock(card.Owner.Creature, amount, ValueProp.Move, play);
    }

    public static async Task ApplyWeak(Creature target, decimal amount, Creature source, CardModel card)
    {
        await PowerCmd.Apply<WeakPower>(target, amount, source, card);
    }

    public static async Task ApplyVulnerable(Creature target, decimal amount, Creature source, CardModel card)
    {
        await PowerCmd.Apply<VulnerablePower>(target, amount, source, card);
    }

    public static async Task GainStrength(Creature target, decimal amount, Creature source, CardModel? card)
    {
        await PowerCmd.Apply<StrengthPower>(target, amount, source, card);
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

    private static void TriggerEnchantHooks(CardModel card)
    {
        if (card.Owner?.Creature == null)
        {
            return;
        }

        SanctuaryPower? sanctuary = card.Owner.Creature.GetPower<SanctuaryPower>();
        if (sanctuary != null)
        {
            TaskHelper.RunSafely(CreatureCmd.GainBlock(card.Owner.Creature, sanctuary.Amount, ValueProp.Move, null));
        }
    }

    private static void TriggerRemoveHooks(CardModel card, int removed)
    {
        if (card.Owner?.Creature == null)
        {
            return;
        }

        DestroyerPower? destroyer = card.Owner.Creature.GetPower<DestroyerPower>();
        if (destroyer != null)
        {
            TaskHelper.RunSafely(PowerCmd.Apply<StrengthPower>(card.Owner.Creature, destroyer.Amount * removed, card.Owner.Creature, null));
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
            ArchitectEnchantKind.Vitality => HoverTipFactory.FromEnchantment<Vigorous>(amount),
            ArchitectEnchantKind.Momentum => HoverTipFactory.FromEnchantment<Momentum>(amount),
            ArchitectEnchantKind.Seed => HoverTipFactory.FromEnchantment<Sown>(amount),
            ArchitectEnchantKind.Chromatic => HoverTipFactory.FromEnchantment<Glam>(amount),
            ArchitectEnchantKind.PerfectFit => HoverTipFactory.FromEnchantment<PerfectFit>(amount),
            ArchitectEnchantKind.Stable => HoverTipFactory.FromEnchantment<Steady>(amount),
            ArchitectEnchantKind.Serpentine => HoverTipFactory.FromEnchantment<Slither>(amount),
            ArchitectEnchantKind.Corruption => HoverTipFactory.FromEnchantment<Corrupted>(amount),
            ArchitectEnchantKind.Ember => HoverTipFactory.FromEnchantment<TezcatarasEmber>(amount),
            ArchitectEnchantKind.SoulPower => HoverTipFactory.FromEnchantment<SoulsPower>(amount),
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
            ArchitectEnchantKind.Vitality => ModelDb.Enchantment<Vigorous>(),
            ArchitectEnchantKind.Momentum => ModelDb.Enchantment<Momentum>(),
            ArchitectEnchantKind.Seed => ModelDb.Enchantment<Sown>(),
            ArchitectEnchantKind.Chromatic => ModelDb.Enchantment<Glam>(),
            ArchitectEnchantKind.PerfectFit => ModelDb.Enchantment<PerfectFit>(),
            ArchitectEnchantKind.Stable => ModelDb.Enchantment<Steady>(),
            ArchitectEnchantKind.Serpentine => ModelDb.Enchantment<Slither>(),
            ArchitectEnchantKind.Corruption => ModelDb.Enchantment<Corrupted>(),
            ArchitectEnchantKind.Ember => ModelDb.Enchantment<TezcatarasEmber>(),
            ArchitectEnchantKind.SoulPower => ModelDb.Enchantment<SoulsPower>(),
            _ => ModelDb.Enchantment<Sharp>()
        };
    }
}
