namespace PropertiesDotNet.Test
{
    [TestFixture]
    public class PropertiesDocumentTests
    {
        [Test]
        public void PropertiesDocument_ShouldWritePropertyData()
        {
            var properties = new PropertiesDocument()
            {
                {"key1", "value1"},
                {"key\n2", "value\n2"}
            };

            var propertyWithComments = new PropertiesProperty("propWithComments", ':', "valueForPropWithComments");
            propertyWithComments.AddComment("Property comment");
            properties.Add(propertyWithComments);

            using var sw = new StringWriter();
            properties.Save(sw, false);

            Assert.That(sw.ToString().TrimEnd(Environment.NewLine.ToCharArray()),
                Is.EqualTo(@"key1=value1
key\n2=value\n2
# Property comment
propWithComments:valueForPropWithComments"));
        }

        [Theory]
        public void PropertiesDocument_VerifyPropertiesOnSample1()
        {
            var document = PropertiesDocument.Load(PropertiesReaderTests.Sample1File);

            Assert.That(document, Has.Count.EqualTo(7));
            Assert.Multiple(() =>
            {
                Assert.That(ToString(document[0]), Is.EqualTo("key1=value1"));
                Assert.That(ToString(document[1]), Is.EqualTo("key2:value2"));
                Assert.That(ToString(document[2]), Is.EqualTo("key3 value3"));
                Assert.That(ToString(document[3]), Is.EqualTo("key4=value4"));
                Assert.That(ToString(document[4]), Is.EqualTo("key5=value5"));
                Assert.That(ToString(document[5]), Is.EqualTo("key6=\v\alue6"));
                Assert.That(ToString(document[6]), Is.EqualTo(@": ==\colon\space\equal"));
            });
        }

        [Theory]
        public void PropertiesDocument_VerifyPropertiesOnSample2()
        {
            using var reader = new PropertiesReader(PropertiesReaderTests.Sample2File,
                new PropertiesReaderSettings()
                {
                    AllCharacters = true
                }
            );

            var document = PropertiesDocument.Load(reader);

            Assert.That(document, Has.Count.EqualTo(15));
            Assert.Multiple(() =>
            {
                Assert.That(ToString(document[0]), Is.EqualTo("website=https://en.wikipedia.org/"));
                Assert.That(ToString(document[1]), Is.EqualTo("language:English"));
                Assert.That(ToString(document[2]), Is.EqualTo("topic .properties files"));
                Assert.That(ToString(document[3]), Is.EqualTo("empty"));
                Assert.That(ToString(document[4]), Is.EqualTo("hello=hello"));
                Assert.That(ToString(document[5]), Is.EqualTo("duplicateKey=second"));
                Assert.That(ToString(document[6]), Is.EqualTo(@"delimiterCharacters:= =This is the value for the key ""delimiterCharacters:= """));
                Assert.That(ToString(document[7]), Is.EqualTo("multiline=This line continues"));
                Assert.That(ToString(document[8]), Is.EqualTo(@"path=c:\wiki\templates"));
                Assert.That(ToString(document[9]), Is.EqualTo(@"evenKey=This is on one line\"));
                Assert.That(ToString(document[10]), Is.EqualTo(@"oddKey=This is line one and\# This is line two"));
                Assert.That(ToString(document[11]), Is.EqualTo("welcome=Welcome to Wikipedia!"));
                Assert.That(ToString(document[12]), Is.EqualTo("valueWithEscapes=This is a newline\n and a carriage return\r and a tab\t."));
                Assert.That(ToString(document[13]), Is.EqualTo("encodedHelloInJapanese=こんにちは"));
                Assert.That(ToString(document[14]), Is.EqualTo("helloInJapanese=こんにちは"));
            });
        }

        [Test]
        public void PropertiesDocument_ShouldErrorOnDuplicateKeysWhenRequested()
        {
            Assert.Throws<ArgumentException>(() => PropertiesDocument.Load(PropertiesReaderTests.Sample2File, false));
        }

        [Test]
        public void PropertiesDocument_ShouldOutputTimestamp()
        {
            using var sw = new StringWriter();
            new PropertiesDocument().Save(sw);

            StringAssert.StartsWith("#", sw.ToString());
        }

        [Test]
        public void PropertiesDocument_ShouldChangePropertiesWhenEdited()
        {
            var document = PropertiesDocument.Load(PropertiesReaderTests.Sample1File);
            document["key1"] = "new value";
            Assert.That(document.Remove("key2"));

            using var sw = new StringWriter();
            document.Save(sw, false);

            StringAssert.StartsWith(@"key1=new value
key3 value3", sw.ToString());
        }

        private string ToString(PropertiesProperty property) => $"{property.Key}{(property.Assigner == default ? string.Empty : property.Assigner.ToString())}{property.Value}";
    }
}