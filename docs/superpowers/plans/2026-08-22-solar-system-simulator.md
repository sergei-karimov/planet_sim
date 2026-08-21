# Solar System Simulator Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build a cross-platform educational 3D N-body simulation of the Sun, eight planets, and Earth's Moon with mouse navigation, body selection and following, time controls, information panels, and historical orbit trails.

**Architecture:** A modular .NET monolith separates a Raylib-independent physics core, application orchestration, and Raylib rendering. Physics advances with fixed 1,800-second velocity Verlet steps and exposes immutable snapshots; rendering consumes snapshots and converts SI positions to scene coordinates without influencing simulation state.

**Tech Stack:** C# 14, .NET 10, Raylib-cs 8.0.0 / Raylib 6.0, xUnit

**Spec:** `docs/superpowers/specs/2026-08-22-solar-system-simulator-design.md`

## Global Constraints

- Target `net10.0`, C# 14, nullable reference types enabled, and warnings treated as errors in project code.
- Support Windows x64, Linux x64, macOS x64, and macOS arm64.
- Use SI units and `double` in physics; never pass Raylib vectors into `PlanetSim.Core`.
- Use a fixed 1,800 simulated-second integration step and velocity Verlet integration.
- Include exactly the Sun, eight planets, and Earth's Moon in the built-in system.
- Use one scene unit per 10 million kilometers, planet/Moon diameter enlargement ×500, and Sun enlargement ×20.
- Sample trails every six simulated hours into 8,192-position ring buffers.
- Keep all user-visible strings in an English string catalog.
- Use Raylib drawing directly; do not introduce another UI framework.
- Missing textures fall back to colored spheres and log warnings.
- Follow TDD for every behavior and commit after every task.

## Planned File Structure

```text
PlanetSim.slnx
Directory.Build.props
src/
  PlanetSim.Core/
    PlanetSim.Core.csproj
    Math/Vector3d.cs
    Model/BodyId.cs
    Model/BodyKind.cs
    Model/CelestialBodyDefinition.cs
    Model/BodyState.cs
    Model/SimulationSnapshot.cs
    Model/SolarSystemCatalog.cs
    Physics/GravityCalculator.cs
    Physics/VelocityVerletIntegrator.cs
    Physics/SimulationEngine.cs
    Physics/SimulationDiagnostics.cs
  PlanetSim.App/
    PlanetSim.App.csproj
    Composition.cs
    Simulation/SimulationController.cs
    Simulation/SimulationStatus.cs
    Simulation/TimeRate.cs
    Trails/OrbitTrail.cs
    Trails/TrailRecorder.cs
    Selection/SelectionState.cs
  PlanetSim.Rendering/
    PlanetSim.Rendering.csproj
    Program.cs
    PlanetSimGame.cs
    Assets/TextureCatalog.cs
    Camera/CameraController.cs
    Camera/CameraMode.cs
    Camera/CameraTransition.cs
    Input/BodyPicker.cs
    Scene/SceneScale.cs
    Scene/SolarSystemRenderer.cs
    Ui/EnglishStrings.cs
    Ui/UnitFormatter.cs
    Ui/HudRenderer.cs
    assets/ATTRIBUTION.md
    assets/textures/*.png
tests/
  PlanetSim.Core.Tests/
    PlanetSim.Core.Tests.csproj
    Math/Vector3dTests.cs
    Model/SolarSystemCatalogTests.cs
    Physics/GravityCalculatorTests.cs
    Physics/VelocityVerletIntegratorTests.cs
    Physics/SimulationEngineScenarioTests.cs
  PlanetSim.App.Tests/
    PlanetSim.App.Tests.csproj
    Simulation/SimulationControllerTests.cs
    Simulation/TimeRateTests.cs
    Trails/OrbitTrailTests.cs
    Trails/TrailRecorderTests.cs
    Selection/SelectionStateTests.cs
  PlanetSim.Rendering.Tests/
    PlanetSim.Rendering.Tests.csproj
    Scene/SceneScaleTests.cs
    Ui/UnitFormatterTests.cs
    Camera/CameraTransitionTests.cs
    Input/BodyPickerTests.cs
README.md
```

---

### Task 1: Solution Skeleton and Core Math

**Files:**
- Create: `PlanetSim.slnx`
- Create: `Directory.Build.props`
- Create: `src/PlanetSim.Core/PlanetSim.Core.csproj`
- Create: `src/PlanetSim.Core/Math/Vector3d.cs`
- Create: `tests/PlanetSim.Core.Tests/PlanetSim.Core.Tests.csproj`
- Create: `tests/PlanetSim.Core.Tests/Math/Vector3dTests.cs`

**Interfaces:**
- Produces: immutable `Vector3d(double X, double Y, double Z)` with arithmetic operators, `Length`, `LengthSquared`, `IsFinite`, `Dot`, and `Lerp`.

- [ ] **Step 1: Scaffold the solution and projects**

Run:

```bash
dotnet new sln --name PlanetSim --format slnx
dotnet new classlib --name PlanetSim.Core --output src/PlanetSim.Core --framework net10.0
dotnet new xunit --name PlanetSim.Core.Tests --output tests/PlanetSim.Core.Tests --framework net10.0
dotnet sln PlanetSim.slnx add src/PlanetSim.Core/PlanetSim.Core.csproj tests/PlanetSim.Core.Tests/PlanetSim.Core.Tests.csproj
dotnet add tests/PlanetSim.Core.Tests/PlanetSim.Core.Tests.csproj reference src/PlanetSim.Core/PlanetSim.Core.csproj
```

Delete the generated `Class1.cs` and `UnitTest1.cs` with `apply_patch`. Create `Directory.Build.props`:

```xml
<Project>
  <PropertyGroup>
    <LangVersion>14</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <AnalysisLevel>latest</AnalysisLevel>
  </PropertyGroup>
</Project>
```

- [ ] **Step 2: Write failing vector tests**

```csharp
using PlanetSim.Core.Math;

namespace PlanetSim.Core.Tests.Math;

public sealed class Vector3dTests
{
    [Fact]
    public void ArithmeticAndLengthUseDoublePrecision()
    {
        var a = new Vector3d(1, 2, 3);
        var b = new Vector3d(4, -2, 1);

        Assert.Equal(new Vector3d(5, 0, 4), a + b);
        Assert.Equal(new Vector3d(-3, 4, 2), a - b);
        Assert.Equal(new Vector3d(2, 4, 6), a * 2);
        Assert.Equal(14, a.LengthSquared);
        Assert.Equal(System.Math.Sqrt(14), a.Length, 12);
    }

    [Fact]
    public void FiniteAndInterpolationHelpersAreDeterministic()
    {
        Assert.True(Vector3d.Zero.IsFinite);
        Assert.False(new Vector3d(double.NaN, 0, 0).IsFinite);
        Assert.Equal(new Vector3d(2.5, 5, 7.5),
            Vector3d.Lerp(Vector3d.Zero, new Vector3d(10, 20, 30), 0.25));
    }
}
```

