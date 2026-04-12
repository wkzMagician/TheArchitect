using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Powers.Architect;

public sealed class SanctumOfVigorPower : TheArchitectPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }

        CardModel? card = await ArchitectEnchantmentHelper.ChooseFromHand(
            choiceContext,
            player,
            $"{Id.Entry}.selectionScreenPrompt",
            static card => ArchitectEnchantmentHelper.CanTargetForSpecificEnchant(card, ArchitectEnchantKind.Vitality),
            this);
        if (card != null)
        {
            ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Vitality, Amount);
        }
    }
}
