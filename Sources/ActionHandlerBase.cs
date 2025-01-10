namespace BandBrosClone;
#nullable enable

using Godot;
using MusicNotation;
using System;
using System.Collections.Generic;
using System.Linq;

public abstract partial class ActionHandlerBase : Node
{
    [Export] public PerformanceManager? performanceManager;

    private SoundfontPlayer _soundfontPlayer { get => performanceManager!.SoundfontPlayer; }

    public abstract MidiChannel Channel { get; set; }

    public Scale? Scale { get; set; } = Constants.DEFAULT_SCALE;

    private MidiNote?[] _currentPlayingMidiNote = new MidiNote?[Enum.GetValues<PerformanceActionKind>().Length];

    public PerformanceAction[] PerformingActions = new PerformanceAction[Enum.GetValues<PerformanceActionKind>().Length];

    [Signal]
    public delegate void OnActionEventHandler();

    public bool IsActionPressing(PerformanceAction action)
    {
        var _action = PerformingActions[(int)action.ActionKind];
        return _action is not null && !_action.IsJustPressed && !_action.IsJustReleased;
    }

    public bool IsActionJustPressed(PerformanceAction action)
    {
        return action.IsJustPressed;
    }

    public bool IsActionJustReleased(PerformanceAction action)
    {
        return action.IsJustReleased;
    }

    public void SetChannel(MidiChannel channel)
    {
        Channel = channel;
    }

    private void _playNoteWithInputAction(PerformanceAction action, PerformanceActionKind actionKind, MidiNote note)
    {
        if (action.IsJustPressed && action.ActionKind.Equals(actionKind))
        {
            if (_currentPlayingMidiNote[(int)actionKind] is not null)
            {
                _soundfontPlayer.PlayNoteOff(Channel, _currentPlayingMidiNote[(int)actionKind]);
                _currentPlayingMidiNote[(int)actionKind] = null;
            }

            _soundfontPlayer.PlayNoteOn(Channel, note.Note, note.Velocity);
            _currentPlayingMidiNote[(int)actionKind] = note;
        }
        else if (action.IsJustReleased && action.ActionKind.Equals(actionKind))
        {
            if (_currentPlayingMidiNote[(int)actionKind] is not null)
            {
                _soundfontPlayer.PlayNoteOff(Channel, _currentPlayingMidiNote[(int)actionKind]);
            }

            _soundfontPlayer.PlayNoteOff(Channel, note);
            _currentPlayingMidiNote[(int)actionKind] = null;
        }

    }

    private int _sharp = 0;
    private int _octave = 0;

    private void _modulateWithAction(PerformanceAction action)
    {
        if (action.IsJustPressed && action.ActionKind.Equals(PerformanceActionKind.SHARP))
        {
            _currentPlayingMidiNote[(int)PerformanceActionKind.SHARP] = new MidiNote(0);
            _sharp = 1;
        }
        if (action.IsJustReleased && action.ActionKind.Equals(PerformanceActionKind.SHARP))
        {
            _currentPlayingMidiNote[(int)PerformanceActionKind.SHARP] = null;
            _sharp = 0;
        }

        if (action.IsJustPressed && action.ActionKind.Equals(PerformanceActionKind.OCTAVE_UP))
        {
            _currentPlayingMidiNote[(int)PerformanceActionKind.OCTAVE_UP] = new MidiNote(0);
            _octave = 1;
        }
        if (action.IsJustReleased && action.ActionKind.Equals(PerformanceActionKind.OCTAVE_UP))
        {
            _currentPlayingMidiNote[(int)PerformanceActionKind.OCTAVE_UP] = null;
            _octave = 0;
        }
    }

    private void _onActionOn(PerformanceActionKind actionKind, int velocity)
    {
        PerformHandler(new PerformanceAction(actionKind, true, false, new MidiNoteVelocity(velocity)));
    }

    private void _onActionOff(PerformanceActionKind actionKind)
    {
        PerformHandler(new PerformanceAction(actionKind, false, true));
    }

    public void PerformHandler(PerformanceAction action)
    {
        PerformingActions[(int)action.ActionKind] = action;
        EmitSignal(SignalName.OnAction);
        var velocity = action.Velocity ?? new MidiNoteVelocity(100);

        if (action.ActionKind.Equals(PerformanceActionKind.SHARP) || action.ActionKind.Equals(PerformanceActionKind.OCTAVE_UP))
        {
            _modulateWithAction(action);
            return;
        }

        var noteNum = action.ToMidiNoteNumber(_sharp, _octave, Scale);
        var note = new MidiNote(noteNum, velocity);

        _playNoteWithInputAction(action, PerformanceActionKind.I, note);
        _playNoteWithInputAction(action, PerformanceActionKind.II, note);
        _playNoteWithInputAction(action, PerformanceActionKind.III, note);
        _playNoteWithInputAction(action, PerformanceActionKind.IV, note);
        _playNoteWithInputAction(action, PerformanceActionKind.V, note);
        _playNoteWithInputAction(action, PerformanceActionKind.VI, note);
        _playNoteWithInputAction(action, PerformanceActionKind.VII, note);
        _playNoteWithInputAction(action, PerformanceActionKind.VIII, note);
    }

    public void AllNotesOff()
    {
        //        for (int i = 0;  i < 127; i++)
        //        {
        //            _soundfontPlayer.PlayNoteOff(Channel, new MidiNote(i));
        //        }
        for (int i = 0; i < _currentPlayingMidiNote.Length; i++)
        {
            if (_currentPlayingMidiNote[i] is not null)
            {
                PerformHandler(new PerformanceAction((PerformanceActionKind)i, false, true));
            }
        }
    }
}