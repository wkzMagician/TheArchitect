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

namespace TheArchitect.TheArchitectCode.Cards.Uncommon;

public sealed class Doctrine() : TheArchitectCard(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => IsUpgraded ? [CardKeyword.Retain, CardKeyword.Exhaust] : [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        int sharp = ArchitectEnchantmentHelper.Hand(Owner).Count(card => ArchitectEnchantmentHelper.Has<Sharp>(card));
        int nimble = ArchitectEnchantmentHelper.Hand(Owner).Count(card => ArchitectEnchantmentHelper.Has<Nimble>(card));
        if (sharp > 0)
        {
            await PowerCmd.Apply<StrengthPower>(Owner.Creature, sharp, Owner.Creature, this);
        }

        if (nimble > 0)
        {
            await PowerCmd.Apply<DexterityPower>(Owner.Creature, nimble, Owner.Creature, this);
        }
    }
}
