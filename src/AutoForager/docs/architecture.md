# AutoForager Architecutre Documentation

## Overview

AutoForager has been refactored into a more modular, maintainable architecture using the **Handler Pattern** with **Service Layer** separation.

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────┐
│                      ModEntry                           │
│                  (Orchestration Layer)                  │
├─────────────────────────────────────────────────────────┤
│  - Coordinates services and handlers                    │
│  - Manages event subscriptions                          │
│  - Initializes components                               │
└────────────┬────────────────────────────────────────────┘
             │
             ├──────────────┬──────────────┬──────────────┐
             ▼              ▼              ▼              ▼
┌──────────────┐ ┌──────────────┐ ┌──────────────┐ ┌──────────────┐
│   Services   │ │   Handlers   │ │     UI       │ │   Classes    │
└──────────────┘ └──────────────┘ └──────────────┘ └──────────────┘
```

## Project Structure

```
AutoForager/
├── Classes/                      # Core data models
│   ├── CategoryComparer.cs       # Custom category sorting
│   ├── ContentEntry.cs           # Content pack model
│   ├── ForageableItem.cs         # Forageable item model
│   └── ForageableItemTracker.cs  # Singleton tracker
│ ├── Extensions/                 # Extension methods
│   └── ListExtensions.cs         # ForageableItem list helpers
│ ├── Handlers/                   # Foraging handlers
│   ├── BaseForagingHandler.cs    # Abstract base handler
│   ├── WildTreeHandler.cs        # Wild tree foraging
│   ├── FruitTreeHandler.cs       # Fruit tree foraging
│   ├── BushHandler.cs            # Bush foraging
│   ├── TerrainFeatureHandler.cs  # Special terrain features
│   ├── ObjectHandler.cs          # Ground object foraging
│   ├── ArtifactSpotHandler.cs    # Digging spots
│   ├── MachineHandler.cs         # Machine harvesting
│   └── PanningHandler.cs         # Ore panning
│ ├── Services/                   # Business logic services
│   ├── IForagingContext.cs       # Context interface
│   ├── ForagingContext.cs        # Context implementation
│   ├── AssetService.cs           # Asset management
│   ├── ContentPackService.cs     # Content pack loading
│   └── ConfigurationService.cs   # Config management
│ ├── UI/                         # User interface
│   └── ConfigMenuBuilder.cs      # GMCM menu builder
│ ├── Helpers/                    # Constants and utilities
│   └── Constants.cs              # Constant values
│ ├── Integrations/               # External mod integrations
│   ├── BushBloomWrapper.cs       # Bush Bloom Mod API
│   ├── CustomBushWrapper.cs      # Custom Bush API
│   └── FarmTypeManagerWrapper.cs # FTM API
│ ├── ModEntry.cs                 # Main entry point
└── ModConfig.cs                  # Configuration model
```

## Design Patterns

### 1. Handler Pattern

**Purpose:** Separate foraging logic into focused, single-responsibility handlers.

**How it Works:**
```c#
// Each handler implements:
public abstract class BaseForagingHandler
{
    public virtual void Initialize(IForagingContext context);
    public virtual int Priority => 100;
    // Handler-specific methods
}

// ModEntry orchestrates:
foreach (var tile in tilesToCheck)
{
    if (handler.CanHandle(item))
    {
        handler.Handle(item);
    }
}
```


**Benefits:**
- Easy to add new foraging types (just add a new handler)
- Easy to test (mock context, test handler independently)
- Easy to maintain (find code by responsibility)

### 2. Service Layer

**Purpose:** Separate business logic from orchestration.

**Services:**
- **AssetService**: Manages game asset loading and parsing
- **ContentPackService**: Handles content pack integration
- **ConfigurationService**: Manages configuration state

**Benefits:**
- Clear separation of concerns
- Reusable across handlers
- Testable independently

### 3. Context Pattern

**Purpose:** Provide shared state and utilities to handlers.

**Interface:**
```c#
public interface IForagingContext
{
    Farmer Player { get; }
    GameLocation Location { get; }
    ModConfig Config { get; }
    IMonitor Monitor { get; }
    ForageableItemTracker ForageableTracker { get; }

