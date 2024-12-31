namespace BandBrosClone;
using Godot;

public static class ResourceManager
{
    public static string GetResourceAbsPath(string path)
    {
        if (OS.HasFeature("editor"))
        {
            return ProjectSettings.GlobalizePath("res://resources/" + path);
        }
        else
        {
            return OS.GetExecutablePath().GetBaseDir().PathJoin("resources").PathJoin(path);
        }
    }

    public static string GetSoundfontAbsPath(string path) => GetResourceAbsPath("soundfonts/" + path);
}
