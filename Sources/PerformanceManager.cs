namespace BandBrosClone;

using BandBrosClone.MusicNotation;
using Godot;

[GlobalClass]
public partial class PerformanceManager : Node
{
    public const double DEFAULT_DELTA_OFFSET = -5;
    public Scale Scale { get; set; } = Constants.DEFAULT_SCALE;

    public MidiChannel PlayerChannel { get; set; } = new MidiChannel(0);

    public double DeltaTime { get; private set; } = 0;

    public MidiInstrumet[] Instruments
    {
        get => SoundfontPlayer.Instruments;
        set => SoundfontPlayer.SetInstruments(value);
    }

    public SoundfontPlayer SoundfontPlayer { get; private set; }
    public AudioStreamPlayer StreamPlayer { get; set; }

    public ulong CurrentTimeUsec { get => Time.GetTicksUsec() - _startTimeUsec; }

    private ulong _startTimeUsec;

    public void Play() => SoundfontPlayer.Play();
    public void Stop() => SoundfontPlayer.Stop();
    public void TogglePlay() => SoundfontPlayer.Toggle();

    public void Reset()
    {
        SoundfontPlayer.Reset();
        _startTimeUsec = Time.GetTicksUsec();
        SetDeltaZero();
    }

    public override void _PhysicsProcess(double delta)
    {
        DeltaTime += delta;
    }

    public override void _Ready()
    {
        StreamPlayer = new SoundfontAudioStreamPlayer();
        AddChild(StreamPlayer);

        SoundfontPlayer = new SoundfontPlayer(StreamPlayer);
        AddChild(SoundfontPlayer);

        _startTimeUsec = Time.GetTicksUsec();
    }

    public void SetInstrument(MidiChannel channel, int bank, int program = 0)
    {
        SoundfontPlayer.SetInstrument(channel, bank, program);
    }

    public void SetDeltaZero(double offset = DEFAULT_DELTA_OFFSET)
    {
        DeltaTime = DEFAULT_DELTA_OFFSET;
    }
}
