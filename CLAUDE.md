# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**HardLight** is a Space Station 14 fork based on Frontier Station 14, built on the RobustToolbox engine. The project uses C# (.NET 9.0), YAML prototypes, and an Entity Component System (ECS) architecture.

**Key Fork Attribution**: This is a downstream fork that incorporates content from multiple SS14 forks including Frontier Station (_NF), Delta-V (_DV), Einstein Engines (_EE), Goobstation (_Goobstation), and many others. See README.md for full attribution details.

## Build & Development Commands

### Building

**Use the scripts in `Scripts/sh/` (Linux/Mac) or `Scripts/bat/` (Windows):**

```bash
# Build all projects with debug configuration (for development)
Scripts/sh/buildAllDebug.sh

# Build all projects with release configuration (for production/server)
Scripts/sh/buildAllRelease.sh

# Build all projects with tools configuration (for mapping/debugging)
Scripts/sh/buildAllTools.sh
```

**Note**: Debug build includes debugging tools. Release build has optimizations. For mapping, use release or tools build for better performance and stability.

### Running

```bash
# Run server without building
Scripts/sh/runQuickServer.sh

# Run client without building
Scripts/sh/runQuickClient.sh

# Run both client and server without building
Scripts/sh/runQuickAll.sh
```

**Alternative**: The root directory scripts also work:
```bash
./runserver.sh       # Builds and runs server
./runclient.sh       # Builds and runs client
```

### Testing

```bash
# Run unit tests
Scripts/sh/runTests.sh

# Run integration tests
Scripts/sh/runTestsIntegration.sh

# Run YAML linter (finds YAML prototype issues)
Scripts/sh/runTestsYAML.sh
```

**Or use dotnet directly**:
```bash
dotnet test Content.Tests
dotnet test Content.IntegrationTests
```

## Architecture

### Entity Component System (ECS)

The game uses RobustToolbox's ECS architecture:

- **Components**: Data containers (e.g., `TransformComponent`, `IFFComponent`)
- **Systems**: Logic processors that operate on components (e.g., `RoundPersistenceSystem`, `ShipyardSystem`)
- **Entities**: Combinations of components defined in YAML prototypes

### Project Structure

```
Content.Shared/      - Shared code between client and server (components, systems, events)
Content.Server/      - Server-only code (gameplay systems, admin commands, persistence)
Content.Client/      - Client-only code (rendering, UI, input handling)
Content.Tests/       - Unit tests
Content.IntegrationTests/ - Integration tests
Resources/           - Game assets (sprites, sounds, prototypes, localization)
  Prototypes/        - YAML entity and system definitions
  Locale/            - Localization files (.ftl format)
RobustToolbox/       - Engine submodule (do not modify directly)
```

### Custom Content Organization

**Critical Convention**: Custom content is organized into fork-specific subdirectories:

- `_HL/` - HardLight-specific content (this fork)
- `_NF/` - Frontier Station content
- `_DV/` - Delta-V content
- `_EE/` - Einstein Engines content
- `_Goobstation/` - Goobstation content
- Other fork prefixes as listed in README.md

**Always place new HardLight features in `_HL/` subdirectories**:
- Code: `Content.Server/_HL/`, `Content.Shared/_HL/`, `Content.Client/_HL/`
- Prototypes: `Resources/Prototypes/_HL/`
- Localization: `Resources/Locale/en-US/_HL/`
- Assets: `Resources/Textures/_HL/`, `Resources/Audio/_HL/`

### Key HardLight Systems

#### Round Persistence System (`Content.Server/_HL/RoundPersistence/`)

**Purpose**: Maintains critical game state across round restarts when the primary station is deleted and recreated.

**What it preserves**:
- Expedition console mission data and cooldowns
- Shuttle ownership records and purchase history
- IFF system ship tracking
- Player manifest and crew records
- Ship-station associations
- Autopay tracking data

**Components**:
- `RoundPersistenceComponent` - Stores all persistent data
- `RoundPersistenceSystem` - Orchestrates save/restore on round events
- `PlayerPaymentPersistenceSystem` - Tracks player work sessions

**Configuration** (via CVars in `Content.Shared/_HL/CCVar/HLCCVars.cs`):
```toml
hardlight.round_persistence.enabled = true
hardlight.round_persistence.expeditions = true
hardlight.round_persistence.shuttle_records = true
hardlight.round_persistence.station_records = true
hardlight.round_persistence.ship_data = true
hardlight.round_persistence.player_payments = true
hardlight.round_persistence.max_rounds = 10
```

**Admin Commands**:
- `save_persistent_data` - Force save all persistent data
- `persistent_data_status` - Show persistence system status
- `clear_persistent_data` - Clear all persistent data (admin only)

See `Content.Server/_HL/RoundPersistence/README.md` for full documentation.

#### Ship Saving System

Comprehensive ship save/load system with admin moderation tools.

**Admin Commands** (see `ADMIN_SHIP_COMMANDS.md`):
- `shipsave_list [player_name]` - List ship saves with metadata
- `shipsave_inspect <filename>` - Deep inspection of save file
- `shipsave_validate <filename>` - Validate integrity and checksums
- `shipsave_delete <filename>` - Delete save file
- `shipsave_cleanup [--dry-run] [--older-than-days=N]` - Automated cleanup

