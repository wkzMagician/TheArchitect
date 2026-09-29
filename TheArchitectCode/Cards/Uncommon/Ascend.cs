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

public sealed class Ascend() : TheArchitectCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8, ValueProp.Move), new EnergyVar("Energy", 1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        int priorPlays = ArchitectCombatState.TimesPlayed(this);
        await ArchitectEnchantmentHelper.Attack(this, choiceContext, play.Target, DynamicVars.Damage.BaseValue);
        if (priorPlays >= 1)
        {
            await CardPileCmd.Draw(choiceContext, 1, Owner);
        }

        if (priorPlays >= 2)
        {
            await PlayerCmd.GainEnergy(1, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }

    protected override string GetCombatPreviewText()
    {
        int priorPlays = ArchitectCombatState.TimesPlayed(this);
        if (priorPlays >= 2)
        {
            return GetLocalizedCombatPreview("THEARCHITECT-ASCEND.combatPreviewDrawAndEnergy");
        }

        if (priorPlays >= 1)
        {
            return GetLocalizedCombatPreview("THEARCHITECT-ASCEND.combatPreviewDraw");
        }

        return string.Empty;
    }
}
