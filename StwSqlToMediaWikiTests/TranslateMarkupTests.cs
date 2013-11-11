using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StwSqlToMediaWiki;

namespace StwSqlToMediaWikiTests
{
    [TestClass]
    public class TranslateMarkupTests
    {
        private void TestReplacement(string original, string expected)
        {
            string translated = Program.TranslateMarkup(original);
            Assert.AreEqual(expected, translated);
        }

        [TestMethod]
        public void TestEmail()
        {
            TestReplacement(
                "&#x0D; John Doe &#x0D; e-mail: &lt;A href=\"mailto:john@domain.com\"&gt;john@domain.com&lt;/A&gt;&#x0D; '''Director'''&#x0D; Mary Basket &#x0D; e-mail: &lt;a href=\"mailto:mary@domain.com\"&gt;mary@domain.com&lt;/a&gt;&#x0D;",
                " John Doe  e-mail: [mailto:john@domain.com john@domain.com] '''Director''' Mary Basket  e-mail: [mailto:mary@domain.com mary@domain.com]"
            );
        }

        [TestMethod]
        public void TestBlock()
        {
            TestReplacement(
                "==&#x0D; &#x0D; (((Some Text &#x0D; in a box&#x0D; )))&#x0D;",
                "==  &lt;blockquote&gt;Some Text  in a box &lt;/blockquote&gt;"
            );
        }

        [TestMethod]
        public void TestLink()
        {
            TestReplacement(
                "nonon [^http://localhost:8080] non^ [http://anotherlink] [^http://test]",
                "nonon [http://localhost:8080] non^ [http://anotherlink] [http://test]"
            );
        }

        [TestMethod]
        public void TestLinkWithDescription()
        {
            TestReplacement(
                "[^http://server/Reports$server/Pages/Report.aspx?ItemPath=%2fsome%2fActivity|Some Activity]",
                "[http://server/Reports$server/Pages/Report.aspx?ItemPath=%2fsome%2fActivity Some Activity]"
            );
        }

        [TestMethod]
        public void TestHtmlLink()
        {
            TestReplacement(
                "&lt;A href=\"http://mysite\" target=\"_blank\"&gt;http://mysite&lt;/A&gt;",
                "[http://mysite http://mysite]"
            );
        }

        [TestMethod]
        public void TestAnchor()
        {
            TestReplacement(
                "[anchor|#Visual_Studio_Project_Files] [anchor|#Some Anchor]",
                "&lt;div id=\"Visual_Studio_Project_Files\"/&gt; &lt;div id=\"Some Anchor\"/&gt;"
            );
        }

        [TestMethod]
        public void TestImage()
        {
            TestReplacement(
                "[imageleft||{UP}/namespace/my_button.PNG]",
                "[[file:my_button.PNG]]"
            );
        }

        [TestMethod]
        public void TestDocument()
        {
            TestReplacement(
                "[{UP}/Statements/Document Name.doc|Document Name] [http://page.htm|Some Page]",
                "[[media:Document_Name.doc|Document Name]] [http://page.htm Some Page]"
            );
        }

        [TestMethod]
        public void TestInlineCode()
        {
            TestReplacement(
                "{{my code}}",
                "&lt;code&gt;my code&lt;/code&gt;"
            );
        }
    }
}
