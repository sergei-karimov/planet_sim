# Solar System Simulator Design

## 1. Purpose and Scope

PlanetSim is a cross-platform educational 3D simulation of the Solar System. It models the mutual gravitational interaction of the Sun, eight planets, and Earth's Moon in real time. The application prioritizes clear visualization and numerically stable, physically meaningful motion over ephemeris-grade astronomical accuracy.

The MVP provides a fixed set of celestial bodies. Users can inspect and follow bodies, control simulation speed, pause or reset the simulation, and display trails of the paths produced by the N-body calculation. Editing, adding, or removing bodies is outside the MVP.

## 2. Technology and Platforms

- Language: C# 14
- Runtime: .NET 10
- Rendering and input: Raylib 6 through Raylib-cs
- Platforms: Windows, Linux, and macOS
- UI language: English, with strings isolated for future localization

The application is published separately for Windows x64, Linux x64, macOS x64, and macOS arm64. Installers and automatic updates are outside the MVP.

## 3. Architecture

The solution is a modular monolith consisting of four projects:

### `PlanetSim.Core`

A Raylib-independent physics library. It owns physical units, celestial-body definitions, simulation state, gravitational acceleration calculation, and numerical integration. It exposes immutable snapshots for consumers.

### `PlanetSim.App`

Application orchestration. It owns pause and reset behavior, the requested time scale, the selected body, the physics-step scheduler, and orbit-trail sampling. Physics advances by fixed internal time steps and is not tied to rendering frame rate.

### `PlanetSim.Rendering`

The Raylib integration. It owns the window, frame loop, mouse and keyboard input, camera behavior, texture loading, coordinate conversion, spheres, lighting, trails, and on-screen UI. It consumes application snapshots and does not modify physics state directly.

### `PlanetSim.Tests`

Automated tests for the physics library and application logic. Tests do not require a graphical window.

The primary data flow is:

```text
user input -> application state -> fixed physics steps -> immutable snapshot -> rendering
```

Dependencies point inward: Rendering depends on App and Core; App depends on Core; Core has no dependency on either application or rendering code.

## 4. Celestial-Body Model

The fixed catalog contains:

- Sun
- Mercury
- Venus
- Earth
- Moon
- Mars
- Jupiter
- Saturn
- Uranus
- Neptune

Each body has:

- a stable identifier;
- an English display name;
- a body type;
- mass in kilograms;
- physical diameter in meters;
- three-dimensional position in meters;
- three-dimensional velocity in meters per second;
- rendering metadata, including a fallback color and texture asset identifier.

Physics uses double-precision floating-point values and SI units. Initial positions and velocities are derived from approximate real orbital parameters, including orbital inclinations and initial phases. The initial state is designed to produce recognizable, stable orbits but does not claim to match published ephemerides for a particular calendar date.

The application validates the body catalog at startup. Mass and diameter must be positive, identifiers must be unique, and all vector components must be finite.

## 5. N-Body Physics

Every body gravitationally interacts with every other body. Acceleration is calculated pairwise using Newtonian gravity. The implementation preserves equal and opposite pair contributions to reduce systematic momentum drift.

The simulation uses a symplectic velocity Verlet integrator with a fixed internal step of 1,800 simulated seconds. Increasing the displayed simulation speed increases the number of fixed physics steps rather than enlarging the integration step. Stability tests must validate this step before release; a change to it requires updating the documented drift baselines.

The application limits the amount of physics work performed during one rendered frame. If the requested rate cannot be sustained, it reports the effective achieved rate instead of silently increasing the integration step or allowing an unbounded backlog.

Runtime physics invariants include finite positions, velocities, and accelerations. If an invalid numeric state appears, the application pauses the simulation, reports the failure, and retains the last valid snapshot for display.

## 6. Time Controls

The simulation provides:

- pause and resume;
- a logarithmic speed slider;
- reset to the initial state;
- displayed elapsed simulated time;
- displayed requested and effective simulation rates.

The slider range is 1 simulated second per real second through 30 simulated days per real second. If the machine cannot sustain the requested maximum, the scheduler retains the 1,800-second integration step, caps per-frame work, and reports the lower effective rate. Reverse time and single-step controls are outside the MVP.

Reset restores the initial body state, elapsed time, orbit trails, and default selection. Camera reset behavior is explicit: reset returns to the system overview so the visual state agrees with the restored simulation state.

## 7. Spatial and Visual Scaling

Physical calculations never use rendered coordinates. Rendering converts meter-based positions to a linearly scaled scene coordinate system.

Distances retain a single linear scale throughout the scene, with one scene unit representing 10 million kilometers. Planet and Moon diameters are enlarged by a factor of 500 so the bodies remain selectable and visible. The Sun uses a factor of 20 to avoid obscuring the inner system. The UI always displays both disclosures: `Planet sizes enlarged ×500` and `Sun size enlarged ×20`.

Celestial bodies are rendered as textured spheres. The Sun uses a bright unlit or emissive-like material. Planets use a simple lighting model oriented toward the Sun. Physically accurate shadows, atmospheres, rings, and post-processing effects are outside the MVP.

