using Content.Shared._Sunrise.TTS;
using Content.Shared._Lust.Preferences;
using Content.Shared.Holopad;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Silicons.StationAi;

namespace Content.Server.Station.Systems;

public sealed partial class StationSpawningSystem
{
    private void ApplySiliconPreferences(EntityUid uid, RoleLoadout loadout, RoleLoadoutPrototype role)
    {
        // У сИИ компонент выбора голоса появится только после установки мозга в ядро.
        if (role.CanCustomizeStationAi && loadout.SiliconVoice is { } savedVoice)
        {
            var preference = EnsureComp<SiliconVoicePreferenceComponent>(uid);
            preference.Voice = savedVoice;
        }

        if (role.CanCustomizeSiliconVoice && loadout.SiliconVoice is { } voiceId &&
            TryComp<BorgVoiceComponent>(uid, out var siliconVoice) &&
            _prototypeManager.TryIndex<TTSVoicePrototype>(voiceId, out var voice) && voice.RoundStart &&
            (siliconVoice.VoiceWhitelist == null || siliconVoice.VoiceWhitelist.Count == 0 || siliconVoice.VoiceWhitelist.Contains(voiceId)) &&
            (siliconVoice.VoiceBlacklist == null || !siliconVoice.VoiceBlacklist.Contains(voiceId)))
        {
            // Права на спонсорские голоса проверяются при валидации профиля перед спавном.
            siliconVoice.SelectedVoiceId = voiceId;
            Dirty(uid, siliconVoice);
        }

        if (!role.CanCustomizeStationAi || !TryComp<StationAiCustomizationComponent>(uid, out var customization))
            return;

        if (RoleLoadout.IsStationAiCustomizationValid(_prototypeManager, "StationAiCoreIconography", loadout.StationAiScreen))
            customization.ProtoIds["StationAiCoreIconography"] = loadout.StationAiScreen!;

        if (RoleLoadout.IsStationAiCustomizationValid(_prototypeManager, "StationAiHolograms", loadout.StationAiHologram))
        {
            customization.ProtoIds["StationAiHolograms"] = loadout.StationAiHologram!;
            var hologram = _prototypeManager.Index<StationAiCustomizationPrototype>(loadout.StationAiHologram!);
            if (TryComp<HolographicAvatarComponent>(uid, out var avatar) &&
                hologram.LayerData.TryGetValue(StationAiState.Hologram.ToString(), out var layer))
            {
                avatar.LayerData = [layer];
                Dirty(uid, avatar);
            }
        }

        Dirty(uid, customization);
    }
}
