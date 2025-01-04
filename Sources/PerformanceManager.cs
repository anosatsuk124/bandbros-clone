namespace BandBrosClone;

using BandBrosClone.MusicNotation;
using Godot;

[GlobalClass]
public partial class PerformanceManager : Node
{
    public Scale Scale { get; set; } = Constants.DEFAULT_SCALE;

    public MidiChannel PlayerChannel { get; set; } = new MidiChannel(0);

    public MidiInstrumet[] Instruments
    {
        get => SoundfontPlayer.Instruments;
        set => SoundfontPlayer.SetInstruments(value);
    }

    public SoundfontPlayer SoundfontPlayer { get; private set; }
    public AudioStreamPlayer StreamPlayer { get; set; }

    public void Play() => SoundfontPlayer.Play();
    public void Stop() => SoundfontPlayer.Stop();
    public void TogglePlay() => SoundfontPlayer.Toggle();

    public void Reset() => SoundfontPlayer.Reset();

    public override void _Ready()
    {
        StreamPlayer = new SoundfontAudioStreamPlayer();
        AddChild(StreamPlayer);

        SoundfontPlayer = new SoundfontPlayer(StreamPlayer);
        AddChild(SoundfontPlayer);
    }

    public void SetInstrument(MidiChannel channel, int bank, int program = 0)
    {
        SoundfontPlayer.SetInstrument(channel, bank, program);
    }
}
