using pkuManager.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace pkuManager.Utilities
{
    /// <summary>
    /// An encapsulation of the encoding and decoding scheme of the
    /// strings in a particular <see cref="Formats.FormatObject"/>.
    /// </summary>
    /// <typeparam name="T">The type of the codepoint each character corresponds to.</typeparam>
    public class CharacterEncoding<T> where T : struct
    {
        /// <summary>
        /// A singleton representing the UTF-16 character encoding used by modern Pokémon games.
        /// </summary>
        public static readonly CharacterEncoding<char> UTF16CharacterEncoding = new CharacterEncodingUTF16();

        /// <summary>
        /// Whether or not this encoding varies with the Pokémon's language.
        /// </summary>
        public bool LanguageDependent { get; }

        /// <summary>
        /// The terminator codepoint.
        /// </summary>
        public T Terminator { get; }

        /// <summary>
        /// The maximum value a codepoint can take.
        /// </summary>
        public ushort MaxValue { get; }

        /// <summary>
        /// A map of each character set indexed by the language they correspond to.<br/>
        /// If <see cref="LanguageDependent"/> is <see langword="false"/>,
        /// then there is only one character set which is indexed by <see langword="null"/>.
        /// </summary>
        private Dictionary<Language?, Dictionary<T, char>> Charsets { get; }

        /// <summary>
        /// Constructs a language-independent character encoding, with the given charset.
        /// </summary>
        /// <param name="charset">A mapping from codepoint to <see cref="char"/>.</param>
        /// <inheritdoc cref="CharacterEncoding(T, bool)"/>
        public CharacterEncoding(T terminator, Dictionary<T, char> charset) : this(terminator, false)
        {
            Charsets = new()
            {
                { null, charset }
            };
        }

        /// <summary>
        /// Constructs a language-dependent character encoding, with the given language-charset pairs.
        /// </summary>
        /// <param name="charsets">An array of mappings from <see cref="Language"/> to character set.</param>
        /// <inheritdoc cref="CharacterEncoding(T, bool)"/>
        public CharacterEncoding(T terminator, params (Language, Dictionary<T, char>)[] charsets) : this(terminator, true)
        {
            Charsets = new();
            foreach (var lcp in charsets)
                Charsets.Add(lcp.Item1, lcp.Item2);
        }

        /// <summary>
        /// Base constructor for character encoding. Checks if <typeparamref name="T"/> is a valid type.
        /// </summary>
        /// <param name="terminator">The codepoint for the terminator.</param>
        /// <param name="languageDependent">Whether or not this character encoding varies with language.</param>
        protected CharacterEncoding(T terminator, bool languageDependent)
        {
            Terminator = terminator;
            LanguageDependent = languageDependent;
            if (typeof(T) == typeof(byte))
                MaxValue = byte.MaxValue;
            else if (typeof(T) == typeof(ushort) || typeof(T) == typeof(char))
                MaxValue = ushort.MaxValue;
            else
                throw new NotSupportedException($"Generic parameter {nameof(T)} must be byte, ushort, or char.");
        }


        /* ------------------------------------
         * Encoding Methods
         * ------------------------------------
        */

        /// <summary>
        /// Encodes a given string, ending with a <see cref="Terminator"/>
        /// if the maximum length is not reached and padded with 0s.<br/>
        /// If an invalid language is passed, an exception will be thrown.
        /// </summary>
        /// <param name="str">The string to be encoded.</param>
        /// <param name="maxLength">The desired length of the encoded string.</param>
        /// <param name="language">The language to encode <paramref name="str"/>, if <see cref="LanguageDependent"/>
        ///                        is <see langword="true"/>. Null otherwise.</param>
        /// <returns>The encoded form of <paramref name="str"/>.</returns>
        public (T[] encodedStr, bool truncated, bool hasInvalidChars)
            Encode(string str, int maxLength, Language? language = null)
        {
            if (!Charsets.ContainsKey(language))
                throw InvalidLanguageException(language);

            bool truncated = false, hasInvalidChars = false;
            T[] encodedStr = new T[maxLength];

            //Encode string
            int successfulChars = 0;
            while (str?.Length > 0 && successfulChars < maxLength)
            {
                T? encodedChar = GetCodepoint(str[0], language); //get next character
                str = str[1..]; //chop off current character

                //if character invalid
                if (encodedChar is null)
                {
                    hasInvalidChars = true;
                    continue;
                }

                //else character not invalid
                encodedStr[successfulChars] = encodedChar.Value;
                successfulChars++;

                //stop encoding when limit reached
                if (successfulChars >= maxLength)
                    break;
            }

            //Deal with terminator
            if (successfulChars < maxLength)
                encodedStr[successfulChars] = Terminator;
            return (encodedStr, truncated, hasInvalidChars);
        }

        /// <summary>
        /// Decodes a given encoded string, stopping at the first instance of <see cref="Terminator"/>.<br/>
        /// If an invalid language is passed, an exception will be thrown.
        /// </summary>
        /// <param name="encodedStr">A string encoded with this character encoding.</param>
        /// <param name="language">The language the string was encoded with, if <see cref="LanguageDependent"/>
        ///                        is <see langword="true"/>. Null otherwise.</param>
        /// <returns>The string decoded from <paramref name="encodedStr"/>.</returns>
        public string Decode(T[] encodedStr, Language? language = null)
        {
            if (!Charsets.ContainsKey(language))
                throw InvalidLanguageException(language);

            StringBuilder sb = new();
            foreach (T e in encodedStr)
            {
                if (e.Equals(Terminator))
                    break;
                char? c = GetChar(e, language);
                if (c is not null)
                    sb.Append(c.Value);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Overlays the given <paramref name="encodedStr"/> over the given <paramref name="trash"/> array.
        /// </summary>
        /// <param name="encodedStr">An encoded string.</param>
        /// <param name="trash">The trash bytes to be applied to <paramref name="encodedStr"/>.</param>
        /// <returns>The encoded string 'trashed' with the given trash bytes.</returns>
        public T[] Trash(T[] encodedStr, ushort[] trash)
        {
            T[] trashedStr = ToGenericArray(trash.Clone() as ushort[]);
            trashedStr = trashedStr[0..encodedStr.Length];
            for (int i = 0; i < encodedStr.Length; i++)
            {
                trashedStr[i] = encodedStr[i];
                if (encodedStr[i].Equals(Terminator))
                    break;
            }
            return trashedStr;
        }


        /* ------------------------------------
         * Helper Members
         * ------------------------------------
        */

        /// <summary>
        /// Searches for the <see langword="char"/> associated with the given <paramref name="codepoint"/>.
        /// </summary>
        /// <param name="codepoint">The codepoint to search.</param>
        /// <param name="language">The language to search, must exist in <see cref="Charsets"/>.Keys.</param>
        /// <returns>The <see langword="char"/> that <paramref name="codepoint"/> maps to,
        ///          or null if none is found.</returns>
        protected virtual char? GetChar(T codepoint, Language? language)
            => Charsets[language].ContainsKey(codepoint) ? Charsets[language][codepoint] : null;

        /// <summary>
        /// Searches for the codepoint associated with the given char.
        /// </summary>
        /// <param name="c">The character to search.</param>
        /// <param name="language">The language to search, must exist in <see cref="Charsets"/>.Keys.</param>
        /// <returns>The codepoint that <paramref name="c"/> maps to, or null if none is found.</returns>
        protected virtual T? GetCodepoint(char c, Language? language)
            => Charsets[language].ContainsValue(c) ? Charsets[language].FirstOrDefault(x => x.Value == c).Key : null;

        /// <summary>
        /// Generates an exception for when an invalid language is passed to
        /// <see cref="Encode(string, int, Language?)"/> or <see cref="Decode(T[], Language?)"/>.
        /// </summary>
        /// <param name="language">The invalid language passed.</param>
        /// <returns>An invalid language exception.</returns>
        protected ArgumentException InvalidLanguageException(Language? language) => LanguageDependent ?
            new ArgumentException($"This {nameof(CharacterEncoding<T>)} is language dependent, and so the given language can't be null.", nameof(language)) :
            new ArgumentException($"The language {language} is not supported by this {nameof(CharacterEncoding<T>)}.", nameof(language));

        /// <summary>
        /// Used to convert the ushort trash found in <see cref="pku.pkuObject"/>s to the codepoint
        /// type <typeparamref name="T"/>.<br/> Since the constructor guarantees that <typeparamref name="T"/>
        /// is either byte, ushort, or char, this will not throw an invalid conversion exception.
        /// </summary>
        /// <param name="arr">A ushort array.</param>
        /// <returns><paramref name="arr"/> converted to a <typeparamref name="T"/> array.</returns>
        protected T[] ToGenericArray(ushort[] arr) => Array.ConvertAll(arr, x => (T)Convert.ChangeType(x, typeof(T)));

        /// <summary>
        /// Represents the UTF-16 character encoding.
        /// </summary>
        protected class CharacterEncodingUTF16 : CharacterEncoding<char>
        {
            public CharacterEncodingUTF16() : base('\0', false) { }

            protected override char? GetChar(char codepoint, Language? language) => codepoint;

            protected override char? GetCodepoint(char c, Language? language) => c;
        }
    }
}
