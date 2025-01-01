namespace BandBrosClone;

using BandBrosClone.MusicNotation;
using Godot;

[GlobalClass]
public partial class PerformanceManager : Node
{
    public MidiChannel PlayerChannel { get => InputHandler.Channel; set => InputHandler.Channel = value; }

    public MidiInstrumet[] Instruments
    {
        get => SoundfontPlayer.Instruments;
        set => SoundfontPlayer.SetInstruments(value);
    }

    public SoundfontPlayer SoundfontPlayer { get; private set; }

    public InputHandler InputHandler { get; private set; }

    public PerformanceManager() : base()
    {
        SoundfontPlayer = new SoundfontPlayer(new SoundfontAudioStreamPlayer());
        InputHandler = new InputHandler(SoundfontPlayer);
    }

    public override void _Ready()
    {
        AddChild(SoundfontPlayer);
        AddChild(InputHandler);
    }
}
