using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace BandBrosClone.MusicNotation;

public sealed record Scale(MidiNoteNumber Key, int I, int II, int III, int IV, int V, int VI, int VII, int VIII)
{
    public int Octave { get => Key.Note / (Key.PitchClass + 1); }
    public static Scale Major(MidiNoteNumber key) => new Scale(key, [0, 2, 4, 5, 7, 9, 11, 12]);
    public static Scale NaturalMinor(MidiNoteNumber key) => new Scale(key, [0, 2, 3, 5, 7, 8, 10, 12]);

    public Scale UpdateKeySig(int keySig)
    {
        if (keySig < 0)
        {
            keySig = 12 + keySig;
        }
        return Scale.Major(new MidiNoteNumber(7 * keySig % 12).ChangeOctave(Octave));
    }

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

    // ------------------------------------------------------------------
    // DetectScale: 長調のみ
    //   1) 0～11 の各ピッチクラスでメジャースケールを生成 → スコアを計算
    //   2) スコア最高の pc を bestPc とする
    //   3) 実際のKeyは、
    //      a) 入力中の最も低いノート "lowestNote"
    //      b) Key が [lowestNote-12, lowestNote] に収まり、かつ Key % 12 == bestPc
    //      c) その中でできるだけ大きいKeyを採用 (lowestNote にできるだけ近い)
    // ------------------------------------------------------------------
    public static Scale? DetectScale(IEnumerable<MidiNoteNumber> notes)
    {
        if (notes is null || !notes.Any())
            return null;

        // (1) 全12ピッチクラスでスコアを計算
        int bestScore = -1;
        int bestPc = 0;
        for (int pc = 0; pc < 12; pc++)
        {
            var candidate = Major(new MidiNoteNumber(pc));
            // スコア = 含まれるノート数
            int score = notes.Count(n => candidate.Contains(n.Note));
            if (score > bestScore)
            {
                bestScore = score;
                bestPc = pc;
            }
        }

        // (2) 入力の最も低いノート値
        int lowestNoteVal = notes.Min(n => n.Note);

        // (3) Keyを "[lowestNoteVal - 12, lowestNoteVal]" の範囲で探し、
        //     かつピッチクラス == bestPc、かつできるだけ大きいKeyを使う
        //
        //   条件:
        //     - keyVal % 12 == bestPc
        //     - (lowestNoteVal - 12) <= keyVal <= lowestNoteVal
        //   の中から最大の keyVal を探す

        int possibleKey = int.MinValue;
        for (int k = lowestNoteVal; k >= lowestNoteVal - 12; k--)
        {
            if (k < 0) break; // 0未満になるとMIDIノート的には不正(ただしMIDI規格外になる場合も)

            if ((k % 12 + 12) % 12 == bestPc)
            {
                possibleKey = k;
                break; // 大きい方から下がっていくので最初に見つけたらそれが最大
            }
        }

        // もし見つからなかった場合(すべて -1 未満など？)
        // あるいはlowestNoteVal < 12で十分にキーが取れないケースなど
        // fallbackとして "bestPc" そのもの(=オクターブなし) で返す
        if (possibleKey == int.MinValue)
        {
            return null;
        }

        // (4) そのKeyでMajorを作って返す
        return Major(new MidiNoteNumber(possibleKey));
    }
}


public static class KrumhanslSchmuckler
{
    // ------------------------------------------------------------
    // (1) Krumhanslの論文等で使われる「長調」「短調」のプロファイル例
    //     以下は 12次元ベクトル(根音=0～11)の “Cメジャー” と “Cマイナー” に相当
    //     参考: Krumhansl, Cognitive Foundations of Musical Pitch (1990)
    //           ただし文献や実装例によって多少値が異なるものもある
    // ------------------------------------------------------------

    /// <summary> K-Sアルゴリズムで使われるCメジャーのプロファイル(12次元) </summary>
    private static readonly double[] MajorProfileC = new double[]
    {
            6.35,  2.23,  3.48,  2.33,  // C,C#,D,D#
            4.38,  4.09,  2.52,  5.19,  // E,F,F#,G
            2.39,  3.66,  2.29,  2.88   // G#,A,A#,B
    };

    /// <summary> K-Sアルゴリズムで使われるCナチュラルマイナーのプロファイル(12次元) </summary>
    private static readonly double[] MinorProfileC = new double[]
    {
            6.33,  2.68,  3.52,  5.38,  // C,C#,D,D#
            2.60,  3.53,  2.54,  4.75,  // E,F,F#,G
            3.98,  2.69,  3.34,  3.17   // G#,A,A#,B
    };

