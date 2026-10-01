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

public sealed class TeachAToFishPower : TheArchitectPower
{
    private sealed class Data
    {
        public bool AttackDone;
        public bool SkillDone;
    }

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.None;

    protected override object InitInternalData()
    {
        return new Data();
    }

    public bool CanTriggerFor(CardModel card)
    {
        if (card.Owner.Creature != Owner) return false;
        Data data = GetInternalData<Data>();
        return card.Type switch
        {
            CardType.Attack => !data.AttackDone && ArchitectEnchantmentHelper.CanTargetForSpecificEnchant(card, ArchitectEnchantKind.Sharp),
            CardType.Skill => !data.SkillDone && ArchitectEnchantmentHelper.CanTargetForSpecificEnchant(card, ArchitectEnchantKind.Nimble),
            _ => false
        };
    }

    public override async Task BeforeCardPlayed(CardPlay cardPlay)
    {
        if (cardPlay.Card.Owner.Creature != Owner)
        {
            return;
        }

        Data data = GetInternalData<Data>();
        if (cardPlay.Card.Type == CardType.Attack && !data.AttackDone)
        {
            data.AttackDone = true;
            ArchitectEnchantmentHelper.Add(cardPlay.Card, ArchitectEnchantKind.Sharp, Amount);
        }
        else if (cardPlay.Card.Type == CardType.Skill && !data.SkillDone)
        {
            data.SkillDone = true;
            ArchitectEnchantmentHelper.Add(cardPlay.Card, ArchitectEnchantKind.Nimble, Amount);
        }

        if (data.AttackDone && data.SkillDone)
        {
            await PowerCmd.Remove(this);
        }
    }
}
