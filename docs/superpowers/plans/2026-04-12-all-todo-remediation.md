# All TODO Remediation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove every current source `TODO` by first hardening helper-layer legality checks, then fixing all runtime errors, then completing card behavior, text, and naming work to match the comment intent.

**Architecture:** Treat the work as three sequential layers. First centralize enchant legality and generated-card creation in helpers so cards stop needing ad hoc guard code. Next fix all crash paths that currently rely on missing prompts, invalid card ownership, or invalid preview/filter logic. Finally repair gameplay behavior, timing, descriptions, localization, and naming so every remaining `TODO` can be deleted without leaving design debt behind.

**Tech Stack:** C#/.NET 9, Godot 4.5.1 Mono, sts2/BaseLib mod APIs, custom `ArchitectTest` harness under `Tests/`

---

## File Structure and Ownership

**Primary helper files**
- Modify: `TheArchitectCode/Helpers/ArchitectEnchantmentHelper.cs`
- Modify if needed: `TheArchitectCode/Helpers/ArchitectCardSelectionHelper.cs`

**Generated-card and token files**
- Modify: `TheArchitectCode/Cards/Basic/Tempering.cs`
- Modify: `TheArchitectCode/Cards/Tokens/TemperingChoiceCard.cs`
- Modify: `TheArchitectCode/Cards/Curse/Drowsy.cs`
- Modify: `TheArchitectCode/Cards/Rare/TeaOfDrowsiness.cs`
- Modify: `TheArchitectCode/Cards/Rare/WriteDestiny.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/CelestialWar.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/GuardedDrowse.cs`

**Crash-fix card and power files**
- Modify: `TheArchitectCode/Cards/Rare/EternalVerdict.cs`
- Modify: `TheArchitectCode/Cards/Rare/HolyLight.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/ChorusOfEvasion.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/DivineSelection.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/ImmovableAsTheMountain.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/InstinctAwakened.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/MonumentHammer.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/RadiantMight.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/SoulExchange.cs`
- Modify: `TheArchitectCode/Powers/Architect/DivineGracePower.cs`
- Modify: `TheArchitectCode/Powers/Architect/SanctumOfVigorPower.cs`
- Modify: `TheArchitectCode/Powers/Architect/TestSubjectPower.cs`

**Gameplay behavior files**
- Modify: `TheArchitectCode/Cards/Rare/FinalJudgmentOfTheRadiantScepter.cs`
- Modify: `TheArchitectCode/Cards/Rare/GrandOpus.cs`
- Modify: `TheArchitectCode/Cards/Rare/Omnipotence.cs`
- Modify: `TheArchitectCode/Cards/Rare/Rollback.cs`
- Modify: `TheArchitectCode/Cards/Rare/SkyrendJudgment.cs`
- Modify: `TheArchitectCode/Cards/Rare/StayTheBlade.cs`
- Modify: `TheArchitectCode/Cards/Rare/WakingCataclysm.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/CyclingEtch.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/Doctrine.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/Proliferation.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/RaiseOffspring.cs`
- Modify: `TheArchitectCode/Powers/Architect/FateVortexPower.cs`

**Likely localization files**
- Modify: `TheArchitect/localization/eng/cards.json`
- Modify: `TheArchitect/localization/eng/powers.json`
- Modify: `TheArchitect/localization/eng/card_keywords.json` if prompt strings or keyword text need to move

**Tests to extend or create**
- Modify/create under:
  - `Tests/Helpers/ArchitectEnchantmentHelperTests.cs`
  - `Tests/Cards/Basic/TemperingTests.cs`
  - `Tests/Cards/Tokens/DrowsyTests.cs`
  - `Tests/Cards/Rare/*.cs`
  - `Tests/Cards/Uncommon/*.cs`
  - `Tests/Powers/Architect/*.cs`

---

### Task 1: Lock the TODO Inventory with Focused Regression Tests

