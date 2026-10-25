using System.Text;
using NUnit.Framework;

namespace RTLTMPro.Tests
{
    public class RichTextFixerTests
    {
        [Test]
        public void FindTag_FindsSimpleOpeningTag()
        {
            
            var text = new FastStringBuilder("text <opening> text");
            
            
            RichTextFixer.FindTag(text, 0, out var tag);
            
            
            Assert.AreEqual(RichTextFixer.TagType.Opening, tag.Type);
            Assert.AreEqual(5, tag.Start);
            Assert.AreEqual(13, tag.End);
        }
        
        [Test]
        public void FindTag_FindsOpeningTagWithSpaceInside()
        {
            
            var text = new FastStringBuilder("text <ope ning> text");
            
            
            RichTextFixer.FindTag(text, 0, out var tag);
            
            
            Assert.AreEqual(RichTextFixer.TagType.Opening, tag.Type);
            Assert.AreEqual(5, tag.Start);
            Assert.AreEqual(14, tag.End);
        }
        
        [Test]
        public void FindTag_FindsOpeningTagWithValue()
        {
            
            var text = new FastStringBuilder("text <opening=12m> text");
            
            
            RichTextFixer.FindTag(text, 0, out var tag);
            
            
            Assert.AreEqual(RichTextFixer.TagType.Opening, tag.Type);
            Assert.AreEqual(5, tag.Start);
            Assert.AreEqual(17, tag.End);
        }
        
        [Test]
        public void FindTag_ProducesTheSameHash_ForOpeningTagsWithDifferentValues()
        {
            
            var text1 = new FastStringBuilder("text <opening=12m> text");
            var text2 = new FastStringBuilder("text <opening=18s> text");
            
            
            RichTextFixer.FindTag(text1, 0, out var tag1);
            RichTextFixer.FindTag(text2, 0, out var tag2);
            
            
            Assert.AreEqual(tag1.HashCode, tag2.HashCode);
        }

        [Test]
        public void FindTag_FindsSimpleSelfContainedTag()
        {
            
            var text = new FastStringBuilder("text <opening/> text");
            
            
            RichTextFixer.FindTag(text, 0, out var tag);
            
            
            Assert.AreEqual(RichTextFixer.TagType.SelfContained, tag.Type);
            Assert.AreEqual(5, tag.Start);
            Assert.AreEqual(14, tag.End);
        }
        
        [Test]
        public void FindTag_FindsSelfContainedTagWithValue()
        {
            
            var text = new FastStringBuilder("text <opening=15m/> text");
            
            
            RichTextFixer.FindTag(text, 0, out var tag);
            
            
            Assert.AreEqual(RichTextFixer.TagType.SelfContained, tag.Type);
            Assert.AreEqual(5, tag.Start);
            Assert.AreEqual(18, tag.End);
        }

        [Test]
        public void FindTag_ProducesTheSameHash_ForSelfContainedTagsWithDifferentValues()
        {
            
            var text1 = new FastStringBuilder("text <opening=15m/> text");
            var text2 = new FastStringBuilder("text <opening=20d/> text");

            
            RichTextFixer.FindTag(text1, 0, out var tag1);
            RichTextFixer.FindTag(text2, 0, out var tag2);

            
            Assert.AreEqual(tag1.HashCode, tag2.HashCode);
        }
        
        [Test]
        public void FindTag_FindsClosingTag()
        {
            
            var text = new FastStringBuilder("text </closing> text");
            
            
            RichTextFixer.FindTag(text, 0, out var tag);
            
            
            Assert.AreEqual(RichTextFixer.TagType.Closing, tag.Type);
            Assert.AreEqual(5, tag.Start);
            Assert.AreEqual(14, tag.End);
        }

        [Test]
        public void FindTag_ProducesTheSameHash_ForSameOpeningAndClosingTag()
        {
            
            var text1 = new FastStringBuilder("text <tag=15m/> text");
            var text2 = new FastStringBuilder("text </tag> text");

            
            RichTextFixer.FindTag(text1, 0, out var tag1);
            RichTextFixer.FindTag(text2, 0, out var tag2);

            
            Assert.AreEqual(tag1.HashCode, tag2.HashCode);   
        }

