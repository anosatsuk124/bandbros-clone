using System;

namespace BandBrosClone.MusicNotation;

public sealed record TonalKey(MidiNoteNumber baseMidiNote);

public sealed record Scale(TonalKey Key, int I, int II, int III, int IV, int V, int VI, int VII, int VIII)
{
    public static Scale Major(TonalKey key) => new Scale(key, [0, 2, 4, 5, 7, 9, 11, 12]);

    public readonly int Size = 8;

    public Scale(TonalKey key, int[] intervals) : this(key, intervals[0], intervals[1], intervals[2], intervals[3], intervals[4], intervals[5], intervals[6], intervals[7])
    {
        if (intervals.Length != 8)
        {
            throw new System.Exception("A scale must have 8 intervals.");
        }
    }

    public int GetIntervalFromNumber(int num)
    {
        return num switch
        {
            0 => I,
            1 => II,
            2 => III,
            3 => IV,
            4 => V,
            5 => VI,
            6 => VII,
            7 => VIII,
            _ => throw new System.Exception("Invalid interval number."),
        };
    }

    public MidiNoteNumber GetNotes(int num)
    {
        return Key.baseMidiNote.Transpose(GetIntervalFromNumber(num));
    }
}
