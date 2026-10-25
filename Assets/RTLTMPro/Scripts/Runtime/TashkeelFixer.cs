using System.Collections.Generic;


namespace RTLTMPro
{
    public static class TashkeelFixer
    {
        private static readonly List<TashkeelLocation> TashkeelLocations = new List<TashkeelLocation>(100);

        private static readonly string ShaddaDammatan = new string(
            new[] { (char)TashkeelCharacters.Shadda, (char)TashkeelCharacters.Dammatan });

        private static readonly string ShaddaKasratan = new string(
            new[] { (char)TashkeelCharacters.Shadda, (char)TashkeelCharacters.Kasratan });

        private static readonly string ShaddaSuperscriptAlef = new string(
            new[] { (char)TashkeelCharacters.Shadda, (char)TashkeelCharacters.SuperscriptAlef });

        private static readonly string ShaddaFatha = new string(
            new[] { (char)TashkeelCharacters.Shadda, (char)TashkeelCharacters.Fatha });

        private static readonly string ShaddaDamma = new string(
            new[] { (char)TashkeelCharacters.Shadda, (char)TashkeelCharacters.Damma });

        private static readonly string ShaddaKasra = new string(
            new[] { (char)TashkeelCharacters.Shadda, (char)TashkeelCharacters.Kasra });

        private static readonly string ShaddaWithFathaIsolatedForm =
            ((char)TashkeelCharacters.ShaddaWithFathaIsolatedForm).ToString();

        private static readonly string ShaddaWithDammaIsolatedForm =
            ((char)TashkeelCharacters.ShaddaWithDammaIsolatedForm).ToString();

        private static readonly string ShaddaWithKasraIsolatedForm =
            ((char)TashkeelCharacters.ShaddaWithKasraIsolatedForm).ToString();

        private static readonly string ShaddaWithDammatanIsolatedForm =
            ((char)TashkeelCharacters.ShaddaWithDammatanIsolatedForm).ToString();

        private static readonly string ShaddaWithKasratanIsolatedForm =
            ((char)TashkeelCharacters.ShaddaWithKasratanIsolatedForm).ToString();

        private static readonly string ShaddaWithSuperscriptAlefIsolatedForm =
            ((char)TashkeelCharacters.ShaddaWithSuperscriptAlefIsolatedForm).ToString();

        private static readonly HashSet<char> TashkeelCharactersSet = new HashSet<char>() {
            (char)TashkeelCharacters.Fathan,
            (char)TashkeelCharacters.Dammatan,
            (char)TashkeelCharacters.Kasratan,
            (char)TashkeelCharacters.Fatha,
            (char)TashkeelCharacters.Damma,
            (char)TashkeelCharacters.Kasra,
            (char)TashkeelCharacters.Shadda,
            (char)TashkeelCharacters.Sukun,
            (char)TashkeelCharacters.MaddahAbove,
            (char)TashkeelCharacters.SuperscriptAlef,
            (char)TashkeelCharacters.ShaddaWithDammatanIsolatedForm,
            (char)TashkeelCharacters.ShaddaWithKasratanIsolatedForm,
            (char)TashkeelCharacters.ShaddaWithFathaIsolatedForm,
            (char)TashkeelCharacters.ShaddaWithDammaIsolatedForm,
            (char)TashkeelCharacters.ShaddaWithKasraIsolatedForm,
            (char)TashkeelCharacters.ShaddaWithSuperscriptAlefIsolatedForm
        };

        private static readonly Dictionary<char, char> ShaddaCombinationMap = new Dictionary<char, char>()
        {
            [(char)TashkeelCharacters.Dammatan] = (char)TashkeelCharacters.ShaddaWithDammatanIsolatedForm,
            [(char)TashkeelCharacters.Kasratan] = (char)TashkeelCharacters.ShaddaWithKasratanIsolatedForm,
            [(char)TashkeelCharacters.Fatha] = (char)TashkeelCharacters.ShaddaWithFathaIsolatedForm,
            [(char)TashkeelCharacters.Damma] = (char)TashkeelCharacters.ShaddaWithDammaIsolatedForm,
            [(char)TashkeelCharacters.Kasra] = (char)TashkeelCharacters.ShaddaWithKasraIsolatedForm,
            [(char)TashkeelCharacters.SuperscriptAlef] = (char)TashkeelCharacters.ShaddaWithSuperscriptAlefIsolatedForm,
        };

        
        
        
        public static void RemoveTashkeel(FastStringBuilder input)
        {
            TashkeelLocations.Clear();
            int j = 0; 
            for (int i = 0; i < input.Length; i++)
            {
                int curChar = input.Get(i);
                if (Char32Utils.IsUnicode16Char(curChar) && TashkeelCharactersSet.Contains((char)curChar))
                {
                    TashkeelLocations.Add(new TashkeelLocation((TashkeelCharacters)curChar, i));
                }
                else
                {
                    input.Set(j, curChar);
                    j++;
                }
            }
            input.Length = j;
        }

        
        
        
        public static void RestoreTashkeel(FastStringBuilder letters)
        {
            foreach (TashkeelLocation location in TashkeelLocations)
            {
                letters.Insert(location.Position, location.Tashkeel);
            }
        }

        
        
        
        public static void FixShaddaCombinations(FastStringBuilder input)
        {
            
            int j = 0; 
            int i = 0; 
            while (i < input.Length)
            {
                int curChar = input.Get(i);
                int nextChar = i < input.Length - 1 ? input.Get(i + 1) : (char)0;
                if ((TashkeelCharacters)curChar == TashkeelCharacters.Shadda
                    && ShaddaCombinationMap.ContainsKey((char)nextChar))
                {
                    input.Set(j, ShaddaCombinationMap[(char)nextChar]);
                    j++;
                    i += 2;
                }
                else
                {
                    input.Set(j, curChar);
                    j++;
                    i++;
                }
            }
            input.Length = j;
        }
    }
}