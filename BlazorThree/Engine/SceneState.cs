namespace BlazorThree.Engine;

public sealed class SceneState
{
    public CameraState Camera { get; set; } = new();

    public LightState Light { get; set; } = new();

    public OrbitControlsState OrbitControls { get; set; } = new();

    public IReadOnlyList<GroupState> Groups { get; set; } = Array.Empty<GroupState>();

    public IReadOnlyList<TransitionState> Transitions { get; set; } = Array.Empty<TransitionState>();

    public IReadOnlyList<TimelineState> Timelines { get; set; } = Array.Empty<TimelineState>();

    public IReadOnlyList<MeshState> Meshes { get; set; } = Array.Empty<MeshState>();
}

public sealed class CameraState
{
    public double Fov { get; set; } = 75;

    public double PositionX { get; set; } = 0;

    public double PositionY { get; set; } = 1;

    public double PositionZ { get; set; } = 5;
}

public sealed class LightState
{
    public LightType Type { get; set; } = LightType.Directional;

    public string Color { get; set; } = "#ffffff";

    public double Intensity { get; set; } = 1;

    public double PositionX { get; set; } = 4;

    public double PositionY { get; set; } = 6;

    public double PositionZ { get; set; } = 8;
}

public sealed class MeshState
{
    public required string Id { get; set; }

    public string? ParentId { get; set; }

    public required object Geometry { get; set; }

    public required object Material { get; set; }

    public OutlineState? Outline { get; set; }

    public string? ClassName { get; set; }

    public double PositionX { get; set; }

    public double PositionY { get; set; }

    public double PositionZ { get; set; }

    public double RotationX { get; set; }

    public double RotationY { get; set; }

    public double RotationZ { get; set; }

    public double ScaleX { get; set; } = 1;

    public double ScaleY { get; set; } = 1;

    public double ScaleZ { get; set; } = 1;
}

public sealed class GroupState
{
    public required string Id { get; set; }

    public string? ParentId { get; set; }

    public string? ClassName { get; set; }

    public double PositionX { get; set; }

    public double PositionY { get; set; }

    public double PositionZ { get; set; }

    public double RotationX { get; set; }

    public double RotationY { get; set; }

    public double RotationZ { get; set; }

    public double ScaleX { get; set; } = 1;

    public double ScaleY { get; set; } = 1;

    public double ScaleZ { get; set; } = 1;
}

public sealed class OutlineState
{
    public string Color { get; set; } = "#ffffff";

    public double Opacity { get; set; } = 1;
}

public sealed class OrbitControlsState
{
    public bool Enabled { get; set; }

    public bool EnableDamping { get; set; } = true;

    public double DampingFactor { get; set; } = 0.08;
}

public sealed class TransitionState
{
    public required string ClassName { get; set; }

    public int DurationMs { get; set; } = 650;

    public string Easing { get; set; } = "easeInOutQuad";

    public double? PositionX { get; set; }

    public double? PositionY { get; set; }

    public double? PositionZ { get; set; }

    public double? RotationX { get; set; }

    public double? RotationY { get; set; }

    public double? RotationZ { get; set; }

    public double? ScaleX { get; set; }

    public double? ScaleY { get; set; }

    public double? ScaleZ { get; set; }
}

public sealed class TimelineState
{
    public required string Name { get; set; }

    public bool IsActive { get; set; } = true;

    public bool Loop { get; set; }

    public double CurrentTimeMs { get; set; }

    public IReadOnlyList<TimelineTrackState> Tracks { get; set; } = Array.Empty<TimelineTrackState>();
}

public sealed class TimelineTrackState
{
    public required string Id { get; set; }

    public required string ClassName { get; set; }

    public string Easing { get; set; } = "linear";

    public IReadOnlyList<TimelineKeyframeState> Keyframes { get; set; } = Array.Empty<TimelineKeyframeState>();
}

public sealed class TimelineKeyframeState
{
    public required string Id { get; set; }

    public double TimeMs { get; set; }

    public double? PositionX { get; set; }

    public double? PositionY { get; set; }

    public double? PositionZ { get; set; }

    public double? RotationX { get; set; }

    public double? RotationY { get; set; }

    public double? RotationZ { get; set; }

    public double? ScaleX { get; set; }

    public double? ScaleY { get; set; }

    public double? ScaleZ { get; set; }
}
