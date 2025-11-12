// Disabled duplicate; use Content.Shared._Mono.ShipGuns.ShipGunTypeComponent instead.
#if false
namespace Content.Shared._Mono.SpaceArtillery;

[RegisterComponent]
public sealed partial class ShipGunTypeComponent : Component
{
    [DataField("shipType")] public ShipWeaponType Type = ShipWeaponType.Ballistic;
}

public enum ShipWeaponType
{
    Ballistic,
    Energy,
    Missile,
}
#endif
