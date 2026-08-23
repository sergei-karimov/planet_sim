# PlanetSim

PlanetSim is an educational real-time 3D simulation of the Solar System. The Sun, eight planets, and Earth's Moon move under mutual Newtonian gravity; the visualization is driven by the calculated N-body trajectories rather than predefined orbital paths.

## Requirements

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- A desktop with OpenGL 3.3 support
- On Linux, the native desktop libraries required by raylib (X11, OpenGL, audio, and related runtime packages)

## Build, test, and run

```bash
dotnet restore PlanetSim.slnx
dotnet build PlanetSim.slnx --configuration Release --no-restore
dotnet test PlanetSim.slnx --configuration Release --no-build
dotnet run --project src/PlanetSim.Rendering/PlanetSim.Rendering.csproj
```

The application UI is currently English-only.

## Controls

| Input | Action |
| --- | --- |
| Left-drag empty space | Orbit the camera |
| Middle- or right-drag | Pan |
| Mouse wheel | Zoom |
| Left-click a body | Select and follow it |
| `Space` | Pause or resume |
| `O` | Show or hide trails |
| `R` | Reset; after one simulated day, press twice within three seconds |
| `Esc` | Return to the system overview |
| `H` | Show or hide help |

The HUD also provides pause, reset, trails, system-view, and logarithmic time-rate controls. Requested and effective rates are shown separately when the machine cannot keep up.

## Publish

Create framework-dependent platform bundles with:

```bash
dotnet publish src/PlanetSim.Rendering/PlanetSim.Rendering.csproj --configuration Release --runtime win-x64 --self-contained false --output artifacts/win-x64
dotnet publish src/PlanetSim.Rendering/PlanetSim.Rendering.csproj --configuration Release --runtime linux-x64 --self-contained false --output artifacts/linux-x64
dotnet publish src/PlanetSim.Rendering/PlanetSim.Rendering.csproj --configuration Release --runtime osx-x64 --self-contained false --output artifacts/osx-x64
dotnet publish src/PlanetSim.Rendering/PlanetSim.Rendering.csproj --configuration Release --runtime osx-arm64 --self-contained false --output artifacts/osx-arm64
```

The target machine must have the .NET 10 runtime installed. The `assets` directory is copied into every published bundle and must stay beside the executable.

## Model and visualization limits

- Physics uses SI units, double precision, Newtonian gravity, and a fixed 1,800-second velocity Verlet step.
- Initial conditions are approximate orbital parameters, not ephemerides for a real calendar date.
- Relativity, non-gravitational forces, atmospheres, rings, shadows, and collisions are outside the current scope.
- Scene distances use one unit per 10 million km. Planet and Moon diameters are enlarged ×500; the Sun is enlarged ×20. These size enlargements affect rendering and picking only, never physics.
- Trails record simulated positions every six simulated hours and are bounded to 8,192 samples per body.
- If a texture cannot be loaded, the body remains usable as a colored sphere and a warning is written to stderr.

Texture sources and licenses are documented in [the asset attribution file](src/PlanetSim.Rendering/assets/ATTRIBUTION.md). The full design and implementation plan are under [`docs/superpowers`](docs/superpowers).
