using System;
using System.Collections.Generic;

namespace RTLTMPro
{
    
    
    
    public static class GlyphTable
    {
        private static readonly Dictionary<char,char> MapList;

        
        
        
        static GlyphTable()
        { 
            
            var isolatedValues = Enum.GetNames(typeof(ArabicIsolatedLetters));
            
            MapList = new Dictionary<char,char>(isolatedValues.Length);
            foreach (var value in isolatedValues)
                MapList.Add((char)(int) Enum.Parse(typeof(ArabicGeneralLetters),value), (char) (int)Enum.Parse(typeof(ArabicIsolatedLetters),value));
        }

        public static char Convert(char toBeConverted)
        {
            return MapList.TryGetValue(toBeConverted, out var convertedValue) ? convertedValue : toBeConverted;
        }
    }
}