# Project Structure Refactor Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Split all cards and powers into single-class files and reorganize cards into `Basic/Common/Uncommon/Rare/Ancient/Curse`.

**Architecture:** Keep the current base classes and behavior intact while replacing aggregate source files with one-class-per-file organization. Move cards by actual rarity/category, remove obsolete directory concepts (`Pvp`, `Power`, `Tokens`), and update behavior golden hashes only where source extraction changes tracked hashes.

**Tech Stack:** C#, .NET 9, Godot, Slay the Spire 2 modding APIs, custom golden-hash tests

---

### Task 1: Inventory The Existing Card And Power Types

**Files:**
- Modify: [TheArchitectCode/Cards/Common/ArchitectEnchantCards.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Common/ArchitectEnchantCards.cs)
- Modify: [TheArchitectCode/Cards/Common/ArchitectUtilityCards.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Common/ArchitectUtilityCards.cs)
- Modify: [TheArchitectCode/Cards/Power/ArchitectPowerCards.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Power/ArchitectPowerCards.cs)
- Modify: [TheArchitectCode/Cards/Pvp/ArchitectPvpCards.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Pvp/ArchitectPvpCards.cs)
- Modify: [TheArchitectCode/Cards/Rare/ArchitectAdvancedCards.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Rare/ArchitectAdvancedCards.cs)
- Modify: [TheArchitectCode/Cards/Tokens/Drowsy.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Tokens/Drowsy.cs)
- Modify: [TheArchitectCode/Powers/Architect/ArchitectCorePowers.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Powers/Architect/ArchitectCorePowers.cs)

- [ ] Read every aggregate card and power file and list each contained type with its target destination folder.
- [ ] Confirm uncommon cards currently stored under `Cards/Common` and note their future `Cards/Uncommon` targets.
- [ ] Confirm whether any cards belong under `Ancient` or `Curse`; if none exist, still create the directories so the layout matches the approved structure.

### Task 2: Split Basic And Common-Rarity Cards

**Files:**
- Create: `TheArchitectCode/Cards/Basic/<CardName>.cs`
- Create: `TheArchitectCode/Cards/Common/<CardName>.cs`
- Modify: [TheArchitectCode/Cards/Basic/DefendArchitect.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Basic/DefendArchitect.cs)
- Modify: [TheArchitectCode/Cards/Basic/Sigilbreaker.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Basic/Sigilbreaker.cs)
- Modify: [TheArchitectCode/Cards/Basic/StrikeArchitect.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Basic/StrikeArchitect.cs)
- Modify: [TheArchitectCode/Cards/Basic/Tempering.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Basic/Tempering.cs)
- Delete contents from: [TheArchitectCode/Cards/Common/ArchitectEnchantCards.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Common/ArchitectEnchantCards.cs)
- Delete contents from: [TheArchitectCode/Cards/Common/ArchitectUtilityCards.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Common/ArchitectUtilityCards.cs)

- [ ] Write a focused failing compile check by moving one low-risk common card into its own file and running `dotnet build` to expose missing usings or namespace assumptions.
- [ ] Extract each basic/common card into its own file with the same class body and namespace.
- [ ] Remove the extracted classes from the aggregate files.
- [ ] Delete the aggregate files once all their classes have been moved.
- [ ] Run `dotnet build TheArchitect.csproj --no-restore`.

### Task 3: Split Uncommon Cards Into Their Own Directory

**Files:**
- Create: `TheArchitectCode/Cards/Uncommon/<CardName>.cs`
- Modify: files created in Task 2 if they temporarily held uncommon cards

- [ ] Move every uncommon card out of `Cards/Common` into `Cards/Uncommon`.
- [ ] Keep one class per file and one file per class.
- [ ] Run `dotnet build TheArchitect.csproj --no-restore`.

### Task 4: Split Rare, Ancient, And Curse Cards

**Files:**
- Create: `TheArchitectCode/Cards/Rare/<CardName>.cs`
- Create: `TheArchitectCode/Cards/Ancient/<CardName>.cs`
- Create: `TheArchitectCode/Cards/Curse/<CardName>.cs`
- Delete contents from: [TheArchitectCode/Cards/Rare/ArchitectAdvancedCards.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Rare/ArchitectAdvancedCards.cs)
- Delete contents from: [TheArchitectCode/Cards/Power/ArchitectPowerCards.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Power/ArchitectPowerCards.cs)
- Delete contents from: [TheArchitectCode/Cards/Pvp/ArchitectPvpCards.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Pvp/ArchitectPvpCards.cs)
- Delete contents from: [TheArchitectCode/Cards/Tokens/Drowsy.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Tokens/Drowsy.cs)

- [ ] Extract each rare card into its own file under `Cards/Rare`.
- [ ] Re-home former `Power`, `Pvp`, and `Token` cards into the approved rarity/category directories.
- [ ] If `Ancient` or `Curse` currently have no card classes, leave the directories present and document that they are intentionally empty after the refactor.
- [ ] Run `dotnet build TheArchitect.csproj --no-restore`.

### Task 5: Split Architect Powers

**Files:**
- Create: `TheArchitectCode/Powers/Architect/<PowerName>.cs`
- Delete contents from: [TheArchitectCode/Powers/Architect/ArchitectCorePowers.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Powers/Architect/ArchitectCorePowers.cs)
- Keep: [TheArchitectCode/Powers/TheArchitectPower.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Powers/TheArchitectPower.cs)

- [ ] Move each power class into its own file under `TheArchitectCode/Powers/Architect`.
- [ ] Preserve namespaces and base types.
- [ ] Delete the old aggregate power file when empty.
- [ ] Run `dotnet build TheArchitect.csproj --no-restore`.

### Task 6: Repair Project References And UIDs

**Files:**
- Modify or create: any generated `.cs.uid` files that need to match the new file layout
- Modify: any project includes that rely on old paths, if present

- [ ] Check for stale references to deleted aggregate files.
- [ ] Ensure each new `.cs` file has a corresponding `.cs.uid` only if required by the project’s current Godot workflow.
- [ ] Run `dotnet build TheArchitect.csproj --no-restore -p:OutputPath=.artifacts/build/ -p:ModsPath=.artifacts/mods/`.

### Task 7: Update Golden Hashes Affected By Extraction

**Files:**
- Modify: [Tests/Infrastructure/BehaviorGoldenHashes.g.cs](/D:/game/sts2/TheArchitect/Tests/Infrastructure/BehaviorGoldenHashes.g.cs)

- [ ] Run `dotnet run --project Tests/TheArchitect.Tests.csproj --no-restore`.
- [ ] Update only the golden hashes for types changed by file extraction.
- [ ] Do not change the unrelated existing `LayeredBrace` expected hash unless explicitly handling that separate issue.
- [ ] Re-run `dotnet run --project Tests/TheArchitect.Tests.csproj --no-restore`.

### Task 8: Final Verification

**Files:**
- No new files expected

- [ ] Run `dotnet build TheArchitect.csproj --no-restore -p:OutputPath=.artifacts/build/ -p:ModsPath=.artifacts/mods/`.
- [ ] Run `dotnet run --project Tests/TheArchitect.Tests.csproj --no-restore`.
- [ ] Confirm the only allowed failure is the known unrelated `LayeredBrace` golden hash mismatch.
- [ ] Summarize the final folder structure and call out any intentionally empty rarity directories.
