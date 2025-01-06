namespace BandBrosClone;

using BandBrosClone.MusicNotation;
using Godot;
using System;

public partial class Rest : NoteBase
{
    public override MidiBeat Beat { get; protected set; }

    public Rest(MidiBeat beat)
    {
        Beat = beat;
    }
}