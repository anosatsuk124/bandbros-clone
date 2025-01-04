namespace BandBrosClone;

using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class ChartSequencer : IEnumerable<(int, ChartNote)>
{
    //TODO: Implement BPM, Time Signature, and Scale changes

    public Chart Chart { get; private set; }

    public ChartSequencer(Chart chart)
    {
        this.Chart = chart;
    }

    public IEnumerator<(int, ChartNote)> GetEnumerator()
    {
        foreach (var (idx, track) in Chart.Tracks.Select((track, idx) => (idx, track)))
        {
            foreach (var note in track.Notes)
            {
                yield return (idx, note);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}