- [ ] **Step 3: Run the test and verify the missing type failure**

Run: `dotnet test tests/PlanetSim.Core.Tests/PlanetSim.Core.Tests.csproj`

Expected: build fails because `PlanetSim.Core.Math.Vector3d` does not exist.

- [ ] **Step 4: Implement `Vector3d`**

```csharp
namespace PlanetSim.Core.Math;

public readonly record struct Vector3d(double X, double Y, double Z)
{
    public static Vector3d Zero => new(0, 0, 0);
    public double LengthSquared => X * X + Y * Y + Z * Z;
    public double Length => System.Math.Sqrt(LengthSquared);
    public bool IsFinite => double.IsFinite(X) && double.IsFinite(Y) && double.IsFinite(Z);

    public static Vector3d operator +(Vector3d a, Vector3d b) =>
        new(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
    public static Vector3d operator -(Vector3d a, Vector3d b) =>
        new(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
    public static Vector3d operator *(Vector3d value, double scalar) =>
        new(value.X * scalar, value.Y * scalar, value.Z * scalar);
    public static Vector3d operator /(Vector3d value, double scalar) =>
        new(value.X / scalar, value.Y / scalar, value.Z / scalar);
    public static double Dot(Vector3d a, Vector3d b) =>
        a.X * b.X + a.Y * b.Y + a.Z * b.Z;
    public static Vector3d Lerp(Vector3d a, Vector3d b, double amount) =>
        a + (b - a) * amount;
}
```

- [ ] **Step 5: Run all tests and commit**

Run: `dotnet test PlanetSim.slnx`

Expected: PASS.

```bash
git add PlanetSim.slnx Directory.Build.props src tests
git commit -m "build: scaffold solution and core vector math"
```

---

### Task 2: Body Model and Validated Solar System Catalog

**Files:**
- Create: `src/PlanetSim.Core/Model/BodyId.cs`
- Create: `src/PlanetSim.Core/Model/BodyKind.cs`
- Create: `src/PlanetSim.Core/Model/CelestialBodyDefinition.cs`
- Create: `src/PlanetSim.Core/Model/BodyState.cs`
- Create: `src/PlanetSim.Core/Model/SimulationSnapshot.cs`
- Create: `src/PlanetSim.Core/Model/SolarSystemCatalog.cs`
- Create: `tests/PlanetSim.Core.Tests/Model/SolarSystemCatalogTests.cs`

**Interfaces:**
- Consumes: `Vector3d` from Task 1.
- Produces: `BodyId`, `BodyKind`, `CelestialBodyDefinition`, mutable-in-engine `BodyState`, immutable `BodySnapshot`, `SimulationSnapshot`, and `SolarSystemCatalog.CreateInitialState()`.

- [ ] **Step 1: Write failing catalog tests**

```csharp
using PlanetSim.Core.Model;

namespace PlanetSim.Core.Tests.Model;

public sealed class SolarSystemCatalogTests
{
    [Fact]
    public void CatalogContainsSunEightPlanetsAndMoon()
    {
        var bodies = SolarSystemCatalog.CreateInitialState();

        Assert.Equal(10, bodies.Count);
        Assert.Equal(10, bodies.Select(x => x.Definition.Id).Distinct().Count());
        Assert.Contains(bodies, x => x.Definition.Id == BodyId.Sun);
        Assert.Contains(bodies, x => x.Definition.Id == BodyId.Moon);
        Assert.All(bodies, x => Assert.True(x.Definition.MassKg > 0));
        Assert.All(bodies, x => Assert.True(x.PositionMeters.IsFinite && x.VelocityMetersPerSecond.IsFinite));
    }

    [Fact]
    public void CatalogStartsNearItsCenterOfMomentumFrame()
    {
        var bodies = SolarSystemCatalog.CreateInitialState();
        var totalMomentum = bodies.Aggregate(
            PlanetSim.Core.Math.Vector3d.Zero,
            (sum, body) => sum + body.VelocityMetersPerSecond * body.Definition.MassKg);

        Assert.True(totalMomentum.Length < 1e20,
            $"Initial momentum was {totalMomentum.Length:E3} kg m/s");
    }
}
```

- [ ] **Step 2: Run the catalog tests and verify failure**

Run: `dotnet test tests/PlanetSim.Core.Tests/PlanetSim.Core.Tests.csproj --filter SolarSystemCatalogTests`

Expected: build fails because the model types do not exist.

- [ ] **Step 3: Implement the model contracts and validation**

Use explicit stable IDs:

```csharp
namespace PlanetSim.Core.Model;

public enum BodyId { Sun, Mercury, Venus, Earth, Moon, Mars, Jupiter, Saturn, Uranus, Neptune }
public enum BodyKind { Star, Planet, Moon }

public sealed record CelestialBodyDefinition(
    BodyId Id,
    string DisplayName,
    BodyKind Kind,
    double MassKg,
    double DiameterMeters,
    string TextureAsset,
    uint FallbackRgba)
{
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(DisplayName)) throw new ArgumentException("Display name is required.");
        if (!double.IsFinite(MassKg) || MassKg <= 0) throw new ArgumentOutOfRangeException(nameof(MassKg));
        if (!double.IsFinite(DiameterMeters) || DiameterMeters <= 0) throw new ArgumentOutOfRangeException(nameof(DiameterMeters));
    }
}
```

Define `BodyState` as a sealed class owned by the engine, with read-only `Definition` and internal setters for position and velocity. Define `BodySnapshot` and `SimulationSnapshot` as immutable records; `SimulationSnapshot` contains `TimeSpan Elapsed` and `IReadOnlyList<BodySnapshot> Bodies`, plus `GetBody(BodyId)`.

Implement `SolarSystemCatalog.CreateInitialState()` with the ten masses and diameters documented from NASA/JPL sources in code comments. Construct approximate 3D position/velocity vectors from semi-major axis, orbital speed, inclination, and fixed phase. Offset all positions and velocities into center-of-mass and center-of-momentum frames before returning. Return a fresh independent list on every call.

Use these fixed educational initial values; angles are degrees and phases are measured in the reference XY plane:

