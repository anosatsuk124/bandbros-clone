namespace BandBrosClone.MusicNotation;

public sealed record MidiInstrumet(int bank, int program);

public sealed record MidiChannel
{
    private int _channel;
    public MidiChannel(int channel)
    {
        if (channel < 0 || channel > Constants.MAX_MIDI_CHANNEL)
        {
            throw new System.ArgumentOutOfRangeException("MIDI channel must be between 0 and 15.");
        }

        _channel = channel;
    }

    public static implicit operator int(MidiChannel channel) => channel._channel;
}

public sealed record MidiNote(int Note)
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
