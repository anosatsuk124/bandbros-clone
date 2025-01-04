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

    // --------------------------------------------------------------------
    // DetectScale:
    //   1) 0~11のすべてのピッチクラス×(メジャー,ナチュラルマイナー)を用意
    //   2) スコア(含まれるノート数)が最大のピッチクラス/スケール種を特定
    //   3) 入力にあるノートの中で「そのピッチクラスと一致」するもののうち、
    //      最も低いノート値をルート(Key)にしてスケールを作って返却
    // --------------------------------------------------------------------
    public static Scale? DetectScale(IEnumerable<MidiNoteNumber> notes)
    {
        if (notes is null) return null;

        // (1) すべての候補スケール (24通り: 12 pc × 2種)
        //     それぞれのスコアを計算
        int bestScore = -1;
        (int pitchClass, bool isMajor) bestCandidate = (0, true);

        for (int pc = 0; pc < 12; pc++)
        {
            // Major
            {
                var scale = Major(new MidiNoteNumber(pc));
                int score = notes.Count(n => scale.Contains(n.Note));
                if (score > bestScore)
                {
                    bestScore = score;
                    bestCandidate = (pc, true);
                }
            }
            // NaturalMinor
            {
                var scale = NaturalMinor(new MidiNoteNumber(pc));
                int score = notes.Count(n => scale.Contains(n.Note));
                if (score > bestScore)
                {
                    bestScore = score;
                    bestCandidate = (pc, false);
                }
            }
        }

        // (2) bestCandidate で特定されたピッチクラスを持つノートのうち、
        //     最も低いノート値を取得する
        var (bestPc, isMajorOrMinor) = bestCandidate;

        var matchingNotes = notes
            .Where(n => n.Note == bestPc)
            .Select(n => n.Note)
            .ToList();

        // もし "同じピッチクラス" を持つノートが一つもなかったら、
        // ピッチクラスだけわかったけど該当実音がない、という矛盾したケース。
        // その場合はオクターブなしの pc だけを Key にして返す
        if (!matchingNotes.Any())
        {
            var fallbackKey = new MidiNoteNumber(bestPc + MidiNoteNumber.C4);
            return isMajorOrMinor
                ? Major(fallbackKey)
                : NaturalMinor(fallbackKey);
        }

        // (3) "最も低い" ノート値を取る
        int lowestNoteValue = matchingNotes.Min();
        var actualKey = new MidiNoteNumber(lowestNoteValue);

        // (4) 実際のスケールを生成して返却
        return isMajorOrMinor
            ? Major(actualKey)
            : NaturalMinor(actualKey);
    }

}
