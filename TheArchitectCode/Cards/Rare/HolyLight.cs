using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Rare;

/*
 * todo: HolyLight 打出时报错，因为这张牌进入了需要选择界面的流程，但没有定义对应的 SelectionScreenPrompt，于是直接抛出异常并导致 PlayCardAction 失败。
 */
public sealed class HolyLight() : TheArchitectCard(1, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Cards", 1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Chromatic, 1);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        List<CardModel> cards = PileType.Draw.GetPile(Owner).Cards.ToList();
        foreach (CardModel card in await CardSelectCmd.FromSimpleGrid(choiceContext, cards, Owner, new CardSelectorPrefs(SelectionScreenPrompt, 1, DynamicVars["Cards"].IntValue)))
        {
            ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Chromatic, 1m);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cards"].UpgradeValueBy(1m);
    }
}
