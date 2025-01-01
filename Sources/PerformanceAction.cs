#nullable enable

namespace BandBrosClone;

using Godot;

using BandBrosClone.MusicNotation;

public static class PerformanceActionKindExtension
{
    public static PerformanceActionKind ToPerformanceActionKind(this string actionName)
    {
        return actionName switch
        {
            // L: ↓←↑→
            "L_DOWN" => PerformanceActionKind.I,
            "L_LEFT" => PerformanceActionKind.II,
            "L_UP" => PerformanceActionKind.III,
            "L_RIGHT" => PerformanceActionKind.IV,
            // R: ←↓→↑ (BYXA)
            "R_LEFT" => PerformanceActionKind.V,
            "R_DOWN" => PerformanceActionKind.VI,
            "R_RIGHT" => PerformanceActionKind.VII,
            "R_UP" => PerformanceActionKind.VIII,
            // SHARP: B_LEFT
            "B_LEFT" => PerformanceActionKind.SHARP,
            // OCTAVE_UP: B_RIGHT
            "B_RIGHT" => PerformanceActionKind.OCTAVE_UP,
            _ => throw new System.NotImplementedException(),
        };
    }

    public static string ToActionName(this PerformanceActionKind actionKind)
    {
        return actionKind switch
        {
            // L: ↓←↑→
            PerformanceActionKind.I => "L_DOWN",
            PerformanceActionKind.II => "L_LEFT",
            PerformanceActionKind.III => "L_UP",
            PerformanceActionKind.IV => "L_RIGHT",
            // R: ←↓→↑ (BYXA)
            PerformanceActionKind.V => "R_LEFT",
            PerformanceActionKind.VI => "R_DOWN",
            PerformanceActionKind.VII => "R_RIGHT",
            PerformanceActionKind.VIII => "R_UP",
            // SHARP: B_LEFT
            PerformanceActionKind.SHARP => "B_LEFT",
            // OCTAVE_UP: B_RIGHT
            PerformanceActionKind.OCTAVE_UP => "B_RIGHT",
            _ => throw new System.NotImplementedException(),
        };
    }
}

public enum PerformanceActionKind
{
    I,
    II,
    III,
    IV,

    V,
    VI,
    VII,
    VIII,

    // Modulation
    SHARP,
    OCTAVE_UP,
}

public sealed record PerformanceAction(PerformanceActionKind ActionKind, bool IsPressed, bool IsReleased)
{
    public bool IsActionPressed(PerformanceActionKind actionKind)
    {
        return IsPressed && ActionKind.Equals(actionKind);
    }

    public bool IsActionReleased(PerformanceActionKind actionKind)
    {
        return IsReleased && ActionKind.Equals(actionKind);
    }

    public string ToActionName()
    {
        return ActionKind.ToActionName();
    }

    public MidiNote ToMidiNote(int sharp = 0, int octave = 0, Scale? scale = null)
    {
        if (scale is null)
        {
            scale = new Scale(new TonalKey(MidiNote.C4), new int[] { 2, 2, 1, 2, 2, 2, 1 });
        }

        return ActionKind switch
        {
            PerformanceActionKind.I => scale.GetNotes(0).Sharp(sharp).ChangeOctave(octave),
            PerformanceActionKind.II => scale.GetNotes(1).Sharp(sharp).ChangeOctave(octave),
            PerformanceActionKind.III => scale.GetNotes(2).Sharp(sharp).ChangeOctave(octave),
            PerformanceActionKind.IV => scale.GetNotes(3).Sharp(sharp).ChangeOctave(octave),
            PerformanceActionKind.V => scale.GetNotes(4).Sharp(sharp).ChangeOctave(octave),
            PerformanceActionKind.VI => scale.GetNotes(5).Sharp(sharp).ChangeOctave(octave),
            PerformanceActionKind.VII => scale.GetNotes(6).Sharp(sharp).ChangeOctave(octave),
            PerformanceActionKind.VIII => scale.GetNotes(7).Sharp(sharp).ChangeOctave(octave),
            _ => throw new System.NotImplementedException(),
        };
    }
}

public abstract partial class PerformanceActionHandler : Node
{
    [Export] public SoundfontPlayer SoundfontPlayer { get; private set; }

    public MidiChannel Channel { get; set; } = new MidiChannel(0);

    public Scale? Scale { get; private set; } = null;

    public PerformanceActionHandler(SoundfontPlayer soundfontPlayer, MidiChannel? channel = null)
    {
        if (channel is not null)
        {
            this.Channel = channel;
        }

        this.SoundfontPlayer = soundfontPlayer;
    }

    private void _playNoteWithInputAction(PerformanceAction action, PerformanceActionKind actionKind, MidiNote note)
    {
        if (action.IsActionPressed(actionKind))
        {
            SoundfontPlayer.PlayNoteOn(Channel, note, 100);
        }
        if (action.IsActionReleased(actionKind))
        {
            SoundfontPlayer.PlayNoteOff(Channel, note);
        }
    }

    private int _sharp = 0;
    private int _octave = 0;

    private void _modulateWithAction(PerformanceAction action)
    {
        if (action.IsActionPressed(PerformanceActionKind.SHARP))
        {
            _sharp = 1;
        }
        if (action.IsActionReleased(PerformanceActionKind.SHARP))
        {
            _sharp = 0;
        }

        if (action.IsActionPressed(PerformanceActionKind.OCTAVE_UP))
        {
            _octave = 1;
        }
        if (action.IsActionReleased(PerformanceActionKind.OCTAVE_UP))
        {
            _octave = 0;
        }
    }

    public void PerformHandler(PerformanceAction action)
    {
        _modulateWithAction(action);

        _playNoteWithInputAction(action, PerformanceActionKind.I, action.ToMidiNote(_sharp, _octave, Scale));
        _playNoteWithInputAction(action, PerformanceActionKind.II, action.ToMidiNote(_sharp, _octave, Scale));
        _playNoteWithInputAction(action, PerformanceActionKind.III, action.ToMidiNote(_sharp, _octave, Scale));
        _playNoteWithInputAction(action, PerformanceActionKind.IV, action.ToMidiNote(_sharp, _octave, Scale));

        _playNoteWithInputAction(action, PerformanceActionKind.V, action.ToMidiNote(_sharp, _octave, Scale));
        _playNoteWithInputAction(action, PerformanceActionKind.VI, action.ToMidiNote(_sharp, _octave, Scale));
        _playNoteWithInputAction(action, PerformanceActionKind.VII, action.ToMidiNote(_sharp, _octave, Scale));
        _playNoteWithInputAction(action, PerformanceActionKind.VIII, action.ToMidiNote(_sharp, _octave, Scale));
    }
}