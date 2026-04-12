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

public sealed class Proliferation() : TheArchitectCard(1, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(1, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        int plays = ArchitectCombatState.TimesPlayed(this);
        // todo: 应该直接更新 Damage.BaseValue 以及 attack times
        decimal damage = DynamicVars.Damage.BaseValue + plays;
        int hits = 1 + plays;
        await DamageCmd.Attack(damage).WithHitCount(hits).FromCard(this).Targeting(play.Target!).Execute(choiceContext);
        if (IsUpgraded)
        {
            ShuffleIntoDrawPileThisCombat = true;
        }
    }

    protected override string GetCombatPreviewText()
    {
        return string.Empty;
    }
}
