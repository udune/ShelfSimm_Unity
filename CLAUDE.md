# CLAUDE.md - AI Assistant Guide for ShelfSim Unity Project

> **Last Updated**: 2025-11-18
> **Unity Version**: 6000.2.6f2
> **Project**: ShelfSim - 서가 시뮬레이션 (Shelf Robot Simulation System)

## Table of Contents

1. [Project Overview](#project-overview)
2. [Codebase Structure](#codebase-structure)
3. [Architecture & Patterns](#architecture--patterns)
4. [Namespace & Assembly Definitions](#namespace--assembly-definitions)
5. [Key Conventions](#key-conventions)
6. [Development Workflow](#development-workflow)
7. [Testing Guidelines](#testing-guidelines)
8. [Common Tasks](#common-tasks)
9. [Important Files Reference](#important-files-reference)
10. [Troubleshooting](#troubleshooting)

---

## Project Overview

### Purpose
ShelfSim is a Unity-based warehouse/library robot simulation system that optimizes book retrieval operations using pathfinding algorithms and automated task scheduling.

### Core Features
- **Automated Task Ordering**: Robots automatically work on nearest tasks using the Nearest-Next algorithm
- **Path Optimization**: A* pathfinding algorithm with Manhattan heuristic
- **Timeout Management**: Detection and handling of movement timeout scenarios
- **Error Handling**: Comprehensive error code system with 14 different error types
- **Results Analysis**: CSV export and performance metrics
- **WebGL Deployment**: Browser-based visualization support
- **API Integration**: Backend connectivity for distributed simulation

### Technology Stack
- **Unity**: 6000.2.6f2 (upgraded from 2022.3.12f1)
- **C#**: 10.0 with .NET Framework 4.x
- **Testing**: NUnit (Unity Test Framework)
- **Platforms**: Windows, macOS, Linux, WebGL
- **Target**: Phase 1 (Core Features) currently in progress

### Project Language
- **Primary Documentation**: Korean (한국어)
- **Code Comments**: Minimal (prefer self-documenting code)
- **Variable/Method Names**: English
- **README.md**: Korean

---

## Codebase Structure

### Directory Layout

```
ShelfSimm_Unity/
├── Assets/
│   ├── Scripts/                          # Main source code (48 C# files)
│   │   ├── Data/                         # Data models (11 files)
│   │   │   ├── RobotData.cs             # Robot state and properties
│   │   │   ├── RobotState.cs            # FSM states enum
│   │   │   ├── ErrorCode.cs             # 14 error codes + extensions
│   │   │   ├── Cell.cs                  # Cell/shelf representation
│   │   │   ├── CellDef.cs               # Cell definitions
│   │   │   ├── CellsLayoutSO.cs         # ScriptableObject for layouts
│   │   │   ├── Book.cs, BookData.cs     # Book inventory
│   │   │   ├── Job.cs                   # Job/task data
│   │   │   └── Summary.cs               # Results aggregation
│   │   │
│   │   ├── Core/                         # Business logic (19 files)
│   │   │   ├── PathFinder.cs            # A* pathfinding
│   │   │   ├── SimpleAStarPathFinder.cs # Alternative pathfinding
│   │   │   ├── PathCache.cs             # Path caching
│   │   │   ├── CodeNormalizer.cs        # Input normalization
│   │   │   ├── CodeValidator.cs         # Validation logic
│   │   │   ├── InputValidator.cs        # Input validation
│   │   │   ├── NearestCellSelector.cs   # Nearest-Next algorithm
│   │   │   ├── BookRegistry.cs          # Book catalog management
│   │   │   ├── CodeRegistry.cs          # Code tracking
│   │   │   ├── RobotController.cs       # Robot control logic
│   │   │   ├── TiebreakerService.cs     # Deterministic tiebreaking
│   │   │   ├── TiebreakerConfig.cs      # Tiebreaker configuration
│   │   │   ├── SimulationConfig.cs      # Simulation parameters
│   │   │   └── [utilities...]           # TokenParser, DeterminismLogger, etc.
│   │   │
│   │   ├── API/                          # External integration (1 file)
│   │   │   └── ApiClient.cs             # REST API client + DTOs
│   │   │
│   │   ├── Managers/                     # System managers (4 files)
│   │   │   ├── SimulationManager.cs     # Main orchestrator (Singleton)
│   │   │   ├── UIManager.cs             # UI coordination
│   │   │   ├── CellHighlightManager.cs  # Cell visualization
│   │   │   └── LayoutHashManager.cs     # Layout hashing
│   │   │
│   │   ├── UI/                           # User interface (11 files)
│   │   │   ├── GridRenderer.cs          # Grid visualization
│   │   │   ├── GridClickHandler.cs      # Grid interaction
│   │   │   ├── CellView.cs              # Cell UI component
│   │   │   ├── CellRegistry.cs          # Cell registry
│   │   │   ├── CellInfoPanel.cs         # Cell info display
│   │   │   ├── JobInputController.cs    # Job input interface
│   │   │   ├── BookDropdownController.cs # Book selection
│   │   │   ├── CodeInputHighlighter.cs  # Real-time highlighting
│   │   │   ├── DashboardUI.cs           # Dashboard display
│   │   │   └── SettingsUI.cs            # Settings interface
│   │   │
│   │   ├── Example/                      # Example implementations
│   │   │   ├── RobotSimulatorExample.cs
│   │   │   └── GridRendererExample.cs
│   │   │
│   │   ├── Editor/                       # Editor tools
│   │   │   └── CellsLayoutSOEditor.cs   # Custom inspector
│   │   │
│   │   └── Tests/                        # Test utilities
│   │       └── TestNearestSelector.cs
│   │
│   ├── Tests_EditMode/                   # Edit-mode unit tests (9 files)
│   │   ├── CodeNormalizerTests.cs
│   │   ├── InputValidatorTests.cs
│   │   ├── PathFindingAndTimeoutTests.cs
│   │   ├── CodeRegistryTests.cs
│   │   ├── BookRegistryTests.cs
│   │   ├── TokenParserTests.cs
│   │   ├── SummaryAggregationTests.cs
│   │   ├── LayoutHashManagerTests.cs
│   │   └── CacheKeyHashTests.cs
│   │
│   ├── Tests_PlayMode/                   # Play-mode integration tests
│   │   ├── PauseResumeTimerIntegrationTest.cs
│   │   └── SummaryIntegrationTest.cs
│   │
│   ├── Config/                           # ScriptableObject configs
│   │   ├── SimulationConfig.asset
│   │   ├── TiebreakerConfig.asset
│   │   └── CellsLayoutSO.asset
│   │
│   ├── WebGLTemplates/                   # WebGL deployment
│   │   └── CustomDefault/
│   │       ├── index.html
│   │       └── TemplateData/style.css
│   │
│   ├── Scenes/                           # Unity scenes
│   │   └── MainScene.unity
│   │
│   ├── Prefabs/                          # Reusable GameObjects
│   ├── Sprites/                          # 2D graphics
│   ├── Resources/                        # Runtime loadable assets
│   └── Settings/                         # Renderer settings
│
├── ProjectSettings/                      # Unity engine config
├── Packages/                             # Unity package dependencies
│   ├── manifest.json
│   └── packages-lock.json
│
├── README.md                             # Korean documentation
├── LICENSE                               # MIT License
└── .gitignore                           # Git ignore rules
```

### Code Metrics
- **Total C# Scripts**: 48 files
- **Lines of Code**: ~5,583
- **Test Files**: 11 (9 edit-mode, 2 play-mode)
- **Test Coverage**: 100% for core logic
- **Assembly Definitions**: 6 modules

---

## Architecture & Patterns

### Design Patterns Used

1. **Singleton Pattern**
   - `SimulationManager.cs`: Main simulation orchestrator
   - Access via `SimulationManager.Instance`

2. **Finite State Machine (FSM)**
   - Robot states: IDLE → MOVING → HANDLING → RETURNING → IDLE
   - Implemented in `RobotData.cs` and `RobotController.cs`

3. **Observer Pattern**
   - Configuration changes trigger events
   - Example: `config.OnHandleTimeChanged += HandleTimeChanged`

4. **Repository Pattern**
   - `BookRegistry.cs`: Book catalog management
   - `CodeRegistry.cs`: Code tracking
   - `CellRegistry.cs`: Cell management

5. **Service Pattern**
   - `TiebreakerService.cs`: Deterministic tiebreaking logic
   - `ApiClient.cs`: HTTP communication service

6. **ScriptableObject Pattern**
   - `SimulationConfig`: Configuration data
   - `CellsLayoutSO`: Layout definitions
   - `TiebreakerConfig`: Tiebreaker settings

7. **Strategy Pattern**
   - `PathFinder.cs` vs `SimpleAStarPathFinder.cs`: Interchangeable pathfinding algorithms
   - `NearestCellSelector.cs`: Cell selection strategy

### Key Algorithms

#### 1. A* Pathfinding (`PathFinder.cs`)
- **Heuristic**: Manhattan distance
- **Input**: start, goal, obstacles, grid dimensions
- **Output**: `List<Vector2Int>` path or null if no path
- **Performance**: < 1ms for 50x50 grids
- **Usage**: Robot navigation from warehouse to target cells

#### 2. Nearest-Next Selection (`NearestCellSelector.cs`)
- **Step 1**: Manhattan distance filtering (1st pass)
- **Step 2**: A* cost re-evaluation (TopN=3)
- **Step 3**: Deterministic tiebreaking (seed-based)
- **Purpose**: Optimal task ordering for robot efficiency

#### 3. Timeout Detection
- **Location**: `RobotController.cs`
- **Default**: 30 seconds per movement
- **Check**: `elapsed = currentTime - moveStartTime`
- **Action**: Return to warehouse on timeout

### Component Dependencies

```
┌─────────────────────────────────────────────┐
│           SimulationManager                 │
│              (Singleton)                    │
└──────────┬──────────────────────────────────┘
           │
           ├─► RobotController
           │   └─► PathFinder (A*)
           │   └─► NearestCellSelector
           │   └─► TiebreakerService
           │
           ├─► ApiClient (REST)
           │   └─► DTOs (CreateRunRequest, etc.)
           │
           ├─► UIManager
           │   └─► DashboardUI
           │   └─► SettingsUI
           │
           └─► Data Models
               ├─► RobotData
               ├─► Job
               ├─► Cell
               ├─► Book
               └─► Summary
```

---

## Namespace & Assembly Definitions

### Namespace Convention

**IMPORTANT**: The project uses **double namespacing** in several modules. This is an existing pattern that should be maintained for consistency.

| Namespace | Assembly | Purpose |
|-----------|----------|---------|
| `Data.Data` | Data.asmdef | Data models and structures |
| `Core.Core` | Core.asmdef | Core business logic |
| `API.API` | API.asmdef | API integration and DTOs |
| `Managers.Managers` | Managers.asmdef | System manager classes |
| `Managers` (partial) | Managers.asmdef | Some manager classes (mixed) |
| `UI` | (no asmdef) | UI components |
| `Example` | (no asmdef) | Example implementations |
| `Editor` | (no asmdef) | Editor tools |
| `Tests` | (no asmdef) | Test utilities |

### Assembly Definition Dependencies

```
Data.asmdef (no dependencies)
    ↑
API.asmdef (no dependencies)
    ↑
Core.asmdef
    │ depends on: Data, API
    ↑
Managers.asmdef
    │ depends on: Core
    ↑
Tests_EditMode.asmdef
    │ depends on: Core, Data
    ↑
Tests_PlayMode.asmdef
    │ depends on: Core
```

### Using Statements Pattern

**Standard imports for most scripts:**
```csharp
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Data.Data;
using Core.Core;
using API.API;
using Managers.Managers;
```

**For Unity-specific Random:**
```csharp
using Random = UnityEngine.Random;  // Prefer Unity's Random over System.Random
```

---

## Key Conventions

### Code Style Guidelines

#### 1. **Simplicity First**
- Prefer 1 line over 10 lines
- Avoid unnecessary abstractions
- Implement only core features
- Keep methods focused on single responsibility

**Good Example:**
```csharp
public static bool CheckTimeout(RobotData robot, float currentTime)
{
    if (robot.state != RobotState.MOVING)
        return false;

    float elapsed = currentTime - robot.moveStartTime;
    return elapsed >= robot.moveTimeoutSec;
}
```

**Bad Example (over-engineered):**
```csharp
public static bool CheckTimeout(RobotData robot, float currentTime)
{
    // Timeout check logic
    var state = robot.state;
    var isMoving = state == RobotState.MOVING || state == RobotState.RETURNING;
    if (!isMoving) return false;

    var config = new TimeoutConfig { /* ... */ };
    var checker = new TimeoutChecker(config);
    return checker.Check(robot, currentTime);
}
```

#### 2. **Self-Documenting Code**
- Use clear, descriptive variable names
- Minimize comments (code should be self-evident)
- Comments should explain "why", not "what"
- Korean comments are acceptable for complex business logic

#### 3. **Region Organization** (for MonoBehaviour scripts)
```csharp
public class SimulationManager : MonoBehaviour
{
    #region Singleton
    public static SimulationManager Instance { get; private set; }
    #endregion

    #region Serialized Fields
    [SerializeField] private SimulationConfig config;
    #endregion

    #region Public Properties
    public float ElapsedTime { get; private set; }
    #endregion

    #region Private Fields
    private Queue<Job> _jobQueue;
    #endregion

    #region Unity Lifecycle Methods
    private void Awake() { }
    private void Start() { }
    private void Update() { }
    #endregion

    #region Public Methods
    #endregion

    #region Private Methods
    #endregion
}
```

#### 4. **Naming Conventions**
- **Classes**: PascalCase (e.g., `SimulationManager`)
- **Methods**: PascalCase (e.g., `FindPath()`)
- **Public fields/properties**: PascalCase (e.g., `ElapsedTime`)
- **Private fields**: _camelCase with underscore (e.g., `_jobQueue`)
- **Serialized fields**: camelCase (e.g., `config`, `useApiMode`)
- **Parameters**: camelCase (e.g., `currentTime`, `robot`)
- **Constants**: UPPER_SNAKE_CASE or PascalCase
- **Enums**: PascalCase for type, UPPER_CASE for values

**Example:**
```csharp
public enum RobotState
{
    IDLE,
    MOVING,
    HANDLING,
    RETURNING
}
```

#### 5. **Error Handling**
- Use the `ErrorCode` enum (14 predefined codes)
- Extension methods for user-friendly messages
- Always log failures with context
```csharp
robot.lastError = ErrorCode.ROUTE_BLOCKED;
robot.lastErrorMessage = ErrorCode.ROUTE_BLOCKED.ToMessage();
Debug.LogError($"Robot {robot.id}: {robot.lastErrorMessage}");
```

#### 6. **SerializeField Usage**
- Use `[SerializeField]` for private fields that need Inspector visibility
- Group with `[Header("Section Name")]` attributes
- Order: Core settings → API settings → Component references → Temporary data

### Testing Conventions

#### Test File Organization
- **Edit-mode tests**: `/Assets/Tests_EditMode/`
  - Pure logic tests (no Unity lifecycle)
  - Fast execution
  - 9 test files covering core algorithms

- **Play-mode tests**: `/Assets/Tests_PlayMode/`
  - Integration tests requiring Unity runtime
  - Scene-based tests
  - 2 test files for pause/resume and summary

#### Test Naming
```csharp
[Test]
public void PathFinder_FindsShortestPath_WhenNoObstacles()
{
    // Arrange
    var start = new Vector2Int(0, 0);
    var goal = new Vector2Int(5, 5);

    // Act
    var path = PathFinder.FindPath(start, goal, new HashSet<Vector2Int>(), 50, 50);

    // Assert
    Assert.IsNotNull(path);
    Assert.AreEqual(11, path.Count); // Manhattan distance + 1
}
```

**Pattern**: `MethodName_ExpectedBehavior_Condition`

#### Test Coverage Goals
- **Core logic**: 100% coverage (PathFinder, RobotController, etc.)
- **UI components**: Integration tests only
- **Managers**: Critical paths covered

---

## Development Workflow

### Branch Strategy
- **Main branch**: Stable releases
- **Feature branches**: `feature/T-XXX` (where XXX is task ID)
- **Current work**: Development happens on claude-specific branches
  - Example: `claude/claude-md-mi47em76ocfel778-01DhkjFc62qVzVJNRpkzwmVt`

### Git Workflow
1. Create feature branch: `git checkout -b feature/T-XXX`
2. Make changes following conventions
3. Write tests (unit + integration)
4. Update documentation (README.md if needed)
5. Commit with clear messages
6. Push to remote
7. Create Pull Request
8. Code review
9. Merge to main

### Commit Message Format
```
<type>: <subject>

Examples:
feat: T-303 A* pathfinding with timeout handling
fix: CellHighlightManager.cs null reference
refactor: namespace consolidation to Data.Data
chore: switch platform to WebGL
test: add PathFinder obstacle avoidance tests
docs: update README with Phase 1 completion status
```

**Types**: feat, fix, refactor, chore, test, docs

### Task Tracking (Jira-style)
- **T-XXX**: Task identifier
- **AC-X.X**: Acceptance Criteria number
- Example tasks:
  - T-303: A* path failure/blocking/timeout handling
  - T-304: Summary aggregation (standard format) + UI display
  - T-307: Pause/Resume functionality

### Platform Configuration
- **Current Platform**: WebGL (as of recent commits)
- **Switching**: Unity → File → Build Settings → Select platform → Switch Platform
- **WebGL Template**: `Assets/WebGLTemplates/CustomDefault/`

---

## Testing Guidelines

### Running Tests

#### Via Unity Test Runner (GUI)
1. Window → General → Test Runner
2. Select **EditMode** or **PlayMode** tab
3. Click "Run All" or select specific tests
4. View results in Test Runner window

#### Via Console (Optional)
```bash
# Run all tests
Unity -runTests -batchmode -projectPath /path/to/ShelfSimm_Unity

# Run specific test category
Unity -runTests -editmodeTestResults editmode-results.xml -batchmode
```

### Test Structure

#### Edit-Mode Test Example
```csharp
using NUnit.Framework;
using Core.Core;
using UnityEngine;
using System.Collections.Generic;

namespace Tests
{
    public class PathFindingTests
    {
        [Test]
        public void PathFinder_ReturnsNull_WhenNoPathExists()
        {
            // Arrange
            var start = new Vector2Int(0, 0);
            var goal = new Vector2Int(5, 5);
            var obstacles = new HashSet<Vector2Int>();
            // Create wall blocking path
            for (int x = 0; x <= 5; x++)
            {
                obstacles.Add(new Vector2Int(x, 3));
            }

            // Act
            var path = PathFinder.FindPath(start, goal, obstacles, 10, 10);

            // Assert
            Assert.IsNull(path);
        }
    }
}
```

#### Play-Mode Test Example
```csharp
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Tests
{
    public class SimulationIntegrationTest
    {
        [UnityTest]
        public IEnumerator Simulation_CompletesSuccessfully_WithValidJobs()
        {
            // Arrange
            var manager = Object.FindObjectOfType<SimulationManager>();

            // Act
            manager.StartSimulation();
            yield return new WaitForSeconds(5f);

            // Assert
            Assert.IsTrue(manager.IsCompleted);
        }
    }
}
```

### Test Scenarios Covered

| Module | Test Count | Scenarios |
|--------|------------|-----------|
| PathFinder | 3 | Normal path, obstacles, no path |
| RobotController | 5 | FSM transitions, timeout detection, error handling |
| ErrorCode | 1 | Message generation |
| CodeNormalizer | 4 | Uppercase, zero-padding, trimming |
| InputValidator | 3 | Valid/invalid codes, edge cases |
| TokenParser | 2 | Comma/space separation |
| Summary | 2 | Success/failure aggregation |
| LayoutHashManager | 1 | Layout hash consistency |
| **Total** | **21+** | Comprehensive coverage |

---

## Common Tasks

### 1. Adding a New Feature

**Steps:**
1. Create Jira task (T-XXX)
2. Create feature branch
   ```bash
   git checkout -b feature/T-XXX
   ```
3. Implement feature following conventions
4. Write unit tests
   - Create test file in `/Assets/Tests_EditMode/`
   - Use NUnit framework
   - Cover all code paths
5. Write integration tests (if needed)
   - Create test file in `/Assets/Tests_PlayMode/`
   - Use UnityTest for coroutines
6. Update documentation
   - Update README.md if user-facing
   - Add code comments for complex logic
7. Run all tests
8. Commit and push
9. Create Pull Request

### 2. Modifying Core Logic (PathFinder, RobotController, etc.)

**⚠️ CRITICAL**: Core logic has 100% test coverage. Follow these steps:

1. **Before changing**:
   - Run existing tests to establish baseline
   - Review test cases to understand expected behavior

2. **During development**:
   - Maintain backward compatibility when possible
   - Update tests if behavior intentionally changes
   - Add new tests for new functionality

3. **After changing**:
   - Run all tests: `Window → General → Test Runner → Run All`
   - Ensure 100% pass rate
   - If tests fail, fix code or update tests with justification

4. **Document changes**:
   - Update method XML comments if signature changes
   - Update README.md if algorithm behavior changes

### 3. Adding a New ScriptableObject Configuration

**Example**: Adding a new configuration for robot speed settings

1. **Create data class** in `/Assets/Scripts/Data/`:
```csharp
using UnityEngine;

namespace Data.Data
{
    [CreateAssetMenu(fileName = "SpeedConfig", menuName = "ShelfSim/Speed Config")]
    public class SpeedConfig : ScriptableObject
    {
        [Header("Speed Settings")]
        [SerializeField] private float robotSpeed = 3f;
        [SerializeField] private float turnSpeed = 180f;

        public float RobotSpeed => robotSpeed;
        public float TurnSpeed => turnSpeed;
    }
}
```

2. **Create asset**:
   - Right-click in `/Assets/Config/`
   - Create → ShelfSim → Speed Config
   - Name it `SpeedConfig.asset`

3. **Use in manager**:
```csharp
[SerializeField] private SpeedConfig speedConfig;
```

### 4. Working with API Mode

**Enabling API Mode:**
1. Locate `SimulationManager` in the scene
2. Inspector → check "Use Api Mode"
3. Ensure `ApiClient` component is attached
4. Configure backend URL in `ApiClient`

**API Client Usage:**
```csharp
// Create a run
var request = new CreateRunRequest
{
    layout_id = "layout_001",
    jobs = jobList
};

yield return apiClient.CreateRun(request, (response) =>
{
    _currentRunId = response.run_id;
    Debug.Log($"Run created: {_currentRunId}");
});

// Submit job batch
yield return apiClient.CreateJobsBatch(_currentRunId, jobBatch, (response) =>
{
    Debug.Log($"Batch submitted: {response.batch_id}");
});
```

### 5. Building for WebGL

**Steps:**
1. File → Build Settings
2. Select **WebGL** platform
3. Click "Switch Platform" (if not current)
4. Configure settings:
   - Template: `CustomDefault`
   - Compression: Gzip or Brotli
5. Click "Build" or "Build and Run"
6. Output to `/Builds/WebGL/`

**Custom WebGL Template:**
- Location: `/Assets/WebGLTemplates/CustomDefault/`
- Modify `index.html` for custom UI
- Modify `style.css` for styling
- Update progress bar graphics in `TemplateData/`

### 6. Creating a Custom Inspector

**Example**: Custom editor for `CellsLayoutSO`

```csharp
using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(CellsLayoutSO))]
    public class CellsLayoutSOEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            CellsLayoutSO layout = (CellsLayoutSO)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Layout Info", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Total Cells: {layout.Cells.Count}");

            if (GUILayout.Button("Generate Layout Hash"))
            {
                // Custom button logic
            }
        }
    }
}
```

### 7. Debugging Common Issues

#### Issue: Robot gets stuck in MOVING state
**Solution:**
1. Check timeout value: `robot.moveTimeoutSec`
2. Verify path is not null: `robot.currentPath != null`
3. Check for infinite loop in path (duplicate positions)
4. Enable determinism logging: `DeterminismLogger.Log()`

#### Issue: Tests fail after namespace change
**Solution:**
1. Update assembly definition references
2. Verify using statements in test files
3. Rebuild assembly definitions: `Assets → Reimport All`

#### Issue: WebGL build fails
**Solution:**
1. Check for incompatible plugins
2. Verify no file I/O operations (not supported in WebGL)
3. Check browser console for JavaScript errors
4. Ensure template path is correct in Build Settings

---

## Important Files Reference

### Configuration Files

| File | Location | Purpose |
|------|----------|---------|
| `SimulationConfig.asset` | `/Assets/Config/` | Core simulation parameters |
| `TiebreakerConfig.asset` | `/Assets/Config/` | Tiebreaker seed and settings |
| `CellsLayoutSO.asset` | `/Assets/Config/` | Cell layout definitions |
| `manifest.json` | `/Packages/` | Unity package dependencies |
| `ProjectSettings.asset` | `/ProjectSettings/` | Unity project configuration |

### Key Source Files

| File | Location | Lines | Purpose |
|------|----------|-------|---------|
| `SimulationManager.cs` | `/Assets/Scripts/Managers/` | ~500 | Main orchestrator (Singleton) |
| `RobotController.cs` | `/Assets/Scripts/Core/` | ~300 | Robot FSM and control |
| `PathFinder.cs` | `/Assets/Scripts/Core/` | ~150 | A* pathfinding algorithm |
| `NearestCellSelector.cs` | `/Assets/Scripts/Core/` | ~200 | Nearest-Next task selection |
| `ApiClient.cs` | `/Assets/Scripts/API/` | ~250 | REST API communication |
| `ErrorCode.cs` | `/Assets/Scripts/Data/` | ~100 | Error code definitions |
| `RobotData.cs` | `/Assets/Scripts/Data/` | ~80 | Robot state data |

### Documentation Files

| File | Purpose |
|------|---------|
| `README.md` | Korean project documentation (comprehensive) |
| `CLAUDE.md` | This file - AI assistant guide |
| `LICENSE` | MIT License |

### Assembly Definitions

| File | Dependencies |
|------|--------------|
| `/Assets/Scripts/Data/Data.asmdef` | None |
| `/Assets/Scripts/API/API.asmdef` | None |
| `/Assets/Scripts/Core/Core.asmdef` | Data, API |
| `/Assets/Scripts/Managers/Managers.asmdef` | Core |
| `/Assets/Tests_EditMode/Tests_EditMode.asmdef` | Core, Data |
| `/Assets/Tests_PlayMode/Tests_PlayMode.asmdef` | Core |

---

## Troubleshooting

### Common Errors & Solutions

#### 1. `NullReferenceException` in SimulationManager.Start()
**Cause**: Missing component references
**Solution**:
```csharp
if (apiClient == null)
{
    apiClient = FindObjectOfType<ApiClient>();
}
```

#### 2. Path not found despite clear path
**Cause**: Obstacles set incorrectly or grid bounds exceeded
**Debug**:
```csharp
Debug.Log($"Start: {start}, Goal: {goal}");
Debug.Log($"Obstacles count: {obstacles.Count}");
Debug.Log($"Grid: {maxWidth}x{maxHeight}");
```

#### 3. Tests fail with "Assembly not found"
**Cause**: Assembly definition not properly configured
**Solution**:
1. Open `.asmdef` file
2. Add missing reference to `references` array
3. Save and reimport

#### 4. Robot timeout not triggering
**Cause**: `Time.time` not advancing (Edit mode)
**Solution**: Use Play-mode tests for time-dependent logic

#### 5. WebGL build too large
**Cause**: Uncompressed assets or debug symbols
**Solution**:
- Enable Brotli compression
- Strip debug symbols: Player Settings → Publishing Settings → Enable Exceptions → None
- Optimize texture compression

### Performance Issues

#### Slow pathfinding (> 5ms)
**Diagnosis**:
```csharp
var stopwatch = System.Diagnostics.Stopwatch.StartNew();
var path = PathFinder.FindPath(...);
stopwatch.Stop();
Debug.Log($"Pathfinding took: {stopwatch.ElapsedMilliseconds}ms");
```

**Solutions**:
1. Reduce grid size
2. Implement path caching (`PathCache.cs` is available)
3. Limit A* search depth
4. Use simpler heuristic

#### Low FPS during simulation
**Common causes**:
- Too many Debug.Log calls (remove in production)
- Inefficient Update() loops
- Excessive garbage collection

**Profile**:
- Window → Analysis → Profiler
- Check CPU Usage → Scripts
- Identify hot paths

### Unity Version Compatibility

**Current**: Unity 6000.2.6f2
**Previous**: 2022.3.12f1 (README mentions this older version)

**If opening in older Unity**:
- Some features may not work
- Recommended: Use Unity Hub to install 6000.2.6f2
- Check `ProjectSettings/ProjectVersion.txt` for exact version

### Platform-Specific Issues

#### Windows
- Path separators: Use `Path.Combine()` instead of hardcoded `/`
- Case-insensitive file system

#### macOS/Linux
- Case-sensitive file system
- Ensure correct capitalization in asset paths

#### WebGL
- No file I/O (use PlayerPrefs or remote storage)
- No threading (use coroutines)
- Limited memory (optimize assets)

---

## Additional Resources

### Learning Resources
- **Unity Documentation**: https://docs.unity3d.com/
- **A* Algorithm**: https://www.redblobgames.com/pathfinding/a-star/introduction.html
- **C# Coding Conventions**: https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions

### Project-Specific Documentation
- README.md: Comprehensive Korean documentation
- Inline code comments: Explain complex business logic
- Test files: Demonstrate expected behavior through test cases

### Contact & Support
- **Project Manager**: [See README.md for contact]
- **Issue Tracking**: Jira (internal)
- **Repository**: Git (check remote URL)

---

## Change Log

### 2025-11-18
- Initial creation of CLAUDE.md
- Documented current state as of Unity 6000.2.6f2
- Comprehensive codebase structure analysis
- Added all conventions and workflows
- Included troubleshooting guide

### Future Updates
- Update when major refactoring occurs
- Add new patterns as they're introduced
- Document breaking changes
- Update metrics and statistics

---

## Quick Reference

### File Locations Cheat Sheet
```
Data models:        /Assets/Scripts/Data/
Core logic:         /Assets/Scripts/Core/
Managers:           /Assets/Scripts/Managers/
UI:                 /Assets/Scripts/UI/
API:                /Assets/Scripts/API/
Edit tests:         /Assets/Tests_EditMode/
Play tests:         /Assets/Tests_PlayMode/
Config assets:      /Assets/Config/
WebGL template:     /Assets/WebGLTemplates/CustomDefault/
Main scene:         /Assets/Scenes/MainScene.unity
```

### Command Cheat Sheet
```bash
# Run tests
Unity Test Runner → Run All

# Build WebGL
File → Build Settings → WebGL → Build

# Switch platform
File → Build Settings → Select Platform → Switch Platform

# Reimport assemblies
Assets → Reimport All

# Open Test Runner
Window → General → Test Runner

# Open Profiler
Window → Analysis → Profiler
```

### Key Class References
```csharp
// Singleton access
SimulationManager.Instance

// Pathfinding
PathFinder.FindPath(start, goal, obstacles, width, height)

// Cell selection
NearestCellSelector.SelectNextCell(robot, cells, obstacles)

// Error handling
robot.lastError = ErrorCode.ROUTE_BLOCKED;
robot.lastErrorMessage = ErrorCode.ROUTE_BLOCKED.ToMessage();

// Timeout check
if (RobotController.CheckTimeout(robot, Time.time)) { }
```

---

**End of CLAUDE.md**
