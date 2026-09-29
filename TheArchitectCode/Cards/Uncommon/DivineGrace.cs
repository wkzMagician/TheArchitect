using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheArchitect.TheArchitectCode.Helpers;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.TheArchitectCode.Cards.Uncommon;

public sealed class DivineGrace() : TheArchitectCard(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [CardKeyword.Innate] : [];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => ArchitectKeywordHoverTips.IncludeEnchant(ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.PerfectFit, 1));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<DivineGracePower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
    protected override void OnUpgrade() => AddKeyword(CardKeyword.Innate);
}
