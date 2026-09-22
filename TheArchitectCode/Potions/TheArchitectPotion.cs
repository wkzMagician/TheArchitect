using BaseLib.Abstracts;
using BaseLib.Utils;
using BaseLib.Extensions;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Potions;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using TheArchitect.TheArchitectCode.Character;

namespace TheArchitect.TheArchitectCode.Potions;

[Pool(typeof(TheArchitectPotionPool))]
public abstract class TheArchitectPotion : CustomPotionModel
{
    private string ArtId => Id.Entry.RemovePrefix().ToLowerInvariant();
    public override string CustomPackedImagePath => $"res://TheArchitect/images/atlases/potion_atlas.sprites/{ArtId}.tres";
    public override string CustomPackedOutlinePath => $"res://TheArchitect/images/atlases/potion_outline_atlas.sprites/{ArtId}.tres";
    public override string CustomLargeImagePath => $"res://TheArchitect/images/potions/{ArtId}.png";
    public override PotionUsage Usage => PotionUsage.CombatOnly;
    public override TargetType TargetType => TargetType.Self;

    internal bool SelectionPrepared { get; private set; }
    protected abstract Task<bool> PrepareSelection(PlayerChoiceContext context);

    internal async Task UseWithSelection(PlayerChoiceContext context, Creature? target)
    {
        // Ask before the engine consumes the bottle or dispatches potion-used hooks.
        // Cancellation therefore cannot farm rewards from potion-use relics.
        if (!PassesCustomUsabilityCheck || !await PrepareSelection(context))
        {
            AfterUsageCanceled();
            return;
        }
        SelectionPrepared = true;
        try { await OnUseWrapper(context, target); }
        finally { SelectionPrepared = false; }
    }
}
