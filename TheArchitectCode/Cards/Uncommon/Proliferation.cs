using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Uncommon;

public sealed class Proliferation() : TheArchitectCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override bool ShufflesAfterPlay => IsUpgraded || base.ShufflesAfterPlay;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(1, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        int plays = ArchitectCombatState.TimesPlayed(this);
        // DynamicVars.Damage.UpgradeValueBy(plays);
        int hits = 1 + plays;

        await DamageCmd.Attack(DynamicVars.Damage.IntValue).WithHitCount(hits).FromCard(this).Targeting(play.Target!).Execute(choiceContext);
        if (IsUpgraded)
        {
            ShuffleIntoDrawPileThisCombat = true;
        }
        
        DynamicVars.Damage.UpgradeValueBy(1);
    }

    protected override string GetCombatPreviewText()
    {
        int hits = 1 + ArchitectCombatState.TimesPlayed(this);
        return GetLocalizedCombatPreview("THEARCHITECT-PROLIFERATION.combatPreview", ("Hits", hits));
    }
}
