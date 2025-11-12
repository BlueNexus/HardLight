// SPDX-FileCopyrightText: 2025 Ark
// SPDX-FileCopyrightText: 2025 Ilya246
// SPDX-FileCopyrightText: 2025 ark1368
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// Disabled duplicate; use Content.Server._NF.Radar.RadarBlipComponent instead.
#if false
namespace Content.Server.Mono.Radar;

using Content.Shared._Mono.Radar;

[RegisterComponent]
public sealed partial class RadarBlipComponent : Component
{
    [DataField] public Color RadarColor = Color.Red;
    [DataField] public Color HighlightedRadarColor = Color.OrangeRed;
    [DataField] public float Scale = 1;
    [DataField] public RadarBlipShape Shape = RadarBlipShape.Circle;
    [DataField] public bool RequireNoGrid = false;
    [DataField] public bool VisibleFromOtherGrids = true;
    [DataField] public bool Enabled = true;
}
#endif

