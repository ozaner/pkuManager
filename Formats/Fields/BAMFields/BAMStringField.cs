using OneOf;
using pkuManager.Utilities;
using System;
using System.Numerics;

namespace pkuManager.Formats.Fields.BAMFields;

public class BAMStringField : BAMArrayField, IField<string>
{
    protected string FormatName { get; }

    public bool LangDependent { get; }
    protected OneOf<IField<BigInteger>, Language?> LanguageField { get; }
    protected Predicate<Language> IsValidLang { get; }

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

    protected Language? GetLanguage() => LangDependent ? LanguageField.Match(
        x => {
            var y = (Language)x.GetAs<int>();
            return IsValidLang(y) ? y : null;
        },
        x => x
    ) : null;

    public BAMStringField(ByteArrayManipulator bam, int startByte, int byteLength, int length, string formatName,
        OneOf<IField<BigInteger>, Language?> langField, Predicate<Language> isValidLang)
        : base(bam, startByte, byteLength, length)
    {
        LangDependent = true;
        LanguageField = langField;
        IsValidLang = isValidLang;
        FormatName = formatName;
    }
}