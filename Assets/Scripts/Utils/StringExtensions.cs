using System;

namespace Utils
{
    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input) =>
            input switch
            {
                null => String.Empty,
                "" => String.Empty,
                _ => string.Concat(input[0].ToString().ToUpper(), input.AsSpan(1).ToString())
            };
        public static string RemoveAllBetween(string strSource, string strStart, string strEnd)
        {
            while (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                strSource = RemoveBetween(strSource, strStart, strEnd);
            }
            return strSource;
        }
        public static string RemoveBetween(string strSource, string strStart, string strEnd)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int Start, End;
                Start = strSource.IndexOf(strStart);
                End = strSource.IndexOf(strEnd, Start) + strEnd.Length;
                return strSource.Remove(Start, End - Start);
            }

            return strSource;
        }
        
        public static string GetBetween(string strSource, string strStart, string strEnd, out int indexEnd, int indexStart = 0)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd))
            {
                int Start, End;
                Start = strSource.IndexOf(strStart, indexStart) + strStart.Length;
                End = strSource.IndexOf(strEnd, Start);
                indexEnd = End + strEnd.Length;
                return strSource.Substring(Start, End - Start);
            }

            indexEnd = 0;
            return "";
        }
        
        public static string GetBetweenUntil(string strSource, string strStart, string strEnd, string strStop, out int indexEnd, int indexStart = 0)
        {
            if (strSource.Contains(strStart) && strSource.Contains(strEnd) && strSource.Contains(strStop))
            {
                var localEnd = 0;
                indexEnd = 0;
                strSource = strSource.Substring(indexStart);
                var result = "";
                do
                {
                    result += $"\r\n - {GetBetween(strSource, strStart, strEnd, out localEnd)}";
                    indexEnd += localEnd;
                    strSource = strSource.Substring(localEnd);
                } while (strSource.IndexOf(strStop, 0) > localEnd);
                return result;
            }

            indexEnd = 0;
            return "";
        }
    }
}