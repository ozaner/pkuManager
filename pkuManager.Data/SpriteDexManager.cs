namespace pkuManager.Data;

/// <summary>
/// Represents a collection of compiled sprite dexes.
/// </summary>
public class SpriteDexManager : DexManager
{
    //Base URL of the https://github.com/project-pku/pkuSprite repo
    protected override string ONLINE_BASE_URL => "https://raw.githubusercontent.com/project-pku/pkuSprite";
}