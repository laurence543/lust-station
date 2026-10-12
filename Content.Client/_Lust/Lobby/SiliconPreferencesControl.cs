using System.Linq;
using Content.Shared._Sunrise.TTS;
using Content.Shared.Preferences.Loadouts;
using Robust.Client.UserInterface;
using Content.Shared.Silicons.StationAi;
using Content.Sunrise.Interfaces.Shared;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Client._Lust.Lobby;

/// <summary>
/// Edits the silicon preferences stored alongside a role's custom name.
/// </summary>
public sealed class SiliconPreferencesControl : BoxContainer
{
    public event Action? OnPreferencesChanged;

    public SiliconPreferencesControl(RoleLoadout loadout, RoleLoadoutPrototype role, ICommonSession session,
        IDependencyCollection collection, ISharedSponsorsManager? sponsors)
    {
        Orientation = LayoutOrientation.Vertical;
        Visible = role.CanCustomizeSiliconVoice || role.CanCustomizeStationAi;
        var prototype = collection.Resolve<IPrototypeManager>();
        if (role.CanCustomizeSiliconVoice)
        {
            string[] allowed = [];
            if (sponsors != null && sponsors.TryGetPrototypes(session.UserId, out var sponsorPrototypes))
                allowed = sponsorPrototypes.ToArray();

            var voices = prototype.EnumeratePrototypes<TTSVoicePrototype>()
                .Where(voice => voice.RoundStart && (!voice.SponsorOnly || allowed.Contains(voice.ID)) &&
                                RoleLoadout.CanUseSiliconVoice(role, prototype, voice.ID))
                .OrderBy(voice => Loc.GetString(voice.Name))
                .Select(voice => (voice.ID, Loc.GetString(voice.Name))).ToArray();
            AddSiliconOption("silicon-preferences-voice", loadout.SiliconVoice, voices, selection => loadout.SiliconVoice = selection);
        }

        if (!role.CanCustomizeStationAi)
            return;

        AddAppearanceOption("StationAiCoreIconography", "station-ai-customization-core", loadout.StationAiScreen,
            selection => loadout.StationAiScreen = selection);
        AddAppearanceOption("StationAiHolograms", "station-ai-customization-hologram", loadout.StationAiHologram,
            selection => loadout.StationAiHologram = selection);

        void AddAppearanceOption(string groupId, string label, string? selected, Action<string?> changed)
        {
            if (!prototype.TryIndex<StationAiCustomizationGroupPrototype>(groupId, out var group))
                return;
            var appearances = group.ProtoIds.Where(id => prototype.HasIndex(id))
                .Select(id => (id.Id, Loc.GetString(prototype.Index(id).Name))).ToArray();
            AddSiliconOption(label, selected, appearances, changed);
        }
    }

    private void AddSiliconOption(string label, string? selected, (string Id, string Name)[] options, Action<string?> changed)
    {
        AddChild(new Label { Text = Loc.GetString(label) });
        var button = new OptionButton { HorizontalExpand = true };
        button.AddItem(Loc.GetString("silicon-preferences-default"), 0);
        var selectedIndex = 0;
        for (var i = 0; i < options.Length; i++)
        {
            button.AddItem(options[i].Name, i + 1);
            if (options[i].Id == selected)
                selectedIndex = i + 1;
        }
        button.SelectId(selectedIndex);
        button.OnItemSelected += args =>
        {
            button.SelectId(args.Id);
            changed(args.Id == 0 ? null : options[args.Id - 1].Id);
            OnPreferencesChanged?.Invoke();
        };
        AddChild(button);
    }
}
