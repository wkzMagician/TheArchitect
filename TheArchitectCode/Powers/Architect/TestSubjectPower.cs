using BaseLib.Extensions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Powers.Architect;

public sealed class TestSubjectPower : TheArchitectPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    // todo: 本地化错误，cards.THEARCHITECT-TEST_SUBJECT_POWER.selectionScreenPrompt
    // todo: 如果手牌中没有手牌有附魔，不会触发选牌界面
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }

        await CardPileCmd.Draw(choiceContext, 1, player);
        CardModel? card = await ArchitectEnchantmentHelper.ChooseFromHand(choiceContext, player, $"{Id.Entry}.selectionScreenPrompt", static _ => true, this);
        if (card != null)
        {
            ArchitectEnchantmentHelper.RemoveAll(card);
        }
    }
}
