using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared._Mono.Shuttle.FTL;

/// <summary>
/// Minimal stub for FTL drive parameters used by shuttle drive prototypes.
/// Exists to satisfy prototype loading; behavior may be implemented server-side later.
/// </summary>
[RegisterComponent]
public sealed partial class FTLDriveComponent : Component
{
    /// <summary>
    /// Effective range in meters (or prototype units).
    /// </summary>
    [DataField("range")]
    public int Range { get; set; } = 0;

    /// <summary>
    /// Thermal signature emitted when operating, used by sensors/radar.
    /// </summary>
    [DataField("thermalSignature")]
    public int ThermalSignature { get; set; } = 0;

    /// <summary>
    /// Cooldown time between FTL uses (seconds).
    /// </summary>
    [DataField("cooldown")]
    public float Cooldown { get; set; } = 0f;

    /// <summary>
    /// Time spent in hyperspace (seconds).
    /// </summary>
    [DataField("hyperSpaceTime")]
    public float HyperSpaceTime { get; set; } = 0f;

    /// <summary>
    /// Startup/spool-up time before jump (seconds).
    /// </summary>
    [DataField("startupTime")]
    public float StartupTime { get; set; } = 0f;
}
