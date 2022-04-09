using pkuManager.Formats.Modules;
using pkuManager.Formats.Modules.Tags;
using pkuManager.Utilities;

namespace pkuManager.Formats.Fields.BAMFields;

public class BAMStringField : BAMArrayField, IField<string>
{
    protected Language_O Language_Field { get; }
    protected (string lang, string format) ExplicitLangFormat;
    protected bool UseLangField;

    //string form of Value
    public string ValueAsString
    {
        get => DexUtil.CharEncoding.Decode(Value, GetFormat(), GetLanguage()).decodedStr;
        set => DexUtil.CharEncoding.Encode(value, Length, GetFormat(), GetLanguage());
    }

    string IField<string>.Value
    {
        get => ValueAsString;
        set => ValueAsString = value;
    }

    protected string GetFormat()
        => UseLangField ? Language_Field.FormatName: ExplicitLangFormat.format;

    protected string GetLanguage()
        => UseLangField ? (Language_Field.IsValid() ? Language_Field.AsString : TagUtil.DEFAULT_SEMANTIC_LANGUAGE)
                        : ExplicitLangFormat.lang; //no checking for invalid langs w/ explicit lang

    public BAMStringField(ByteArrayManipulator bam, int startByte, int byteLength, int length, Language_O langField)
        : base(bam, startByte, byteLength, length)
    {
        Language_Field = langField;
        UseLangField = true;
    }

    public BAMStringField(ByteArrayManipulator bam, int startByte, int byteLength, int length, string lang, string format)
        : base(bam, startByte, byteLength, length)
    {
        ExplicitLangFormat = (lang, format);
        UseLangField = false;
    }
}