using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using TheArchitect.TheArchitectCode.Character;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Tokens;

[Pool(typeof(TheArchitectTokenPool))]
public sealed class EnchantChoiceCard : TheArchitectCard
{
    public override CardPoolModel Pool => ModelDb.CardPool<TheArchitectTokenPool>();

    public EnchantChoiceCard()
        : this(new ArchitectEnchantOption(ArchitectEnchantKind.Sharp, 2))
    {
    }

    public EnchantChoiceCard(ArchitectEnchantOption option)
        : base(-1, CardType.Skill, CardRarity.Token, TargetType.None)
    {
        Option = option;
    }

    public ArchitectEnchantOption Option { get; private set; }

    public void SetOption(ArchitectEnchantOption option)
    {
        Option = option;
    }

    public EnchantmentModel? SelectedEnchantment { get; private set; }

    public void SetEnchantment(EnchantmentModel enchantment) => SelectedEnchantment = enchantment;

    protected override IEnumerable<IHoverTip> ExtraHoverTips => SelectedEnchantment?.HoverTips ?? ArchitectEnchantmentHelper.HoverFor(Option.Kind, Option.Amount);

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
    }
}
