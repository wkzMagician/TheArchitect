using BaseLib.Utils;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using TheArchitect.TheArchitectCode.Character;
using TheArchitect.TheArchitectCode.Helpers;

namespace TheArchitect.TheArchitectCode.Cards.Tokens;

[Pool(typeof(TheArchitectTokenPool))]
public sealed class EnchantChoiceCard : TheArchitectCard
{
    public override CardPoolModel Pool => ModelDb.CardPool<TheArchitectTokenPool>();

    public EnchantChoiceCard()
        : this(new ArchitectEnchantOption(ArchitectEnchantKind.Sharp, 3))
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

    private EnchantmentModel DisplayEnchantment => ArchitectEnchantmentHelper.Create(Option.Kind);

    public override string Title => DisplayEnchantment.Title.GetFormattedText();

    protected override void AddExtraArgsToDescription(LocString description)
    {
        base.AddExtraArgsToDescription(description);
        string amountPrefix = string.Empty;
        if (!ArchitectEnchantmentHelper.IsStackless(Option.Kind))
        {
            LocString prefix = new("cards", "THEARCHITECT-ENCHANT_CHOICE_CARD.amountPrefix");
            prefix.Add("Amount", Option.Amount);
            amountPrefix = prefix.GetFormattedText();
        }
        description.Add("AmountPrefix", amountPrefix);
        description.Add("Enchantment", DisplayEnchantment.Title.GetFormattedText());
    }

    protected override IEnumerable<IHoverTip> ExtraHoverTips => ArchitectKeywordHoverTips.IncludeEnchant(ArchitectEnchantmentHelper.HoverFor(Option.Kind, Option.Amount));

    protected override Task OnPlay(PlayerChoiceContext choiceContext, CardPlay play)
    {
        return Task.CompletedTask;
    }

    protected override void OnUpgrade()
    {
    }
}
