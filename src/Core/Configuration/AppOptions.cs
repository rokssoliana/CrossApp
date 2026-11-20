namespace Core.Configuration;

public sealed class AppOptions
{
    public StorageOptions Storage { get; init; } = new();
    public string EnvironmentName { get; init; } = "Development";
}

public sealed class StorageOptions
{
    public string DataPath { get; init; } = "data";
    public string FileName { get; init; } = "items.json";

    public bool Validate(out string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(DataPath))
        {
            errorMessage = "Помилка конфігурації: Storage:DataPath не може бути порожнім.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(FileName) || !FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            errorMessage = $"Помилка конфігурації: Storage:FileName ('{FileName}') повинен мати розширення '.json'.";
            return false;
        }

        errorMessage = string.Empty;
        return true;
    }

    public string ResolveFullPath(string baseDir)
    {
        var root = Path.IsPathRooted(DataPath) ? DataPath : Path.Combine(baseDir, DataPath);
        Directory.CreateDirectory(root);
        return Path.GetFullPath(Path.Combine(root, FileName));
    }
}
