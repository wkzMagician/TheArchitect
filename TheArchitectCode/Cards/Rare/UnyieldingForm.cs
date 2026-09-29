using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;

namespace TheArchitect.TheArchitectCode.Cards.Rare;

public sealed class UnyieldingForm() : TheArchitectCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Artifact", 3)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<ArtifactPower>(choiceContext, Owner.Creature, DynamicVars["Artifact"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade() => DynamicVars["Artifact"].UpgradeValueBy(2m);
}
