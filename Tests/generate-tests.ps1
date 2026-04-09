$root = $PSScriptRoot

function Write-CardTest($folder, $namespaceSuffix, $className, $prodNamespace, $type, $rarity, $target) {
  $dir = Join-Path $root $folder
  New-Item -ItemType Directory -Force -Path $dir | Out-Null
  $content = @"
using MegaCrit.Sts2.Core.Entities.Cards;
using TheArchitect.Tests.Infrastructure;
using $prodNamespace;

namespace TheArchitect.Tests.$namespaceSuffix;

public static class ${className}Tests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertCardMetadata<$className>($type, $rarity, $target);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertCardBehavior<$className>();
    }
}
"@
  Set-Content -Path (Join-Path $dir "$className`Tests.cs") -Value $content -Encoding UTF8
}

function Write-PowerTest($className, $type, $stackType) {
  $dir = Join-Path $root 'Powers\Architect'
  New-Item -ItemType Directory -Force -Path $dir | Out-Null
  $content = @"
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Powers.Architect;

namespace TheArchitect.Tests.Powers.Architect;

public static class ${className}Tests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertPowerMetadata<$className>($type, $stackType);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertPowerBehavior<$className>();
    }
}
"@
  Set-Content -Path (Join-Path $dir "$className`Tests.cs") -Value $content -Encoding UTF8
}

