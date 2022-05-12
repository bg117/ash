namespace OpenProject.ASH;

internal static class Config
{
    /// <summary>
    /// Parses the config file specified (i.e. executes commands, assigns variables).
    /// </summary>
    /// <param name="path">The path of the config file to parse. Recommended to be an absolute path.</param>
    /// <param name="keyValuePairs">The key-value pairs that correspond to variables. May be modified.</param>
    internal static void ReadConfigFile(string path, ref Dictionary<string, string> keyValuePairs)
    {
        if (!File.Exists(path))
            return;

        var lines = File.ReadAllLines(path);

        foreach (var line in lines)
            CliParser.ExecuteCommand(line, ref keyValuePairs); // run this on each line of the config
    }
}
