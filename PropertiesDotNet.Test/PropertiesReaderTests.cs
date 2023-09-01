using System.Reflection;

namespace PropertiesDotNet.Test
{
    [TestFixture]
    public class PropertiesReaderTests
    {
        #region Samples
        public static readonly string EmptyFile;
        public static readonly string CommentsFile;
        public static readonly string CommentEscapesFile;
        public static readonly string LineBreaksFile;
        public static readonly string Sample1File;
        public static readonly string Sample2File;
        #endregion

        static PropertiesReaderTests()
        {
            string assemblyPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location)!.FullName;
            string testDataDir = Path.Combine(assemblyPath, "data");

            EmptyFile = Path.Combine(testDataDir, "empty.properties");
            CommentsFile = Path.Combine(testDataDir, "comments.properties");
            CommentEscapesFile = Path.Combine(testDataDir, "comment-escapes.properties");
            LineBreaksFile = Path.Combine(testDataDir, "line-breaks.properties");
            Sample1File = Path.Combine(testDataDir, "sample-1.properties");
            Sample2File = Path.Combine(testDataDir, "sample-2.properties");
        }

        #region PropertiesReader
        [Test]
        public void PropertiesReader_ShouldReadCommentsByDefault()
        {
            using var reader = PropertiesReader.FromFile(CommentsFile);

            AssertTokenTypesEqual(
                reader,
                PropertiesTokenType.Comment
            );
        }

        [Test]
        public void PropertiesReader_ShouldRetrieveCommentData()
        {
            using var reader = PropertiesReader.FromFile(CommentsFile);

            AssertTokensEqual(
                reader,
                PropertiesToken.Comment("#this is a comment"),
                PropertiesToken.Comment("!a comment beginning with an exclamation point"),
                PropertiesToken.Comment("!comment with a bunch of leading whitespace"),
                PropertiesToken.Comment("#"),
                PropertiesToken.Comment("!^ blank comment"),
                PropertiesToken.Comment("#not property"),
                PropertiesToken.Comment("#not property"),
                PropertiesToken.Comment("#not property"),
                PropertiesToken.Comment("!still not property"),
                PropertiesToken.Comment("!!not property"),
                PropertiesToken.Comment("###again not property"),
                PropertiesToken.Comment("#comments before values"),
                PropertiesToken.Key("name"), PropertiesToken.Assigner(' '), PropertiesToken.Value("value"),
                PropertiesToken.Comment("!a comment before property with newlines"),
                PropertiesToken.Key("key\nwith\nnewlines"), PropertiesToken.Assigner('='), PropertiesToken.Value("value"),
                PropertiesToken.Comment("#a comment after and before"),
                PropertiesToken.Key("key-no-newlines"), PropertiesToken.Assigner('='), PropertiesToken.Value("Value\nwith\nnewlines"),
                PropertiesToken.Comment("#a comment after property with newlines"),
                PropertiesToken.Key("\\"), PropertiesToken.Value(null)
            );

            AssertEnd(reader);
        }

