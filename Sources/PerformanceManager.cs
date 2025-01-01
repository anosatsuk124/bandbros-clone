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
    public AudioStreamPlayer StreamPlayer { get; set; }

    public InputHandler InputHandler { get; private set; }

    public override void _Ready()
    {
        StreamPlayer = new SoundfontAudioStreamPlayer();
        AddChild(StreamPlayer);

        SoundfontPlayer = new SoundfontPlayer(StreamPlayer);
        AddChild(SoundfontPlayer);

        InputHandler = new InputHandler(SoundfontPlayer);
        AddChild(InputHandler);
    }
}
