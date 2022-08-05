using System.Text.Json;
using pkuManager.Data.Dexes;

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
     * DataDex Fields
     * ------------------------------------
    */
    public FormatDex FormatDex => _formatDex!.Result;
    private Task<FormatDex>? _formatDex;


    /* ------------------------------------
     * Initialization
     * ------------------------------------
    */
    /// <summary>
    /// Asynchronously creates a <see cref="DataDexManager"/>
    /// that sources its data online from the pkuData repo.
    /// </summary>
    /// <param name="hash">The hash number of the commit to refernce for data.
    ///     If <see langword="null"/>, uses the latest commit</param>
    /// <returns>A task that returns a <see cref="DataDexManager"/>.</returns>
    /// <inheritdoc cref="GetLatestDex(string)" path="/exception"/>
    public static async Task<DataDexManager> CreateOnlineManager(string? hash = null)
    {
        DataDexManager ddm = new(hash is null ? DataSourceType.Latest : DataSourceType.Previous, hash);
        await ddm.DataDexLoader();
        return ddm;
    }

    /// <summary>
    /// Asynchronously creates a <see cref="DataDexManager"/>
    /// that sources its data from a local <paramref name="directory"/>.
    /// </summary>
    /// <param name="directory">A <u>complete</u> local directory of masterdexes.</param>
    /// <returns>A task that returns a <see cref="DataDexManager"/>.</returns>
    /// <inheritdoc cref="GetLocalDex(string)" path="/exception"/>
    public static async Task<DataDexManager> CreateLocalManager(DirectoryInfo directory)
    {
        DataDexManager ddm = new(DataSourceType.Local, null, directory);
        await ddm.DataDexLoader();
        return ddm;
    }

    //Constructs a ddm, without loading any datadexes.
    private DataDexManager(DataSourceType dataSource, string? hash = null, DirectoryInfo? dir = null)
    {
        DataSource = dataSource;
        CommitHash = hash;
        LocalDirectory = dir;
    }

    /// <summary>
    /// Creates a task that schedules the downloading/reading of all the datadexes.
    /// </summary>
    /// <returns>A task that completes when all datadexes have been read.</returns>
    /// <inheritdoc cref="GetDex(string)" path="/exception"/>
    private Task DataDexLoader()
    {
        _formatDex = Task.Run(async () => new FormatDex(await GetDex("Format")));
        return Task.WhenAll(_formatDex);
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
    /// <param name="name">The name of the master dex. As in $"master{name}Dex.json".</param>
    /// <returns>A task returning a JSONDocument of the parsed datadex.</returns>
    /// <inheritdoc cref="GetLatestDex(string)" path="/exception"/>
    /// <inheritdoc cref="GetLocalDex(string)" path="/exception"/>
    private async Task<JsonDocument> GetDex(string name) => DataSource switch
    {
        DataSourceType.Latest => await GetLatestDex(name),
        DataSourceType.Previous => await GetHashDex(name),
        DataSourceType.Local => await GetLocalDex(name),
        _ => throw new NotImplementedException($"DataSource: {DataSource}, not implemented")
    };

    /// <summary>
    /// Gets a datadex from the latest commit on the
    /// <see href="https://github.com/project-pku/pkuData">pkuData</see> repo.<br/>
    /// Is meant to be run only when <see cref="DataSource"/> is <see cref="DataSourceType.Latest"/>
    /// </summary>
    /// <exception cref="InvalidOperationException">The <paramref name="name"/> masterdex doesn't exist.</exception>
    /// <inheritdoc cref="GetDex(string)" path="/not(exception)"/>
    /// <inheritdoc cref="DownloadUtil.DownloadJSON(string)" path="/exception"/>
    private static async Task<JsonDocument> GetLatestDex(string name)
        => await DownloadUtil.DownloadJSON($"{ONLINE_BASE_URL}/build/{name}");

    /// <summary>
    /// Gets a datadex from the commit specified by the <see cref="CommitHash"/> on the
    /// <see href="https://github.com/project-pku/pkuData">pkuData</see> repo.<br/>
    /// Is meant to be run only when <see cref="DataSource"/> is <see cref="DataSourceType.Previous"/>
    /// </summary>
    /// <inheritdoc cref="GetLatestDex(string)"/>
    private async Task<JsonDocument> GetHashDex(string name)
        => await DownloadUtil.DownloadJSON($"{ONLINE_BASE_URL}/{CommitHash}/{name}");

    /// <summary>
    /// Gets a datadex from <see cref="LocalDirectory"/>.<br/>
    /// Should only be run if <see cref="DataSource"/> is <see cref="DataSourceType.Local"/>
    /// </summary>
    /// <exception cref="JsonException">Thrown if the dex is invalid JSON.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown if <see cref="LocalDirectory"/>
    ///     is not a valid directory.</exception>
    /// <exception cref="System.Security.SecurityException">Thrown if the application
    ///     does not have access to <see cref="LocalDirectory"/></exception>
    /// <inheritdoc cref="GetDex(string)" path="/not(exception)"/>
    private async Task<JsonDocument> GetLocalDex(string name)
    {
        //only called when DataSource = local
        FileInfo[] files = LocalDirectory!.GetFiles(name);
        string rawJson = await File.ReadAllTextAsync(files[0].FullName);
        return JsonDocument.Parse(rawJson);
    }
}