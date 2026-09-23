using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;

namespace TheArchitect.TheArchitectCode.Relics;

public sealed class CalibrationRuler : ArchitectItemRelic
{
    public override RelicRarity Rarity => RelicRarity.Common;
    protected override IEnumerable<DynamicVar> CanonicalVars => [new BlockVar(5, ValueProp.Unpowered)];

    public async Task OnFirstEnchantment(CardModel card)
    {
        if (card.Owner != Owner || card.CombatState == null || card.Pile?.Type == PileType.Deck || Data.TriggeredThisTurn) return;
        Data.TriggeredThisTurn = true;
        Flash();
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block.BaseValue, ValueProp.Unpowered, null);
    }
}