| Body | Mass kg | Diameter m | Semi-major axis m | Tangential speed m/s | Inclination | Phase |
|---|---:|---:|---:|---:|---:|---:|
| Sun | 1.98847e30 | 1.39270e9 | 0 | 0 | 0 | 0 |
| Mercury | 3.3011e23 | 4.8794e6 | 5.7909e10 | 47,360 | 7.005 | 15 |
| Venus | 4.8675e24 | 1.21036e7 | 1.08210e11 | 35,020 | 3.3946 | 75 |
| Earth | 5.97237e24 | 1.27420e7 | 1.49598e11 | 29,780 | 0 | 140 |
| Moon | 7.342e22 | 3.4748e6 | 3.844e8 relative to Earth | 1,022 relative to Earth | 5.145 relative to ecliptic | 45 relative to Earth |
| Mars | 6.4171e23 | 6.7790e6 | 2.27939e11 | 24,070 | 1.850 | 210 |
| Jupiter | 1.8982e27 | 1.39820e8 | 7.7857e11 | 13,070 | 1.303 | 255 |
| Saturn | 5.6834e26 | 1.16460e8 | 1.43353e12 | 9,680 | 2.485 | 300 |
| Uranus | 8.6810e25 | 5.0724e7 | 2.87246e12 | 6,800 | 0.773 | 330 |
| Neptune | 1.02413e26 | 4.9244e7 | 4.49506e12 | 5,430 | 1.770 | 20 |

For a non-Moon body, rotate `(axis, 0, 0)` by phase around Z and then inclination around X; construct its prograde tangential vector with the same rotations. Construct the Moon relative to Earth's already calculated state, then add Earth's position and velocity. Finally shift every body into barycentric position and momentum frames. Add links to the exact NASA/JPL fact pages used to verify these constants in source comments; do not silently replace a value during implementation.

- [ ] **Step 4: Add validation regression tests**

```csharp
[Theory]
[InlineData(0)]
[InlineData(-1)]
[InlineData(double.NaN)]
public void DefinitionRejectsInvalidMass(double mass)
{
    var definition = new CelestialBodyDefinition(
        BodyId.Earth, "Earth", BodyKind.Planet, mass, 12_742_000, "earth.png", 0xFFFFFFFF);

    Assert.ThrowsAny<ArgumentException>(definition.Validate);
}
```

- [ ] **Step 5: Run tests and commit**

Run: `dotnet test PlanetSim.slnx`

Expected: PASS with 10 unique bodies and finite initial values.

```bash
git add src/PlanetSim.Core/Model tests/PlanetSim.Core.Tests/Model
git commit -m "feat: add validated solar system body catalog"
```

---

### Task 3: Pairwise Gravity Calculation

**Files:**
- Create: `src/PlanetSim.Core/Physics/GravityCalculator.cs`
- Create: `tests/PlanetSim.Core.Tests/Physics/GravityCalculatorTests.cs`

**Interfaces:**
- Consumes: `IReadOnlyList<BodyState>`.
- Produces: `GravityCalculator.ComputeAccelerations(IReadOnlyList<BodyState>) -> Vector3d[]` and constant `GravitationalConstant`.

- [ ] **Step 1: Write failing two-body and momentum-symmetry tests**

```csharp
using PlanetSim.Core.Math;
using PlanetSim.Core.Model;
using PlanetSim.Core.Physics;

namespace PlanetSim.Core.Tests.Physics;

public sealed class GravityCalculatorTests
{
    [Fact]
    public void TwoBodiesAccelerateTowardEachOther()
    {
        var left = TestBody(BodyId.Sun, 2, new Vector3d(0, 0, 0));
        var right = TestBody(BodyId.Earth, 3, new Vector3d(2, 0, 0));

        var accelerations = GravityCalculator.ComputeAccelerations([left, right]);

        Assert.Equal(GravityCalculator.GravitationalConstant * 3 / 4, accelerations[0].X, 15);
        Assert.Equal(-GravityCalculator.GravitationalConstant * 2 / 4, accelerations[1].X, 15);
        Assert.Equal(0, accelerations[0].Y);
    }

    [Fact]
    public void PairContributionsPreserveMomentumDerivative()
    {
        var a = TestBody(BodyId.Sun, 7, new Vector3d(-2, 1, 0));
        var b = TestBody(BodyId.Earth, 11, new Vector3d(3, -4, 1));
        var acceleration = GravityCalculator.ComputeAccelerations([a, b]);
        var netForce = acceleration[0] * 7 + acceleration[1] * 11;
        Assert.True(netForce.Length < 1e-24);
    }
}
```

Add a private `TestBody` helper that creates a valid definition and zero-velocity `BodyState`.

- [ ] **Step 2: Run tests and confirm the missing calculator failure**

Run: `dotnet test tests/PlanetSim.Core.Tests/PlanetSim.Core.Tests.csproj --filter GravityCalculatorTests`

Expected: build fails because `GravityCalculator` is missing.

- [ ] **Step 3: Implement pairwise acceleration once per unordered pair**

```csharp
public static class GravityCalculator
{
    public const double GravitationalConstant = 6.67430e-11;

    public static Vector3d[] ComputeAccelerations(IReadOnlyList<BodyState> bodies)
    {
        var result = new Vector3d[bodies.Count];
        for (var i = 0; i < bodies.Count; i++)
        for (var j = i + 1; j < bodies.Count; j++)
        {
            var delta = bodies[j].PositionMeters - bodies[i].PositionMeters;
            var distanceSquared = delta.LengthSquared;
            if (!double.IsFinite(distanceSquared) || distanceSquared <= 0)
                throw new InvalidOperationException("Bodies must have distinct finite positions.");
            var inverseDistanceCubed = 1.0 / (distanceSquared * System.Math.Sqrt(distanceSquared));
            var directionFactor = delta * (GravitationalConstant * inverseDistanceCubed);
            result[i] += directionFactor * bodies[j].Definition.MassKg;
            result[j] -= directionFactor * bodies[i].Definition.MassKg;
        }
        return result;
    }
}
```

- [ ] **Step 4: Run tests and commit**

Run: `dotnet test PlanetSim.slnx`

Expected: PASS.

```bash
git add src/PlanetSim.Core/Physics/GravityCalculator.cs tests/PlanetSim.Core.Tests/Physics/GravityCalculatorTests.cs
git commit -m "feat: calculate symmetric n-body gravity"
```

---

### Task 4: Velocity Verlet Engine and Diagnostics

**Files:**
- Create: `src/PlanetSim.Core/Physics/VelocityVerletIntegrator.cs`
- Create: `src/PlanetSim.Core/Physics/SimulationDiagnostics.cs`
- Create: `src/PlanetSim.Core/Physics/SimulationEngine.cs`
- Create: `tests/PlanetSim.Core.Tests/Physics/VelocityVerletIntegratorTests.cs`
- Create: `tests/PlanetSim.Core.Tests/Physics/SimulationEngineScenarioTests.cs`

