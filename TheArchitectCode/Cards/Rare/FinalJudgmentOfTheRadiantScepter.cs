using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Rare;

public sealed class FinalJudgmentOfTheRadiantScepter() : TheArchitectCard(1, CardType.Attack, CardRarity.Rare, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        int hits = ArchitectCombatState.TimesPlayed(this) >= 9 ? 10 : 1;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue).WithHitCount(hits).FromCard(this).Targeting(play.Target!).Execute(choiceContext);
        ShuffleIntoDrawPileThisCombat = true;
        if (IsUpgraded)
        {
            await ArchitectEnchantmentHelper.MoveToPile(this, PileType.Draw, CardPilePosition.Top);
        }
    }

    protected override string GetCombatPreviewText()
    {
        int hits = ArchitectCombatState.TimesPlayed(this) >= 9 ? 10 : 1;
        return $"hits {CountNoun(hits, "time")}";
    }
}
