using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared._Mono.ShipShield;

/// <summary>
/// Minimal stub for grid-wide ship shield generator components used by prototypes.
/// Provides fields for prototype configuration but no runtime behavior in this stub.
/// </summary>
[RegisterComponent]
public sealed partial class GridShieldGeneratorComponent : Component
{
    /// <summary>
    /// Prototype ID of the field entity to create when the generator is active.
    /// </summary>
    [DataField("createdField")]
    public string? CreatedField { get; set; }
}
