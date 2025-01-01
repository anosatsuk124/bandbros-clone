namespace BandBrosClone;

using Godot;

public static class Constants
{
    public static readonly int MAX_MIDI_CHANNEL = 15;
    public static readonly int MAX_MIDI_CHANNEL_COUNT = MAX_MIDI_CHANNEL + 1;

    public static readonly string DEFAULT_SOUNDFONT = ResourceManager.GetSoundfontAbsPath(ProjectSettings.GetSetting("audio/soundfont_player/default_soundfont").AsString());

    public static readonly int SAMPLE_RATE = ProjectSettings.GetSetting("audio/soundfont_player/sample_rate").AsInt32();
}