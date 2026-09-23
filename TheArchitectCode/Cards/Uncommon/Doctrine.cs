using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Uncommon;

public sealed class Doctrine() : TheArchitectCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [CardKeyword.Retain, CardKeyword.Exhaust] : [CardKeyword.Exhaust];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Sharp, 1)
            .Concat(ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Nimble, 1));

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        List<CardModel> hand = ArchitectEnchantmentHelper.Hand(Owner);
        int sharp = hand.Sum(card => ArchitectEnchantmentHelper.GetAll(card).OfType<Sharp>().Sum(enchantment => enchantment.Amount));
        int nimble = hand.Sum(card => ArchitectEnchantmentHelper.GetAll(card).OfType<Nimble>().Sum(enchantment => enchantment.Amount));

        if (sharp > 0)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner.Creature, sharp, Owner.Creature, this);
        }

        if (nimble > 0)
        {
            await PowerCmd.Apply<DexterityPower>(choiceContext, Owner.Creature, nimble, Owner.Creature, this);
        }

        foreach (CardModel card in hand)
        {
            ArchitectEnchantmentHelper.RemoveWhere(card, enchantment => enchantment is Sharp or Nimble);
        }
    }
    protected override void OnUpgrade() => AddKeyword(CardKeyword.Retain);
}
