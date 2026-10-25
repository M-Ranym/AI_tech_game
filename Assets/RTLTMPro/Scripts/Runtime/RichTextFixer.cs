namespace RTLTMPro
{
    public static class RichTextFixer
    {
        public enum TagType
        {
            None,
            Opening,
            Closing,
            SelfContained,
        }

        public struct Tag
        {
            public int Start;
            public int End;
            public int HashCode;
            public TagType Type;

            public Tag(int start, int end, TagType type, int hashCode)
            {
                Type = type;
                Start = start;
                End = end;
                HashCode = hashCode;
            }
        }

        
        
        
        public static void Fix(FastStringBuilder text)
        {
            for (int i = 0; i < text.Length; i++)
            {
                FindTag(text, i, out Tag tag);

                
                if (tag.Type == TagType.None)
                {
                    break;
                }

                text.Reverse(tag.Start, tag.End - tag.Start + 1);

                i = tag.End;
            }
        }

        public static void FindTag(
            FastStringBuilder str,
            int start,
            out Tag tag)
        {
            for (int i = start; i < str.Length;)
            {
                if (str.Get(i) != '<')
                {
                    i++;
                    continue;
                }

                bool calculateHashCode = true;
                tag.HashCode = 0;
                for (int j = i + 1; j < str.Length; j++)
                {
                    int jChar = str.Get(j);

                    if (calculateHashCode)
                    {
                        if (Char32Utils.IsLetter(jChar))
                        {
                            unchecked
                            {
                                if (tag.HashCode == 0)
                                {
                                    tag.HashCode = jChar.GetHashCode();
                                }
                                else
                                {
                                    tag.HashCode = (tag.HashCode * 397) ^ jChar.GetHashCode();
                                }
                            }
                        }
                        else if (tag.HashCode != 0)
                        {
                            
                            calculateHashCode = false;
                        }
                    }

                    
                    if ((j == i + 1 && jChar == ' ') || jChar == '<')
                    {
                        break;
                    }

                    if (jChar == '>')
                    {
                        

                        tag.Start = i;
                        tag.End = j;

                        if (str.Get(j - 1) == '/')
                        {
                            
                            tag.Type = TagType.SelfContained;
                        }
                        else if (str.Get(i + 1) == '/')
                        {
                            
                            tag.Type = TagType.Closing;
                        }
                        else
                        {
                            tag.Type = TagType.Opening;
                        }

                        return;
                    }
                }

                i++;
            }

            tag.Start = 0;
            tag.End = 0;
            tag.Type = TagType.None;
            tag.HashCode = 0;
        }
    }
}