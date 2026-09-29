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

public sealed class Lullaby() : TheArchitectCard(1, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
{
    protected override IEnumerable<(ArchitectEnchantKind Kind, int Amount)> StartingEnchantments => [(ArchitectEnchantKind.Glam, 1)];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Weak", 1)];

    protected override IEnumerable<IHoverTip> ExtraHoverTips =>
        ArchitectEnchantmentHelper.HoverFor(ArchitectEnchantKind.Glam, 1)
            .Concat([HoverTipFactory.FromPower<WeakPower>()]);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        foreach (Creature enemy in CombatState!.HittableEnemies)
        {
            await ArchitectEnchantmentHelper.ApplyWeak(choiceContext, enemy, DynamicVars["Weak"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Weak"].UpgradeValueBy(1m);
    }
}