**Files:**
- Modify: `Tests/Helpers/ArchitectEnchantmentHelperTests.cs`
- Modify/create: `Tests/Cards/Basic/TemperingTests.cs`
- Modify/create targeted tests matching each touched card/power file

- [ ] **Step 1: Write the failing helper and regression tests**

```csharp
[ArchitectTest]
public static async Task RejectsInvalidEnchantTargetsBeforeSelection()
{
    // Arrange an ineligible card in hand.
    // Act through the helper-facing card/power entry point.
    // Assert the invalid card is absent from legal choices or remains unchanged.
}
```

- [ ] **Step 2: Run only the new targeted tests to verify they fail**

Run: `dotnet test Tests/TheArchitect.Tests.csproj --filter FullyQualifiedName~ArchitectEnchantmentHelperTests`
Expected: FAIL for missing legality guard, missing prompt key, or wrong gameplay timing.

- [ ] **Step 3: Record one test bucket per TODO cluster**

Buckets:
- helper legality
- generated-card ownership
- prompt/localization crashes
- selection/filter crashes
- gameplay timing/effect bugs
- text/naming/localization assertions

- [ ] **Step 4: Commit**

```bash
git add Tests
git commit -m "test: lock todo remediation regressions"
```

### Task 2: Centralize Enchant Legality in Helpers

**Files:**
- Modify: `TheArchitectCode/Helpers/ArchitectEnchantmentHelper.cs`
- Modify if needed: `TheArchitectCode/Helpers/ArchitectCardSelectionHelper.cs`
- Test: `Tests/Helpers/ArchitectEnchantmentHelperTests.cs`

- [ ] **Step 1: Write the failing test for helper-level legality**

```csharp
[ArchitectTest]
public static void CanTargetForSpecificEnchant_RejectsIncompatibleCards()
{
    // Assert false for cards that Canonical(kind).CanEnchant(card) rejects.
}
```

- [ ] **Step 2: Run the helper test to verify it fails**

Run: `dotnet test Tests/TheArchitect.Tests.csproj --filter FullyQualifiedName~CanTargetForSpecificEnchant`
Expected: FAIL because current helper paths still allow incompatible cards or rely on caller discipline.

- [ ] **Step 3: Implement the minimal helper consolidation**

```csharp
public static bool CanTargetForSpecificEnchant(CardModel card, ArchitectEnchantKind kind)
{
    return CanReceiveAnotherEnchant(card) && Canonical(kind).CanEnchant(card);
}
```

- [ ] **Step 4: Extend shared helper paths to consume the centralized predicate**

Apply this to:
- single-card selection
- many-card selection
- random enchant helpers
- `EnchantAll` and any bulk helper that currently trusts a loose filter

- [ ] **Step 5: Run the helper test bucket**

