// SPDX-FileCopyrightText: 2025 Avalon
//
// SPDX-License-Identifier: MPL-2.0

using Content.Shared.Damage;
using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.Weapons.Hitscan.Components;

/// <summary>
/// Stub component describing damage dealt by a hitscan impact.
/// Prototype field: damage (DamageSpecifier with type map).
/// </summary>
[RegisterComponent]
public sealed partial class HitscanBasicDamageComponent : Component
{
    [DataField("damage")]
    public DamageSpecifier Damage = new();
}
