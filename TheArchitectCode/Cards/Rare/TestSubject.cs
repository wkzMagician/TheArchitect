using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using TheArchitect.TheArchitectCode.Helpers;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.TheArchitectCode.Cards.Rare;

public sealed class TestSubject() : TheArchitectCard(0, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => ArchitectKeywordHoverTips.IncludeEnchant([ArchitectKeywordHoverTips.RemoveEnchantments]);
    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [CardKeyword.Innate] : [];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PowerCmd.Apply<TestSubjectPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }
    protected override void OnUpgrade() => AddKeyword(CardKeyword.Innate);
}