Run: `dotnet test Tests/TheArchitect.Tests.csproj --filter FullyQualifiedName~ArchitectEnchantmentHelperTests`
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add TheArchitectCode/Helpers/ArchitectEnchantmentHelper.cs Tests/Helpers/ArchitectEnchantmentHelperTests.cs
git commit -m "fix: centralize enchant legality checks"
```

### Task 3: Make Generated Cards and Choice Cards Use Canonical Creation Paths

**Files:**
- Modify: `TheArchitectCode/Cards/Basic/Tempering.cs`
- Modify: `TheArchitectCode/Cards/Tokens/TemperingChoiceCard.cs`
- Modify: `TheArchitectCode/Helpers/ArchitectEnchantmentHelper.cs`
- Modify: `TheArchitectCode/Cards/Curse/Drowsy.cs`
- Test: `Tests/Cards/Basic/TemperingTests.cs`
- Test: `Tests/Cards/Tokens/DrowsyTests.cs`

- [ ] **Step 1: Write failing tests for canonical model creation**

```csharp
[ArchitectTest]
public static async Task Tempering_UsesModelDbChoiceCards()
{
    // Assert choice cards are canonical models and do not duplicate model construction.
}
```

```csharp
[ArchitectTest]
public static async Task AddDrowsy_CreatesCombatCardsWithValidOwnerAndRunState()
{
    // Assert generated Drowsy cards can enter combat piles without mock-pool crashes.
}
```

- [ ] **Step 2: Run the targeted tests to verify they fail**

Run: `dotnet test Tests/TheArchitect.Tests.csproj --filter "FullyQualifiedName~TemperingTests|FullyQualifiedName~DrowsyTests"`
Expected: FAIL because choice cards are manually `new`-ed and generated cards are not always using a safe creation path.

- [ ] **Step 3: Replace manual token construction with `ModelDb` or equivalent canonical retrieval**

```csharp
private static CardModel CreateChoiceCard(ArchitectEnchantKind kind)
{
    return kind switch
    {
        ArchitectEnchantKind.Sharp => ModelDb.Card<TemperingSharpChoice>().ToMutable(),
        ArchitectEnchantKind.Nimble => ModelDb.Card<TemperingNimbleChoice>().ToMutable(),
        _ => throw new InvalidOperationException()
    };
}
```

- [ ] **Step 4: Add or extract one helper for safe generated-card creation**

Apply it to:
- `AddDrowsy`
- cards that inject `Drowsy` or `WakingCataclysm`
- any generated-card path currently tripping MockCardPool or owner binding

- [ ] **Step 5: Re-run the generated-card and token tests**

Run: `dotnet test Tests/TheArchitect.Tests.csproj --filter "FullyQualifiedName~TemperingTests|FullyQualifiedName~DrowsyTests|FullyQualifiedName~DrowsyEnginePowerTests"`
Expected: PASS

- [ ] **Step 6: Commit**

```bash
git add TheArchitectCode/Cards/Basic/Tempering.cs TheArchitectCode/Cards/Tokens/TemperingChoiceCard.cs TheArchitectCode/Helpers/ArchitectEnchantmentHelper.cs TheArchitectCode/Cards/Curse/Drowsy.cs Tests/Cards/Basic/TemperingTests.cs Tests/Cards/Tokens/DrowsyTests.cs
git commit -m "fix: use canonical generated card creation paths"
```

### Task 4: Fix Helper-Driven Runtime Errors Caused by Invalid Filters

**Files:**
- Modify: `TheArchitectCode/Cards/Rare/EternalVerdict.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/ChorusOfEvasion.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/DivineSelection.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/InstinctAwakened.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/RadiantMight.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/SoulExchange.cs`
- Modify: `TheArchitectCode/Powers/Architect/DivineGracePower.cs`
- Modify: `TheArchitectCode/Powers/Architect/SanctumOfVigorPower.cs`
- Test: corresponding card and power tests

- [ ] **Step 1: Write one failing test per invalid-target crash path**

```csharp
[ArchitectTest]
public static async Task ChorusOfEvasion_DoesNotOfferCardsThatCannotReceiveNimble()
{
    // Assert the invalid card is filtered out before enchantment application.
}
```

- [ ] **Step 2: Run the affected test group to verify red**

Run: `dotnet test Tests/TheArchitect.Tests.csproj --filter "FullyQualifiedName~ChorusOfEvasion|FullyQualifiedName~InstinctAwakened|FullyQualifiedName~DivineGrace|FullyQualifiedName~SoulExchange|FullyQualifiedName~SanctumOfVigor|FullyQualifiedName~EternalVerdict|FullyQualifiedName~DivineSelection|FullyQualifiedName~RadiantMight"`
Expected: FAIL due to missing or incomplete target filtering.

- [ ] **Step 3: Replace ad hoc predicates with helper-backed legality checks**

Implementation rule:
- caller supplies enchant kind and any additional business constraints
- helper guarantees enchant compatibility
- cards stop duplicating `CanEnchant` logic

- [ ] **Step 4: Re-run the filter/crash test group**

Run: same command as Step 2
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add TheArchitectCode/Cards/Rare/EternalVerdict.cs TheArchitectCode/Cards/Uncommon/ChorusOfEvasion.cs TheArchitectCode/Cards/Uncommon/DivineSelection.cs TheArchitectCode/Cards/Uncommon/InstinctAwakened.cs TheArchitectCode/Cards/Uncommon/RadiantMight.cs TheArchitectCode/Cards/Uncommon/SoulExchange.cs TheArchitectCode/Powers/Architect/DivineGracePower.cs TheArchitectCode/Powers/Architect/SanctumOfVigorPower.cs Tests
git commit -m "fix: filter invalid enchant targets before selection"
```

