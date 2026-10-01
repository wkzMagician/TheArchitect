using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Relics;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Character;

namespace TheArchitect.Tests.Character;

public static class ArchitectAncientCardTests
{
    [ArchitectTest]
    public static void ToothTransformsSigilbreakerAndTomeOffersInfiniteBlueprint()
    {
        var mapping = (Dictionary<ModelId, CardModel>)typeof(ArchaicTooth)
            .GetProperty("TranscendenceUpgrades", BindingFlags.NonPublic | BindingFlags.Static)!
            .GetValue(null)!;
        AssertEx.True(mapping.TryGetValue(ModelDb.Card<Sigilbreaker>().Id, out var transformed)
            && transformed == ModelDb.Card<AncientVerdict>(),
            "ArchaicTooth should transform Sigilbreaker into AncientVerdict.");

        var tomeCards = ModelDb.CardPool<TheArchitectCardPool>().AllCards
            .Where(card => card.Rarity == CardRarity.Ancient
                && !ArchaicTooth.TranscendenceCards.Contains(card)).ToList();
        AssertEx.Equal(1, tomeCards.Count, "DustyTome should have exactly one Architect Ancient reward.");
        AssertEx.True(tomeCards[0] is InfiniteBlueprint,
            "DustyTome should offer InfiniteBlueprint, excluding AncientVerdict.");
    }
}
