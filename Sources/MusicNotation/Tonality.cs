using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace BandBrosClone.MusicNotation;

public sealed record Scale(MidiNoteNumber Key, int I, int II, int III, int IV, int V, int VI, int VII, int VIII)
{
    public static Scale Major(MidiNoteNumber key) => new Scale(key, [0, 2, 4, 5, 7, 9, 11, 12]);
    public static Scale NaturalMinor(MidiNoteNumber key) => new Scale(key, [0, 2, 3, 5, 7, 8, 10, 12]);

    public readonly int Size = 8;

    public Scale(MidiNoteNumber key, int[] intervals) : this(key, intervals[0], intervals[1], intervals[2], intervals[3], intervals[4], intervals[5], intervals[6], intervals[7])
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
        return Key.Transpose(GetIntervalFromNumber(num));
    }

    // I ~ VIII のインターバル配列化 (mod 12 して使う場面が多い)
    private int[] Intervals => new[] { I, II, III, IV, V, VI, VII, VIII };

    /// <summary>
    /// このスケールが引数のノートを「含む」かどうか判定
    /// </summary>
    public bool Contains(MidiNoteNumber note)
    {
        // Key との相対半音差（0~11）を計算
        int diff = (note.Note - Key.Note) % 12;
        if (diff < 0) diff += 12;

        // スケールの各インターバルを mod 12 して比較
        return Intervals
            .Select(interval => interval % 12)
            .Contains(diff);
    }

    public static Scale? DetectScale(IEnumerable<MidiNoteNumber> notes)
    {
        if (notes.Count().Equals(0)) return null;

        var lowestNoteValue = notes
            .Select(n => n.Note)
            .Min();

        MidiNoteNumber actualKey = Constants.DEFAULT_SCALE.Key;

        for (int i = 0; i < 128; i += 12)
        {
            if (lowestNoteValue >= i && lowestNoteValue < i + 12)
            {
                actualKey = i;
                break;
            }
        }

        return Scale.Major(actualKey);
    }

}
