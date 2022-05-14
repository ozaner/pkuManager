using pkuManager.Formats.Modules;
using pkuManager.Formats.Modules.Tags;
using pkuManager.Formats.Modules.Templates;
using pkuManager.Utilities;

namespace pkuManager.Formats.Fields.BAMFields;

public class BAMStringField : BAMArrayField, IField<string>
{
    protected Language_O Language_Field { get; }
    protected string ExplicitLang;
    protected string FormatName;

    //string form of Value
    public string ValueAsString
    {
        get => DexUtil.CharEncoding.Decode(Value, FormatName, GetLanguage()).decodedStr;
        set => DexUtil.CharEncoding.Encode(value, Length, FormatName, GetLanguage());
    }

    string IField<string>.Value
    {
        get => ValueAsString;
        set => ValueAsString = value;
    }

    protected string GetLanguage()
    {
        if (ExplicitLang is null)
        {
            string decodedLang = IndexTagUtil.DecodeFormatField(Language_Field.Language, LANGUAGE_DEX, FormatName);
            bool langIsValid = LANGUAGE_DEX.ExistsIn(FormatName, decodedLang);
            return langIsValid ? decodedLang : TagUtil.DEFAULT_SEMANTIC_LANGUAGE;
        }
        else 
            return ExplicitLang;
    }

    public BAMStringField(ByteArrayManipulator bam, int startByte, int byteLength, int length, Language_O langField, string format)
        : base(bam, startByte, byteLength, length)
    {
        Language_Field = langField;
        FormatName = format;
    }

    public BAMStringField(ByteArrayManipulator bam, int startByte, int byteLength, int length, string lang, string format)
        : base(bam, startByte, byteLength, length)
    {
        ExplicitLang = lang;
        FormatName = format;
    }
}