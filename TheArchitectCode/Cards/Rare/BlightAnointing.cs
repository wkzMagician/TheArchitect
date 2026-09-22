using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Helpers;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.TheArchitectCode.Cards.Rare;

public sealed class BlightAnointing() : TheArchitectCard(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Corrupted, 1);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        foreach (CardModel card in ArchitectEnchantmentHelper.Hand(Owner).Where(card => card.Type == CardType.Attack))
        {
        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Corrupted, 1m);
        }

        await PowerCmd.Apply<BlightAnointingPower>(choiceContext, Owner.Creature, 1, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
