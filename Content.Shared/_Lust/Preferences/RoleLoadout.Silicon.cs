using System.Linq;
using Content.Shared._Sunrise.TTS;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;

namespace Content.Shared.Preferences.Loadouts;

public sealed partial class RoleLoadout
{
    private void ValidateSiliconPreferences(RoleLoadoutPrototype role, IPrototypeManager prototype, string[] sponsorPrototypes)
    {
        if (!role.CanCustomizeSiliconVoice || SiliconVoice is { } voiceId &&
            (!prototype.TryIndex<TTSVoicePrototype>(voiceId, out var voice) || !voice.RoundStart ||
             voice.SponsorOnly && !sponsorPrototypes.Contains(voiceId) ||
             !CanUseSiliconVoice(role, prototype, voiceId)))
        {
            SiliconVoice = null;
        }

        if (!role.CanCustomizeStationAi || !IsStationAiCustomizationValid(prototype, "StationAiCoreIconography", StationAiScreen))
            StationAiScreen = null;

        if (!role.CanCustomizeStationAi || !IsStationAiCustomizationValid(prototype, "StationAiHolograms", StationAiHologram))
            StationAiHologram = null;
    }

    /// <summary>
    /// Applies the voice restrictions of the silicon entity used by this role.
    /// </summary>
    public static bool CanUseSiliconVoice(RoleLoadoutPrototype role, IPrototypeManager prototype, string voiceId)
    {
        BorgVoiceComponent? component = null;
        if (role.CanCustomizeStationAi)
        {
            if (prototype.TryIndex<EntityPrototype>("AiHeld", out var held))
                held.TryGetComponent(out component);
        }
        else
        {
            var jobId = role.ID.StartsWith("Job", StringComparison.Ordinal) ? role.ID[3..] : role.ID;
            if (prototype.TryIndex<JobPrototype>(jobId, out var job) &&
                job.JobEntity is { } entityId && prototype.TryIndex(entityId, out var entity))
                entity.TryGetComponent(out component);
        }

        return component == null ||
               (component.VoiceWhitelist == null || component.VoiceWhitelist.Count == 0 || component.VoiceWhitelist.Contains(voiceId)) &&
               (component.VoiceBlacklist == null || !component.VoiceBlacklist.Contains(voiceId));
    }

    /// <summary>
    /// Checks that a saved appearance belongs to the requested customization group.
    /// </summary>
    public static bool IsStationAiCustomizationValid(IPrototypeManager prototype, string groupId, string? customizationId)
    {
        return customizationId != null &&
               prototype.TryIndex<StationAiCustomizationGroupPrototype>(groupId, out var group) &&
               group.ProtoIds.Contains(new ProtoId<StationAiCustomizationPrototype>(customizationId)) &&
               prototype.HasIndex<StationAiCustomizationPrototype>(customizationId);
    }
}
