using System.Text.Json;

namespace pkuManager.Data.Dexes;

public static class SpeciesSpriteDex
{
    /* ------------------------------------
     * Sprite Methods
     * ------------------------------------
    */
    public static bool TryGetFrontSprite(this SpriteDexManager sdm, SFAM sfam, out string url, out string author)
        => sdm.TryGetSpriteBase(sfam, out url, out author, "Front");

    public static bool TryGetBackSprite(this SpriteDexManager sdm, SFAM sfam, out string url, out string author)
        => sdm.TryGetSpriteBase(sfam, out url, out author, "Back");

    public static bool TryGetBoxSprite(this SpriteDexManager sdm, SFAM sfam, out string url, out string author)
        => sdm.TryGetSpriteBase(sfam, out url, out author, "Box");


    /* ------------------------------------
     * Default Sprite Methods
     * ------------------------------------
    */
    private static SFAM GetDefaultSFAM(this SFAM sfam) => new("", "", "", sfam.Modifiers);

    public static bool TryGetDefaultFrontSprite(this SpriteDexManager sdm, SFAM sfam, out string url, out string author)
        => sdm.TryGetFrontSprite(sfam.GetDefaultSFAM(), out url, out author);

    public static bool TryGetDefaultBackSprite(this SpriteDexManager sdm, SFAM sfam, out string url, out string author)
        => sdm.TryGetBackSprite(sfam.GetDefaultSFAM(), out url, out author);

    public static bool TryGetDefaultBoxSprite(this SpriteDexManager sdm, SFAM sfam, out string url, out string author)
        => sdm.TryGetBoxSprite(sfam.GetDefaultSFAM(), out url, out author);


    /* ------------------------------------
     * Batch Sprites
     * ------------------------------------
    */
    public static (string url, string author)[] Get3Sprites(this SpriteDexManager sdm, SFAM sfam)
    {
        if (!sdm.TryGetFrontSprite(sfam, out string frontUrl, out string frontAuth))
            sdm.TryGetDefaultFrontSprite(sfam, out frontUrl, out frontAuth);
        if (!sdm.TryGetBackSprite(sfam, out string backUrl, out string backAuth))
            sdm.TryGetDefaultBackSprite(sfam, out backUrl, out backAuth);
        if (!sdm.TryGetBoxSprite(sfam, out string boxUrl, out string boxAuth))
            sdm.TryGetDefaultBoxSprite(sfam, out boxUrl, out boxAuth);
        return new[] { (boxUrl, boxAuth),(frontUrl, frontAuth),(backUrl, backAuth) };
    }


    /* ------------------------------------
     * Conveience Methods
     * ------------------------------------
    */
    private static JsonElement SDR(SpriteDexManager sdm) => sdm.GetDexRoot("Species");

    private static bool TryGetSpriteBase(this SpriteDexManager sdm, SFAM sfam, out string url, out string author, string key)
    {
        url = author = null!; //shouldn't be used if false returned
        if (!SDR(sdm).TryGetSFAMValue(sfam, out string[] temp, "Sprites", key))
            return false;
        
        //success (temp = [url, author])
        url = temp[0];
        author = temp[1];
        return true;
    }
}