        [Test]
        public void PropertiesReader_ShouldNotTranslateEscapesInComments()
        {
            using var reader = PropertiesReader.FromFile(CommentEscapesFile,
                new PropertiesReaderSettings()
                {
                    AllUnicodeEscapes = true
                }
            );

            AssertTokensEqual(
                reader,
                PropertiesToken.Comment(@"#\!\#\0\a\f\r\n\t\v\\\""\'\=\:\ \x20\u0020\U00000020\"),
                PropertiesToken.Comment(@"!\!\#\0\a\f\r\n\t\v\\\""\'\=\:\ \x20\u0020\U00000020\")
            );

            AssertEnd(reader);
        }

        [Theory]
        public void PropertiesReader_ShouldNotReadOnEmptyDocument()
        {
            using var reader = PropertiesReader.FromFile(EmptyFile);
            AssertEnd(reader);
        }

        [Theory]
        public void PropertiesReader_VerifyTokensOnValuesWithLineBreaks()
        {
            using var reader = PropertiesReader.FromFile(LineBreaksFile);

            AssertTokensEqual(
                reader,
                PropertiesToken.Key("fruits"), PropertiesToken.Assigner(' '), PropertiesToken.Value("apple, banana, pear, cantaloupe, watermelon, kiwi, mango"),
                PropertiesToken.Key("fruits2"), PropertiesToken.Assigner(' '), PropertiesToken.Value("apple, banana, pear, cantaloupe, watermelon, kiwi, mango")
            );

            AssertEnd(reader);
        }

        [Theory]
        public void PropertiesReader_VerifyTokensOnSample1()
        {
            using var reader = PropertiesReader.FromFile(Sample1File);

            AssertTokensEqual(
                reader,
                PropertiesToken.Comment("#A comment line that starts with '#'."),
                PropertiesToken.Comment("#This is a comment line having leading white spaces."),
                PropertiesToken.Comment("!A comment line that starts with '!'."),
                PropertiesToken.Key("key1"), PropertiesToken.Assigner('='), PropertiesToken.Value("value1"),
                PropertiesToken.Key("key2"), PropertiesToken.Assigner(':'), PropertiesToken.Value("value2"),
                PropertiesToken.Key("key3"), PropertiesToken.Assigner(' '), PropertiesToken.Value("value3"),
                PropertiesToken.Key("key4"), PropertiesToken.Assigner('='), PropertiesToken.Value("value4"),
                PropertiesToken.Key("key5"), PropertiesToken.Assigner('='), PropertiesToken.Value("value5"),
                PropertiesToken.Key("key6"), PropertiesToken.Assigner('='), PropertiesToken.Value("\v\alue6"),
                PropertiesToken.Key(": ="), PropertiesToken.Assigner('='), PropertiesToken.Value("\\colon\\space\\equal")
            );

            AssertEnd(reader);
        }

        [Theory]
        public void PropertiesReader_VerifyTokensOnSample2()
        {
            using var reader = PropertiesReader.FromFile(Sample2File,
                new PropertiesReaderSettings()
                {
                    AllCharacters = true
                }
            );

            AssertTokensEqual(
                reader,
                PropertiesToken.Comment("#You are reading a comment in \".properties\" file."),
                PropertiesToken.Comment("!The exclamation mark can also be used for comments."),
                PropertiesToken.Comment("#Lines with \"properties\" contain a key and a value separated by a delimiting character."),
                PropertiesToken.Comment(@"#There are 3 delimiting characters: '=' (equal), ':' (colon) and whitespace (space, \t and \f)."),
                PropertiesToken.Key("website"), PropertiesToken.Assigner('='), PropertiesToken.Value("https://en.wikipedia.org/"),
                PropertiesToken.Key("language"), PropertiesToken.Assigner(':'), PropertiesToken.Value("English"),
                PropertiesToken.Key("topic"), PropertiesToken.Assigner(' '), PropertiesToken.Value(".properties files"),
                PropertiesToken.Comment("#A word on a line will just create a key with no value."),
                PropertiesToken.Key("empty"), PropertiesToken.Value(null),
                PropertiesToken.Comment("#White space that appears between the key, the value and the delimiter is ignored."),
                PropertiesToken.Comment("#This means that the following are equivalent (other than for readability)."),
                PropertiesToken.Key("hello"), PropertiesToken.Assigner('='), PropertiesToken.Value("hello"),
                PropertiesToken.Key("hello"), PropertiesToken.Assigner('='), PropertiesToken.Value("hello"),
                PropertiesToken.Comment("#Keys with the same name will be overwritten by the key that is the furthest in a file."),
                PropertiesToken.Comment("#For example the final value for \"duplicateKey\" will be \"second\"."),
                PropertiesToken.Key("duplicateKey"), PropertiesToken.Assigner('='), PropertiesToken.Value("first"),
                PropertiesToken.Key("duplicateKey"), PropertiesToken.Assigner('='), PropertiesToken.Value("second"),
                PropertiesToken.Comment("#To use the delimiter characters inside a key, you need to escape them with a \\."),
                PropertiesToken.Comment("#However, there is no need to do this in the value."),
                PropertiesToken.Key("delimiterCharacters:= "), PropertiesToken.Assigner('='), PropertiesToken.Value(@"This is the value for the key ""delimiterCharacters:= """),
                PropertiesToken.Comment("#Adding a \\ at the end of a line means that the value continues to the next line."),
                PropertiesToken.Key("multiline"), PropertiesToken.Assigner('='), PropertiesToken.Value("This line continues"),
                PropertiesToken.Comment("#If you want your value to include a \\, it should be escaped by another \\."),
                PropertiesToken.Key("path"), PropertiesToken.Assigner('='), PropertiesToken.Value(@"c:\wiki\templates"),
                PropertiesToken.Comment("#This means that if the number of \\ at the end of the line is even, the next line is not included in the value. "),
                PropertiesToken.Comment("#In the following example, the value for \"evenKey\" is \"This is on one line\\\"."),
                PropertiesToken.Key("evenKey"), PropertiesToken.Assigner('='), PropertiesToken.Value("This is on one line\\"),
                PropertiesToken.Comment("#This line is a normal comment and is not included in the value for \"evenKey\""),
                PropertiesToken.Comment("#If the number of \\ is odd, then the next line is included in the value."),
                PropertiesToken.Comment("#In the following example, the value for \"oddKey\" is \"This is line one and\\#This is line two\"."),
                PropertiesToken.Key("oddKey"), PropertiesToken.Assigner('='), PropertiesToken.Value(@"This is line one and\# This is line two"),
                PropertiesToken.Comment("#White space characters are removed before each line."),
                PropertiesToken.Comment("#Make sure to add your spaces before your \\ if you need them on the next line."),
                PropertiesToken.Comment("#In the following example, the value for \"welcome\" is \"Welcome to Wikipedia!\"."),
                PropertiesToken.Key("welcome"), PropertiesToken.Assigner('='), PropertiesToken.Value("Welcome to Wikipedia!"),
                PropertiesToken.Comment(@"#If you need to add newlines and carriage returns, they need to be escaped using \n and \r respectively."),
                PropertiesToken.Comment(@"#You can also optionally escape tabs with \t for readability purposes."),
                PropertiesToken.Key("valueWithEscapes"), PropertiesToken.Assigner('='), PropertiesToken.Value("This is a newline\n and a carriage return\r and a tab\t."),
                PropertiesToken.Comment("#You can also use Unicode escape characters (maximum of four hexadecimal digits)."),
                PropertiesToken.Comment("#In the following example, the value for \"encodedHelloInJapanese\" is \"こんにちは\"."),
                PropertiesToken.Key("encodedHelloInJapanese"), PropertiesToken.Assigner('='), PropertiesToken.Value("\u3053\u3093\u306b\u3061\u306f"),
                PropertiesToken.Comment("#But with more modern file encodings like UTF-8, you can directly use supported characters."),
                PropertiesToken.Key("helloInJapanese"), PropertiesToken.Assigner('='), PropertiesToken.Value("こんにちは")
            );

            AssertEnd(reader);
        }
        #endregion

        #region UnsafePropertiesReader
        [Test]
        public void UnsafePropertiesReader_ShouldReadCommentsByDefault()
        {
            using var reader = new UnsafePropertiesReader(File.ReadAllText(CommentsFile));

            AssertTokenTypesEqual(
                reader,
                PropertiesTokenType.Comment
            );
        }

        [Test]
        public void UnsafePropertiesReader_ShouldRetrieveCommentData()
        {
            using var reader = new UnsafePropertiesReader(File.ReadAllText(CommentsFile));

            AssertTokensEqual(
                reader,
                PropertiesToken.Comment("#this is a comment"),
                PropertiesToken.Comment("!a comment beginning with an exclamation point"),
                PropertiesToken.Comment("!comment with a bunch of leading whitespace"),
                PropertiesToken.Comment("#"),
                PropertiesToken.Comment("!^ blank comment"),
                PropertiesToken.Comment("#not property"),
                PropertiesToken.Comment("#not property"),
                PropertiesToken.Comment("#not property"),
                PropertiesToken.Comment("!still not property"),
                PropertiesToken.Comment("!!not property"),
                PropertiesToken.Comment("###again not property"),
                PropertiesToken.Comment("#comments before values"),
                PropertiesToken.Key("name"), PropertiesToken.Assigner(' '), PropertiesToken.Value("value"),
                PropertiesToken.Comment("!a comment before property with newlines"),
                PropertiesToken.Key("key\nwith\nnewlines"), PropertiesToken.Assigner('='), PropertiesToken.Value("value"),
                PropertiesToken.Comment("#a comment after and before"),
                PropertiesToken.Key("key-no-newlines"), PropertiesToken.Assigner('='), PropertiesToken.Value("Value\nwith\nnewlines"),
                PropertiesToken.Comment("#a comment after property with newlines"),
                PropertiesToken.Key("\\"), PropertiesToken.Value(null)
            );

            AssertEnd(reader);
        }

        [Test]
        public void UnsafePropertiesReader_ShouldNotTranslateEscapesInComments()
        {
            using var reader = new UnsafePropertiesReader(File.ReadAllText(CommentEscapesFile),
                new PropertiesReaderSettings()
                {
                    AllUnicodeEscapes = true
                }
            );

            AssertTokensEqual(
                reader,
                PropertiesToken.Comment(@"#\!\#\0\a\f\r\n\t\v\\\""\'\=\:\ \x20\u0020\U00000020\"),
                PropertiesToken.Comment(@"!\!\#\0\a\f\r\n\t\v\\\""\'\=\:\ \x20\u0020\U00000020\")
            );

            AssertEnd(reader);
        }

        [Theory]
        public void UnsafePropertiesReader_ShouldNotReadOnEmptyDocument()
        {
            using var reader = new UnsafePropertiesReader(File.ReadAllText(EmptyFile));
            AssertEnd(reader);
        }

        [Theory]
        public void UnsafePropertiesReader_VerifyTokensOnValuesWithLineBreaks()
        {
            using var reader = new UnsafePropertiesReader(File.ReadAllText(LineBreaksFile));

            AssertTokensEqual(
                reader,
                PropertiesToken.Key("fruits"), PropertiesToken.Assigner(' '), PropertiesToken.Value("apple, banana, pear, cantaloupe, watermelon, kiwi, mango"),
                PropertiesToken.Key("fruits2"), PropertiesToken.Assigner(' '), PropertiesToken.Value("apple, banana, pear, cantaloupe, watermelon, kiwi, mango")
            );

            AssertEnd(reader);
        }

        [Theory]
        public void UnsafePropertiesReader_VerifyTokensOnSample1()
        {
            using var reader = new UnsafePropertiesReader(File.ReadAllText(Sample1File));

            AssertTokensEqual(
                reader,
                PropertiesToken.Comment("#A comment line that starts with '#'."),
                PropertiesToken.Comment("#This is a comment line having leading white spaces."),
                PropertiesToken.Comment("!A comment line that starts with '!'."),
                PropertiesToken.Key("key1"), PropertiesToken.Assigner('='), PropertiesToken.Value("value1"),
                PropertiesToken.Key("key2"), PropertiesToken.Assigner(':'), PropertiesToken.Value("value2"),
                PropertiesToken.Key("key3"), PropertiesToken.Assigner(' '), PropertiesToken.Value("value3"),
                PropertiesToken.Key("key4"), PropertiesToken.Assigner('='), PropertiesToken.Value("value4"),
                PropertiesToken.Key("key5"), PropertiesToken.Assigner('='), PropertiesToken.Value("value5"),
                PropertiesToken.Key("key6"), PropertiesToken.Assigner('='), PropertiesToken.Value("\v\alue6"),
                PropertiesToken.Key(": ="), PropertiesToken.Assigner('='), PropertiesToken.Value("\\colon\\space\\equal")
            );

            AssertEnd(reader);
        }

        [Theory]
        public void UnsafePropertiesReader_VerifyTokensOnSample2()
        {
            using var reader = new UnsafePropertiesReader(File.ReadAllText(Sample2File),
                new PropertiesReaderSettings()
                {
                    AllCharacters = true
                }
            );

            AssertTokensEqual(
                reader,
                PropertiesToken.Comment("#You are reading a comment in \".properties\" file."),
                PropertiesToken.Comment("!The exclamation mark can also be used for comments."),
                PropertiesToken.Comment("#Lines with \"properties\" contain a key and a value separated by a delimiting character."),
                PropertiesToken.Comment(@"#There are 3 delimiting characters: '=' (equal), ':' (colon) and whitespace (space, \t and \f)."),
                PropertiesToken.Key("website"), PropertiesToken.Assigner('='), PropertiesToken.Value("https://en.wikipedia.org/"),
                PropertiesToken.Key("language"), PropertiesToken.Assigner(':'), PropertiesToken.Value("English"),
                PropertiesToken.Key("topic"), PropertiesToken.Assigner(' '), PropertiesToken.Value(".properties files"),
                PropertiesToken.Comment("#A word on a line will just create a key with no value."),
                PropertiesToken.Key("empty"), PropertiesToken.Value(null),
                PropertiesToken.Comment("#White space that appears between the key, the value and the delimiter is ignored."),
                PropertiesToken.Comment("#This means that the following are equivalent (other than for readability)."),
                PropertiesToken.Key("hello"), PropertiesToken.Assigner('='), PropertiesToken.Value("hello"),
                PropertiesToken.Key("hello"), PropertiesToken.Assigner('='), PropertiesToken.Value("hello"),
                PropertiesToken.Comment("#Keys with the same name will be overwritten by the key that is the furthest in a file."),
                PropertiesToken.Comment("#For example the final value for \"duplicateKey\" will be \"second\"."),
                PropertiesToken.Key("duplicateKey"), PropertiesToken.Assigner('='), PropertiesToken.Value("first"),
                PropertiesToken.Key("duplicateKey"), PropertiesToken.Assigner('='), PropertiesToken.Value("second"),
                PropertiesToken.Comment("#To use the delimiter characters inside a key, you need to escape them with a \\."),
                PropertiesToken.Comment("#However, there is no need to do this in the value."),
                PropertiesToken.Key("delimiterCharacters:= "), PropertiesToken.Assigner('='), PropertiesToken.Value(@"This is the value for the key ""delimiterCharacters:= """),
                PropertiesToken.Comment("#Adding a \\ at the end of a line means that the value continues to the next line."),
                PropertiesToken.Key("multiline"), PropertiesToken.Assigner('='), PropertiesToken.Value("This line continues"),
                PropertiesToken.Comment("#If you want your value to include a \\, it should be escaped by another \\."),
                PropertiesToken.Key("path"), PropertiesToken.Assigner('='), PropertiesToken.Value(@"c:\wiki\templates"),
                PropertiesToken.Comment("#This means that if the number of \\ at the end of the line is even, the next line is not included in the value. "),
                PropertiesToken.Comment("#In the following example, the value for \"evenKey\" is \"This is on one line\\\"."),
                PropertiesToken.Key("evenKey"), PropertiesToken.Assigner('='), PropertiesToken.Value("This is on one line\\"),
                PropertiesToken.Comment("#This line is a normal comment and is not included in the value for \"evenKey\""),
                PropertiesToken.Comment("#If the number of \\ is odd, then the next line is included in the value."),
                PropertiesToken.Comment("#In the following example, the value for \"oddKey\" is \"This is line one and\\#This is line two\"."),
                PropertiesToken.Key("oddKey"), PropertiesToken.Assigner('='), PropertiesToken.Value(@"This is line one and\# This is line two"),
                PropertiesToken.Comment("#White space characters are removed before each line."),
                PropertiesToken.Comment("#Make sure to add your spaces before your \\ if you need them on the next line."),
                PropertiesToken.Comment("#In the following example, the value for \"welcome\" is \"Welcome to Wikipedia!\"."),
                PropertiesToken.Key("welcome"), PropertiesToken.Assigner('='), PropertiesToken.Value("Welcome to Wikipedia!"),
                PropertiesToken.Comment(@"#If you need to add newlines and carriage returns, they need to be escaped using \n and \r respectively."),
                PropertiesToken.Comment(@"#You can also optionally escape tabs with \t for readability purposes."),
                PropertiesToken.Key("valueWithEscapes"), PropertiesToken.Assigner('='), PropertiesToken.Value("This is a newline\n and a carriage return\r and a tab\t."),
                PropertiesToken.Comment("#You can also use Unicode escape characters (maximum of four hexadecimal digits)."),
                PropertiesToken.Comment("#In the following example, the value for \"encodedHelloInJapanese\" is \"こんにちは\"."),
                PropertiesToken.Key("encodedHelloInJapanese"), PropertiesToken.Assigner('='), PropertiesToken.Value("\u3053\u3093\u306b\u3061\u306f"),
                PropertiesToken.Comment("#But with more modern file encodings like UTF-8, you can directly use supported characters."),
                PropertiesToken.Key("helloInJapanese"), PropertiesToken.Assigner('='), PropertiesToken.Value("こんにちは")
            );

            AssertEnd(reader);
        }
        #endregion

        #region Assertion Utilities
        private void AssertEnd(IPropertiesReader reader) => Assert.That(reader.MoveNext(), Is.False);

        private void AssertTokenTypesEqual(IPropertiesReader reader, params PropertiesTokenType[] tokenTypes)
        {
            for (int i = 0; i < tokenTypes.Length; i++)
                Assert.That(reader.Read().Type, Is.EqualTo(tokenTypes[i]));
        }

        private void AssertTokensEqual(IPropertiesReader reader, params PropertiesToken[] tokens)
        {
            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];

                if (token.Type == PropertiesTokenType.Comment)
                {
                    while (reader.Token.Type == PropertiesTokenType.None && reader.MoveNext())
                        ;

                    char? commentHandle = reader is PropertiesReader propReader ?
                        propReader.CommentHandle : ((reader is UnsafePropertiesReader unsafeReader) ?
                        unsafeReader.CommentHandle : null);

                    if (commentHandle.HasValue)
                    {
                        Assert.That(commentHandle.Value, Is.EqualTo(token.Text?[0]));

                        if (!string.IsNullOrEmpty(token.Text))
                            token = PropertiesToken.Comment(token.Text[1..]);
                    }
                }

                Assert.That(reader.Read(), Is.EqualTo(token));
            }
        }
        #endregion
    }
}