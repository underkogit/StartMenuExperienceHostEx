using System.Diagnostics;
using System.IO;

namespace StartMenuExperienceHostEx.Helper;

public class IconExtractor
{
    private readonly string _programPath;

    public IconExtractor(string programPath)
    {
        if (!File.Exists(programPath))
            throw new FileNotFoundException($"Program not found: {programPath}");
        _programPath = programPath;
    }

    public bool ExtractIcon(string sourceFile, string exportPath)
    {
        if (!File.Exists(sourceFile) || !Directory.Exists(exportPath))
            // throw new FileNotFoundException($"Source file not found: {sourceFile}");
            return false;

        var dir = Path.GetDirectoryName(exportPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = _programPath,
                    Arguments = $"\"{sourceFile}\" \"{exportPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                }
            };

            process.Start();
            process.WaitForExit();
            return process.ExitCode == 1 && File.Exists(exportPath);
        }
        catch
        {
            return false;
        }
    }

    public byte[] ExtractIconAsBytes(string sourceFile)
    {
        var tempFile = Path.GetTempFileName() + ".ico";
        try
        {
            if (ExtractIcon(sourceFile, tempFile))
                return File.ReadAllBytes(tempFile);
            return null;
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }
}