namespace BandBrosClone;

using BandBrosClone.MusicNotation;
using Godot;
using System;

public abstract partial class NoteBase : Node2D
{
    public abstract MidiTime Duration { get; protected set; }

}