### Task 5: Remove Prompt-Key, Preview, and Null-Input Crash Paths

**Files:**
- Modify: `TheArchitectCode/Cards/Rare/HolyLight.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/ImmovableAsTheMountain.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/MonumentHammer.cs`
- Modify: `TheArchitectCode/Powers/Architect/DivineGracePower.cs`
- Modify: `TheArchitectCode/Powers/Architect/TestSubjectPower.cs`
- Modify: `TheArchitect/localization/eng/cards.json`
- Modify: `TheArchitect/localization/eng/powers.json`
- Test: corresponding card/power tests plus localization tests

- [ ] **Step 1: Write failing tests for missing prompt keys and null-preview guards**

```csharp
[ArchitectTest]
public static void MonumentHammer_CombatPreview_DoesNotCallStateWithNullKey()
{
    // Assert preview generation succeeds.
}
```

- [ ] **Step 2: Run the prompt and preview tests to verify red**

Run: `dotnet test Tests/TheArchitect.Tests.csproj --filter "FullyQualifiedName~HolyLight|FullyQualifiedName~ImmovableAsTheMountain|FullyQualifiedName~MonumentHammer|FullyQualifiedName~DivineGrace|FullyQualifiedName~TestSubject|FullyQualifiedName~Localization"`
Expected: FAIL from missing `selectionScreenPrompt` entries or null-sensitive preview code.

- [ ] **Step 3: Add the missing localization entries and guard preview code**

Implementation checklist:
- add missing `selectionScreenPrompt` localization keys
- use existing prompt naming conventions
- stop passing `null` into weak-table-backed preview state
- ensure “no valid target” paths skip selection screens cleanly

- [ ] **Step 4: Re-run the prompt and preview tests**

Run: same command as Step 2
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add TheArchitectCode/Cards/Rare/HolyLight.cs TheArchitectCode/Cards/Uncommon/ImmovableAsTheMountain.cs TheArchitectCode/Cards/Uncommon/MonumentHammer.cs TheArchitectCode/Powers/Architect/DivineGracePower.cs TheArchitectCode/Powers/Architect/TestSubjectPower.cs TheArchitect/localization/eng/cards.json TheArchitect/localization/eng/powers.json Tests
git commit -m "fix: remove prompt and preview crash paths"
```

### Task 6: Apply the Safe Generated-Card Path to Runtime Error Callers

**Files:**
- Modify: `TheArchitectCode/Cards/Rare/TeaOfDrowsiness.cs`
- Modify: `TheArchitectCode/Cards/Rare/WriteDestiny.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/CelestialWar.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/GuardedDrowse.cs`
- Modify if needed: `TheArchitectCode/Helpers/ArchitectEnchantmentHelper.cs`
- Test: corresponding card/power tests

- [ ] **Step 1: Write failing tests for each runtime caller that currently creates broken generated cards**

```csharp
[ArchitectTest]
public static async Task TeaOfDrowsiness_DoesNotEnterMockPoolRefreshPath()
{
    // Assert pile refresh succeeds after generated Drowsy insertion.
}
```

- [ ] **Step 2: Run the targeted tests to verify red**

Run: `dotnet test Tests/TheArchitect.Tests.csproj --filter "FullyQualifiedName~TeaOfDrowsiness|FullyQualifiedName~WriteDestiny|FullyQualifiedName~CelestialWar|FullyQualifiedName~GuardedDrowse"`
Expected: FAIL because generated cards are missing valid ownership/run-state setup.

- [ ] **Step 3: Route all four callers through the shared safe helper**

Implementation rule:
- no direct raw card construction
- no special-case mock-only API calls
- all generated cards must be bound before pile insertion

- [ ] **Step 4: Re-run the targeted generated-card runtime tests**

Run: same command as Step 2
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add TheArchitectCode/Cards/Rare/TeaOfDrowsiness.cs TheArchitectCode/Cards/Rare/WriteDestiny.cs TheArchitectCode/Cards/Uncommon/CelestialWar.cs TheArchitectCode/Cards/Uncommon/GuardedDrowse.cs TheArchitectCode/Helpers/ArchitectEnchantmentHelper.cs Tests
git commit -m "fix: route generated card callers through safe helper"
```

