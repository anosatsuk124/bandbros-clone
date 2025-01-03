namespace BandBrosClone;

using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    private Logger _logger { get; } = new Logger();

    public override void _Ready()
    {
        Instance = this;
        AddChild(_logger);
    }

    public static void Error(string message)
    {
        Instance.EmitSignal(nameof(Logger.ErrorSignal), message);
        Instance.Quit();
    }

    public static void Log(string message)
    {
        Instance.EmitSignal(nameof(Logger.LogSignal), message);
    }

    public static void Warn(string message)
    {
        Instance.EmitSignal(nameof(Logger.WarnSignal), message);
    }

    public void Quit()
    {
        GetTree().Quit();
    }
}

public partial class Logger : Node
{
    [Signal]
    public delegate void LogSignalEventHandler(string message);

    [Signal]
    public delegate void ErrorSignalEventHandler(string message);

    [Signal]
    public delegate void WarnSignalEventHandler(string message);

    public override void _Ready()
    {
        LogSignal += Info;

        ErrorSignal += Error;

        WarnSignal += Warn;
    }

    public void Info(string message)
    {
        GD.Print($"[INFO] [{Time.GetTimeStringFromSystem()}]: {message}");
    }

    public void Warn(string message)
    {
        GD.PushWarning($"[WARN] [{Time.GetTimeStringFromSystem()}]: {message}");
    }

    public void Error(string message)
    {
        GD.PushError($"[ERROR] [{Time.GetTimeStringFromSystem()}]: {message}");
    }
}
