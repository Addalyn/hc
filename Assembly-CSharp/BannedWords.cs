using System;
using System.Text;

[Serializable]
public class BannedWords
{
    public BannedWordsData[] m_bannedWords;

    public static BannedWords Get()
    {
        return GameWideData.Get().m_bannedWords;
    }

    public string FilterPhrase(string phrase, string languageCode)
    {
        if (phrase.IsNullOrEmpty())
        {
            return phrase;
        }

        StringBuilder stringBuilder = new StringBuilder(phrase);
        char[] anyOf = " ,./?'<>:;\"[]{}-_+=!@#^&*()\\|".ToCharArray();
        int startIndex = 0;
        while (startIndex < phrase.Length)
        {
            int endIndex = phrase.IndexOfAny(anyOf, startIndex);
            int len = endIndex < 0 ? phrase.Length - startIndex : endIndex - startIndex;
            if (len > 0)
            {
                string text = phrase.Substring(startIndex, len).ToLower();
                BannedWordsData[] bannedWords = m_bannedWords;
                foreach (BannedWordsData bannedWordsData in bannedWords)
                {
                    if (bannedWordsData.Name != languageCode)
                    {
                        continue;
                    }

                    foreach (string bannedFullString in bannedWordsData.m_fullStrings)
                    {
                        if (text == bannedFullString)
                        {
                            Mask(stringBuilder, startIndex, len);
                        }
                    }

                    foreach (string bannedSubString in bannedWordsData.m_subStrings)
                    {
                        for (int i = 0; i < len; i++)
                        {
                            i = text.IndexOf(bannedSubString, i);
                            if (i == -1)
                            {
                                break;
                            }

                            Mask(stringBuilder, startIndex + i, bannedSubString.Length);
                        }
                    }

                    foreach (string bannedPrefix in bannedWordsData.m_prefixStrings)
                    {
                        if (text.StartsWith(bannedPrefix))
                        {
                            Mask(stringBuilder, startIndex, bannedPrefix.Length);
                        }
                    }

                    foreach (string bannedSuffix in bannedWordsData.m_suffixStrings)
                    {
                        if (text.EndsWith(bannedSuffix))
                        {
                            Mask(stringBuilder, startIndex + len - bannedSuffix.Length, bannedSuffix.Length);
                        }
                    }
                }
            }

            if (endIndex < 0)
            {
                break;
            }

            startIndex = endIndex + 1;
        }

        return stringBuilder.ToString();
    }

    private void Mask(StringBuilder stringBuilder, int maskStartIndex, int maskLength)
    {
        stringBuilder.Remove(maskStartIndex, maskLength);
        stringBuilder.Insert(maskStartIndex, new string('*', maskLength));
    }
}