using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Uncommon;

public sealed class DivineSelection() : TheArchitectCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        CardModel? card = await ArchitectEnchantmentHelper.ChooseFromHand(
            choiceContext,
            Owner,
            $"{Id.Entry}.selectionScreenPrompt",
            ArchitectEnchantmentHelper.CanTargetForRandomBasic,
            this);
        if (card == null)
        {
            return;
        }

        ArchitectEnchantmentHelper.AddRandomBasic(Owner, card);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
