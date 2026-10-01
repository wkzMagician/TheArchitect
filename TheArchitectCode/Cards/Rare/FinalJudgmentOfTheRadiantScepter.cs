using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Rare;

public sealed class FinalJudgmentOfTheRadiantScepter() : TheArchitectCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override bool ShouldGlowGoldInternal => IsMutable && CombatState != null && (ArchitectCombatState.TimesPlayed(this) >= 4);

    protected override bool ShufflesAfterPlay => true;
    protected override CardPilePosition ShufflePosition => IsUpgraded ? CardPilePosition.Top : CardPilePosition.Random;

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        int hits = ArchitectCombatState.TimesPlayed(this) >= 4 ? 10 : 1;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(hits)
            .FromCard(this)
            .Targeting(play.Target!)
            .Execute(choiceContext);
        ShuffleIntoDrawPileThisCombat = true;
    }

    protected override string GetCombatPreviewText()
    {
        int hits = ArchitectCombatState.TimesPlayed(this) >= 4 ? 10 : 1;
        return GetLocalizedCombatPreview("THEARCHITECT-FINAL_JUDGMENT_OF_THE_RADIANT_SCEPTER.combatPreview", ("Hits", hits));
    }
}
