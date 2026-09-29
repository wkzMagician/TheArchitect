using System.Reflection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards.Mocks;
using MegaCrit.Sts2.Core.Models.Encounters.Mocks;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Multiplayer;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.TestSupport;
using MegaCrit.Sts2.Core.Unlocks;
using TheArchitect.TheArchitectCode.Cards;
using TheArchitect.TheArchitectCode.Character;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.Tests.Infrastructure;

public sealed class CombatTestContext : IDisposable
{
    private static readonly MethodInfo OnPlayMethod = typeof(CardModel).GetMethod("OnPlay", BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly MethodInfo ResultPileMethod = typeof(CardModel).GetMethod("GetResultPileTypeForCardPlay", BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly PropertyInfo CombatInProgressProperty = typeof(CombatManager).GetProperty("IsInProgress", BindingFlags.Instance | BindingFlags.Public)!;
    private static readonly PropertyInfo RunManagerStateProperty = typeof(RunManager).GetProperty("State", BindingFlags.Instance | BindingFlags.NonPublic)!;
    private static readonly MethodInfo CombatAddCardMethod = typeof(CombatState).GetMethod("AddCard", BindingFlags.Instance | BindingFlags.NonPublic)!;

    private readonly IDisposable _selectorScope;

    public RunState RunState { get; }
    public CombatState CombatState { get; }
    public BlockingPlayerChoiceContext ChoiceContext { get; } = new();
    public TestCardSelector Selector { get; } = new();
    public Player Player { get; }
    public Player? Ally { get; }
    public Creature Enemy { get; }
    public Creature? SecondEnemy { get; }

    public CombatTestContext(bool includeAlly = false, bool includeSecondEnemy = false)
    {
        TestMode.TurnOnInternal();
        LocalContext.NetId = 1;
        EndPreviousRun();

        Player = CreatePlayer(1);
        Ally = includeAlly ? CreatePlayer(2) : null;
        List<Player> players = [Player];
        if (Ally != null)
        {
            players.Add(Ally);
        }

        RunState = RunState.CreateForTest(players);
        RunManager.Instance.SetUpTest(RunState, new NetSingleplayerGameService());
        CombatState = new CombatState(ModelDb.Encounter<MockMonsterEncounter>().ToMutable(), RunState, RunState.Modifiers, RunState.BadgeModels, RunState.MultiplayerScalingModel);
        foreach (Player player in players)
        {
            CombatState.AddPlayer(player);
        }

        Enemy = CombatState.CreateCreature(ModelDb.Monster<BigDummy>().ToMutable(), CombatSide.Enemy, null);
        CombatState.AddCreature(Enemy);
        if (includeSecondEnemy)
        {
            SecondEnemy = CombatState.CreateCreature(ModelDb.Monster<BigDummy>().ToMutable(), CombatSide.Enemy, "2");
            CombatState.AddCreature(SecondEnemy);
        }

        CombatManager.Instance.SetUpCombat(CombatState);
        CombatInProgressProperty.SetValue(CombatManager.Instance, true);
        ClearAllPiles(Player);
        if (Ally != null)
        {
            ClearAllPiles(Ally);
        }

        _selectorScope = CardSelectCmd.UseSelector(Selector);
    }

    public void Dispose()
    {
        _selectorScope.Dispose();
        EndPreviousRun();
    }

    /// <summary>
    /// Drops the run and combat a previous test left behind so the next test can set up
    /// a fresh one. <see cref="RunManager.CleanUp"/> is not usable here: it clears modal
    /// overlays and null-references when the game UI is not loaded, which is the case for
    /// both the headless test host and the in-game runner.
    /// </summary>
    private static void EndPreviousRun()
    {
        if (CombatManager.Instance.IsInProgress)
        {
            CombatManager.Instance.Reset(graceful: true);
        }

        RunManagerStateProperty.SetValue(RunManager.Instance, null);
    }

    public T CardInHand<T>(Player? owner = null) where T : CardModel
    {
        T card = CreateCard<T>(owner);
        owner ??= Player;
        owner.PlayerCombatState!.Hand.AddInternal(card, silent: true);
        return card;
    }

    public T CardInDraw<T>(Player? owner = null) where T : CardModel
    {
        T card = CreateCard<T>(owner);
        owner ??= Player;
        owner.PlayerCombatState!.DrawPile.AddInternal(card, silent: true);
        return card;
    }

    public T CardInDiscard<T>(Player? owner = null) where T : CardModel
    {
        T card = CreateCard<T>(owner);
        owner ??= Player;
        owner.PlayerCombatState!.DiscardPile.AddInternal(card, silent: true);
        return card;
    }

    public T CardInDeck<T>(Player? owner = null) where T : CardModel
    {
        owner ??= Player;
        T card = (T)ModelDb.Card<T>().ToMutable();
        card.Owner = owner;
        owner.Deck.AddInternal(card, silent: true);
        return card;
    }

    public MockAttackCard MockAttackInHand(decimal damage = 6, Player? owner = null, TargetType targetType = TargetType.AnyEnemy)
    {
        MockAttackCard card = CreateCard<MockAttackCard>(owner).MockDamage(damage).MockTargetingType(targetType);
        (owner ?? Player).PlayerCombatState!.Hand.AddInternal(card, silent: true);
        return card;
    }

    public MockSkillCard MockSkillInHand(int block = 0, Player? owner = null)
    {
        MockSkillCard card = CreateCard<MockSkillCard>(owner).MockBlock(block);
        (owner ?? Player).PlayerCombatState!.Hand.AddInternal(card, silent: true);
        return card;
    }

    public void Select(params CardModel[] cards)
    {
        Selector.PrepareToSelect(cards);
    }

    public void SelectIndexes(params int[] indexes)
    {
        Selector.PrepareToSelect(indexes);
    }

    public async Task Play(CardModel card, Creature? target = null, int xValue = 0)
    {
        if (card.EnergyCost.CostsX)
        {
            card.EnergyCost.CapturedXValue = xValue;
        }

        CardPlay play = new()
        {
            Card = card,
            Target = target,
            ResultPile = GetResultPile(card),
            Resources = new ResourceInfo { EnergySpent = xValue, EnergyValue = xValue, StarsSpent = 0, StarValue = 0 },
            IsAutoPlay = false,
            PlayIndex = 0,
            PlayCount = 1
        };

        ArchitectCombatState.CapturePlay(card);
        await (Task)OnPlayMethod.Invoke(card, [ChoiceContext, play])!;
        ArchitectCombatState.RecordPlayed(card);
        await ArchitectEffectQueue.Drain(card.Owner);
        await card.AfterCardPlayed(ChoiceContext, play);
    }

    public PileType GetResultPile(CardModel card)
    {
        return (PileType)ResultPileMethod.Invoke(card, null)!;
    }

    public int HpLost(Creature creature, int before)
    {
        return before - creature.CurrentHp;
    }

    public static int PowerAmount<T>(Creature creature) where T : MegaCrit.Sts2.Core.Models.PowerModel
    {
        return creature.GetPowerAmount<T>();
    }

    public static int EnchantCount(CardModel card)
    {
        return ArchitectEnchantmentHelper.Count(card);
    }

    public static bool HasEnchant<T>(CardModel card) where T : MegaCrit.Sts2.Core.Models.EnchantmentModel
    {
        return ArchitectEnchantmentHelper.Has<T>(card);
    }

    public static int EnchantAmount(CardModel card)
    {
        return ArchitectEnchantmentHelper.Get(card)?.Amount ?? 0;
    }

    public async Task<T> ApplyPower<T>(Creature? owner = null, decimal amount = 1m) where T : PowerModel
    {
        owner ??= Player.Creature;
        await PowerCmd.Apply<T>(ChoiceContext, owner, amount, owner, null);
        return owner.GetPower<T>()!;
    }

    public int CountInHand<T>(Player? owner = null) where T : CardModel
    {
        owner ??= Player;
        return owner.PlayerCombatState!.Hand.Cards.OfType<T>().Count();
    }

    public int CountInDraw<T>(Player? owner = null) where T : CardModel
    {
        owner ??= Player;
        return owner.PlayerCombatState!.DrawPile.Cards.OfType<T>().Count();
    }

    public int CountInDiscard<T>(Player? owner = null) where T : CardModel
    {
        owner ??= Player;
        return owner.PlayerCombatState!.DiscardPile.Cards.OfType<T>().Count();
    }

    public void MarkPlayed(CardModel card, int times = 1)
    {
        for (int i = 0; i < times; i++)
        {
            ArchitectCombatState.RecordPlayed(card);
        }
    }

    private T CreateCard<T>(Player? owner = null) where T : CardModel
    {
        owner ??= Player;
        // Cards have to live in both scopes to behave like real ones: the run scope owns
        // them (so effects can move them into the deck) and the combat scope lets them sit
        // in combat piles.
        T card = RunState.CreateCard<T>(owner);
        CombatAddCardMethod.Invoke(CombatState, [card]);
        card.AfterCardEnteredCombat(card).GetAwaiter().GetResult();
        return card;
    }

    private static Player CreatePlayer(ulong netId)
    {
        Player player = Player.CreateForNewRun(ModelDb.Character<TheArchitect.TheArchitectCode.Character.TheArchitect>(), UnlockState.all, netId);
        player.Deck.Clear(silent: true);
        foreach (var relic in player.Relics.ToList())
        {
            player.RemoveRelicInternal(relic, silent: true);
        }

        return player;
    }

    private static void ClearAllPiles(Player player)
    {
        player.PlayerCombatState!.Hand.Clear(silent: true);
        player.PlayerCombatState.DrawPile.Clear(silent: true);
        player.PlayerCombatState.DiscardPile.Clear(silent: true);
        player.PlayerCombatState.ExhaustPile.Clear(silent: true);
        player.PlayerCombatState.PlayPile.Clear(silent: true);
    }
}