**Interfaces:**
- Consumes: body model and `GravityCalculator`.
- Produces: `VelocityVerletIntegrator.Step(List<BodyState>, double)`, `SimulationEngine.Step()`, `SimulationEngine.CreateSnapshot()`, `SimulationDiagnostics.TotalEnergy(...)`, and `TotalAngularMomentum(...)`.

- [ ] **Step 1: Write failing one-step integrator test**

```csharp
[Fact]
public void StepUpdatesPositionThenVelocityUsingNewAcceleration()
{
    var bodies = TwoBodyFixture.Create();
    var before = bodies.Select(x => x.Copy()).ToArray();
    var initialAcceleration = GravityCalculator.ComputeAccelerations(before);

    VelocityVerletIntegrator.Step(bodies, 10);

    var expectedPosition = before[0].PositionMeters
        + before[0].VelocityMetersPerSecond * 10
        + initialAcceleration[0] * 50;
    Assert.True((bodies[0].PositionMeters - expectedPosition).Length < 1e-6);
    Assert.All(bodies, body => Assert.True(body.PositionMeters.IsFinite && body.VelocityMetersPerSecond.IsFinite));
}
```

Create `TwoBodyFixture` in the test project with a Sun/Earth circular approximation. Add `public BodyState Copy()` to `BodyState`; it returns a new state containing the same immutable definition and copied position and velocity values.

- [ ] **Step 2: Run the integrator test and verify failure**

Run: `dotnet test tests/PlanetSim.Core.Tests/PlanetSim.Core.Tests.csproj --filter VelocityVerletIntegratorTests`

Expected: build fails because the integrator does not exist.

- [ ] **Step 3: Implement velocity Verlet and finite-state checks**

```csharp
public static void Step(List<BodyState> bodies, double deltaSeconds)
{
    if (!double.IsFinite(deltaSeconds) || deltaSeconds <= 0)
        throw new ArgumentOutOfRangeException(nameof(deltaSeconds));

    var oldAcceleration = GravityCalculator.ComputeAccelerations(bodies);
    for (var i = 0; i < bodies.Count; i++)
        bodies[i].PositionMeters += bodies[i].VelocityMetersPerSecond * deltaSeconds
            + oldAcceleration[i] * (0.5 * deltaSeconds * deltaSeconds);

    var newAcceleration = GravityCalculator.ComputeAccelerations(bodies);
    for (var i = 0; i < bodies.Count; i++)
    {
        bodies[i].VelocityMetersPerSecond +=
            (oldAcceleration[i] + newAcceleration[i]) * (0.5 * deltaSeconds);
        if (!bodies[i].PositionMeters.IsFinite || !bodies[i].VelocityMetersPerSecond.IsFinite)
            throw new InvalidOperationException($"Non-finite state for {bodies[i].Definition.Id}.");
    }
}
```

Keep the engine's private storage as `List<BodyState>` and pass it directly to `ComputeAccelerations(IReadOnlyList<BodyState>)`.

- [ ] **Step 4: Write failing engine stability tests**

```csharp
[Fact]
public void FullSystemRemainsFiniteAndEnergyDriftIsBoundedForOneYear()
{
    var engine = SimulationEngine.CreateSolarSystem();
    var initialEnergy = SimulationDiagnostics.TotalEnergy(engine.Bodies);

    for (var i = 0; i < 365 * 48; i++) engine.Step();

    var finalEnergy = SimulationDiagnostics.TotalEnergy(engine.Bodies);
    var relativeDrift = System.Math.Abs((finalEnergy - initialEnergy) / initialEnergy);
    Assert.True(relativeDrift < 1e-4, $"Relative energy drift: {relativeDrift:E3}");
    Assert.All(engine.CreateSnapshot().Bodies,
        body => Assert.True(body.PositionMeters.IsFinite && body.VelocityMetersPerSecond.IsFinite));
}
```

Add a two-body test with a tighter `1e-6` one-year energy-drift threshold and a full-system angular-momentum drift threshold of `1e-10`. Record measured values in assertion messages; relax a threshold only with an explanatory comment and evidence from the baseline run.

- [ ] **Step 5: Implement engine, snapshots, energy, and angular momentum**

`SimulationEngine.FixedStepSeconds` must equal `1800`. `Step()` advances exactly one fixed step and increments elapsed simulated seconds only after a successful step. `CreateSnapshot()` deep-copies values into immutable records. Diagnostics calculate kinetic energy, pairwise gravitational potential energy, and `Σ m(r × v)` using a new `Vector3d.Cross` helper covered by a unit test.

- [ ] **Step 6: Run the focused long test and then the suite**

Run:

```bash
dotnet test tests/PlanetSim.Core.Tests/PlanetSim.Core.Tests.csproj --filter SimulationEngineScenarioTests
dotnet test PlanetSim.slnx
```

Expected: PASS; the one-year test stays finite and within documented drift bounds.

- [ ] **Step 7: Commit**

```bash
git add src/PlanetSim.Core tests/PlanetSim.Core.Tests
git commit -m "feat: add stable velocity verlet simulation engine"
```

---

### Task 5: Time Rate and Simulation Controller

**Files:**
- Create: `src/PlanetSim.App/PlanetSim.App.csproj`
- Create: `src/PlanetSim.App/Simulation/TimeRate.cs`
- Create: `src/PlanetSim.App/Simulation/SimulationStatus.cs`
- Create: `src/PlanetSim.App/Simulation/SimulationController.cs`
- Create: `src/PlanetSim.App/Selection/SelectionState.cs`
- Create: `tests/PlanetSim.App.Tests/PlanetSim.App.Tests.csproj`
- Create: `tests/PlanetSim.App.Tests/Simulation/TimeRateTests.cs`
- Create: `tests/PlanetSim.App.Tests/Simulation/SimulationControllerTests.cs`
- Create: `tests/PlanetSim.App.Tests/Selection/SelectionStateTests.cs`

**Interfaces:**
- Consumes: `SimulationEngine` and snapshots.
- Produces: `TimeRate.FromSlider(double)`, `SimulationController.Update(double realDeltaSeconds)`, `TogglePause()`, `Reset()`, `Select(BodyId)`, `ReturnToSystemView()`, and immutable `SimulationStatus`.

- [ ] **Step 1: Scaffold projects and references**

Run:

