using OneOf;
using pkuManager.Formats.Modules;
using pkuManager.Utilities;
using System;
using System.Numerics;

namespace pkuManager.Formats.Fields.BAMFields;

public class BAMStringField<T> : BAMArrayField, IField<string> where T: struct
{
    protected string FormatName { get; }

    protected bool LangDependent { get; }
    protected OneOf<IField<BigInteger>, Language?> LanguageField { get; }
    protected Predicate<Language> IsValidLang { get; }

    //string form of Value
    public string ValueAsString
    {
        get => DexUtil.CharEncoding<T>.Decode(this.GetAs<T>(), FormatName, GetLanguage());
        set => DexUtil.CharEncoding<T>.Encode(value, Length, FormatName, GetLanguage());
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

    public BAMStringField(ByteArrayManipulator bam, int startByte, int length, string formatName)
        : base(bam, startByte, ByteArrayManipulator.GetByteSize<T>(), length) => FormatName = formatName;

    public BAMStringField(ByteArrayManipulator bam, int startByte, int length, string formatName,
        OneOf<IField<BigInteger>, Language?> langField, Predicate<Language> isValidLang) : this(bam, startByte, length, formatName)
    {
        LangDependent = true;
        LanguageField = langField;
        IsValidLang = isValidLang;
    }
}