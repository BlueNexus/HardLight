// SPDX-FileCopyrightText: 2025 Avalon
//
// SPDX-License-Identifier: MPL-2.0

using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;
using Robust.Shared.Utility;

namespace Content.Shared.Weapons.Hitscan.Components;

/// <summary>
/// Stub component for configuring simple visual effects for hitscans.
/// Prototype fields accept SpriteSpecifier for muzzle/travel/impact flashes.
/// </summary>
[RegisterComponent]
public sealed partial class HitscanBasicVisualsComponent : Component
{
    [DataField("muzzleFlash")]
    public SpriteSpecifier? MuzzleFlash { get; set; }

    [DataField("travelFlash")]
    public SpriteSpecifier? TravelFlash { get; set; }

    [DataField("impactFlash")]
    public SpriteSpecifier? ImpactFlash { get; set; }
}
