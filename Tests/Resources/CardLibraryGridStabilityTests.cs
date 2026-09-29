using HarmonyLib;
using Godot;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Screens.CardLibrary;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Cards.Rare;

namespace TheArchitect.Tests.Resources;

public static class CardLibraryGridStabilityTests
{
    [ArchitectTest]
    public static void OneRowDuringCardLibraryFilterTransitionDoesNotReallocate()
    {
        var grid = new NCardLibraryGrid();
        var scrollContainer = new Control { Size = new Vector2(600, 300) };
        try
        {
            AccessTools.Field(typeof(NCardGrid), "_scrollContainer").SetValue(grid, scrollContainer);
            AccessTools.Field(typeof(NCardGrid), "_cardSize").SetValue(grid, new Vector2(100, 150));
            AccessTools.Property(typeof(NCardGrid), "DisplayedRows").SetValue(grid, 1);
            int columns = (int)AccessTools.Property(typeof(NCardGrid), "Columns").GetValue(grid)!;
            var rows = (List<List<NGridCardHolder>>)AccessTools.Field(typeof(NCardGrid), "_cardRows").GetValue(grid)!;
            var cards = (List<CardModel>)AccessTools.Field(typeof(NCardGrid), "_cards").GetValue(grid)!;
            var row = new List<NGridCardHolder>();
            rows.Add(row);

            AccessTools.Field(typeof(NCardGrid), "_slidingWindowCardIndex").SetValue(grid, columns);
            AccessTools.Method(typeof(NCardGrid), "ReallocateAbove").Invoke(grid, [row]);
            AssertEx.Equal(1, rows.Count, "A one-row library grid stays intact while scrolling upward.");

            AccessTools.Field(typeof(NCardGrid), "_slidingWindowCardIndex").SetValue(grid, 0);
            for (int i = 0; i < 11; i++) cards.Add(ModelDb.Card<Genesis>());
            AccessTools.Method(typeof(NCardGrid), "ReallocateBelow").Invoke(grid, [row]);
            AssertEx.Equal(1, rows.Count, "A one-row library grid stays intact while scrolling downward.");
        }
        finally
        {
            grid.Free();
            scrollContainer.Free();
        }
    }
}
