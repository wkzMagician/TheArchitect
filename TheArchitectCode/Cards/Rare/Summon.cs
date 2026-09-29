using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Rare;

public sealed class Summon() : TheArchitectCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    protected override IEnumerable<IHoverTip> ExtraHoverTips => [ArchitectKeywordHoverTips.Enchant];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Cards", 1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        List<CardModel> chosen = await ArchitectEnchantmentHelper.ChooseManyFromDrawPile(
            choiceContext,
            Owner,
            $"{Id.Entry}.selectionScreenPrompt",
            1,
            DynamicVars["Cards"].IntValue,
            ArchitectEnchantmentHelper.HasAny);

        foreach (CardModel card in chosen)
        {
            await ArchitectEnchantmentHelper.MoveToPile(card, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Cards"].UpgradeValueBy(1m);
    }
}