### Task 7: Repair Replay, Duplicate-Effect, and Pending-Effect Runtime Semantics

**Files:**
- Modify: `TheArchitectCode/Cards/Rare/Omnipotence.cs`
- Modify: `TheArchitectCode/Powers/Architect/FateVortexPower.cs`
- Modify if needed: shared replay/pending-effect infrastructure under `TheArchitectCode/Helpers/`
- Test: `Tests/Cards/Rare/*Omnipotence*.cs`
- Test: `Tests/Powers/Architect/FateVortexPowerTests.cs`

- [ ] **Step 1: Write failing tests that capture the intended non-duplicate behavior**

```csharp
[ArchitectTest]
public static async Task FateVortexPower_EnchantsAtTurnStart()
{
    // Assert one valid hand card receives a basic enchant at the intended timing.
}
```

- [ ] **Step 2: Run the targeted tests to verify red**

Run: `dotnet test Tests/TheArchitect.Tests.csproj --filter "FullyQualifiedName~FateVortex|FullyQualifiedName~Omnipotence"`
Expected: FAIL because timing or replay behavior is wrong/duplicated.

- [ ] **Step 3: Align timing and uniqueness with the intended card/power roles**

Implementation rule:
- `FateVortexPower` must trigger at the actual intended turn-start timing
- `Omnipotence` must stop duplicating `FateVortex` behavior if the comments indicate overlap is accidental

- [ ] **Step 4: Re-run the targeted timing tests**

Run: same command as Step 2
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add TheArchitectCode/Cards/Rare/Omnipotence.cs TheArchitectCode/Powers/Architect/FateVortexPower.cs Tests
git commit -m "fix: align vortex and omnipotence runtime behavior"
```

### Task 8: Fix Pile Timing, Reinsert Timing, and Refresh Semantics

**Files:**
- Modify: `TheArchitectCode/Cards/Rare/FinalJudgmentOfTheRadiantScepter.cs`
- Modify: `TheArchitectCode/Cards/Rare/StayTheBlade.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/CyclingEtch.cs`
- Test: `Tests/Cards/Rare/FinalJudgmentOfTheRadiantScepterTests.cs`
- Test: `Tests/Cards/Rare/StayTheBladeTests.cs` if missing, create
- Test: `Tests/Cards/Rare/CyclingEtchTests.cs`

- [ ] **Step 1: Write failing tests for first-play shuffle/refresh timing**

```csharp
[ArchitectTest]
public static async Task StayTheBlade_ShufflesBackOnFirstPlay()
{
    // Assert draw-pile reinsertion occurs after the first play, not the second.
}
```

- [ ] **Step 2: Run the timing tests to verify red**

Run: `dotnet test Tests/TheArchitect.Tests.csproj --filter "FullyQualifiedName~FinalJudgmentOfTheRadiantScepter|FullyQualifiedName~StayTheBlade|FullyQualifiedName~CyclingEtch"`
Expected: FAIL because reinsertion or enchant refresh timing is late.

- [ ] **Step 3: Implement the minimal timing fixes**

Implementation checklist:
- first play should reinsert immediately when intended
- `CyclingEtch` should also refresh its enchant state when the comment says so
- do not broaden effect scope beyond the test

- [ ] **Step 4: Re-run the pile timing tests**

Run: same command as Step 2
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add TheArchitectCode/Cards/Rare/FinalJudgmentOfTheRadiantScepter.cs TheArchitectCode/Cards/Rare/StayTheBlade.cs TheArchitectCode/Cards/Uncommon/CyclingEtch.cs Tests
git commit -m "fix: correct shuffle and refresh timing"
```