```bash
dotnet new classlib --name PlanetSim.App --output src/PlanetSim.App --framework net10.0
dotnet new xunit --name PlanetSim.App.Tests --output tests/PlanetSim.App.Tests --framework net10.0
dotnet sln PlanetSim.slnx add src/PlanetSim.App/PlanetSim.App.csproj tests/PlanetSim.App.Tests/PlanetSim.App.Tests.csproj
dotnet add src/PlanetSim.App/PlanetSim.App.csproj reference src/PlanetSim.Core/PlanetSim.Core.csproj
dotnet add tests/PlanetSim.App.Tests/PlanetSim.App.Tests.csproj reference src/PlanetSim.App/PlanetSim.App.csproj
```

Remove the generated `Class1.cs` and `UnitTest1.cs` files with `apply_patch`.

- [ ] **Step 2: Write failing logarithmic-rate tests**

```csharp
[Theory]
[InlineData(0, 1)]
[InlineData(1, 2_592_000)]
public void SliderMapsEndpointsToSecondsPerSecond(double slider, double expected)
{
    Assert.Equal(expected, TimeRate.FromSlider(slider).SimulatedSecondsPerRealSecond, 6);
}

[Fact]
public void SliderMidpointIsGeometricMean()
{
    var expected = System.Math.Sqrt(2_592_000);
    Assert.Equal(expected, TimeRate.FromSlider(0.5).SimulatedSecondsPerRealSecond, 6);
}
```

- [ ] **Step 3: Implement `TimeRate`**

Clamp slider input to `[0,1]` and calculate `exp(log(min) + t * (log(max)-log(min)))`, with min `1` and max `2_592_000` seconds per second.

- [ ] **Step 4: Write failing controller tests using an engine interface**

Extract `ISimulationEngine` with `FixedStepSeconds`, `Elapsed`, `Step()`, `Reset()`, and `CreateSnapshot()`. Use a fake engine to assert:

```csharp
[Fact]
public void UpdateCapsWorkAndReportsEffectiveRateWithoutGrowingStep()
{
    var engine = new FakeEngine(fixedStepSeconds: 1800);
    var controller = new SimulationController(engine, maxStepsPerFrame: 100);
    controller.SetSlider(1);

    controller.Update(1.0 / 60.0);

    Assert.Equal(100, engine.StepCalls);
    Assert.True(controller.Status.EffectiveSecondsPerRealSecond
        < controller.Status.RequestedSecondsPerRealSecond);
}

[Fact]
public void InvalidPhysicsPausesAndKeepsLastValidSnapshot()
{
    var engine = new FakeEngine(1800) { ThrowOnStep = true };
    var controller = new SimulationController(engine, 100);
    var valid = controller.Status.Snapshot;

    controller.Update(1);

    Assert.True(controller.Status.IsPaused);
    Assert.Same(valid, controller.Status.Snapshot);
    Assert.NotNull(controller.Status.ErrorMessage);
}
```

Also test pause, reset, selection, and `ReturnToSystemView()` explicitly.

- [ ] **Step 5: Implement controller scheduling**

Maintain a simulated-seconds accumulator. Each update adds `realDelta * requestedRate`; execute `min(floor(accumulator / 1800), maxStepsPerFrame)` fixed steps. Remove only executed time from the accumulator, but cap the remaining backlog to one real frame's maximum work so the app cannot remain permanently behind. Compute effective rate from executed simulated seconds divided by real delta. On engine exception, pause and retain the prior snapshot.

- [ ] **Step 6: Run tests and commit**

Run: `dotnet test PlanetSim.slnx`

Expected: PASS.

```bash
git add PlanetSim.slnx src/PlanetSim.App tests/PlanetSim.App.Tests
git commit -m "feat: orchestrate simulation time pause reset and selection"
```

---

### Task 6: Deterministic Orbit Trail Recording

**Files:**
- Create: `src/PlanetSim.App/Trails/OrbitTrail.cs`
- Create: `src/PlanetSim.App/Trails/TrailRecorder.cs`
- Create: `tests/PlanetSim.App.Tests/Trails/OrbitTrailTests.cs`
- Create: `tests/PlanetSim.App.Tests/Trails/TrailRecorderTests.cs`
- Modify: `src/PlanetSim.App/Simulation/SimulationController.cs`

**Interfaces:**
- Consumes: consecutive `SimulationSnapshot` values.
- Produces: `OrbitTrail` bounded at 8,192 entries and `TrailRecorder.Record(previous, current)` with six-hour boundary interpolation.

- [ ] **Step 1: Write failing ring-buffer tests**

```csharp
[Fact]
public void AddingPastCapacityEvictsOldestPosition()
{
    var trail = new OrbitTrail(capacity: 3);
    for (var x = 1; x <= 4; x++) trail.Add(new Vector3d(x, 0, 0));

    Assert.Equal([2d, 3d, 4d], trail.Positions.Select(p => p.X));
}

[Fact]
public void ClearRemovesEveryPosition()
{
    var trail = new OrbitTrail(3);
    trail.Add(new Vector3d(1, 0, 0));
    trail.Clear();
    Assert.Empty(trail.Positions);
}
```

- [ ] **Step 2: Run tests, implement `OrbitTrail`, and rerun**

Run focused tests first and confirm missing-type failure. Implement a fixed array with head/count indices and expose positions in oldest-to-newest order without allowing mutation. Rerun and expect PASS.

- [ ] **Step 3: Write failing six-hour interpolation test**

```csharp
[Fact]
public void CrossingSampleBoundaryInterpolatesEveryBodyAtBoundary()
{
    var recorder = new TrailRecorder(TimeSpan.FromHours(6), capacity: 8192);
    var before = Snapshots.At(hours: 5, earthX: 10);
    var after = Snapshots.At(hours: 7, earthX: 30);

    recorder.Record(before, after);

    Assert.Equal(20, recorder.GetTrail(BodyId.Earth).Positions.Single().X, 12);
}
```

- [ ] **Step 4: Implement recorder and controller integration**

For each crossed six-hour boundary, compute `t = (boundary - previous.Elapsed) / (current.Elapsed - previous.Elapsed)` and interpolate every body's position with `Vector3d.Lerp`. Do not record the Sun by default, but retain a trail entry and `includeSun` option. `SimulationController.Reset()` clears trails; toggling visibility does not.

- [ ] **Step 5: Run tests and commit**

Run: `dotnet test PlanetSim.slnx`

Expected: PASS.

```bash
git add src/PlanetSim.App tests/PlanetSim.App.Tests
git commit -m "feat: record bounded simulation-time orbit trails"
```

---

### Task 7: Rendering-Safe Scaling and Formatting

