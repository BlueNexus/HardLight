namespace Content.Shared._Mono.Weapons;

/// <summary>
/// No-op component to satisfy prototypes that reference WieldedCrosshair.
/// If client-side behavior is desired later, implement it in a client system.
/// </summary>
[RegisterComponent]
public sealed partial class WieldedCrosshairComponent : Component
{
}
