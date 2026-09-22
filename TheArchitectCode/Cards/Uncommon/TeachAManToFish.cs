using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.TheArchitectCode.Cards.Uncommon;

public sealed class TeachAManToFish() : TheArchitectCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("EnchantAmount", 3)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
    [
        .. ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Sharp, DynamicVars["EnchantAmount"].IntValue),
        .. ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Nimble, DynamicVars["EnchantAmount"].IntValue)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target == null)
        {
            return;
        }

        await PowerCmd.Apply<TeachAToFishPower>(choiceContext, play.Target, DynamicVars["EnchantAmount"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["EnchantAmount"].UpgradeValueBy(2m);
    }
}
