namespace BandBrosClone;

using System.Collections;
using System.Collections.Generic;

public partial class ChartSequencer : IEnumerable<ChartNote>
{
    //TODO: Implement BPM, Time Signature, and Scale changes

    public Chart Chart { get; private set; }

    public ChartSequencer(Chart chart)
    {
        this.Chart = chart;
    }

    public IEnumerator<ChartNote> GetEnumerator()
    {
        foreach (var track in Chart.Tracks)
        {
            foreach (var note in track.Notes)
            {
                yield return note;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}