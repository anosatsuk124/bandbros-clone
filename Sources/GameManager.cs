namespace BandBrosClone;

using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    private static Logger _logger { get; } = new Logger();

    public override void _Ready()
    {
        Instance = this;
        AddChild(_logger);
    }

    public static void Error(string message)
    {
        _logger.Error(message);
        Instance.Quit();
    }

    public static void Info(string message)
    {
        _logger.Info(message);
    }

    public static void Warn(string message)
    {
        _logger.Warn(message);
    }

    public void Quit()
    {
        GetTree().Quit();
    }
}

public partial class Logger : Node
{
    public override void _Ready()
    {
    }

    public void Info(string message)
    {
        // GD.Print($"[INFO] [{Time.GetTimeStringFromSystem()}]: {message}");
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
