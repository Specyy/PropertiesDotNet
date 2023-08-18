namespace PropertiesDotNet.Test
{
    public class PropertiesWriterTests
    {
        [Theory]
        public void PropertiesWriter_VerifyCommentWriting()
        {
            using var sw = new StringWriter();
            using (var writer = new PropertiesWriter(sw))
            {
                Assert.Multiple(() =>
                {
                    Assert.That(writer.WriteComment("This is a test comment w/ implicit default handle"));
                    Assert.That(writer.WriteComment('!', "This is a test comment w/ exclamation handle"));
                    Assert.That(writer.WriteComment('#', "This is a test comment w/ explicit default handle"));
                    Assert.That(writer.WriteComment("\0\a\f\r\n\t\v "));
                    Assert.That(writer.WriteComment(""));
                    Assert.That(writer.WriteComment(null));
                });
            }

            Assert.That(sw.ToString(), 
                Is.EqualTo(@"# This is a test comment w/ implicit default handle
! This is a test comment w/ exclamation handle
# This is a test comment w/ explicit default handle
# \0\a\f\r\n\t\v 
# 
# 
"));
        }
    }
}
