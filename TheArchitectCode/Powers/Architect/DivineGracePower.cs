using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Powers.Architect;

public sealed class DivineGracePower : TheArchitectPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public override async Task BeforeTurnEnd(PlayerChoiceContext choiceContext, MegaCrit.Sts2.Core.Combat.CombatSide side)
    {
        if (Owner.Player == null || side != Owner.Player.Creature.CombatState!.CurrentSide)
        {
            return;
        }

        CardModel? card = await ArchitectEnchantmentHelper.ChooseFromHand(
            choiceContext,
            Owner.Player,
            $"{Id.Entry}.selectionScreenPrompt",
            static card => ArchitectEnchantmentHelper.CanTargetForSpecificEnchant(card, ArchitectEnchantKind.PerfectFit),
            this);
        if (card != null)
        {
            ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.PerfectFit, 1m);
        }
    }
}