        [Test]
        public void FindTag_ReturnsZero_WhenNoTagIsFound()
        {
            
            var text = new FastStringBuilder("text");
            
            
            RichTextFixer.FindTag(text, 0, out var tag);
            
            
            Assert.AreEqual(0, tag.Start);
            Assert.AreEqual(0, tag.End);
            Assert.AreEqual(RichTextFixer.TagType.None, tag.Type);
            Assert.AreEqual(0, tag.HashCode);
        }

        [Test]
        public void FindTag_StartFromTheProvidedStartPosition()
        {
            
            var text = new FastStringBuilder(" <tag> text");
            
            
            RichTextFixer.FindTag(text, 6, out var tag);
            
            
            Assert.AreEqual(0, tag.Start);
            Assert.AreEqual(0, tag.End);
            Assert.AreEqual(RichTextFixer.TagType.None, tag.Type);
            Assert.AreEqual(0, tag.HashCode);
        }

        [Test]
        public void FindTag_OpeningTagWithAttributes()
        {
            
            var text = new FastStringBuilder("test <mytag=4 name=\"asdf\" id=5> text");

            
            RichTextFixer.FindTag(text, 0, out var tag);

            
            Assert.AreEqual(5, tag.Start);
            Assert.AreEqual(30, tag.End);
            Assert.AreEqual(RichTextFixer.TagType.Opening, tag.Type);
        }

        [Test]
        public void FindTag_IgnoreTagWithoutClosingChar()
        {
            
            var text = new FastStringBuilder("test <my invalid tag<b> text");

            
            RichTextFixer.FindTag(text, 0, out var tag);

            
            Assert.AreEqual(20, tag.Start);
            Assert.AreEqual(22, tag.End);
            Assert.AreEqual(RichTextFixer.TagType.Opening, tag.Type);
        }

        [Test]
        public void FindTag_IgnoreTagStartingWithSpace()
        {
            
            var text = new FastStringBuilder("test < my invalid tag> text");

            
            RichTextFixer.FindTag(text, 0, out var tag);

            
            Assert.AreEqual(0, tag.Start);
            Assert.AreEqual(0, tag.End);
            Assert.AreEqual(RichTextFixer.TagType.None, tag.Type);
        }

        
        [Test]
        public void FindTag_SpriteWithIdTag()
        {
            
            var text = new FastStringBuilder("test <sprite=6> text");

            
            RichTextFixer.FindTag(text, 0, out var tag);

            
            Assert.AreEqual(5, tag.Start);
            Assert.AreEqual(14, tag.End);
            Assert.AreEqual(RichTextFixer.TagType.Opening, tag.Type);
        }

        [Test]
        public void Fix_Reverses_SelfContainedTags()
        {
            
            var text = new FastStringBuilder("text <self-contained/> text");
            
            
            RichTextFixer.Fix(text);
            
            
            Assert.AreEqual("text >/deniatnoc-fles< text", text.ToString());
        }

        [Test]
        public void Fix_Reverses_OpenTag_ThatDoesntHaveClosingTag()
        {
            
            var text = new FastStringBuilder("text <open> text");
            
            
            RichTextFixer.Fix(text);
            
            
            Assert.AreEqual("text >nepo< text", text.ToString());
        }
        
        [Test]
        public void Fix_Reverses_SimpleOpenAndClosingTag()
        {
            
            var text = new FastStringBuilder("text </open> text <open>");
            
            
            RichTextFixer.Fix(text);
            
            
            Assert.AreEqual("text >nepo/< text >nepo<", text.ToString());
        }
        
        [Test]
        public void Fix_Reverses_SimpleOpenAndClosingTagWithValue()
        {
            
            var text = new FastStringBuilder("text </open> text <open=134>");
            
            
            RichTextFixer.Fix(text);
            
            
            Assert.AreEqual("text >nepo/< text >431=nepo<", text.ToString());
        }
    }
}
