// SPDX-FileCopyrightText: 2025 Ark
// SPDX-FileCopyrightText: 2025 ark1368
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared._Mono.Radar;
using Content.Shared.Shuttles.Components;
using RadarBlipServerComp = Content.Server._NF.Radar.RadarBlipComponent;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.GameObjects;

namespace Content.Server.Mono.Radar;

public sealed partial class RadarBlipSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    private EntityQuery<PhysicsComponent> _physQuery;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<RequestBlipsEvent>(OnBlipsRequested);

        _physQuery = GetEntityQuery<PhysicsComponent>();
    }

    private void OnBlipsRequested(RequestBlipsEvent ev, EntitySessionEventArgs args)
    {
        if (!TryGetEntity(ev.Radar, out var radarUid))
            return;

        if (!TryComp<RadarConsoleComponent>(radarUid, out var radar))
            return;

        var blips = AssembleBlipsReport((EntityUid)radarUid, radar);

        var giveEv = new GiveBlipsEvent(blips);
        RaiseNetworkEvent(giveEv, args.SenderSession);
    }

    private List<(NetCoordinates Position, Vector2 Vel, float Scale, Color Color, RadarBlipShape Shape)> AssembleBlipsReport(EntityUid uid, RadarConsoleComponent? component = null)
    {
        var blips = new List<(NetCoordinates Position, Vector2 Vel, float Scale, Color Color, RadarBlipShape Shape)>();

        if (Resolve(uid, ref component))
        {
            var radarXform = Transform(uid);
            var radarPosition = _xform.GetWorldPosition(uid);
            var radarGrid = _xform.GetGrid(uid);
            var radarMapId = radarXform.MapID;

            var blipQuery = EntityQueryEnumerator<RadarBlipServerComp, TransformComponent>();

            while (blipQuery.MoveNext(out var blipUid, out var blip, out var blipXform))
            {
                if (!blip.Enabled)
                    continue;

                // This prevents blips from showing on radars that are on different maps
                if (blipXform.MapID != radarMapId)
                    continue;

                var blipGrid = _xform.GetGrid(blipUid);

                // if (HasComp<CircularShieldRadarComponent>(blipUid))
                // {
                //     // Skip if in FTL
                //     if (isFtlMap)
                //         continue;
                //
                //     // Skip if no grid
                //     if (blipGrid == null)
                //         continue;
                //
                //     // Ensure the grid is a valid MapGrid
                //     if (!HasComp<MapGridComponent>(blipGrid.Value))
                //         continue;
                //
                //     // Ensure the shield is a direct child of the grid
                //     if (blipXform.ParentUid != blipGrid)
                //         continue;
                // }

                // var blipVelocity = _physics.GetMapLinearVelocity(blipUid);

                var distance = (blipXform.WorldPosition - radarPosition).Length();
                if (distance > component.MaxRange)
                    continue;


                // If a shield indicator is orphaned from its grid, skip it.
                // (Original check referenced a missing CircularShieldRadarComponent; now relying on other flags.)
                if (blipGrid == null && blip.RequireNoGrid == false)
                    continue;

                // Respect grid visibility flags
                if (blip.RequireNoGrid && blipGrid != null)
                    continue;
                if (!blip.VisibleFromOtherGrids && blipGrid != radarGrid)
                    continue;

                // Get velocity for prediction
                var blipVelocity = _physQuery.TryGetComponent(blipUid, out var phys)
                    ? _physics.GetMapLinearVelocity(blipUid, component: phys)
                    : Vector2.Zero;

                // Convert RadarBlipShapeNF to RadarBlipShape (they should be compatible enums)
                var shape = (RadarBlipShape)(int)blip.Shape;

                // Send entity coordinates directly (not mover coordinates)
                var netCoords = GetNetCoordinates(blipXform.Coordinates);
                blips.Add((netCoords, blipVelocity, blip.Scale, blip.RadarColor, shape));
            }
        }

        return blips;
    }

    // Hitscan trajectory reporting is disabled until a proper source component exists server-side.
}
