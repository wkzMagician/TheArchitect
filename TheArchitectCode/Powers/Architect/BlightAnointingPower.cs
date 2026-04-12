using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Powers.Architect;

public sealed class BlightAnointingPower : TheArchitectPower
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    public static void TryEnchant(CardModel card)
    {
        if (card.Type != CardType.Attack || card.Owner?.Creature?.GetPower<BlightAnointingPower>() == null)
        {
            return;
        }

        ArchitectEnchantmentHelper.Add(card, ArchitectEnchantKind.Corruption, 1m);
    }
}
