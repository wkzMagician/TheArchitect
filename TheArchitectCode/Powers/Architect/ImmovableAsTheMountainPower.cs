using BaseLib.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Powers.Architect;

public sealed class ImmovableAsTheMountainPower : TheArchitectPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override Task BeforeSideTurnEnd(
        PlayerChoiceContext choiceContext,
        MegaCrit.Sts2.Core.Combat.CombatSide side,
        IEnumerable<MegaCrit.Sts2.Core.Entities.Creatures.Creature> participants)
    {
        return BeforeTurnEnd(choiceContext, side);
    }

    public async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, MegaCrit.Sts2.Core.Combat.CombatSide side)
    {
        if (Owner.Player == null || side != MegaCrit.Sts2.Core.Combat.CombatSide.Player)
        {
            return;
        }

        List<CardModel> selected = (await CardSelectCmd.FromHand(
            choiceContext,
            Owner.Player,
            new CardSelectorPrefs(SelectionScreenPrompt, 0, Amount),
            static card => !ArchitectEnchantmentHelper.HasAny(card),
            this)).ToList();

        foreach (CardModel card in selected)
        {
            ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Steady, 1m);
        }
    }
}
