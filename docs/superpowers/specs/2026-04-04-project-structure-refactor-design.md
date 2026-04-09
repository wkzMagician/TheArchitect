# Project Structure Refactor Design

**Date:** 2026-04-04

**Goal**

Refactor the project structure so that:
- every card lives in its own file
- cards are organized strictly by rarity/category under `Basic`, `Common`, `Uncommon`, `Rare`, `Ancient`, and `Curse`
- every power lives in its own file

**Current Problems**

- `Common`, `Uncommon`, `Power`, `Pvp`, and `Token` cards are mixed inside aggregate files such as [ArchitectUtilityCards.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Common/ArchitectUtilityCards.cs), [ArchitectEnchantCards.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Common/ArchitectEnchantCards.cs), [ArchitectPowerCards.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Power/ArchitectPowerCards.cs), and [ArchitectPvpCards.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/Pvp/ArchitectPvpCards.cs).
- There is no explicit `Uncommon` directory even though uncommon cards exist.
- Powers are grouped in [ArchitectCorePowers.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Powers/Architect/ArchitectCorePowers.cs), which makes navigation and isolated edits harder.

**Approved Structure**

Cards will be organized as:
- `TheArchitectCode/Cards/Basic`
- `TheArchitectCode/Cards/Common`
- `TheArchitectCode/Cards/Uncommon`
- `TheArchitectCode/Cards/Rare`
- `TheArchitectCode/Cards/Ancient`
- `TheArchitectCode/Cards/Curse`

Rules:
- one card class per file
- file name matches class name
- `Pvp`, `Power`, and `Tokens` card directories are removed
- those cards are reassigned into the approved rarity/category folders based on their actual in-game classification
- [TheArchitectCard.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Cards/TheArchitectCard.cs) remains the shared base class

Powers will be organized as:
- `TheArchitectCode/Powers/Architect/<PowerClassName>.cs`

Rules:
- one power class per file
- file name matches class name
- [TheArchitectPower.cs](/D:/game/sts2/TheArchitect/TheArchitectCode/Powers/TheArchitectPower.cs) remains the shared base class
- `ArchitectCorePowers.cs` is removed after all powers are split out

**Migration Constraints**

- This is a structural refactor only. Behavior changes are out of scope.
- Namespaces should stay consistent with the current project style unless a file split requires a local adjustment.
- Tests must continue to pass except for the already-known unrelated `LayeredBrace` golden hash mismatch.
- Behavior golden hashes will need updates for any types whose source file content changes as part of extraction.

**Verification**

- Project builds successfully with:
  - `dotnet build TheArchitect.csproj --no-restore -p:OutputPath=.artifacts/build/ -p:ModsPath=.artifacts/mods/`
- Test suite runs successfully except for the known unrelated `LayeredBrace` failure:
  - `dotnet run --project Tests/TheArchitect.Tests.csproj --no-restore`

