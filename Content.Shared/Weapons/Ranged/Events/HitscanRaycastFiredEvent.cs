using Robust.Shared;

namespace Content.Shared.Weapons.Ranged.Events;

[ByRefEvent]
public record struct HitscanRaycastFiredEvent(EntityUid? HitEntity, EntityUid? Shooter, EntityUid? Gun)
{
    public bool Canceled;
}
