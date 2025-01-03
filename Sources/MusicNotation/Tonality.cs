using System;

namespace BandBrosClone.MusicNotation;

public sealed record TonalKey(MidiNote baseMidiNote);

public sealed record Scale
{
    public static Scale Major(TonalKey key) => new Scale(key, 0, 2, 4, 5, 7, 9, 11, 12);

    public TonalKey Key;
    public int[] Intervals;

    public Scale(TonalKey key, params int[] intervals)
    {
        if (intervals.Length != 8)
        {
            throw new System.Exception("A scale must have 8 intervals.");
        }

        Key = key;
        Intervals = intervals;
    }

    public MidiNote GetNotes(int num)
    {
        return Key.baseMidiNote.Transpose(Intervals[num]);
    }
}
