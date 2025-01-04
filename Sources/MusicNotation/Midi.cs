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
    public int channel { get; }

    public MidiChannel(int channel)
    {
        if (channel < 0 || channel > Constants.MAX_MIDI_CHANNEL)
        {
            throw new System.ArgumentOutOfRangeException("MIDI channel must be between 0 and 15.");
        }
    }

    public static implicit operator int(MidiChannel channel) => channel.channel;
}

public sealed record MidiNoteVelocity(int velocity = 100)
{
    public static implicit operator int(MidiNoteVelocity velocity) => velocity.velocity;
}

public sealed record MidiNoteNumber(int Note)
{
    public static implicit operator int(MidiNoteNumber note) => note.Note;

    public static implicit operator MidiNoteNumber(int note) => new MidiNoteNumber(note);

    public static MidiNoteNumber C4 => new MidiNoteNumber(60);
    public static MidiNoteNumber D4 => new MidiNoteNumber(62);
    public static MidiNoteNumber E4 => new MidiNoteNumber(64);
    public static MidiNoteNumber F4 => new MidiNoteNumber(65);
    public static MidiNoteNumber G4 => new MidiNoteNumber(67);
    public static MidiNoteNumber A4 => new MidiNoteNumber(69);
    public static MidiNoteNumber B4 => new MidiNoteNumber(71);
    public static MidiNoteNumber C5 => new MidiNoteNumber(72);

    public MidiNoteNumber Transpose(int semitones) => new MidiNoteNumber(Note + semitones);
    public MidiNoteNumber Sharp() => new MidiNoteNumber(Note + 1);
    public MidiNoteNumber Flat() => new MidiNoteNumber(Note - 1);
    public MidiNoteNumber ChangeOctave(int octave) => new MidiNoteNumber(Note + octave * 12);
}

public sealed record MidiNote(MidiNoteNumber Note, MidiNoteVelocity Velocity)
{
    public static implicit operator int(MidiNote note) => note.Note;

    public MidiNote(int Note) : this(new MidiNoteNumber(Note), new MidiNoteVelocity()) { }
    public MidiNote(int Note, int Velocity) : this(new MidiNoteNumber(Note), new MidiNoteVelocity(Velocity)) { }

    public MidiNote Transpose(int semitones) => this with { Note = Note.Transpose(semitones) };

    public MidiNote Sharp() => this with { Note = Note.Sharp() };
    public MidiNote Flat() => this with { Note = Note.Flat() };
    public MidiNote ChangeOctave(int octave) => this with { Note = Note.ChangeOctave(octave) };
}
