namespace BandBrosClone;

using Godot;

public partial class GameManager : Node
{
    public static GameManager Instance { get; private set; }

    [Signal]
    public delegate void ErrorHandleSignalEventHandler(string message);

    public override void _Ready()
    {
        Instance = this;

        ErrorHandleSignal += _handlerError;
    }

    public static void ReportError(string message)
    {
        Instance.EmitSignal(nameof(ErrorHandleSignal), message);
    }

    private void _handlerError(string message)
    {
        GD.PushError(message);
        Quit();
    }

    public void Quit()
    {
        GetTree().Quit();
    }
}