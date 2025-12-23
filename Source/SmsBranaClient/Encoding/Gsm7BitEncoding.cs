using System.Buffers;
using System.Collections.Frozen;
using System.Globalization;
using System.Text;

namespace SmsBranaClient.Encoding;

internal static class Gsm7BitEncoding
{
    /// <summary>
    /// Gets whether given <paramref name="text"/> can be expressed in GSM 7-bit encoding.
    /// </summary>
    public static bool IsGsm7(string text)
    {
        foreach (var c in text.ReplaceLineEndings("\n"))
        {
            if (!Gsm7Basic.Contains(c) && !Gsm7Extended.Contains(c))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the message length in characters (as an old cell phone would count it when typing).
    /// </summary>
    public static int GetMessageLength(string text)
    {
        var count = 0;

        foreach (var c in text)
        {
            if (Gsm7Basic.Contains(c))
                count++;
            else if (Gsm7Extended.Contains(c))
                count += 2; // These require an ESC prefix (0x1B)
            else
                throw new ArgumentException($"Invalid character '{c}'");
        }

        return count;
    }

    public static int EstimateMessageCount(int messageLength)
    {
        // GSM 7-bit messages are limited to 160 characters.
        if (messageLength <= 160)
            return 1;

        // Multipart GSM 7-bit messages can contain 153 characters each.
        return (int)Math.Ceiling(messageLength / 153d);
    }

    public static string Convert(string text, EAcuteHandling eAcuteHandling = EAcuteHandling.Preserve, bool replaceSimilarChars = true)
    {
        var sb = new StringBuilder();

        foreach (var t in text)
        {
            // When the character is representable as is
            if (Gsm7Basic.Contains(t) || Gsm7Extended.Contains(t))
            {
                if (t == 'é' && eAcuteHandling == EAcuteHandling.Strip)
                    sb.Append('e');
                else if (t == 'É' && eAcuteHandling == EAcuteHandling.Strip)
                    sb.Append('E');
                else
                    sb.Append(t);
                
                continue;
            }
            
            // When the character is representable after diacritic removal
            var normalized = RemoveDiacritics(t.ToString())[0];
            if (Gsm7Basic.Contains(normalized) || Gsm7Extended.Contains(normalized))
            {
                sb.Append(normalized);
                continue;
            }

            // When the character can be substituted with a representable one (if allowed)
            if (replaceSimilarChars && SimilarChars.TryGetValue(t, out var replacement))
            {
                sb.Append(replacement);
                continue;
            }
            
            // Unrepresentable character
            sb.Append('?');
        }

        return sb.ToString();
    }

    private static string RemoveDiacritics(string str)
    {
        var chars =
            from c in str.Normalize(NormalizationForm.FormD).ToCharArray()
            let uc = CharUnicodeInfo.GetUnicodeCategory(c)
            where uc != UnicodeCategory.NonSpacingMark
            select c;

        return new string(chars.ToArray()).Normalize(NormalizationForm.FormC);
    }

    private static readonly FrozenDictionary<char, char> SimilarChars =
        new Dictionary<char, char>()
        {
            ['—'] = '-',  // Em dash
            ['–'] = '-',  // En dash
            ['“'] = '"',
            ['”'] = '"',
            ['„'] = '"',
            ['‘'] = '\'',
            ['’'] = '\'',
            ['‚'] = '\''
        }.ToFrozenDictionary();
    
    /// <summary>
    /// These symbols count as 1 character.
    /// </summary>
    private static readonly SearchValues<char> Gsm7Basic = 
        SearchValues.Create("@£$¥èéùìòÇ\nØø\rÅåΔ_ΦΓΛΩΠΨΣΘΞÆæßÉ !\"#¤%&'()*+,-./0123456789:;<=>?¡ABCDEFGHIJKLMNOPQRSTUVWXYZÄÖÑÜ§¿abcdefghijklmnopqrstuvwxyzäöñüà");
    
    /// <summary>
    /// These symbols count as 2 characters (as they require an ESC prefix).
    /// </summary>
    private static readonly SearchValues<char> Gsm7Extended = 
        SearchValues.Create("^{}\\[]~|€\f");
}