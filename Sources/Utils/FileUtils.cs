namespace BandBrosClone.Utils;

using Godot;

public static class FileUtils
{
    public static bool CopyFile(string srcPath, string dstPath)
    {
        using var src = FileAccess.Open(srcPath, FileAccess.ModeFlags.Read);
        if (src is null)
        {
            GD.PrintErr($"Failed to open source file: {srcPath}");
            return false;
        }

        using var dst = FileAccess.Open(dstPath, FileAccess.ModeFlags.Write);
        if (dst is null)
        {
            GD.PrintErr($"Failed to open destination file: {dstPath}");
            return false;
        }

        int chunkSize = 8192;

        while (!src.EofReached())
        {
            var chunk = src.GetBuffer(chunkSize);
            if (chunk.Length == 0)
            {
                break;
            }
            dst.StoreBuffer(chunk);
        }

        return true;
    }
}
