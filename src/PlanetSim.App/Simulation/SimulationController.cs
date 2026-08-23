using PlanetSim.App.Selection;
using PlanetSim.App.Trails;
using PlanetSim.Core.Math;
using PlanetSim.Core.Model;
using PlanetSim.Core.Physics;

namespace PlanetSim.App.Simulation;

public sealed class SimulationController
{
    private readonly ISimulationEngine _engine;
    private readonly int _maxStepsPerFrame;
    private readonly SelectionState _selection = new();
    private readonly TrailRecorder _trailRecorder = new(TimeSpan.FromHours(6), 8_192);
    private double _accumulatedSimulatedSeconds;
    private TimeRate _timeRate = TimeRate.FromSlider(0.5);
    private bool _isPaused;
    private bool _trailsVisible = true;
    private SimulationSnapshot _lastValidSnapshot;
    private string? _errorMessage;
    private double _effectiveRate;

    public SimulationController(ISimulationEngine engine, int maxStepsPerFrame)
    {
        if (maxStepsPerFrame <= 0) throw new ArgumentOutOfRangeException(nameof(maxStepsPerFrame));
        _engine = engine;
        _maxStepsPerFrame = maxStepsPerFrame;
        _lastValidSnapshot = engine.CreateSnapshot();
    }

    public SimulationStatus Status => new(
        _lastValidSnapshot, _isPaused, _timeRate.SliderValue,
        _timeRate.SimulatedSecondsPerRealSecond, _effectiveRate,
        _selection.SelectedBody, _trailsVisible, _errorMessage);

    public IReadOnlyDictionary<BodyId, IReadOnlyList<Vector3d>> TrailPositions =>
        _trailRecorder.Trails.ToDictionary(pair => pair.Key,
            pair => (IReadOnlyList<Vector3d>)pair.Value.Positions);

    public void SetSlider(double value) => _timeRate = TimeRate.FromSlider(value);
    public void TogglePause() { _isPaused = !_isPaused; _effectiveRate = 0; }
    public void Select(BodyId body) => _selection.Select(body);
    public void ReturnToSystemView() => _selection.ReturnToSystemView();
    public void ToggleTrails() => _trailsVisible = !_trailsVisible;

    public void Update(double realDeltaSeconds)
    {
        if (!double.IsFinite(realDeltaSeconds) || realDeltaSeconds < 0)
            throw new ArgumentOutOfRangeException(nameof(realDeltaSeconds));
        if (_isPaused || realDeltaSeconds == 0) { _effectiveRate = 0; return; }

        _accumulatedSimulatedSeconds += realDeltaSeconds * _timeRate.SimulatedSecondsPerRealSecond;
        var due = (int)System.Math.Min(int.MaxValue,
            System.Math.Floor(_accumulatedSimulatedSeconds / _engine.FixedStepSeconds));
        var steps = System.Math.Min(due, _maxStepsPerFrame);
        var completed = 0;
        try
        {
            for (; completed < steps; completed++) _engine.Step();
            if (completed > 0)
            {
                var previous = _lastValidSnapshot;
                _lastValidSnapshot = _engine.CreateSnapshot();
                _trailRecorder.Record(previous, _lastValidSnapshot);
            }
        }
        catch (Exception exception) when (exception is InvalidOperationException or ArithmeticException)
        {
            _isPaused = true;
            _errorMessage = exception.Message;
        }

        _accumulatedSimulatedSeconds -= completed * _engine.FixedStepSeconds;
        _accumulatedSimulatedSeconds = System.Math.Min(
            _accumulatedSimulatedSeconds, _maxStepsPerFrame * _engine.FixedStepSeconds);
        _effectiveRate = completed * _engine.FixedStepSeconds / realDeltaSeconds;
    }

    public void Reset()
    {
        _engine.Reset();
        _lastValidSnapshot = _engine.CreateSnapshot();
        _selection.ReturnToSystemView();
        _trailRecorder.Clear();
        _accumulatedSimulatedSeconds = 0;
        _effectiveRate = 0;
        _errorMessage = null;
        _isPaused = false;
    }
}
