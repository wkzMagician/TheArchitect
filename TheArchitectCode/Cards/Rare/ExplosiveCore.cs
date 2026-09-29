using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.TheArchitectCode.Cards.Rare;

public sealed class ExplosiveCore() : TheArchitectCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Damage", 5)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<ExplosiveCorePower>(choiceContext, Owner.Creature, DynamicVars["Damage"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Damage"].UpgradeValueBy(2m);
}
