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

namespace TheArchitect.TheArchitectCode.Cards.Rare;

public sealed class PrimedSpark() : TheArchitectCard(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<(ArchitectEnchantKind Kind, int Amount)> StartingEnchantments => [];

    protected override void ApplyStartingEnchantments()
    {
        if (ArchitectEnchantmentHelper.HasAny(this))
        {
            return;
        }

        ArchitectEnchantKind kind = Owner?.RunState.Rng.CombatCardSelection.NextInt(2) == 0
            ? ArchitectEnchantKind.Sown
            : ArchitectEnchantKind.Swift;
        ApplyStartingEnchantment(kind, IsUpgraded ? 2 : 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        await PlayerCmd.GainEnergy(1, Owner);
        await CardPileCmd.Draw(choiceContext, 1, Owner);
    }
}