    /// <summary>
    /// (2) ピッチクラス分布を作る (出現回数 or 時間など)
    ///     ここでは「出現回数」の簡易実装
    /// </summary>
    /// <param name="notes">入力ノートの列挙</param>
    /// <returns>12次元ベクトル (index=0~11)</returns>
    private static double[] GetPitchClassDistribution(IEnumerable<MidiNoteNumber> notes)
    {
        var distribution = new double[12];
        foreach (var n in notes)
        {
            distribution[n.PitchClass]++;
        }

        // 正規化 (分母=合計)
        double sum = distribution.Sum();
        if (sum > 0)
        {
            for (int i = 0; i < 12; i++)
            {
                distribution[i] /= sum;
            }
        }

        return distribution;
    }

    /// <summary>
    /// (3) 与えられたスケール・プロファイル(例えばCメジャーのもの)を
    ///     root だけシフトしたベクトルを作る
    ///     root=0 → 変換なし, root=1 → 1半音上げ(C#メジャー相当), root=2 → Dメジャー相当, … 
    /// </summary>
    /// <param name="baseProfile">Cメジャー or Cナチュラルマイナーなどのベクトル</param>
    /// <param name="root">0~11</param>
    /// <returns>シフト後の12次元ベクトル</returns>
    private static double[] RotateProfile(double[] baseProfile, int root)
    {
        // 例: root=2 なら [0,1,2,3,4,5,6,7,8,9,10,11] -> [10,11,0,1,2,3,4,5,6,7,8,9] みたいに回転
        // ただし「先頭を root だけずらす」のか「末尾を root だけずらす」のかで若干の位相が変わるので注意
        // 下記は "Cメジャーがroot=0" → root=1(C#メジャー)なら「1つ下がる」形で回転させるサンプル
        var shifted = new double[12];
        for (int i = 0; i < 12; i++)
        {
            int index = (i + 12 - root) % 12;
            shifted[i] = baseProfile[index];
        }
        return shifted;
    }

    /// <summary>
    /// (4) 2つの12次元ベクトルの相関係数 (Pearson's correlation) を返す
    ///     ※スケールを取った後、Σ(x_i*y_i)/…
    /// </summary>
    private static double Correlation(double[] dist, double[] profile)
    {
        if (dist.Length != 12 || profile.Length != 12)
            throw new ArgumentException("Vector size must be 12.");

        double avgDist = dist.Average();
        double avgProf = profile.Average();

        double numerator = 0.0;
        double denomDist = 0.0;
        double denomProf = 0.0;

        for (int i = 0; i < 12; i++)
        {
            double xd = dist[i] - avgDist;
            double yd = profile[i] - avgProf;
            numerator += xd * yd;
            denomDist += xd * xd;
            denomProf += yd * yd;
        }

        double denom = Math.Sqrt(denomDist * denomProf);
        if (denom == 0.0) return 0.0; // 一方が定数ベクトルなどの場合
        return numerator / denom;
    }

    // ------------------------------------------------------------
    // (5) K-S アルゴリズムでキー判定
    //     - 入力ノートから 12次元分布(dist) を生成
    //     - 12キー * (メジャー/マイナー) = 24候補で相関係数を計算
    //     - 最も相関が高いキーを返す
    // ------------------------------------------------------------
    public static (int root, bool isMajor, double correlation) DetectKey(IEnumerable<MidiNoteNumber> notes)
    {
        // a) ピッチクラス分布を作る
        var dist = GetPitchClassDistribution(notes);

        double bestCorr = double.NegativeInfinity;
        int bestRoot = 0;
        bool bestIsMajor = true;

        // b) root=0~11, major/minor=2通り => 24候補
        for (int root = 0; root < 12; root++)
        {
            // メジャー
            {
                var majorProfile = RotateProfile(MajorProfileC, root);
                var c = Correlation(dist, majorProfile);
                if (c > bestCorr)
                {
                    bestCorr = c;
                    bestRoot = root;
                    bestIsMajor = true;
                }
            }
            // マイナー
            {
                var minorProfile = RotateProfile(MinorProfileC, root);
                var c = Correlation(dist, minorProfile);
                if (c > bestCorr)
                {
                    bestCorr = c;
                    bestRoot = root;
                    bestIsMajor = false;
                }
            }
        }

        return (bestRoot, bestIsMajor, bestCorr);
    }
}