### Task 9: Complete Missing Card Effects

**Files:**
- Modify: `TheArchitectCode/Cards/Rare/GrandOpus.cs`
- Modify: `TheArchitectCode/Cards/Rare/SkyrendJudgment.cs`
- Modify: `TheArchitectCode/Cards/Rare/WakingCataclysm.cs`
- Modify: `TheArchitectCode/Cards/Rare/Rollback.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/Doctrine.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/Proliferation.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/RaiseOffspring.cs`
- Test: corresponding card tests

- [ ] **Step 1: Write failing tests for each unfinished behavior**

Examples:

```csharp
[ArchitectTest]
public static async Task GrandOpus_HitsXTimesInsteadOfScalingSingleHitDamage()
{
    // Assert repeated hit count, not multiplied base damage.
}
```

```csharp
[ArchitectTest]
public static async Task WakingCataclysm_PlaysImmediatelyAtCombatStartWhenEnchanted()
{
    // Assert enchant is removed and card is auto-played at combat start.
}
```

- [ ] **Step 2: Run the missing-effect tests to verify red**

Run: `dotnet test Tests/TheArchitect.Tests.csproj --filter "FullyQualifiedName~GrandOpus|FullyQualifiedName~SkyrendJudgment|FullyQualifiedName~WakingCataclysm|FullyQualifiedName~Rollback|FullyQualifiedName~Doctrine|FullyQualifiedName~Proliferation|FullyQualifiedName~RaiseOffspring"`
Expected: FAIL because current implementations are incomplete or use the wrong state value.

- [ ] **Step 3: Implement the minimal game-rule fixes**

Implementation checklist:
- `GrandOpus`: convert scalar damage/block scaling into repeated applications
- `SkyrendJudgment`: implement cost reduction whenever an enchanted card is played
- `WakingCataclysm`: add combat-start trigger and upgraded damage 32
- `Doctrine`: count total Sharp/Nimble stacks in hand, then consume them
- `Proliferation`: update actual base damage and attack-times values directly
- `RaiseOffspring`: write directly to card state instead of combat-preview-only data
- `Rollback`: add the missing described effect text if runtime logic already exists, or implement the described effect if absent

- [ ] **Step 4: Re-run the missing-effect tests**