$cards = @(
@{F='Cards\Basic'; N='Cards.Basic'; P='TheArchitect.TheArchitectCode.Cards.Basic'; C='StrikeArchitect'; T='CardType.Attack'; R='CardRarity.Basic'; G='TargetType.AnyEnemy'},
@{F='Cards\Basic'; N='Cards.Basic'; P='TheArchitect.TheArchitectCode.Cards.Basic'; C='DefendArchitect'; T='CardType.Skill'; R='CardRarity.Basic'; G='TargetType.Self'},
@{F='Cards\Basic'; N='Cards.Basic'; P='TheArchitect.TheArchitectCode.Cards.Basic'; C='Tempering'; T='CardType.Skill'; R='CardRarity.Basic'; G='TargetType.Self'},
@{F='Cards\Basic'; N='Cards.Basic'; P='TheArchitect.TheArchitectCode.Cards.Basic'; C='Sigilbreaker'; T='CardType.Attack'; R='CardRarity.Basic'; G='TargetType.AnyEnemy'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='OpeningDraft'; T='CardType.Skill'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='InstinctAwakened'; T='CardType.Skill'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='MomentumStrike'; T='CardType.Attack'; R='CardRarity.Common'; G='TargetType.AnyEnemy'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='HolyLight'; T='CardType.Skill'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='Guardtrace'; T='CardType.Skill'; R='CardRarity.Common'; G='TargetType.Self'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='JacobsLadder'; T='CardType.Skill'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='Whetstone'; T='CardType.Skill'; R='CardRarity.Common'; G='TargetType.Self'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='Cultivate'; T='CardType.Attack'; R='CardRarity.Common'; G='TargetType.AnyEnemy'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='Erasure'; T='CardType.Skill'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='AncientSeed'; T='CardType.Attack'; R='CardRarity.Common'; G='TargetType.AnyEnemy'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='BlueprintRevision'; T='CardType.Skill'; R='CardRarity.Common'; G='TargetType.Self'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='ShardBarrage'; T='CardType.Attack'; R='CardRarity.Common'; G='TargetType.AllEnemies'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='Sketchcleave'; T='CardType.Attack'; R='CardRarity.Common'; G='TargetType.AnyEnemy'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='StrokeOfRuin'; T='CardType.Attack'; R='CardRarity.Common'; G='TargetType.AnyEnemy'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='Oracle'; T='CardType.Skill'; R='CardRarity.Common'; G='TargetType.Self'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='StayTheBlade'; T='CardType.Skill'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='SweepTheHost'; T='CardType.Attack'; R='CardRarity.Common'; G='TargetType.AnyEnemy'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='WardedCut'; T='CardType.Attack'; R='CardRarity.Common'; G='TargetType.AnyEnemy'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='MonumentHammer'; T='CardType.Attack'; R='CardRarity.Uncommon'; G='TargetType.AnyEnemy'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='CrashingBlow'; T='CardType.Attack'; R='CardRarity.Common'; G='TargetType.AnyEnemy'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='Chant'; T='CardType.Skill'; R='CardRarity.Common'; G='TargetType.AnyEnemy'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='DoublePlatedGuard'; T='CardType.Skill'; R='CardRarity.Common'; G='TargetType.Self'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='Reforge'; T='CardType.Skill'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='RetrieveTheFragments'; T='CardType.Attack'; R='CardRarity.Common'; G='TargetType.AnyEnemy'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='Summon'; T='CardType.Skill'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='CuratedHand'; T='CardType.Attack'; R='CardRarity.Common'; G='TargetType.AnyEnemy'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='RapidDrafting'; T='CardType.Skill'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='HotStart'; T='CardType.Skill'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='PrimedSpark'; T='CardType.Skill'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='MagicCircle'; T='CardType.Skill'; R='CardRarity.Common'; G='TargetType.Self'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='SeedcoreCannon'; T='CardType.Attack'; R='CardRarity.Rare'; G='TargetType.AnyEnemy'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='OriginalSin'; T='CardType.Attack'; R='CardRarity.Common'; G='TargetType.AnyEnemy'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='Lullaby'; T='CardType.Skill'; R='CardRarity.Rare'; G='TargetType.AllEnemies'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='GuardedDrowse'; T='CardType.Skill'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Common'; N='Cards.Common'; P='TheArchitect.TheArchitectCode.Cards.Common'; C='CelestialWar'; T='CardType.Attack'; R='CardRarity.Uncommon'; G='TargetType.AnyEnemy'},
@{F='Cards\Power'; N='Cards.Power'; P='TheArchitect.TheArchitectCode.Cards.Power'; C='SanctumOfVigor'; T='CardType.Power'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Power'; N='Cards.Power'; P='TheArchitect.TheArchitectCode.Cards.Power'; C='DivineGrace'; T='CardType.Power'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Power'; N='Cards.Power'; P='TheArchitect.TheArchitectCode.Cards.Power'; C='ImmovableAsTheMountain'; T='CardType.Power'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Power'; N='Cards.Power'; P='TheArchitect.TheArchitectCode.Cards.Power'; C='Resonance'; T='CardType.Power'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Power'; N='Cards.Power'; P='TheArchitect.TheArchitectCode.Cards.Power'; C='TestSubject'; T='CardType.Power'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Power'; N='Cards.Power'; P='TheArchitect.TheArchitectCode.Cards.Power'; C='Rebirth'; T='CardType.Power'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Power'; N='Cards.Power'; P='TheArchitect.TheArchitectCode.Cards.Power'; C='FormOfCreation'; T='CardType.Power'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Power'; N='Cards.Power'; P='TheArchitect.TheArchitectCode.Cards.Power'; C='Omnipotence'; T='CardType.Power'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Power'; N='Cards.Power'; P='TheArchitect.TheArchitectCode.Cards.Power'; C='Sanctuary'; T='CardType.Power'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Power'; N='Cards.Power'; P='TheArchitect.TheArchitectCode.Cards.Power'; C='FateVortex'; T='CardType.Power'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Power'; N='Cards.Power'; P='TheArchitect.TheArchitectCode.Cards.Power'; C='Destroyer'; T='CardType.Power'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Power'; N='Cards.Power'; P='TheArchitect.TheArchitectCode.Cards.Power'; C='Recuperate'; T='CardType.Power'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Power'; N='Cards.Power'; P='TheArchitect.TheArchitectCode.Cards.Power'; C='DrowsyEngine'; T='CardType.Power'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Power'; N='Cards.Power'; P='TheArchitect.TheArchitectCode.Cards.Power'; C='Requiem'; T='CardType.Power'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Pvp'; N='Cards.Pvp'; P='TheArchitect.TheArchitectCode.Cards.Pvp'; C='TeachAManToFish'; T='CardType.Skill'; R='CardRarity.Uncommon'; G='TargetType.AnyAlly'},
@{F='Cards\Pvp'; N='Cards.Pvp'; P='TheArchitect.TheArchitectCode.Cards.Pvp'; C='HandOff'; T='CardType.Skill'; R='CardRarity.Rare'; G='TargetType.AnyAlly'},
@{F='Cards\Pvp'; N='Cards.Pvp'; P='TheArchitect.TheArchitectCode.Cards.Pvp'; C='SharedSanctum'; T='CardType.Power'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Pvp'; N='Cards.Pvp'; P='TheArchitect.TheArchitectCode.Cards.Pvp'; C='Doctrine'; T='CardType.Skill'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='GrandOpus'; T='CardType.Skill'; R='CardRarity.Rare'; G='TargetType.AnyEnemy'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='SoulExchange'; T='CardType.Skill'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='BlightAnointing'; T='CardType.Skill'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='DivineSelection'; T='CardType.Skill'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='EternalVerdict'; T='CardType.Attack'; R='CardRarity.Rare'; G='TargetType.AnyEnemy'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='RadiantMight'; T='CardType.Attack'; R='CardRarity.Uncommon'; G='TargetType.AnyEnemy'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='ChorusOfEvasion'; T='CardType.Skill'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='Daydream'; T='CardType.Skill'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='DivineHammerfall'; T='CardType.Attack'; R='CardRarity.Uncommon'; G='TargetType.AnyEnemy'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='StripLife'; T='CardType.Skill'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='ShieldOfSacrifice'; T='CardType.Skill'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='SpearOfSacrifice'; T='CardType.Attack'; R='CardRarity.Uncommon'; G='TargetType.AnyEnemy'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='CyclingEtch'; T='CardType.Attack'; R='CardRarity.Uncommon'; G='TargetType.AnyEnemy'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='SkyrendJudgment'; T='CardType.Skill'; R='CardRarity.Rare'; G='TargetType.AllEnemies'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='TeaOfDrowsiness'; T='CardType.Skill'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='CursePurge'; T='CardType.Skill'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='Depose'; T='CardType.Attack'; R='CardRarity.Rare'; G='TargetType.AnyEnemy'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='Rollback'; T='CardType.Skill'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='WriteDestiny'; T='CardType.Skill'; R='CardRarity.Rare'; G='TargetType.Self'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='ChannelPower'; T='CardType.Skill'; R='CardRarity.Rare'; G='TargetType.AnyEnemy'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='LayeredBrace'; T='CardType.Skill'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='Ascend'; T='CardType.Attack'; R='CardRarity.Uncommon'; G='TargetType.AnyEnemy'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='Trinity'; T='CardType.Attack'; R='CardRarity.Common'; G='TargetType.AnyEnemy'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='Proliferation'; T='CardType.Attack'; R='CardRarity.Uncommon'; G='TargetType.AnyEnemy'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='RaiseOffspring'; T='CardType.Skill'; R='CardRarity.Uncommon'; G='TargetType.Self'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='FinalJudgmentOfTheRadiantScepter'; T='CardType.Attack'; R='CardRarity.Rare'; G='TargetType.AnyEnemy'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='WakingCataclysm'; T='CardType.Attack'; R='CardRarity.Rare'; G='TargetType.AllEnemies'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='AncientVerdict'; T='CardType.Attack'; R='CardRarity.Ancient'; G='TargetType.AnyEnemy'},
@{F='Cards\Rare'; N='Cards.Rare'; P='TheArchitect.TheArchitectCode.Cards.Rare'; C='InfiniteBlueprint'; T='CardType.Power'; R='CardRarity.Ancient'; G='TargetType.Self'},
@{F='Cards\Tokens'; N='Cards.Tokens'; P='TheArchitect.TheArchitectCode.Cards.Tokens'; C='Drowsy'; T='CardType.Status'; R='CardRarity.Token'; G='TargetType.None'}
)
$cards | ForEach-Object { Write-CardTest $_.F $_.N $_.C $_.P $_.T $_.R $_.G }

$powers = @(
@{C='SanctumOfVigorPower'; T='PowerType.Buff'; S='PowerStackType.Counter'},
@{C='DivineGracePower'; T='PowerType.Buff'; S='PowerStackType.None'},
@{C='ImmovableAsTheMountainPower'; T='PowerType.Buff'; S='PowerStackType.Counter'},
@{C='ResonancePower'; T='PowerType.Buff'; S='PowerStackType.Counter'},
@{C='TestSubjectPower'; T='PowerType.Buff'; S='PowerStackType.None'},
@{C='RebirthPower'; T='PowerType.Buff'; S='PowerStackType.None'},
@{C='FormOfCreationPower'; T='PowerType.Buff'; S='PowerStackType.Counter'},
@{C='OmnipotencePower'; T='PowerType.Buff'; S='PowerStackType.Counter'},
@{C='SanctuaryPower'; T='PowerType.Buff'; S='PowerStackType.Counter'},
@{C='DestroyerPower'; T='PowerType.Buff'; S='PowerStackType.Counter'},
@{C='RecuperatePower'; T='PowerType.Buff'; S='PowerStackType.Counter'},
@{C='FateVortexPower'; T='PowerType.Buff'; S='PowerStackType.None'},
@{C='DrowsyEnginePower'; T='PowerType.Buff'; S='PowerStackType.None'},
@{C='RequiemPower'; T='PowerType.Buff'; S='PowerStackType.Counter'},
@{C='SharedSanctumPower'; T='PowerType.Buff'; S='PowerStackType.None'},
@{C='TeachAToFishPower'; T='PowerType.Buff'; S='PowerStackType.None'}
)
$powers | ForEach-Object { Write-PowerTest $_.C $_.T $_.S }

$relicDir = Join-Path $root 'Relics'
New-Item -ItemType Directory -Force -Path $relicDir | Out-Null
$relicContent = @"
using MegaCrit.Sts2.Core.Entities.Relics;
using TheArchitect.Tests.Infrastructure;
using TheArchitect.TheArchitectCode.Relics;

namespace TheArchitect.Tests.Relics;

public static class FoundationalCompassTests
{
    [ArchitectTest]
    public static void Metadata()
    {
        ModelTestHelper.AssertRelicMetadata<FoundationalCompass>(RelicRarity.Starter);
    }

    [ArchitectTest]
    public static Task SpecificEffect()
    {
        return BehaviorCatalog.AssertRelicBehavior<FoundationalCompass>();
    }
}
"@
Set-Content -Path (Join-Path $relicDir 'FoundationalCompassTests.cs') -Value $relicContent -Encoding UTF8