**Files:**
- Create: `src/PlanetSim.Rendering/PlanetSim.Rendering.csproj`
- Create: `src/PlanetSim.Rendering/Scene/SceneScale.cs`
- Create: `src/PlanetSim.Rendering/Ui/EnglishStrings.cs`
- Create: `src/PlanetSim.Rendering/Ui/UnitFormatter.cs`
- Create: `tests/PlanetSim.Rendering.Tests/PlanetSim.Rendering.Tests.csproj`
- Create: `tests/PlanetSim.Rendering.Tests/Scene/SceneScaleTests.cs`
- Create: `tests/PlanetSim.Rendering.Tests/Ui/UnitFormatterTests.cs`

**Interfaces:**
- Consumes: Core SI values.
- Produces: `SceneScale.Position(Vector3d) -> System.Numerics.Vector3`, `Radius(CelestialBodyDefinition) -> float`, `UnitFormatter` methods, and `EnglishStrings` constants.

- [ ] **Step 1: Scaffold rendering projects and install Raylib**

Run:

```bash
dotnet new console --name PlanetSim.Rendering --output src/PlanetSim.Rendering --framework net10.0
dotnet new xunit --name PlanetSim.Rendering.Tests --output tests/PlanetSim.Rendering.Tests --framework net10.0
dotnet sln PlanetSim.slnx add src/PlanetSim.Rendering/PlanetSim.Rendering.csproj tests/PlanetSim.Rendering.Tests/PlanetSim.Rendering.Tests.csproj
dotnet add src/PlanetSim.Rendering/PlanetSim.Rendering.csproj reference src/PlanetSim.Core/PlanetSim.Core.csproj src/PlanetSim.App/PlanetSim.App.csproj
dotnet add src/PlanetSim.Rendering/PlanetSim.Rendering.csproj package Raylib-cs --version 8.0.0
dotnet add tests/PlanetSim.Rendering.Tests/PlanetSim.Rendering.Tests.csproj reference src/PlanetSim.Rendering/PlanetSim.Rendering.csproj
```

- [ ] **Step 2: Write failing scale and formatting tests**

```csharp
[Fact]
public void PositionUsesTenMillionKilometersPerSceneUnit()
{
    Assert.Equal(new Vector3(1, 2, -3),
        SceneScale.Position(new Vector3d(1e10, 2e10, -3e10)));
}

[Fact]
public void RadiusUsesSeparateSunAndPlanetFactors()
{
    Assert.Equal((float)(12_742_000d / 2 / 1e10 * 500), SceneScale.Radius(Definitions.Earth));
    Assert.Equal((float)(1_392_700_000d / 2 / 1e10 * 20), SceneScale.Radius(Definitions.Sun));
}

[Theory]
[InlineData(149_597_870_700, "1.000 AU")]
[InlineData(384_400_000, "384,400 km")]
public void DistanceFormattingChoosesReadableUnits(double meters, string expected)
{
    Assert.Equal(expected, UnitFormatter.Distance(meters));
}
```

- [ ] **Step 3: Implement scale, formatter, and string catalog**

Use constants `MetersPerSceneUnit = 1e10`, `PlanetDiameterScale = 500`, and `SunDiameterScale = 20`. Check converted components for float overflow before casting. `UnitFormatter` uses invariant English formatting and constants `MetersPerKilometer = 1000` and `MetersPerAstronomicalUnit = 149_597_870_700`. Include every UI label from the spec in `EnglishStrings`; rendering code must reference that catalog rather than literal labels.

- [ ] **Step 4: Run tests and commit**

Run: `dotnet test PlanetSim.slnx`

Expected: PASS.

```bash
git add PlanetSim.slnx src/PlanetSim.Rendering tests/PlanetSim.Rendering.Tests
git commit -m "feat: add scene scaling and localized value formatting"
```

---

### Task 8: Camera Transition and Body Picking Logic

**Files:**
- Create: `src/PlanetSim.Rendering/Camera/CameraMode.cs`
- Create: `src/PlanetSim.Rendering/Camera/CameraTransition.cs`
- Create: `src/PlanetSim.Rendering/Camera/CameraController.cs`
- Create: `src/PlanetSim.Rendering/Input/BodyPicker.cs`
- Create: `tests/PlanetSim.Rendering.Tests/Camera/CameraTransitionTests.cs`
- Create: `tests/PlanetSim.Rendering.Tests/Input/BodyPickerTests.cs`

**Interfaces:**
- Consumes: scene positions/radii and app selection commands.
- Produces: pure `CameraTransition.Evaluate(...)`, `BodyPicker.Pick(Ray, candidates) -> BodyId?`, and stateful Raylib-facing `CameraController`.

- [ ] **Step 1: Write failing pure transition tests**

```csharp
[Fact]
public void TransitionStartsAtCurrentTargetAndEndsAtSelectedBody()
{
    var transition = new CameraTransition(Vector3.Zero, new Vector3(10, 0, 0), 0.4);
    Assert.Equal(Vector3.Zero, transition.Evaluate(0));
    Assert.Equal(new Vector3(10, 0, 0), transition.Evaluate(0.4));
    Assert.Equal(new Vector3(10, 0, 0), transition.Evaluate(1));
}
```

Use smoothstep `t*t*(3-2*t)` with clamped normalized time.

- [ ] **Step 2: Write failing nearest-hit picking tests**

```csharp
[Fact]
public void PickReturnsNearestPositiveSphereIntersection()
{
    var ray = new Ray3(Vector3.Zero, Vector3.UnitZ);
    var candidates = new[]
    {
        new PickSphere(BodyId.Mars, new Vector3(0, 0, 10), 2),
        new PickSphere(BodyId.Earth, new Vector3(0, 0, 5), 1)
    };
    Assert.Equal(BodyId.Earth, BodyPicker.Pick(ray, candidates));
}
```

Define `Ray3` and `PickSphere` as rendering-owned immutable records so tests do not require an open window.

- [ ] **Step 3: Implement transition and analytic ray/sphere picking**

Normalize ray direction. Solve the quadratic intersection, reject negative distances, and return the closest nonnegative hit. Handle a ray originating inside a sphere by selecting its positive exit intersection.

- [ ] **Step 4: Implement `CameraController` around pure logic**

Maintain `CameraMode.SystemOverview` and `CameraMode.FollowBody`, target, yaw, pitch, distance, and optional transition. Clamp pitch away from poles and distance to positive bounds. Mouse drag changes yaw/pitch, wheel changes distance exponentially, middle/right drag pans only in overview, and follow updates the target from the latest selected-body scene position. `ReturnToSystemView()` starts a smooth transition to the origin. Keep Raylib calls in the controller adapter; calculations use `System.Numerics`.

- [ ] **Step 5: Run tests and commit**

Run: `dotnet test PlanetSim.slnx`