**Save Location**: `{UserData}/saved_ships/`
**Format**: `{ShipName}_{YYYYMMDD}_{HHMMSS}.yml`

## Code Modification Guidelines

### Modifying Upstream Files

**Critical**: When modifying files from upstream (Space Wizards) or other forks, you **must** add comments:

**C# Changes**:
```csharp
// Frontier: added null check for ship component
if (component != null)
{
    // Frontier code here
}
// End Frontier
```

For value changes:
```csharp
public int FireRate = 4; // Frontier: 3<4
```

**YAML Changes**:
```yaml
- type: Gun
  fireRate: 4 # Frontier: 3<4

# Frontier: added new component
- type: CustomComponent
  value: 100
# End Frontier
```

**Fluent (.ftl) Files**:
```fluent
# Frontier: "Job Whitelists"<"Role Whitelists"
player-panel-job-whitelists = Role Whitelists
```

### Partial Classes

When adding significant code to upstream files, consider using partial classes:

```csharp
// In Content.Server/_HL/Systems/MySystem.Partial.cs
namespace Content.Server.Some.Namespace;

public partial class SomeUpstreamSystem
{
    // HardLight additions here
}
```

### License Headers

All new C# and YAML files created after July 1, 2024 must include:

**C#**:
```csharp
/*
 * New Frontiers - This file is licensed under AGPLv3
 * Copyright (c) 2024 New Frontiers Contributors
 * See AGPLv3.txt for details.
 */
```

**YAML**:
```yaml
# New Frontiers - This file is licensed under AGPLv3
# Copyright (c) 2024 New Frontiers Contributors
# See AGPLv3.txt for details.
```

See `MARKERS.md` for complete details.

## Configuration & CVars

Configuration variables are defined in CVar files:

- `Content.Shared/CCVar/CCVars.cs` - Upstream CVars
- `Content.Shared/_HL/CCVar/HLCCVars.cs` - HardLight CVars
- `Content.Shared/_DV/CCVars/DCCVars.cs` - Delta-V CVars
- `Content.Shared/_EE/CCVar/EECCVars.cs` - Einstein Engines CVars

Configuration files are in TOML format. Example: `server_config.toml`, `persistence_retention_config.toml`

## Prototypes

The game uses a prototype-based data system with ~4000 YAML files in `Resources/Prototypes/`:

**Common Prototype Types**:
- Entities (`- type: entity`)
- Recipes (`- type: construction`, `- type: recipe`)
- Reagents (`- type: reagent`)
- Technologies (`- type: technology`)
- Roles & Jobs (`- type: job`, `- type: startingGear`)
- Access Levels (`- type: accessLevel`)

**Frontier-specific prototypes** are in `Resources/Prototypes/_NF/`
**HardLight prototypes** should go in `Resources/Prototypes/_HL/`

## Localization

Localization uses Fluent (.ftl) format in `Resources/Locale/en-US/`:

**Important**: Fluent files **do not support inline comments**. Place comments on the line above:

```fluent
# This is the correct way to comment
my-locale-key = My Value
```

## Important Notes

- **Never modify RobustToolbox directly** - it's a submodule. Upstream changes are pulled in periodically.
- **Do not use GitHub web editor** - PRs created via web editor may be closed without review.
- **No AI-generated content** - Submitting AI-generated code, sprites, or assets may result in a ban.
- **Check for existing implementations** - With 68+ custom systems in `Content.Server/_NF/` alone, similar features may already exist.
- **Map changes require coordination** - Consult map maintainers before modifying maps to avoid conflicts.

## Debugging & Development

### Admin Commands

Server admin commands are available in-game with admin permissions:
- Ship save management commands (see `ADMIN_SHIP_COMMANDS.md`)
- Round persistence commands
- Standard SS14 admin commands

### Logging

Enable debug logging for specific systems via CVars:
```toml
hardlight.round_persistence.debug_logging = true
```

## Additional Documentation

- `CONTRIBUTING.md` - Contribution guidelines and PR requirements
- `ADMIN_SHIP_COMMANDS.md` - Ship save admin command reference
- `Content.Server/_HL/RoundPersistence/README.md` - Round persistence system
- `SHIPYARD_GRID_SAVE_IMPLEMENTATION.md` - Shipyard save implementation
- `MARKERS.md` - License marking requirements
- `LEGAL.md` - Legal information and attributions

## Common Patterns

### Creating a New System

1. Define component in appropriate namespace (Shared, Server, or Client)
2. Implement system class inheriting from `EntitySystem`
3. Use dependency injection for required managers
4. Subscribe to relevant events in `Initialize()`
5. Add prototypes in `Resources/Prototypes/_HL/`
6. Add localization in `Resources/Locale/en-US/_HL/`

### Adding CVars

1. Add constant to `Content.Shared/_HL/CCVar/HLCCVars.cs`
2. Use `CVarDef` with appropriate type and default value
3. Access in code via `IConfigurationManager`

### Entity Component Best Practices

- Components should be data-only (no logic)
- Systems contain all logic
- Use events for cross-system communication
- Prefer composition over inheritance
- Use `[Dependency]` for system dependencies
