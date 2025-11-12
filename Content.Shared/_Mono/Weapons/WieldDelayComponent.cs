using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared._Mono.Weapons;

/// <summary>
/// Minimal stub for the WieldDelay component referenced by certain weapon prototypes.
/// This exists to satisfy prototype composition on both client and server. It currently
/// does not implement any runtime behavior.
/// </summary>
[RegisterComponent]
public sealed partial class WieldDelayComponent : Component
{
    /// <summary>
    /// Base delay in seconds applied on wield/unwield (from prototype). Informational only in this stub.
    /// </summary>
    [DataField("baseDelay")]
    public float BaseDelay { get; set; } = 0f;

    /// <summary>
    /// Modified delay in seconds (e.g., after traits/modifiers). Informational only in this stub.
    /// </summary>
    [DataField("modifiedDelay")]
    public float ModifiedDelay { get; set; } = 0f;

    /// <summary>
    /// If true, prevents firing during the delay window. Informational only in this stub.
    /// </summary>
    [DataField("preventFiring")]
    public bool PreventFiring { get; set; } = false;
}
