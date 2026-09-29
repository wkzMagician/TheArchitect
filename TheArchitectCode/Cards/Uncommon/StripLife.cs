using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Uncommon;

public sealed class StripLife() : TheArchitectCard(0, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => ArchitectKeywordHoverTips.IncludeEnchant([ArchitectKeywordHoverTips.RemoveEnchantments]);
    protected override IEnumerable<DynamicVar> CanonicalVars => [new EnergyVar("Energy", 2)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        CardModel? card = await ArchitectEnchantmentHelper.ChooseFromHand(choiceContext, Owner, $"{Id.Entry}.selectionScreenPrompt", ArchitectEnchantmentHelper.HasAny, this);
        if (card == null)
        {
            return;
        }

        ArchitectEnchantmentHelper.Remove(card);
        await PlayerCmd.GainEnergy(DynamicVars["Energy"].IntValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Energy"].UpgradeValueBy(1m);
    }
}
