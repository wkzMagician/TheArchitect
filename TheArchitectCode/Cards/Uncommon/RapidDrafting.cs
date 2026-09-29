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

namespace TheArchitect.TheArchitectCode.Cards.Uncommon;

public sealed class RapidDrafting() : TheArchitectCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new CardsVar(2), new DynamicVar("Swift", 1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips => ArchitectKeywordHoverTips.IncludeEnchant(ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Swift, DynamicVars["Swift"].IntValue));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        IEnumerable<CardModel> drawn = await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
        foreach (CardModel card in drawn)
        {
            ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Swift, DynamicVars["Swift"].BaseValue);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Swift"].UpgradeValueBy(1m);
    }
}
