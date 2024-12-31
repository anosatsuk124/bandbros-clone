namespace BandBrosClone.MIDI;

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
