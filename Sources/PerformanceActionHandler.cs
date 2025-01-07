namespace BandBrosClone;

using BandBrosClone.MusicNotation;
using Godot;
using System.Collections.Generic;

[GlobalClass]
public sealed partial class PerformanceActionHandler : ActionHandlerBase
{
    public override MidiChannel Channel { get; set; } = new MidiChannel(0);

}