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

    public static void ReportError(string message)
    {
        Instance.EmitSignal(nameof(Logger.ErrorSignal), message);
        Instance.Quit();
    }

    public static void Log(string message)
    {
        Instance.EmitSignal(nameof(Logger.LogSignal), message);
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

    public override void _Ready()
    {
        LogSignal += Info;

        ErrorSignal += Error;
    }

    public void Info(string message)
    {
        GD.Print($"[INFO] [{Time.GetTimeStringFromSystem()}]: {message}");
    }

    public void Error(string message)
    {
        GD.PrintErr($"[ERROR] [{Time.GetTimeStringFromSystem()}]: {message}");
    }
}
