// SPDX-FileCopyrightText: 2024 Aviu00
// SPDX-FileCopyrightText: 2025 Redrover1760
// SPDX-FileCopyrightText: 2025 starch
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._White.Blocking;

[RegisterComponent, NetworkedComponent]
public sealed partial class RechargeableBlockingComponent : Component
{
    /// <summary>
    /// Whether the battery is currently discharged.
    /// </summary>
    [DataField]
    public bool Discharged;

    /// <summary>
    /// Auto-recharge rate when the battery is discharged.
    /// </summary>
    [DataField]
    public float DischargedRechargeRate = 50f;

    /// <summary>
    /// Auto-recharge rate when the battery is charged.
    /// </summary>
    [DataField]
    public float ChargedRechargeRate = 10f;
}