    void TrackForagedItem(string category, string displayName);
    bool PlayerHasTool<T>() where T : Tool;
    void ShowThrottledError(string message);
}
```


**Benefits:**
- Handlers don't need individual dependencies
- Easy to add new shared functionality
- Simplified testing (mock one interface)

## Component Responsibilities

### ModEntry (Orchestration)
**Responsibilities:**
- Initialize services and handlers
- Register event handlers
- Coordinate game loop (OnUpdateTicked)
- Manage mushroom log tracking
- Asset editing (FruitTrees, Objects, WildTrees)

**Does NOT:**
- Implement foraging logic (delegated to handlers)
- Parse assets (delegated to AssetService)
- Build UI (delegated to ConfigMenuBuilder)

### Handlers

#### BaseForagingHandler
- Provides common utilities (logging, tracking, quality calculation)
- Defines handler contract
- Priority system for execution order

#### WildTreeHandler (Priority: 10)
- Shakes trees for seeds
- Harvests moss
- Respects mushroom log exclusion

#### FruitTreeHandler (Priority: 20)
- Shakes fruit trees
- Checks fruit count threshold
- Tracks harvested fruits

#### BushHandler (Priority: 30)
- Handles berry bushes (salmonberry, blackberry)
- Tea bushes
- Walnut bushes
- Custom bushes (integration)

#### TerrainFeatureHandler (Priority: 40)
- Spring onions
- Ginger roots
- Hoe requirement checking

#### ObjectHandler (Priority: 50)
- Ground-spawned forageables
- Quality calculation
- Gatherer profession support

#### ArtifactSpotHandler (Priority: 60)
- Artifact spots
- Seed spots
- Hoe requirement checking

#### MachineHandler (Priority: 70)
- Mushroom boxes
- Mushroom logs
- Tappers (all types)
- Experience granting

#### PanningHandler (Priority: 80)
- Ore panning spots
- Upgraded pan support
- Additional spot spawning

### Services

#### AssetService
**Responsibilities:**
- Load and cache game assets
- Parse assets into ForageableItems
- Handle asset invalidation/reloading
- Update ForageableItemTracker

**Methods:**
- `LoadInitialAssets()`: First-time load
- `ReloadAllAssets()`: Reload after locale change
- `HandleAssetReady(assetName)`: Handle dynamic updates

#### ContentPackService
**Responsibilities:**
- Parse content pack definitions
- Initialize mod integrations (Bush Bloom, Custom Bush, FTM)
- Track custom categories
- Provide integration data to other components

**Methods:**
- `LoadAllContentAsync()`: Load everything
- Integration-specific initialization methods

#### ConfigurationService
**Responsibilities:**
- Load/save configuration
- Register GMCM menu
- Toggle mod on/off
- Update enabled states

**Methods:**
- `LoadConfiguration()`: Load from disk
- `SaveConfiguration()`: Save to disk
- `RegisterConfigMenu()`: Register with GMCM
- `ToggleAutoForagingAsync()`: Toggle mod

### UI Layer

#### ConfigMenuBuilder
**Responsibilities:**
- Build GMCM menu structure
- Create all pages and sections
- Wire up configuration callbacks

**Pages:**
- General settings
- Wild Trees
- Fruit Trees
- Bushes
- Forageables

## Data Flow

### Initialization Flow
```
1.	ModEntry.Entry() ↓
2.	Initialize Services (Config, Asset, ContentPack) ↓
3.	Load Configuration ↓
4.	Register Event Handlers ↓
5.	InitializeMod() (on UpdateTicked) ↓
6.	Load Content Packs & Integrations (async) ↓
7.	Initialize Handlers ↓
8.	Load Assets ↓
9.	Register GMCM Menu ↓
10.	Game Ready
```

### Foraging Flow (OnUpdateTicked)
```
1.	Check if player moved ↓
2.	Create ForagingContext ↓
3.	Get tiles to check (based on radius) ↓
4.	For each tile:
├─> Check terrain features
│   ├─> Try WildTreeHandler
│   ├─> Try FruitTreeHandler
│   ├─> Try BushHandler
│   └─> Try TerrainFeatureHandler
│ └─> Check objects
├─> Try ArtifactSpotHandler
├─> Try MachineHandler
└─> Try ObjectHandler ↓
5.	Check panning (PanningHandler) ↓
6.	Track all foraged items
```

### Asset Update Flow
```
1.	Game invalidates asset ↓
2.	OnAssetReady event fires ↓
3.	AssetService.HandleAssetReady() ↓
4.	Load new asset data ↓
5.	Parse into ForageableItems ↓
6.	Update ForageableItemTracker ↓
7.	Re-register GMCM menu (if needed)
```

## Extension Points

### Adding a New Handler

1. **Create handler class:**
```c#
internal class MyNewHandler : BaseForagingHandler
{
    public override int Priority => 45; // Set priority

    public bool CanHandle(MyFeature feature)
    {
        // Check if this handler should process
        return /* condition */;
    }

    public void Handle(MyFeature feature, Vector2 tile)
    {
        // Implement foraging logic
        // Use Context for shared functionality
        // Call TrackItem() to log statistics
    }
}
```

2. **Initialize in ModEntry:**
```c#
private void InitializeHandlers()
{
    // ... existing handlers ...
    _myNewHandler = new MyNewHandler();
    _myNewHandler.Initialize(context);
}
```

3. **Call in OnUpdateTicked:**
```c#
if (_myNewHandler.CanHandle(feature))
{
    _myNewHandler.Handle(feature, tile);
}
```

### Adding a New Service

1. **Create service class:**
```c#
internal class MyNewService
{
    private readonly IMonitor _monitor;