Run: same command as Step 2
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add TheArchitectCode/Cards/Rare/GrandOpus.cs TheArchitectCode/Cards/Rare/SkyrendJudgment.cs TheArchitectCode/Cards/Rare/WakingCataclysm.cs TheArchitectCode/Cards/Rare/Rollback.cs TheArchitectCode/Cards/Uncommon/Doctrine.cs TheArchitectCode/Cards/Uncommon/Proliferation.cs TheArchitectCode/Cards/Uncommon/RaiseOffspring.cs Tests
git commit -m "fix: implement remaining card todo behaviors"
```

### Task 10: Finish Text, Localization, and Naming Cleanup

**Files:**
- Modify: `TheArchitectCode/Cards/Rare/FinalJudgmentOfTheRadiantScepter.cs`
- Modify: `TheArchitectCode/Cards/Uncommon/ImmovableAsTheMountain.cs`
- Modify: `TheArchitectCode/Cards/Rare/SkyrendJudgment.cs`
- Modify: `TheArchitectCode/Cards/Rare/WakingCataclysm.cs`
- Modify: `TheArchitectCode/Powers/Architect/DivineGracePower.cs`
- Modify: `TheArchitectCode/Powers/Architect/TestSubjectPower.cs`
- Modify: `TheArchitect/localization/eng/cards.json`
- Modify: `TheArchitect/localization/eng/powers.json`
- Test: localization and metadata tests

- [ ] **Step 1: Write failing metadata/localization tests where coverage is missing**

```csharp
[ArchitectTest]
public static void FinalJudgment_UsesShortNameAndMatchingLocalization()
{
    // Assert code identifier and localization align after renaming.
}
```

- [ ] **Step 2: Run the text/localization tests to verify red**

Run: `dotnet test Tests/TheArchitect.Tests.csproj --filter "FullyQualifiedName~Localization|FullyQualifiedName~Metadata|FullyQualifiedName~SkyrendJudgment|FullyQualifiedName~WakingCataclysm|FullyQualifiedName~ImmovableAsTheMountain|FullyQualifiedName~FinalJudgment"`
Expected: FAIL because text, keys, or names still reflect stale behavior.

- [ ] **Step 3: Update names, descriptions, and prompt strings to match final behavior**

Implementation checklist:
- shorten `FinalJudgmentOfTheRadiantScepter` if codebase patterns allow safe rename
- shorten `ImmovableAsTheMountain` name
- update `SkyrendJudgment` wording to emphasize “cost 1 less”
- update `WakingCataclysm` wording from “awakes immediately” to immediate play text
- ensure all localization keys referenced in code exist and match final IDs

- [ ] **Step 4: Re-run the text/localization tests**

Run: same command as Step 2
Expected: PASS

- [ ] **Step 5: Commit**

```bash
git add TheArchitectCode/Cards/Rare/FinalJudgmentOfTheRadiantScepter.cs TheArchitectCode/Cards/Uncommon/ImmovableAsTheMountain.cs TheArchitectCode/Cards/Rare/SkyrendJudgment.cs TheArchitectCode/Cards/Rare/WakingCataclysm.cs TheArchitectCode/Powers/Architect/DivineGracePower.cs TheArchitectCode/Powers/Architect/TestSubjectPower.cs TheArchitect/localization/eng/cards.json TheArchitect/localization/eng/powers.json Tests
git commit -m "fix: align card text and localization with behavior"
```

### Task 11: Resolve Remaining Non-Blocking Helper TODOs and Full Regression

**Files:**
- Modify if needed: `TheArchitectCode/Helpers/ArchitectEnchantmentHelper.cs`
- Review: any touched gameplay files
- Test: full suite

- [ ] **Step 1: Write the last failing tests for any helper TODO still open**

Focus:
- `AttackAll` behavior if a simultaneous-hit path exists
- any leftover assertions from the earlier TODO inventory that remain unmatched

- [ ] **Step 2: Run the targeted helper tail tests to verify red**

Run: `dotnet test Tests/TheArchitect.Tests.csproj --filter FullyQualifiedName~ArchitectEnchantmentHelper`
Expected: FAIL only if a helper TODO is still unresolved.

- [ ] **Step 3: Implement the last minimal helper cleanup**

Rule:
- prefer matching engine-supported semantics over speculative redesign
- if simultaneous attack is not supported, update the helper comment and associated behavior tests to the accepted sequential semantics

- [ ] **Step 4: Run the full suite**

Run: `dotnet test Tests/TheArchitect.Tests.csproj`
Expected: PASS with zero failing tests.

- [ ] **Step 5: Commit**

```bash
git add TheArchitectCode/Helpers/ArchitectEnchantmentHelper.cs Tests
git commit -m "fix: close remaining helper todos and verify suite"
```

---

## Recommended Execution Order

1. Task 1
2. Task 2
3. Task 3
4. Task 4
5. Task 5
6. Task 6
7. Task 7
8. Task 8
9. Task 9
10. Task 10
11. Task 11

This order preserves the required sequence:
- helper legality first
- runtime errors second
- gameplay/text completion last

## Review Notes

- This plan intentionally supersedes the narrower `2026-04-11-todo-remediation-plan.md` by covering every current source `TODO`, including naming and localization work.
- If a code rename such as `FinalJudgmentOfTheRadiantScepter` proves to have wide asset or save-compatibility impact, split the rename into its own micro-task but keep the descriptive text fix in Task 10.
