using System.Text.Json;

namespace pkuManager.Data;

/// <summary>
/// Represents a complete collection of MasterDexes to be referenced when manipulating pkus.
/// </summary>
public class DataDexManager
{
    /* ------------------------------------
     * Data Source Parameters
     * ------------------------------------
    */
    /// <summary>
    /// The data source this manager is getting its MasterDexes from.
    /// </summary>
    public DataSourceType DataSource { get; }

    /// <summary>
    /// The commit hash number this manager is getting its dexes from,<br/> if <see cref="DataSource"/>
    /// equals <see cref="DataSourceType.Previous"/>. Otherwise, <see langword="null"/>.
    /// </summary>
    public string? CommitHash { get; }

    /// <summary>
    /// The local directory this manager is getting its dexes from,<br/> if <see cref="DataSource"/>
    /// equals <see cref="DataSourceType.Local"/>. Otherwise, <see langword="null"/>.
    /// </summary>
    public DirectoryInfo? LocalDirectory { get; }

    /// <summary>
    /// Represents one of the possible types of data sources a <see cref="DataDexManager"/> can pull from.
    /// </summary>
    public enum DataSourceType
    {
        Latest,
        Previous,
        Local
    }


    /* ------------------------------------
     * DataDexes
     * ------------------------------------
    */
    private readonly Dictionary<string, JsonDocument> DexList = new();
    internal JsonElement GetDexRoot(string name) => DexList[name].RootElement;


    /* ------------------------------------
     * Initialization
     * ------------------------------------
    */
    /// <summary>
    /// Constructs a <see cref="DataDexManager"/> that sources its
    /// data online from the latest build of the pkuData repo.
    /// </summary>
    /// <inheritdoc cref="ReadLatestDex(string)" path="/exception"/>
    public DataDexManager() : this(DataSourceType.Latest, null, null) { }

    /// <summary>
    /// Constructs a <see cref="DataDexManager"/> that sources its
    /// data online from a previous build of the pkuData repo, given by the <paramref name="hash"/>.
    /// </summary>
    /// <param name="hash">The hash number of the commit to refernce for data.
    ///     If <see langword="null"/>, uses the latest commit</param>
    /// <inheritdoc cref="ReadLatestDex(string)" path="/exception"/>
    public DataDexManager(string hash) : this(DataSourceType.Previous, hash, null) { }

    /// <summary>
    /// Constructs a <see cref="DataDexManager"/>
    /// that sources its data from a local <paramref name="directory"/>.
    /// </summary>
    /// <param name="directory">A <u>complete</u> local directory of masterdexes.</param>
    /// <inheritdoc cref="ReadLocalDex(string)" path="/exception"/>
    public DataDexManager(DirectoryInfo directory) : this(DataSourceType.Local, null, directory) { }

    //Constructs a ddm, and starts downloading the datadexes in the background.
    private DataDexManager(DataSourceType dataSource, string? hash = null, DirectoryInfo? dir = null)
    {
        DataSource = dataSource;
        CommitHash = hash;
        LocalDirectory = dir;

        var config = Task.Run(() => ReadDex("config.json")).Result;

        JsonElement dexesDict = config.RootElement.GetProperty("Dexes");
        List<Task<JsonDocument>> tasks = new();
        foreach (JsonProperty dex in dexesDict.EnumerateObject())
            tasks.Add(Task.Run(async () => DexList[dex.Name] = await ReadDex(dex.Value.GetString()!))); //assume no null filenames
        Task.Run(() => Task.WhenAll(tasks)).Wait();
    }


    /* ------------------------------------
     * DataDex Retrieval Methods
     * ------------------------------------
    */
    //Base URL of the https://github.com/project-pku/pkuData repo
    private const string ONLINE_BASE_URL = "https://raw.githubusercontent.com/project-pku/pkuData";

    /// <summary>
    /// Gets a datadex using the appropriate method depending on the state of <see cref="DataSource"/>.
    /// </summary>
    /// <param name="name">The name of the master dex. As in $"{name}Dex.json".</param>
    /// <returns>A task returning a JSONDocument of the parsed datadex.</returns>
    /// <inheritdoc cref="ReadLatestDex(string)" path="/exception"/>
    /// <inheritdoc cref="ReadHashDex(string)" path="/exception"/>
    /// <inheritdoc cref="ReadLocalDex(string)" path="/exception"/>
    private async Task<JsonDocument> ReadDex(string name) => DataSource switch
    {
        DataSourceType.Latest => await ReadLatestDex(name),
        DataSourceType.Previous => await ReadHashDex(name),
        DataSourceType.Local => await ReadLocalDex(name),
        _ => throw new NotImplementedException($"DataSource: {DataSource}, not implemented")
    };

    /// <summary>
    /// Gets a datadex from the latest commit on the
    /// <see href="https://github.com/project-pku/pkuData">pkuData</see> repo.<br/>
    /// Is meant to be run only when <see cref="DataSource"/> is <see cref="DataSourceType.Latest"/>
    /// </summary>
    /// <inheritdoc cref="ReadDex(string)" path='/param[@name="name"]'/>
    /// <inheritdoc cref="ReadDex(string)" path='/returns'/>
    /// <exception cref="InvalidOperationException">The '<paramref name="name"/>' dex doesn't exist.</exception>
    /// <inheritdoc cref="DownloadUtil.DownloadJSON(string)" path="/exception"/>
    private static async Task<JsonDocument> ReadLatestDex(string name)
        => await DownloadUtil.DownloadJSON($"{ONLINE_BASE_URL}/build/{name}");

    /// <summary>
    /// Gets a datadex from the commit specified by the <see cref="CommitHash"/> on the
    /// <see href="https://github.com/project-pku/pkuData">pkuData</see> repo.<br/>
    /// Is meant to be run only when <see cref="DataSource"/> is <see cref="DataSourceType.Previous"/>
    /// </summary>
    /// <inheritdoc cref="ReadLatestDex(string)"/>
    private async Task<JsonDocument> ReadHashDex(string name)
        => await DownloadUtil.DownloadJSON($"{ONLINE_BASE_URL}/{CommitHash}/{name}");

    /// <summary>
    /// Gets a datadex from <see cref="LocalDirectory"/>.<br/>
    /// Should only be run if <see cref="DataSource"/> is <see cref="DataSourceType.Local"/>
    /// </summary>
    /// <inheritdoc cref="ReadDex(string)" path='/param[@name="name"]'/>
    /// <inheritdoc cref="ReadDex(string)" path='/returns'/>
    /// <exception cref="JsonException">Thrown if the dex is invalid JSON.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown if <see cref="LocalDirectory"/>
    ///     is not a valid directory.</exception>
    /// <exception cref="System.Security.SecurityException">Thrown if the application
    ///     does not have access to <see cref="LocalDirectory"/></exception>
    private async Task<JsonDocument> ReadLocalDex(string name)
    {
        //only called when DataSource = local
        FileInfo[] files = LocalDirectory!.GetFiles(name);
        string rawJson = await File.ReadAllTextAsync(files[0].FullName);
        return JsonDocument.Parse(rawJson);
    }
}