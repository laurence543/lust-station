using System.Linq;
using Content.IntegrationTests.Fixtures;
using Content.Server.Station.Systems;
using Content.Shared._Sunrise.TTS;
using Content.Shared._Lust.Preferences;
using Content.Shared.Holopad;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.IoC;
using Robust.Shared.Containers;

namespace Content.IntegrationTests.Tests._Lust.Preferences;

[TestFixture]
public sealed class SiliconPreferencesTest : GameTest
{
    public override PoolSettings PoolSettings => new() { Dirty = true, Connected = true };

    [TestCase("Borg")]
    [TestCase("StationAi")]
    [TestCase("MedicalBorg")]
    public async Task SpawnAppliesSavedSiliconPreferences(string job)
    {
        var server = Pair.Server;
        var map = await Pair.CreateTestMap();
        EntityUid silicon = default;
        string voiceId = null!;
        await server.WaitPost(() =>
        {
            var prototype = server.ResolveDependency<IPrototypeManager>();
            voiceId = prototype.EnumeratePrototypes<TTSVoicePrototype>().First(voice => voice.RoundStart && !voice.SponsorOnly).ID;
            var profile = new HumanoidCharacterProfile().WithLoadout(new RoleLoadout($"Job{job}")
            {
                SiliconVoice = voiceId,
                StationAiScreen = "StationAiIconClown",
                StationAiHologram = "StationAiHologramCat",
            });
            silicon = server.EntMan.System<StationSpawningSystem>().SpawnPlayerMob(map.GridCoords, job, profile, null);
            if (job == "StationAi")
            {
                var core = server.EntMan.SpawnEntity("PlayerStationAiEmpty", map.GridCoords);
                var containers = server.EntMan.System<SharedContainerSystem>();
                containers.TryGetContainer(core, StationAiCoreComponent.Container, out var container);
                containers.Insert(silicon, container!);
            }
        });
        await server.WaitAssertion(() =>
        {
            Assert.That(server.EntMan.GetComponent<BorgVoiceComponent>(silicon).SelectedVoiceId?.Id, Is.EqualTo(voiceId));
            if (job != "StationAi")
                return;
            Assert.That(server.EntMan.HasComponent<SiliconVoicePreferenceComponent>(silicon), Is.False,
                "The saved voice must be consumed once so later in-game choices are preserved.");
            var customization = server.EntMan.GetComponent<StationAiCustomizationComponent>(silicon);
            Assert.That(customization.ProtoIds["StationAiCoreIconography"].Id, Is.EqualTo("StationAiIconClown"));
            Assert.That(customization.ProtoIds["StationAiHolograms"].Id, Is.EqualTo("StationAiHologramCat"));
            var avatar = server.EntMan.GetComponent<HolographicAvatarComponent>(silicon);
            var expected = server.ResolveDependency<IPrototypeManager>().Index<StationAiCustomizationPrototype>("StationAiHologramCat");
            Assert.That(avatar.LayerData[0].State, Is.EqualTo(expected.LayerData[StationAiState.Hologram.ToString()].State));
        });
    }

    [Test]
    public async Task UnconfiguredStationAiKeepsDefaultAppearance()
    {
        var server = Pair.Server;
        var map = await Pair.CreateTestMap();
        EntityUid silicon = default;
        await server.WaitPost(() => silicon = server.EntMan.System<StationSpawningSystem>()
            .SpawnPlayerMob(map.GridCoords, "StationAi", new HumanoidCharacterProfile(), null));
        await server.WaitAssertion(() =>
        {
            var customization = server.EntMan.GetComponent<StationAiCustomizationComponent>(silicon);
            Assert.That(customization.ProtoIds["StationAiCoreIconography"].Id, Is.EqualTo("StationAiIconAi"));
            Assert.That(customization.ProtoIds["StationAiHolograms"].Id, Is.EqualTo("StationAiHologramFemale"));
            Assert.That(server.EntMan.HasComponent<SiliconVoicePreferenceComponent>(silicon), Is.False);
        });
    }

    [Test]
    public async Task HumanoidRoleDoesNotAcceptSiliconPreferences()
    {
        var server = Pair.Server;
        var loadout = new RoleLoadout("JobCaptain")
        {
            SiliconVoice = "MissingVoice",
            StationAiScreen = "StationAiIconClown",
            StationAiHologram = "StationAiHologramCat",
        };
        await server.WaitPost(() => loadout.EnsureValid(new HumanoidCharacterProfile(), ServerSession!,
            server.InstanceDependencyCollection, []));
        await server.WaitAssertion(() =>
        {
            Assert.That(loadout.SiliconVoice, Is.Null);
            Assert.That(loadout.StationAiScreen, Is.Null);
            Assert.That(loadout.StationAiHologram, Is.Null);
        });
    }

    [Test]
    public async Task ValidationRejectsUnavailableAndWrongGroupPreferences()
    {
        var server = Pair.Server;
        var session = ServerSession!;
        var loadout = new RoleLoadout("JobStationAi")
        {
            SiliconVoice = "MissingVoice",
            StationAiScreen = "StationAiHologramCat",
            StationAiHologram = "StationAiIconClown",
        };
        await server.WaitPost(() => loadout.EnsureValid(new HumanoidCharacterProfile(), session, server.InstanceDependencyCollection, []));
        await server.WaitAssertion(() =>
        {
            Assert.That(loadout.SiliconVoice, Is.Null);
            Assert.That(loadout.StationAiScreen, Is.Null);
            Assert.That(loadout.StationAiHologram, Is.Null);
        });
    }
}