Expected: PASS.

```bash
git add src/PlanetSim.Rendering/Camera src/PlanetSim.Rendering/Input tests/PlanetSim.Rendering.Tests
git commit -m "feat: add camera transitions and nearest body picking"
```

---

### Task 9: Texture Loading and Solar System Scene Rendering

**Files:**
- Create: `src/PlanetSim.Rendering/Assets/TextureCatalog.cs`
- Create: `src/PlanetSim.Rendering/Scene/SolarSystemRenderer.cs`
- Create: `src/PlanetSim.Rendering/assets/ATTRIBUTION.md`
- Create: `src/PlanetSim.Rendering/assets/textures/sun.png`
- Create: `src/PlanetSim.Rendering/assets/textures/mercury.png`
- Create: `src/PlanetSim.Rendering/assets/textures/venus.png`
- Create: `src/PlanetSim.Rendering/assets/textures/earth.png`
- Create: `src/PlanetSim.Rendering/assets/textures/moon.png`
- Create: `src/PlanetSim.Rendering/assets/textures/mars.png`
- Create: `src/PlanetSim.Rendering/assets/textures/jupiter.png`
- Create: `src/PlanetSim.Rendering/assets/textures/saturn.png`
- Create: `src/PlanetSim.Rendering/assets/textures/uranus.png`
- Create: `src/PlanetSim.Rendering/assets/textures/neptune.png`
- Modify: `src/PlanetSim.Rendering/PlanetSim.Rendering.csproj`

**Interfaces:**
- Consumes: `SimulationStatus`, trail positions, `SceneScale`, and camera.
- Produces: disposable `TextureCatalog.Load(...)` and `SolarSystemRenderer.Draw3D(...)`.

- [ ] **Step 1: Acquire redistribution-compatible texture assets and document them before code**

For every texture, record the exact title, creator/organization, source URL, license, downloaded filename, and any modifications in `assets/ATTRIBUTION.md`. Prefer NASA public-domain imagery; if a source has ambiguous redistribution terms, do not use it. Resize mechanically to power-of-two equirectangular PNGs no larger than 2048×1024. This task must not commit an asset without its attribution row.

- [ ] **Step 2: Configure assets to copy on build and publish**

Add:

```xml
<ItemGroup>
  <Content Include="assets/**/*">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
    <CopyToPublishDirectory>PreserveNewest</CopyToPublishDirectory>
  </Content>
</ItemGroup>
```

- [ ] **Step 3: Implement texture loading with fallback behavior**

`TextureCatalog.Load(definitions, assetRoot, Action<string> logWarning)` checks file existence and `Raylib.IsTextureValid`. It stores only valid GPU textures and invokes the warning sink once per failed body. `TryGet(BodyId, out Texture2D)` returns false on failure. `Dispose()` unloads each successfully loaded texture exactly once.

- [ ] **Step 4: Implement scene drawing**

`SolarSystemRenderer.Draw3D` performs, in order:

1. dark background and a deterministic static star field;
2. orbit trails as line strips transformed through `SceneScale`;
3. textured spheres when a texture exists, otherwise fallback-color spheres;
4. a visible selection outline for the selected body.

Generate one reusable UV sphere mesh with `GenMeshSphere`, create one material per loaded texture outside the frame loop, and draw instances with `DrawMesh`. Draw the Sun and planets with full-bright textured materials in the MVP; directional lighting and custom shaders remain outside this plan.

- [ ] **Step 5: Run build and perform a minimal window smoke test**

Run:

```bash
dotnet build PlanetSim.slnx
dotnet run --project src/PlanetSim.Rendering/PlanetSim.Rendering.csproj
```

Expected: a window opens, textures or fallback spheres render, and closing the window exits without native-resource warnings. At this task render `SimulationEngine.CreateSolarSystem().CreateSnapshot()` once in `Program.cs`; Task 11 removes that exact static path.

- [ ] **Step 6: Commit**

```bash
git add src/PlanetSim.Rendering
git commit -m "feat: render textured solar system with safe fallbacks"
```

---

### Task 10: HUD and Interactive Controls

**Files:**
- Create: `src/PlanetSim.Rendering/Ui/HudRenderer.cs`
- Modify: `src/PlanetSim.Rendering/Ui/EnglishStrings.cs`
- Modify: `src/PlanetSim.Rendering/PlanetSimGame.cs`

**Interfaces:**
- Consumes: `SimulationStatus`, current slider value, selected snapshot, and screen size.
- Produces: `HudActions` containing `TogglePause`, `Reset`, `ReturnToSystemView`, `ToggleTrails`, and optional `NewSliderValue`.

- [ ] **Step 1: Extract pure hit-testing and slider mapping helpers and test them**

Add tests proving that inclusive rectangle edges register clicks, slider X positions clamp to `[0,1]`, and a hidden help overlay does not consume clicks. Use framework-independent `UiRect` and `UiPoint` records in `HudRenderer.cs` so tests do not open Raylib.

- [ ] **Step 2: Implement the HUD layout and actions**

Draw translucent panels with Raylib primitives and `DrawText`. The top row shows pause state, elapsed simulated time, requested/effective rates, and FPS. Controls expose pause/resume, logarithmic slider, reset, trails, and system view. The selected panel shows name, kind, mass, diameter, Sun distance, speed, `Planet sizes enlarged ×500`, and `Sun size enlarged ×20`. Place all labels in `EnglishStrings`.

Return actions instead of directly mutating the controller. In `PlanetSimGame`, apply actions once per frame after input collection. Require confirmation only for reset if more than one simulated day has elapsed; a second click within three real seconds confirms it.

- [ ] **Step 3: Implement dismissible help overlay**

List left-drag orbit, middle/right-drag pan, wheel zoom, left-click select, `Space` pause, `R` reset, `O` trails, and `Esc` system view. Keyboard shortcuts dispatch the same `HudActions` path as pointer controls.

- [ ] **Step 4: Run automated tests and manual resize/input smoke test**

Run: `dotnet test PlanetSim.slnx`

Then run the app and verify controls at 1280×720 and after resizing to 800×600. Expected: panels remain on screen, clicks map to visible controls, and no literal English label exists outside `EnglishStrings` (verify with a code review, not a brittle grep test).

- [ ] **Step 5: Commit**

```bash
git add src/PlanetSim.Rendering tests/PlanetSim.Rendering.Tests
git commit -m "feat: add simulation HUD and interactive controls"
```

---

### Task 11: Compose the Live Game Loop