    public MyNewService(IMonitor monitor)
    {
        _monitor = monitor;
    }

    public void DoSomething()
    {
        // Service logic
    }
}
```

2. **Initialize in ModEntry.Entry():**
```c#
_myNewService = new MyNewService(Monitor);
```

3. **Use in handlers or ModEntry:**
```c#
_myNewService.DoSomething();
```

### Adding Configuration Options

1. **Add property to ModConfig.cs:**
```c#
public bool MyNewOption { get; set; }
```

2. **Add to ConfigMenuBuilder:**
```c#
gmcmApi.AddBoolOption(
    mod: _manifest,
    name: () => "My New Option",
    tooltip: () => "Description",
    getValue: () => _config.MyNewOption,
    setValue: val => _config.MyNewOption = val);
```

3. **Use in handlers:**
```c#
if (Config.MyNewOption)
{
    // Do something
}
```

## Testing Strategy

### Unit Testing (Future)
Each component can be tested independently:

**Handler Testing:**
```c#
[Test]
public void WildTreeHandler_CanHandle_ReturnsTrueForValidTree()
{
    var mockContext = CreateMockContext();
    var handler = new WildTreeHandler(...);
    handler.Initialize(mockContext);
    var tree = CreateTestTree(hasSeed: true);
    Assert.IsTrue(handler.CanHandle(tree, Vector2.Zero));
}
```

**Service Testing:**
```c#
[Test]
public void ConfigurationService_LoadConfiguration_MergesWithDefaults()
{
    var service = new ConfigurationService(...);
    var config = service.LoadConfiguration();
    Assert.IsNotNull(config);
    Assert.AreEqual(true, config.AutoForagingEnabled);
}
```

### Integration Testing
Test component interactions:
- Service → Handler communication
- Asset loading → Tracker updates
- Configuration changes → Handler behavior

### Manual Testing
Use testing documentation:
- `TESTING.md`: Comprehensive test checklist
- `QUICK_TEST.md`: Quick validation script

## Performance Considerations

### Optimization Points

1. **Radius Calculation**
   - Default: 2 tiles (covers 25 tiles)
   - Max recommended: 10 tiles (covers 441 tiles)
   - Use player magnetism for dynamic radius

2. **Handler Priority**
   - Lower priority = checked first
   - Optimize order based on frequency
   - Early exit when handler matches

3. **Caching**
   - ForageableItemTracker caches parsed items
   - Asset caches prevent repeated parsing
   - Context created once per tick

4. **Lazy Evaluation**
   - Handlers only process when CanHandle returns true
   - Assets only parse when invalidated
   - Integrations initialize once

## Troubleshooting

### Common Issues

**Issue: Handler not triggering**
- Check Priority (lower = earlier)
- Verify CanHandle logic
- Check if handler initialized
- Verify context passed correctly

**Issue: Items not tracked**
- Check TrackItem() calls
- Verify category keys match Constants
- Check _trackingCounts dictionary initialized

**Issue: GMCM menu missing items**
- Verify ForageableTracker populated
- Check asset parsing completed
- Ensure handlers initialized before menu registration

**Issue: Performance problems**
- Reduce foraging radius
- Check for infinite loops
- Profile handler execution time
- Verify early exits in CanHandle

## Maintenance Guidelines

### Code Style
- Follow .editorconfig settings (tabs, CRLF)
- Use XML documentation for public methods
- Keep handlers under 300 lines
- Keep services under 400 lines

### Adding Features
1. Determine if it's a new handler or service
2. Follow existing patterns
3. Add tests (when test framework exists)
4. Update documentation
5. Test thoroughly before committing

### Refactoring
- Change one component at a time
- Keep tests passing
- Update documentation
- Communicate breaking changes

## Future Improvements

### Short Term
- Add unit tests
- Performance profiling
- Additional logging options

### Long Term
- Plugin system for custom handlers
- API for other mods to register forageables
- Enhanced statistics tracking
- In-game configuration panel

## Contributing

When contributing to AutoForager:

1. **Understand the architecture** (read this document)
2. **Follow the patterns** (Handler, Service, Context)
3. **Keep single responsibility** (one handler = one concern)
4. **Test thoroughly** (use testing documentation)
5. **Document changes** (update this file if needed)

## Questions?

If you have questions about the architecture:
1. Check this document first
2. Review the code (well-documented)
3. Check TESTING.md for testing procedures
4. Open an issue on GitHub

---

**Last Updated:** 06/09/2026
**Architecture Version:** 2.0
**Author:** Hedgehog Technologies