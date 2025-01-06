namespace BandBrosClone;
#nullable enable

using Godot;
using MusicNotation;
using System;

public abstract partial class ActionHandlerBase : Node
{
    [Export] public PerformanceManager? performanceManager;

    private SoundfontPlayer _soundfontPlayer { get => performanceManager!.SoundfontPlayer; }

    public abstract MidiChannel Channel { get; set; }

    public Scale? Scale { get; set; } = Constants.DEFAULT_SCALE;

    private MidiNote?[] _currentPlayingActions = new MidiNote?[Enum.GetValues<PerformanceActionKind>().Length];

    public void SetChannel(MidiChannel channel)
    {
        Channel = channel;
    }

    private void _playNoteWithInputAction(PerformanceAction action, PerformanceActionKind actionKind, MidiNote note)
    {
        if (action.IsActionPressed(actionKind))
        {
            if (_currentPlayingActions[(int)actionKind] is not null)
            {
                _soundfontPlayer.PlayNoteOff(Channel, _currentPlayingActions[(int)actionKind]);
                _currentPlayingActions[(int)actionKind] = null;
            }

            _soundfontPlayer.PlayNoteOn(Channel, note.Note, note.Velocity);
            _currentPlayingActions[(int)actionKind] = note;
        }
        if (action.IsActionReleased(actionKind))
        {
            if (_currentPlayingActions[(int)actionKind] is not null)
            {
                _soundfontPlayer.PlayNoteOff(Channel, _currentPlayingActions[(int)actionKind]);
            }

            _soundfontPlayer.PlayNoteOff(Channel, note);
            _currentPlayingActions[(int)actionKind] = null;
        }

        _modulateWithAction(new PerformanceAction(PerformanceActionKind.SHARP, false, true));
        _modulateWithAction(new PerformanceAction(PerformanceActionKind.OCTAVE_UP, false, true));
    }

    private int _sharp = 0;
    private int _octave = 0;

    private void _modulateWithAction(PerformanceAction action)
    {
        if (action.IsActionPressed(PerformanceActionKind.SHARP))
        {
            _currentPlayingActions[(int)PerformanceActionKind.SHARP] = new MidiNote(0);
            _sharp = 1;
        }
        if (action.IsActionReleased(PerformanceActionKind.SHARP))
        {
            _currentPlayingActions[(int)PerformanceActionKind.SHARP] = null;
            _sharp = 0;
        }

        if (action.IsActionPressed(PerformanceActionKind.OCTAVE_UP))
        {
            _currentPlayingActions[(int)PerformanceActionKind.OCTAVE_UP] = new MidiNote(0);
            _octave = 1;
        }
        if (action.IsActionReleased(PerformanceActionKind.OCTAVE_UP))
        {
            _currentPlayingActions[(int)PerformanceActionKind.OCTAVE_UP] = null;
            _octave = 0;
        }
    }

    private void _onActionOn(PerformanceActionKind actionKind, int velocity)
    {
        PerformHandler(new PerformanceAction(actionKind, true, false), new MidiNoteVelocity(velocity));
    }

    private void _onActionOff(PerformanceActionKind actionKind)
    {
        PerformHandler(new PerformanceAction(actionKind, false, true), new MidiNoteVelocity());
    }

    public void PerformHandler(PerformanceAction action, MidiNoteVelocity? velocity = null)
    {
        if (velocity is null)
        {
            velocity = new MidiNoteVelocity();
        }
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
        for (int i = 0; i < _currentPlayingActions.Length; i++)
        {
            if (_currentPlayingActions[i] is not null)
            {
                PerformHandler(new PerformanceAction((PerformanceActionKind)i, false, true));
            }
        }
    }
}