namespace PropertiesDotNet.Test
{
    [TestFixture]
    public class PropertiesWriterTests
    {
        [Test]
        public void PropertiesWriter_ShouldWriteCommentsWithDifferentHandles()
        {
            using var sw = new StringWriter();
            using (var writer = new PropertiesWriter(sw))
            {
                Assert.Multiple(() =>
                {
                    Assert.That(writer.WriteComment("This is a test comment w/ implicit default handle"));
                    Assert.That(writer.WriteComment('!', "This is a test comment w/ exclamation handle"));
                    Assert.That(writer.WriteComment('#', "This is a test comment w/ explicit default handle"));
                });
            }

            Assert.That(sw.ToString().TrimEnd(Environment.NewLine.ToCharArray()),
                Is.EqualTo(@"# This is a test comment w/ implicit default handle
! This is a test comment w/ exclamation handle
# This is a test comment w/ explicit default handle"));
        }

        [Test]
        public void PropertiesWriter_ShouldEscapeSpecialCharsInComments()
        {
            using var sw = new StringWriter();
            using (var writer = new PropertiesWriter(sw))
            {
                Assert.Multiple(() =>
                {
                    Assert.That(writer.WriteComment("\0\a\f\r\n\t\v "));
                    Assert.That(writer.WriteComment("Привет"));
                });
            }

            Assert.That(sw.ToString().TrimEnd(Environment.NewLine.ToCharArray()),
                Is.EqualTo(@"# \0\a\f\r\n\t\v 
# \u041F\u0440\u0438\u0432\u0435\u0442"));
        }

        [Test]
        public void PropertiesWriter_ShouldWriteEmptyAndNullComments()
        {
            using var sw = new StringWriter();
            using (var writer = new PropertiesWriter(sw))
            {
                Assert.Multiple(() =>
                {
                    Assert.That(writer.WriteComment(string.Empty));
                    Assert.That(writer.WriteComment(null));
                });
            }

            Assert.That(sw.ToString().TrimEnd(Environment.NewLine.ToCharArray()),
                Is.EqualTo(@"# 
# "));
        }

        [Test]
        public void PropertiesWriter_ShouldWritePropertiesWithDifferentAssigners()
        {
            using var sw = new StringWriter();
            using (var writer = new PropertiesWriter(sw))
            {
                Assert.Multiple(() =>
                {
                    Assert.That(writer.WriteProperty("key1", "value1"));
                    Assert.That(writer.WriteProperty("key2", '=', "value2"));
                    Assert.That(writer.WriteProperty("key3", ':', "value3"));
                    Assert.That(writer.WriteProperty("key4", ' ', "value4"));
                    Assert.That(writer.WriteProperty("key5", '\t', "value5"));
                    Assert.That(writer.WriteProperty("key6", '\f', "value6"));
                });
            }

            Assert.That(sw.ToString().TrimEnd(Environment.NewLine.ToCharArray()),
                Is.EqualTo(@$"key1=value1
key2=value2
key3:value3
key4 value4
key5{'\t'}value5
key6{'\f'}value6"));
        }

        [Test]
        public void PropertiesWriter_ShouldEscapeNewLineOrWriteLogicalLine()
        {
            using var sw = new StringWriter();
            using (var writer = new PropertiesWriter(sw))
            {
                Assert.Multiple(() =>
                {
                    Assert.That(writer.WriteProperty("key\nwith\nnew\nlines", "value\nwith\nnew\nlines"));
                    Assert.That(writer.WriteKey($"key\nwith\nlogical\nlines", true) && writer.WriteValue("value\nwith\nlogical\nlines", true));
                });
            }

            Assert.That(sw.ToString().TrimEnd(Environment.NewLine.ToCharArray()),
                Is.EqualTo(@$"key\nwith\nnew\nlines=value\nwith\nnew\nlines
key\
with\
logical\
lines=value\
      with\
      logical\
      lines"));
        }

        [Test]
        public void PropertiesWriter_ShouldEscapeSpecialCharsInProperties()
        {
            using var sw = new StringWriter();
            using (var writer = new PropertiesWriter(sw))
            {
                Assert.Multiple(() =>
                {
                    Assert.That(writer.WriteKey("#!\0\a\f\r\n\t\v:= ") && writer.WriteValue("#!\0\a\f\r\n\t\v:= "));
                    Assert.That(writer.WriteKey("!!\0\a\f\r\n\t\v ") && writer.WriteValue("!!\0\a\f\r\n\t\v "));
                    Assert.That(writer.WriteKey("\0\a\f\r\n\t\v#! ") && writer.WriteValue("\0\a\f\r\n\t\v#! "));
                    Assert.That(writer.WriteKey("Привет") && writer.WriteValue("Привет"));
                    Assert.That(writer.WriteKey("フーバー") && writer.WriteValue("フーバー"));
                });
            }

            Assert.That(sw.ToString().TrimEnd(Environment.NewLine.ToCharArray()),
                Is.EqualTo(@$"\#!\0\a\f\r\n\t\v\:\=\ =#!\0\a{'\f'}\r\n{'\t'}\v:= 
\!!\0\a\f\r\n\t\v\ =!!\0\a{'\f'}\r\n{'\t'}\v 
\0\a\f\r\n\t\v#!\ =\0\a{'\f'}\r\n{'\t'}\v#! 
\u041F\u0440\u0438\u0432\u0435\u0442=\u041F\u0440\u0438\u0432\u0435\u0442
\u30D5\u30FC\u30D0\u30FC=\u30D5\u30FC\u30D0\u30FC"));
        }

        [Test]
        public void PropertiesWriter_ShouldErrorForInvalidAssigner()
        {
            using var sw = new StringWriter();
            using var writer = new PropertiesWriter(sw);

            Assert.Multiple(() =>
            {
                Assert.Throws<PropertiesException>(() => writer.WriteProperty("key", 'a', "value"));
                Assert.Throws<PropertiesException>(() => writer.WriteProperty("foo", 'フ', "bar"));
                Assert.Throws<PropertiesException>(() => writer.WriteProperty("baz", '\n', "qux"));
            });
        }

        [Test]
        public void PropertiesWriter_ShouldOnlyFlushOnceFlushIntervalIsReached()
        {
            using var sw = new StringWriter();
            using var writer = new PropertiesWriter(sw);

            const string SAMPLE_COMMENT_TEXT = "Example comment";

            for (int i = 0; i < writer.Settings.FlushInterval - 1; i++)
                writer.WriteComment(SAMPLE_COMMENT_TEXT);

            Assert.That(string.IsNullOrEmpty(sw.ToString()));

            writer.WriteComment(SAMPLE_COMMENT_TEXT);

            string[] lines = sw.ToString().TrimEnd(Environment.NewLine.ToCharArray()).Split(Environment.NewLine);
            foreach (string line in lines)
                Assert.That(line, Is.EqualTo($"# {SAMPLE_COMMENT_TEXT}"));
        }
    }
}
