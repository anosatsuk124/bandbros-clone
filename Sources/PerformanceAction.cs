#nullable enable

namespace BandBrosClone;

using BandBrosClone.MusicNotation;
using System;


public static class PerformanceActionKindExtension
{
    public static PerformanceActionKind[] FromMidiNote(MidiNoteNumber note, Scale? scale = null)
    {
        if (scale is null)
        {
            scale = Scale.Major(Constants.DEFAULT_TONAL_KEY);
        }

        var actionKinds = Enum.GetValues<PerformanceActionKind>();

        for (int i = 0; i < scale.Size; i++)
        {
            if (note.Equals(scale.GetNotes(i)))
            {
                return new PerformanceActionKind[] { actionKinds[i] };
            }
            else if (note.Equals(scale.GetNotes(i).Transpose(1)))
            {
                return new PerformanceActionKind[] { PerformanceActionKind.SHARP, actionKinds[i] };
            }
            else if (note.Equals(scale.GetNotes(i).ChangeOctave(1)))
            {
                return new PerformanceActionKind[] { PerformanceActionKind.OCTAVE_UP, actionKinds[i] };
            }
            else if (note.Equals(scale.GetNotes(i).Transpose(1).ChangeOctave(1)))
            {
                return new PerformanceActionKind[] { PerformanceActionKind.SHARP, PerformanceActionKind.OCTAVE_UP, actionKinds[i] };
            }
        }

        return new PerformanceActionKind[] { };
    }

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
    I = 0,
    II = 1,
    III = 2,
    IV = 3,

    V = 4,
    VI = 5,
    VII = 6,
    VIII = 7,

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

    public MidiNoteNumber ToMidiNoteNumber(int sharp = 0, int octave = 0, Scale? scale = null)
    {
        if (scale is null)
        {
            scale = Scale.Major(Constants.DEFAULT_TONAL_KEY);
        }

        return ActionKind switch
        {
            PerformanceActionKind.I => scale.GetNotes(0).Transpose(sharp).ChangeOctave(octave),
            PerformanceActionKind.II => scale.GetNotes(1).Transpose(sharp).ChangeOctave(octave),
            PerformanceActionKind.III => scale.GetNotes(2).Transpose(sharp).ChangeOctave(octave),
            PerformanceActionKind.IV => scale.GetNotes(3).Transpose(sharp).ChangeOctave(octave),
            PerformanceActionKind.V => scale.GetNotes(4).Transpose(sharp).ChangeOctave(octave),
            PerformanceActionKind.VI => scale.GetNotes(5).Transpose(sharp).ChangeOctave(octave),
            PerformanceActionKind.VII => scale.GetNotes(6).Transpose(sharp).ChangeOctave(octave),
            PerformanceActionKind.VIII => scale.GetNotes(7).Transpose(sharp).ChangeOctave(octave),
            _ => throw new System.NotImplementedException(),
        };
    }
}
