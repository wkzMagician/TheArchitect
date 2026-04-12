# TODO Remediation Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Resolve all current source TODOs by first fixing behavior-breaking card logic, then correcting card/pool ownership semantics, then cleaning up helper boundaries and open design confirmations.

**Architecture:** Treat the current TODOs as three tracks: rules-correctness fixes, model/pool registration fixes, and helper/API cleanup. Execute behavior fixes first because several TODOs describe current effects that are wrong at runtime; only after those are green should we refactor helper ownership and selection APIs.

**Tech Stack:** C#/.NET 9, Godot 4.5.1 Mono, sts2/BaseLib mod APIs, custom lightweight test harness in `Tests/`

---

## TODO Inventory

### Rules or behavior bugs

- `TheArchitectCode/Cards/Common/CuratedHand.cs`
  Current implementation discards from the post-draw hand snapshot; TODO says the current behavior is wrong and should discard all non-Enchanted cards, not only the newly drawn subset.

- `TheArchitectCode/Cards/Curse/Drowsy.cs`
  TODO says the card behavior is wrong: it should be playable, use `Exhaust` instead of `Ethereal`, and if not exhausted by end of combat it should be added to the deck.

- `TheArchitectCode/Cards/Rare/ChannelPower.cs`
  TODO says current behavior is incorrect; it should apply a replay-X effect to a chosen card rather than immediately auto-playing it X times.

- `TheArchitectCode/Cards/Rare/CursePurge.cs`
  TODO says purge scope is incomplete; it currently removes curses only from deck, but should cover all relevant piles.

- `TheArchitectCode/Cards/Rare/Erasure.cs`
  TODO documents an actual runtime error caused by applying an incompatible enchantment without validating target compatibility.

- `TheArchitectCode/Cards/Ancient/InfiniteBlueprint.cs`
  TODO says the current “set enchantment amount to 999” logic is likely wrong; intended behavior is closer to making cards `IMultiEnchantCapable`.

### Card or pool identity confirmations

- `TheArchitectCode/Cards/Ancient/AncientVerdict.cs`
  Needs rules confirmation: whether `AncientVerdict` is the upgraded form of `Sigilbreaker`.

- `TheArchitectCode/Cards/Curse/Drowsy.cs`
  Needs confirmation that `Drowsy` is not part of The Architect card pool and should behave as a curse/token card.

- `TheArchitectCode/Cards/Tokens/TemperingChoiceCard.cs`
  Needs confirmation that Tempering choice cards are special UI cards, not part of the normal pool, and whether `Skill` is the right type.

### Optional redesign or cleanup

- `TheArchitectCode/Cards/Common/SweepTheHost.cs`
  TODO proposes changing target type dynamically on enchant, but current runtime behavior is already implementable without that mutation. This is a design cleanup, not a correctness blocker.

- `TheArchitectCode/Cards/Rare/BlightAnointing.cs`
  TODO proposes redesigning the card into a persistent power. This is a feature redesign, not a bug fix.

- `TheArchitectCode/Helpers/ArchitectEnchantmentHelper.cs:ChooseFromHand`
  TODO says helper boundaries are muddled between general card-selection logic and enchantment-specific validation.

- `TheArchitectCode/Helpers/ArchitectEnchantmentHelper.cs:AddDrowsy`
  Embedded TODO documents a deeper card creation/pool ownership issue around generated `Drowsy` cards and mock pool paths.

---

## Execution Order

1. Fix behavior bugs that can cause user-visible incorrect gameplay or crashes.
2. Fix token/pool ownership and generated-card lifecycle issues.
3. Confirm unresolved design assumptions with the human before changing card upgrade lineage or redesigning effects.
4. Refactor helper boundaries only after the existing behavior is stable and covered by tests.

---

### Task 1: Lock in the Current TODO Surface with Regression Tests

**Files:**
- Modify: `Tests/Cards/Common/CuratedHandTests.cs`
- Modify: `Tests/Cards/Tokens/DrowsyTests.cs`
- Modify: `Tests/Cards/Rare/ChannelPowerTests.cs`
- Modify: `Tests/Cards/Rare/CursePurgeTests.cs`
- Modify: `Tests/Cards/Rare/ErasureTests.cs`
- Modify: `Tests/Cards/Ancient/InfiniteBlueprintTests.cs`

- [ ] Add or extend failing tests for each TODO-backed behavior bug.
- [ ] Ensure each test names the intended game-rule behavior, not implementation details.
- [ ] Run the targeted tests and verify they fail for the expected reason before changing production code.

### Task 2: Fix `Erasure` Compatibility Validation First

**Files:**
- Modify: `TheArchitectCode/Cards/Rare/Erasure.cs`
- Modify: `TheArchitectCode/Helpers/ArchitectEnchantmentHelper.cs`
- Test: `Tests/Cards/Rare/ErasureTests.cs`

- [ ] Add a failing test showing `Erasure` cannot target cards that cannot legally receive `SoulPower`.
- [ ] Reuse existing enchantment compatibility checks instead of adding a one-off special case.
- [ ] Make the selection filter reject incompatible cards up front.
- [ ] Run the targeted test until it passes.

### Task 3: Correct `CuratedHand` Discard Semantics

**Files:**
- Modify: `TheArchitectCode/Cards/Common/CuratedHand.cs`
- Test: `Tests/Cards/Common/CuratedHandTests.cs`

- [ ] Add a failing test that distinguishes “discard all non-Enchanted cards in hand” from “discard only newly drawn cards.”
- [ ] Implement the minimal logic change needed to inspect the full hand state after draw.
- [ ] Verify the test passes and no adjacent card-selection behavior regresses.

