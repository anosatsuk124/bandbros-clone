namespace BandBrosClone.MusicNotation;

public sealed record TonalKey(MidiNote baseMidiNote);

public sealed record Scale(TonalKey key, int[] intervals)
{
    public MidiNote GetNotes(int num)
    {
        return key.baseMidiNote.Transpose(intervals[num]);
    }
}
