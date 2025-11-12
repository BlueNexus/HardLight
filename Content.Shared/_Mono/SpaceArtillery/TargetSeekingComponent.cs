#if false
using Robust.Shared.Utility;

namespace Content.Shared._Mono.SpaceArtillery;

/// <summary>
/// Disabled duplicate; canonical TargetSeekingComponent lives in Content.Server.Mono.Projectiles.TargetSeeking.
/// </summary>
[RegisterComponent]
public sealed partial class TargetSeekingComponent : Component
{
    [DataField("acceleration")] public float Acceleration = 0f;
    [DataField("detectionRange")] public float DetectionRange = 0f;
    [DataField("scanArc")] public float ScanArc = 0f;
    [DataField("launchSpeed")] public float LaunchSpeed = 0f;
    [DataField("maxSpeed")] public float MaxSpeed = 0f;
}
#endif
