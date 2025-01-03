namespace BandBrosClone.MusicNotation;

public sealed record MidiInstrumet(int bank = 0, int program = 0);

public sealed record MidiTimeSignature(int numerator, int denominator);

/// <summary>
/// Represents a MIDI tempo in microseconds per quarter note.
/// </summary>
/// <param name="tempo"></param>
public sealed record MidiTempo(MidiTime microSecondsPerQuarterNote)
{
    public MidiTempo(long tempo) : this(new MidiTime(tempo)) { }
}

/// <summary>
/// Represents a MIDI time in microseconds.
/// </summary>
/// <param name="time"></param>
public sealed record MidiTime(long time)
{
    public MidiTime Add(MidiTime time) => new MidiTime(this.time + time.time);
    public MidiTime Add(long time) => new MidiTime(this.time + time);
}


public sealed record MidiChannel
{
    public int channel;
    public MidiChannel(int channel)
    {
        if (channel < 0 || channel > Constants.MAX_MIDI_CHANNEL)
        {
            throw new System.ArgumentOutOfRangeException("MIDI channel must be between 0 and 15.");
        }

        this.channel = channel;
    }

    public static implicit operator int(MidiChannel channel) => channel.channel;
}

public sealed record MidiNote(int Note, int Velocity = 100)
{
    public static implicit operator int(MidiNote note) => note.Note;

    public MidiNote Transpose(int semitones) => new MidiNote(Note + semitones);

    public MidiNote Sharp(int num) => new MidiNote(Note + num);
    public MidiNote Flat(int num) => new MidiNote(Note - num);

    public MidiNote ChangeOctave(int octave) => new MidiNote(Note + octave * 12);

    public static MidiNote C4 => new MidiNote(60);
    public static MidiNote D4 => new MidiNote(62);
    public static MidiNote E4 => new MidiNote(64);
    public static MidiNote F4 => new MidiNote(65);
    public static MidiNote G4 => new MidiNote(67);
    public static MidiNote A4 => new MidiNote(69);
    public static MidiNote B4 => new MidiNote(71);
    public static MidiNote C5 => new MidiNote(72);
}