**Files:**
- Create: `src/PlanetSim.App/Composition.cs`
- Create: `src/PlanetSim.Rendering/PlanetSimGame.cs`
- Modify: `src/PlanetSim.Rendering/Program.cs`
- Modify: `src/PlanetSim.Rendering/PlanetSim.Rendering.csproj`
- Create: `tests/PlanetSim.App.Tests/Simulation/LiveCompositionTests.cs`

**Interfaces:**
- Consumes: all prior Core, App, and Rendering APIs.
- Produces: the final executable lifecycle and a headless composition factory for integration tests.

- [ ] **Step 1: Write a failing headless composition test**

```csharp
[Fact]
public void DefaultCompositionAdvancesAllTenBodiesAndResetRestoresInitialSnapshot()
{
    var controller = Composition.CreateController(maxStepsPerFrame: 2000);
    var initial = controller.Status.Snapshot;
    controller.SetSlider(1);
    controller.Update(1);
    var advanced = controller.Status.Snapshot;

    Assert.Equal(10, advanced.Bodies.Count);
    Assert.True(advanced.Elapsed > TimeSpan.Zero);
    Assert.Contains(advanced.Bodies.Zip(initial.Bodies), pair =>
        pair.First.PositionMeters != pair.Second.PositionMeters);

    controller.Reset();
    Assert.Equal(initial.Bodies, controller.Status.Snapshot.Bodies);
}
```

- [ ] **Step 2: Implement composition and game lifecycle**

`Program.Main` wraps startup in `try/catch`, writes fatal initialization errors to stderr, returns nonzero on failure, and applies `[STAThread]` for Windows NativeAOT compatibility. `PlanetSimGame.Run()` initializes a resizable 1280×720 window, sets 60 target FPS, loads textures, and loops until `WindowShouldClose()`.

Per frame:

1. read real frame delta and input;
2. translate UI/keyboard/mouse input into controller and camera commands;
3. update `SimulationController`;
4. update follow-camera target from the newest snapshot;
5. begin drawing, enter 3D mode, draw scene, exit 3D mode, draw HUD, end drawing;
6. dispose textures and close the window in `finally`.

When clicking outside HUD controls, obtain a Raylib mouse ray, convert it to `Ray3`, build pick spheres from rendered body radii, and call `BodyPicker.Pick`. Forward the result to `SimulationController.Select` and `CameraController.Follow`.

- [ ] **Step 3: Remove temporary static-scene composition**

Delete the Task 9 temporary snapshot path. Search for `CreateInitialState` usages in Rendering and ensure only the composition root creates the engine; the renderer must consume `SimulationStatus` exclusively.

- [ ] **Step 4: Run integration tests and app smoke test**

Run:

```bash
dotnet test PlanetSim.slnx
dotnet run --project src/PlanetSim.Rendering/PlanetSim.Rendering.csproj
```

Expected: ten bodies move, time controls affect physics, selection enters follow mode, trails accumulate, reset restores the original system and overview camera, and closing exits cleanly.

- [ ] **Step 5: Commit**

```bash
git add src tests
git commit -m "feat: compose interactive live solar system simulation"
```

---

### Task 12: Documentation, Publish Matrix, and Final Verification

**Files:**
- Create: `README.md`
- Create: `.github/workflows/ci.yml`
- Modify: `src/PlanetSim.Rendering/assets/ATTRIBUTION.md`
- Modify: `tests/PlanetSim.Core.Tests/Physics/SimulationEngineScenarioTests.cs`

**Interfaces:**
- Consumes: completed executable and tests.
- Produces: user/developer documentation, automated CI, and four verified publish outputs.

- [ ] **Step 1: Write README from verified behavior**

Include prerequisites (.NET 10 SDK and platform OpenGL requirements), restore/build/test/run commands, every mouse/keyboard control, physical model limitations, the 1,800-second step, time-rate behavior under load, distance/body scaling disclosures, fallback-texture behavior, and publish commands. Link `assets/ATTRIBUTION.md` and the design spec.

- [ ] **Step 2: Add cross-platform CI**

Create `.github/workflows/ci.yml` with `windows-latest`, `ubuntu-latest`, and `macos-latest`; use `actions/checkout@v5`, `actions/setup-dotnet@v5` with `dotnet-version: 10.0.x`, then `dotnet restore`, `dotnet build --no-restore --configuration Release`, and `dotnet test --no-build --configuration Release`.

- [ ] **Step 3: Run release build and the full test suite**

Run:

```bash
dotnet clean PlanetSim.slnx
dotnet restore PlanetSim.slnx
dotnet build PlanetSim.slnx --configuration Release --no-restore
dotnet test PlanetSim.slnx --configuration Release --no-build
```

Expected: zero warnings, zero failures, and scenario-test output within documented drift thresholds.

- [ ] **Step 4: Publish every target runtime**

Run:

```bash
dotnet publish src/PlanetSim.Rendering/PlanetSim.Rendering.csproj -c Release -r win-x64 --self-contained true -o artifacts/win-x64
dotnet publish src/PlanetSim.Rendering/PlanetSim.Rendering.csproj -c Release -r linux-x64 --self-contained true -o artifacts/linux-x64
dotnet publish src/PlanetSim.Rendering/PlanetSim.Rendering.csproj -c Release -r osx-x64 --self-contained true -o artifacts/osx-x64
dotnet publish src/PlanetSim.Rendering/PlanetSim.Rendering.csproj -c Release -r osx-arm64 --self-contained true -o artifacts/osx-arm64
```

Expected: all four commands succeed and each output includes the executable, Raylib native library, and `assets` directory. Do not commit `artifacts/`; add it to `.gitignore`.

- [ ] **Step 5: Perform platform smoke checklist**

On each available platform verify startup, orbit/pan/zoom, body selection, smooth following, pause, min/max speed, reset confirmation, trail toggle, window resize, texture fallback (temporarily rename one texture outside Git), and clean shutdown. Record unavailable platform checks in the final handoff instead of claiming them.

- [ ] **Step 6: Check spec coverage and repository cleanliness**

Run:

```bash
git status --short
git diff --check
rg -n "NotImplementedException|throw new NotSupportedException" src tests
```

Expected: only intentional final documentation changes are present, `git diff --check` is silent, and the scan has no unresolved production items.

- [ ] **Step 7: Commit final documentation and CI**

```bash
git add README.md .gitignore .github src/PlanetSim.Rendering/assets/ATTRIBUTION.md tests/PlanetSim.Core.Tests/Physics/SimulationEngineScenarioTests.cs
git commit -m "docs: add build publish and verification guidance"
```

- [ ] **Step 8: Run final verification after the commit**

Run:

```bash
dotnet test PlanetSim.slnx --configuration Release
git status --short
```

Expected: all tests pass and the working tree is clean.
