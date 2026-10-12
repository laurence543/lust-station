using Robust.Shared.Prototypes;
using Content.Shared._Sunrise.TTS;

namespace Content.Shared._Lust.Preferences;

/// <summary>
/// Retains a spawn voice until the AI brain receives its voice component from the core.
/// </summary>
[RegisterComponent]
public sealed partial class SiliconVoicePreferenceComponent : Component
{
    /// <summary>
    /// Saved voice to apply when the AI is installed in a core.
    /// </summary>
    public ProtoId<TTSVoicePrototype> Voice;
}
