namespace BandBrosClone;

using BandBrosClone.MusicNotation;
using Godot;

public static class Constants
{
    public static readonly int MAX_MIDI_CHANNEL = 15;
    public static readonly int MAX_MIDI_CHANNEL_COUNT = MAX_MIDI_CHANNEL + 1;

    public static readonly int MAX_CHART_TRACK_COUNT = MAX_MIDI_CHANNEL_COUNT;

    public static readonly MidiTempo DEFAULT_TEMPO = new MidiTempo(90 * 1000 * 1000);

    public static readonly MidiTime DEFAULT_TICKS_PER_QUARTER_NOTE = new MidiTime(98);

    public static readonly MidiTimeSignature DEFAULT_TIME_SIGNATURE = new MidiTimeSignature(4, 4);

    public static readonly MidiNoteNumber DEFAULT_TONAL_KEY = MidiNoteNumber.C4;

    public static readonly Scale DEFAULT_SCALE = Scale.Major(DEFAULT_TONAL_KEY);

    public static readonly int RENDER_TO_BUFFER_RATE = ProjectSettings.GetSetting("audio/soundfont_player/render_to_buffer_rate").AsInt32();

    public static readonly string DEFAULT_SOUNDFONT = ResourceManager.GetResourceAbsPath(ProjectSettings.GetSetting("audio/soundfont_player/default_soundfont").AsString());

    public static readonly int SAMPLE_RATE = ProjectSettings.GetSetting("audio/soundfont_player/sample_rate").AsInt32();
}