namespace pkuManager.Data;

/// <summary>
/// Represents a collection of compiled data dexes.
/// </summary>
public class DataDexManager : DexManager
{
    //Base URL of the https://github.com/project-pku/pkuData repo
    protected override string ONLINE_BASE_URL => "https://raw.githubusercontent.com/project-pku/pkuData";

    public DataDexManager() : base() { }
    public DataDexManager(string hash) : base(hash) { }
    public DataDexManager(DirectoryInfo directory) : base(directory) { }
}