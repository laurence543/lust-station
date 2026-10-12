using Content.Shared._Lust.Preferences;
using Content.Shared._Sunrise.TTS;
using Content.Shared.Silicons.StationAi;
using Robust.Shared.Containers;

namespace Content.Server._Lust.Preferences;

/// <summary>
/// Applies a saved AI voice after the core adds its temporary voice component.
/// </summary>
public sealed class SiliconVoicePreferenceSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SiliconVoicePreferenceComponent, EntGotInsertedIntoContainerMessage>(OnInserted);
    }

    private void OnInserted(Entity<SiliconVoicePreferenceComponent> ent, ref EntGotInsertedIntoContainerMessage args)
    {
        // Это событие приходит мозгу после добавления компонентов контейнера ядра.
        if (args.Container.ID != StationAiCoreComponent.Container || !HasComp<StationAiCoreComponent>(args.Container.Owner))
            return;

        TryApplyPreference(ent.Owner);
    }

    private bool TryApplyPreference(EntityUid uid)
    {
        if (!TryComp<SiliconVoicePreferenceComponent>(uid, out var preference) ||
            !TryComp<BorgVoiceComponent>(uid, out var voice))
            return false;

        if (voice.VoiceWhitelist is { Count: > 0 } && !voice.VoiceWhitelist.Contains(preference.Voice) ||
            voice.VoiceBlacklist is { } blacklist && blacklist.Contains(preference.Voice))
        {
            RemComp<SiliconVoicePreferenceComponent>(uid);
            return false;
        }

        voice.SelectedVoiceId = preference.Voice;
        Dirty(uid, voice);
        var tts = EnsureComp<TTSComponent>(uid);
        tts.VoicePrototypeId = preference.Voice;
        Dirty(uid, tts);
        RemComp<SiliconVoicePreferenceComponent>(uid);
        return true;
    }
}
