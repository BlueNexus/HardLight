// SPDX-FileCopyrightText: 2025 Avalon
//
// SPDX-License-Identifier: MPL-2.0

using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.Weapons.Hitscan.Components;

/// <summary>
/// Stub component describing basic hitscan raycast settings.
/// Prototype field: maxDistance (meters/tiles).
/// </summary>
[RegisterComponent]
public sealed partial class HitscanBasicRaycastComponent : Component
{
    [DataField("maxDistance")]
    public float MaxDistance { get; set; } = 0f;
}
