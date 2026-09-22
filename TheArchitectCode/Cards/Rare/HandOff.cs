using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using TheArchitect.TheArchitectCode.Helpers;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.TheArchitectCode.Cards.Rare;

public sealed class HandOff() : TheArchitectCard(1, CardType.Skill, CardRarity.Rare, TargetType.AnyAlly)
{
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        if (play.Target?.Player == null)
        {
            return;
        }

        CardModel? card = await ArchitectEnchantmentHelper.ChooseFromHand(choiceContext, Owner, $"{Id.Entry}.selectionScreenPrompt", card => card != this, this);
        if (card == null)
        {
            return;
        }

        CardModel copy = play.Target.Player.Creature.CombatState!.CreateCard(card.CanonicalInstance, play.Target.Player);
        foreach (EnchantmentModel enchantment in ArchitectEnchantmentHelper.GetAll(card))
        {
            ArchitectEnchantmentHelper.AddRaw(copy, EnchantmentModel.FromSerializable(enchantment.ToSerializable()), enchantment.Amount);
        }

        await CardPileCmd.AddGeneratedCardToCombat(copy, PileType.Hand, play.Target.Player);
        await CardCmd.Exhaust(choiceContext, card);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
