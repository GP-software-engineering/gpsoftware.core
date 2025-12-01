using GPSoftware.Core.Helpers;


namespace GPSoftware.core.Tests.Helpers {

    public class HtmlParserHelper_Tests {

        public HtmlParserHelper_Tests() {
        }

        [Fact]
        public void ExtractLocalImgPaths_Test() {
            // prepare html with tag <img .../>
            string input =
                "<p>fdfasfdas</p>\n" +
                "<p>fsda</p>\n" +
                "<p><img style=\"display: block; margin-left: auto; margin-right: auto;\" src=\"/images/blog/a300f088-efc9-47a5-ad49-d66f3a3b8a8b.jpg\" alt=\"\" width=\"794\" height=\"338\" /></p>\n" +
                "<p>&nbsp;</p>\n" +
                "<p>cia</p>\n" +
                "<p>&nbsp;</p>";
            // run
            var output = HtmlParserHelper.ExtractLocalImgPaths(input, trimFilePath: true);
            // assert
            output.Count.ShouldBe(1);
            output.Contains("a300f088-efc9-47a5-ad49-d66f3a3b8a8b.jpg").ShouldBeTrue();


            // prepare tag with <img ...>
            input = "<p>fdfasfdas</p>\n" +
                "<p>fsda</p>\n<p>" +
                "<img style=\"display: block;\" src=\"/images/blog/a300f088-efc9-47a5-ad49-d66f3a3b8a8b.jpg\" width=\"794\" height=\"338\"></p>\n" +
                "<p>&nbsp;</p>\n<p>cia</p>\n<p>&nbsp;</p>";
            // run
            output = HtmlParserHelper.ExtractLocalImgPaths(input, trimFilePath: true);
            // assert
            output.Count.ShouldBe(1);
            output.Contains("a300f088-efc9-47a5-ad49-d66f3a3b8a8b.jpg").ShouldBeTrue();
        }

    }
}
