using pkuManager.Data.Dexes;
using pkuManager.WinForms.Formats.Modules;
using pkuManager.WinForms.Formats.Modules.Tags;
using pkuManager.WinForms.Formats.Modules.Templates;
using pkuManager.WinForms.Utilities;

namespace pkuManager.WinForms.Formats.Fields.BAMFields;

public class BAMStringField : BAMArrayField, IField<string>
{
    protected Language_O Language_Field { get; }
    protected string ExplicitLang;
    protected string FormatName;

    //string form of Value
    public string ValueAsString
    {
        get => StringTagUtil.Decode(Value, FormatName, GetLanguage()).decodedStr;
        set => StringTagUtil.Encode(value, Length, FormatName, GetLanguage());
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
            string lang = null;
            bool success = Language_Field.Language.Match(
                (v) => DDM.TryGetLanguageName(FormatName, out lang, v.GetAs<int>()),
                (v) => DDM.TryGetLanguageName(FormatName, out lang, v.Value));
            return success ? lang : TagUtil.DEFAULT_SEMANTIC_LANGUAGE;
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