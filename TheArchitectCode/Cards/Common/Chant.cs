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
using TheArchitect.TheArchitectCode.Enchantments.Framework;
using TheArchitect.TheArchitectCode.Cards.Tokens;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Common;

public sealed class Chant() : TheArchitectCard(1, CardType.Skill, CardRarity.Common, TargetType.AnyEnemy)
{
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Move), new DynamicVar("Weak", 1), new DynamicVar("EnchantWeakBonus", 1)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await ArchitectEnchantmentHelper.GainBlock(this, play, DynamicVars.Block.BaseValue);
        if (play.Target != null)
        {
            decimal weak = DynamicVars["Weak"].BaseValue;
            if (ArchitectEnchantmentHelper.HasAny(this))
            {
                weak += DynamicVars["EnchantWeakBonus"].BaseValue;
            }

            await ArchitectEnchantmentHelper.ApplyWeak(play.Target, weak, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3m);
        DynamicVars["EnchantWeakBonus"].UpgradeValueBy(1m);
    }

    protected override string GetCombatPreviewText()
    {
        int weak = DynamicVars["Weak"].IntValue + (ArchitectEnchantmentHelper.HasAny(this) ? DynamicVars["EnchantWeakBonus"].IntValue : 0);
        return $"applies {weak} Weak";
    }
}