### Task 4: Fix `CursePurge` Scope Across All Relevant Piles

**Files:**
- Modify: `TheArchitectCode/Cards/Rare/CursePurge.cs`
- Modify: `TheArchitectCode/Helpers/ArchitectEnchantmentHelper.cs` if shared pile enumeration is needed
- Test: `Tests/Cards/Rare/CursePurgeTests.cs`

- [ ] Add a failing test that places curses in deck plus other reachable piles and asserts all eligible curses are removed.
- [ ] Decide exact scope from game rules: deck, draw pile, discard pile, hand, and any in-combat piles.
- [ ] Implement with one shared pile enumeration path instead of separate ad hoc loops.
- [ ] Verify plating gain matches total curses removed.

### Task 5: Rework `ChannelPower` to Apply Replay-X Instead of Immediate Replay

**Files:**
- Modify: `TheArchitectCode/Cards/Rare/ChannelPower.cs`
- Modify or create supporting replay-effect infrastructure in `TheArchitectCode/`
- Test: `Tests/Cards/Rare/ChannelPowerTests.cs`

- [ ] Confirm intended rule from reference card behavior before implementation.
- [ ] Add a failing test that proves the chosen card receives a replay-style effect instead of being auto-played immediately.
- [ ] Implement the minimal effect carrier needed for “replay this card X times.”
- [ ] Re-run the targeted tests and any affected combat-history tests.

### Task 6: Fix `Drowsy` Card Semantics and Generated-Card Ownership

**Files:**
- Modify: `TheArchitectCode/Cards/Curse/Drowsy.cs`
- Modify: `TheArchitectCode/Helpers/ArchitectEnchantmentHelper.cs`
- Modify: `TheArchitectCode/Character/*` only if pool registration changes require it
- Test: `Tests/Cards/Tokens/DrowsyTests.cs`
- Test: `Tests/Powers/Architect/DrowsyEnginePowerTests.cs`

- [ ] Add failing tests for the intended keyword/state behavior: playable, exhausts, and persistence-to-deck if not exhausted by combat end.
- [ ] Add a failing test for generated `Drowsy` creation that catches the pool/owner/mock-path issue documented in `AddDrowsy`.
- [ ] Correct `Drowsy` model metadata and runtime lifecycle first.
- [ ] Fix `AddDrowsy` so generated cards are created as proper canonical/token models with valid pool ownership.
- [ ] Verify both direct `Drowsy` behavior and `DrowsyEnginePower` generation scenarios.

### Task 7: Correct `InfiniteBlueprint` Infinite-Enchant Semantics

**Files:**
- Modify: `TheArchitectCode/Cards/Ancient/InfiniteBlueprint.cs`
- Modify: `TheArchitectCode/Enchantments/Framework/*` if a reusable “infinite stacks” representation is needed
- Test: `Tests/Cards/Ancient/InfiniteBlueprintTests.cs`

- [ ] Confirm exact intended mechanic: infinite count on existing enchants, multi-enchant support, or both.
- [ ] Add a failing test that captures the intended persistent behavior.
- [ ] Replace the hard-coded `999` sentinel with an explicit representation if the engine supports one; otherwise isolate the fallback in a named helper.
- [ ] Verify the card still interacts correctly with stack refresh and removal effects.

### Task 8: Confirm Card-Lineage and Pool Assumptions

**Files:**
- Review only unless follow-up edits are requested:
  - `TheArchitectCode/Cards/Ancient/AncientVerdict.cs`
  - `TheArchitectCode/Cards/Curse/Drowsy.cs`
  - `TheArchitectCode/Cards/Tokens/TemperingChoiceCard.cs`

- [ ] Ask the human to confirm:
  - Is `AncientVerdict` the true upgraded/replacement form of `Sigilbreaker`?
  - Is `Drowsy` intentionally excluded from the normal Architect pool and treated as a generated curse?
  - Are `TemperingSharpChoice` and `TemperingNimbleChoice` purely UI choice cards, and is `CardType.Skill` intentional?
- [ ] Only after confirmation, update pool/type metadata and related tests.

### Task 9: Defer or Separate Redesign TODOs

**Files:**
- `TheArchitectCode/Cards/Common/SweepTheHost.cs`
- `TheArchitectCode/Cards/Rare/BlightAnointing.cs`

- [ ] Do not mix these redesigns into bugfix work unless explicitly requested.
- [ ] Convert them into separate feature specs if they are still desired after bugfixes land.

### Task 10: Refactor Helper Boundaries After Behavior Stabilizes

**Files:**
- Modify: `TheArchitectCode/Helpers/ArchitectEnchantmentHelper.cs`
- Create if needed: `TheArchitectCode/Helpers/ArchitectSelectionHelper.cs`
- Test: any affected helper or card tests

- [ ] Extract generic hand/draw/discard selection utilities away from enchantment-specific validation.
- [ ] Leave enchant legality checks in enchantment-focused code paths.
- [ ] Update callers incrementally, not in one large rewrite.
- [ ] Run the full test suite after the refactor.

---

## Recommended First Pass

If the goal is maximum risk reduction with minimum scope, execute in this order:

1. `Erasure`
2. `CuratedHand`
3. `CursePurge`
4. `Drowsy` + `AddDrowsy`
5. `ChannelPower`
6. `InfiniteBlueprint`
7. confirmation items
8. helper cleanup

This order addresses current crash potential and rules mismatches before optional redesigns.
