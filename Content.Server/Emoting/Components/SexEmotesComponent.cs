using Content.Server.Emoting.Systems;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Server.Emoting.Components;

/// <summary>
///     Component required for entities to be able to do  sexy emotes
/// </summary>
[RegisterComponent]
[Access(typeof(SexEmotesSystem))]
public sealed partial class SexEmotesComponent : Component
{
    /// <summary>
    ///     Emote sounds prototype id for sex emotes.
    /// </summary>
    [DataField("soundsId", customTypeSerializer: typeof(PrototypeIdSerializer<EmoteSoundsPrototype>))]
    public string? SoundsId;

    /// <summary>
    ///     Loaded emote sounds prototype used for sex emotes.
    /// </summary>
    public EmoteSoundsPrototype? Sounds;
}
