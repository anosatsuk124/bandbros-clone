namespace BandBrosClone;

using BandBrosClone.Utils;
using Godot;

public static class ResourceManager
{
    public static string GetResourceAbsPath(string path)
    {
        var resourcePrefix = "res://";
        var userPrefix = "user://";

        var userPath = userPrefix + path;
        var resourcePath = resourcePrefix + path;

        if (OS.HasFeature("editor"))
        {
            return ProjectSettings.GlobalizePath(resourcePath);
        }
        else
        {
            if (FileAccess.FileExists(userPath))
            {
                return ProjectSettings.GlobalizePath(userPath);
            }

            if (!DirAccess.DirExistsAbsolute(userPath.GetBaseDir()))
            {
                if (DirAccess.MakeDirRecursiveAbsolute(userPath.GetBaseDir()) is not Error.Ok)
                {
                    GameManager.Error($"Failed to create directory: {userPath.GetBaseDir()}");
                }
            }

            if (!FileUtils.CopyFile(resourcePath, userPath))
            {
                GameManager.Error($"Failed to copy resource file: {path}");
            }

            return ProjectSettings.GlobalizePath(userPrefix + path);
        }
    }
}