The background is a dark star field that does not participate in physical scaling.

## 8. Camera and Selection

The camera has two modes:

### System overview

- orbits around the system origin by mouse dragging;
- zooms with the mouse wheel;
- supports mouse-based panning;
- frames the useful extent of the Solar System at its default position.

### Follow body

- begins after the user clicks a rendered body;
- smoothly moves its target to the selected body;
- follows the body's changing position;
- orbits and zooms relative to that body;
- preserves the user's view direction where practical.

Mouse selection casts a ray into the scene and tests it against the visually enlarged sphere. If multiple spheres intersect the ray, the nearest positive hit is selected. Selection emphasizes both the body and its trail.

`Esc` and a `System View` button leave follow mode and restore the system overview. Selecting an already selected body does not reset the current camera angle.

## 9. Orbit Trails

An orbit line is the historical path actually produced by the N-body simulation, not a predefined ellipse or a predicted trajectory.

Each planet and the Moon owns a ring buffer of 8,192 sampled positions. Samples are added every six simulated hours, so trail density does not depend on rendering FPS or the selected time scale. Linear interpolation places a sample at the interval boundary when one physics step crosses it.

Users can globally show or hide trails. The selected body's trail is emphasized. The Sun's barycentric trail is supported by the data model but hidden by default. Reset clears all trail buffers.

## 10. User Interface

The MVP uses lightweight immediate-mode drawing through Raylib and does not add a separate UI framework.

The top status area shows:

- paused or running state;
- elapsed simulated time;
- requested and effective time rates;
- frames per second.

The controls provide:

- pause or resume;
- logarithmic time-rate slider;
- reset;
- trail visibility toggle;
- system-view action.

The selected-body panel shows:

- name;
- body type;
- mass;
- physical diameter;
- current distance from the Sun;
- current speed;
- active visual diameter enlargement.

A dismissible help overlay explains mouse and keyboard controls. Values use readable units: kilometers, astronomical units, kilometers per second, and scientific notation for mass.

All user-facing strings are accessed through a dedicated string catalog rather than being scattered through rendering logic. Only English resources ship in the MVP.

## 11. Assets and Degraded Behavior

Textures ship with the application. Their sources and licenses are recorded in `assets/ATTRIBUTION.md`.

If a texture is absent, corrupt, or unsupported, the affected body uses its configured fallback color and the application logs a warning. A missing nonessential asset must not prevent startup. Rendering features that cannot initialize should fall back to the simplest usable representation when possible.

Failure to initialize the Raylib window or native library is fatal and produces a concise error on stderr. Invalid initial physics data is also fatal because continuing would make simulation results meaningless.

## 12. Testing Strategy

### Unit tests

- vector and unit calculations;
- pairwise gravitational acceleration and force symmetry;
- velocity Verlet stepping;
- physical-to-render coordinate scaling;
- logarithmic slider mapping;
- human-readable unit formatting;
- ring-buffer behavior.

### Scenario tests

- a two-body circular-orbit approximation;
- the Sun-Earth-Moon subsystem;
- a long-running full-system stability scenario;
- bounded drift of total energy and angular momentum;
- finite values throughout the run.

Acceptance tolerances are defined numerically in the tests after baseline experiments; tests must explain why each tolerance is appropriate and must not rely on exact floating-point equality.

### Application tests without a window

- pause and resume;
- requested versus effective time rate;
- reset semantics;
- selection state;
- trail sampling based on simulated time;
- safe pause after an invalid physics state.

### Manual smoke tests

Windows, Linux, and macOS smoke tests cover startup, camera controls, selection, body following, speed control, reset, trail visibility, fallback textures, resizing, and clean shutdown. Screenshot-regression testing is outside the MVP.

## 13. MVP Acceptance Criteria

The MVP is complete when:

1. The solution builds with .NET 10 and all automated tests pass.
2. Published builds start on Windows x64, Linux x64, macOS x64, and macOS arm64.
3. The Sun, eight planets, and Moon move solely from mutual N-body gravity after initialization.
4. Long-run automated scenarios remain finite and within documented energy and angular-momentum drift limits.
5. Mouse controls support orbiting, panning, zooming, and selecting bodies.
6. Selection smoothly enters follow mode, and the system view can be restored.
7. The selected-body panel displays consistent physical values and disclosed visual scaling.
8. Pause, logarithmic speed control, and reset behave predictably.
9. Historical orbit trails can be shown and hidden and are independent of rendering FPS.
10. Missing individual textures degrade to colored spheres without terminating the application.
11. README documentation covers prerequisites, build, test, publish, controls, numerical-model limitations, and visual scaling.
12. All distributed assets have source and license attribution.

## 14. Explicit Non-Goals

The MVP does not include:

- ephemeris-grade positions tied to a real date;
- general relativity or non-gravitational forces;
- body creation, deletion, or parameter editing;
- reverse time;
- predicted trajectories or predefined orbit ellipses;
- detailed atmospheres, rings, shadows, or collision physics;
- spacecraft or maneuver planning;
- save files or session persistence;
- installers, automatic updates, or web/mobile builds;
- automatic visual screenshot comparisons.
