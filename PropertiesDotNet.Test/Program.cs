using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Xml;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using Kajabity.Tools.Java;

using Newtonsoft.Json;

using PropertiesDotNet.Core;
using PropertiesDotNet.ObjectModel;
using PropertiesDotNet.Serialization;
using PropertiesDotNet.Serialization.ObjectProviders;
using PropertiesDotNet.Serialization.PropertiesTree;
using PropertiesDotNet.Utils;

namespace PropertiesDotNet.Test
{
    public class Program
    {
        /// <summary>
        /// Properties source
        /// </summary>
        internal const string SOURCE =
            @"# You are reading a comment in "".properties"" file.
! The exclamation mark can also be used for comments.
# Lines with ""properties"" contain a key and a value separated by a delimiting character.
# There are 3 delimiting characters: '=' (equal), ':' (colon) and whitespace (space, \t and \f).
website = https://en.wikipedia.org/
language : English
topic .properties files
# A word on a line will just create a key with no value.
empty
# White space that appears between the key, the value and the delimiter is ignored.
# This means that the following are equivalent (other than for readability).
hello=hello
#hello = hello
# Keys with the same name will be overwritten by the key that is the furthest in a file.
# For example the final value for ""duplicateKey"" will be ""second"".
duplicateKey = first
duplicateKey = second
# To use the delimiter characters inside a key, you need to escape them with a \.
# However, there is no need to do this in the value.
delimiterCharacters\:\=\ = This is the value for the key ""delimiterCharacters\:\=\ ""
# Adding a \ at the end of a line means that the value continues to the next line.
multiline = This line \
continues
# If you want your value to include a \, it should be escaped by another \.
path = c:\\wiki\\templates
# This means that if the number of \ at the end of the line is even, the next line is not included in the value. 
# In the following example, the value for ""evenKey"" is ""This is on one line\"".
evenKey = This is on one line\\
# This line is a normal comment and is not included in the value for ""evenKey""
# If the number of \ is odd, then the next line is included in the value.
# In the following example, the value for ""oddKey"" is ""This is line one and\#This is line two"".
oddKey = This is line one and\\\
# This is line two
# White space characters are removed before each line.
# Make sure to add your spaces before your \ if you need them on the next line.
# In the following example, the value for ""welcome"" is ""Welcome to Wikipedia!"".
welcome = Welcome to \
          Wikipedia!
# If you need to add newlines and carriage returns, they need to be escaped using \n and \r respectively.
# You can also optionally escape tabs with \t for readability purposes.
valueWithEscapes = This is a newline\n and a carriage return\r and a tab\t.
# You can also use Unicode escape characters (maximum of four hexadecimal digits).
# In the following example, the value for ""encodedHelloInJapanese"" is ""e"".
encodedHelloInJapanese = \u3053\u3093\u306b\u3061\u306f
# But with more modern file encodings like UTF-8, you can directly use supported characters.
helloInJapanese = e";

        internal const string JSON_SOURCE = @"{""widget"": {
    ""debug"": ""on"",
    ""window"": {
        ""title"": ""Sample Konfabulator Widget"",
        ""name"": ""main_window"",
        ""width"": 500,
        ""height"": 500
    },
    ""image"": { 
        ""src"": ""Images/Sun.png"",
        ""name"": ""sun1"",
        ""hOffset"": 250,
        ""vOffset"": 250,
        ""alignment"": ""center"",
        ""width"": 500,
        ""height"": 500,
        ""title"": ""Sample Konfabulator Widget"",
    }
}}";

        public const string SIMPLE_SOURCE = @"# A comment line that starts with '#'.
   # This is a comment line having leading white spaces.
! A comment line that starts with '!'.

key\r\n1=value1
  key2 :       value2
    key3             value3
key\
  4=value\
    4
\u006B\u0065\u00795=\u0076\u0061\u006c\u0075\u00655
\k\e\y\6=\v\a\lu\e\6

\:\ \== \\colon\\space\\equal
";
        internal const string JSON_SOURCE_2 = @" ";
        internal const string SERI_SOURCE = @"hello.world.bye = 5
age= 9999
world.me = text
hello.baby.'. = wow.e
hello.baby.zook = sad_face


world.age = 27

world.third = 3
#cmt
world.list1[0] = 3
world.list1[1] = 3
world.list1[2] = 3
world.list1[3] = 3
world.list2.0 = 3
world.list2.1 = 3
world.list2.2 = 3
world.list2.3 = -53e2$";

        internal const string OTHER_SERI_SOURCE = @"Age = 5
Name = Timmy
friends = ['Johnny', 'Theodore']
Parents.mom=elsa
Parents.dad=brayden";

        private class CustomReader : IPropertiesReader
        {
            public PropertiesReaderSettings Settings { get; set; }
            public PropertiesToken Token => _innerReader.Token;
            public StreamMark? TokenStart { get; }
            public StreamMark? TokenEnd { get; }
            public bool HasLineInfo { get; }

            public event TokenRead TokenRead;

            private readonly IPropertiesReader _innerReader;

            public CustomReader(string doc) : this(new PropertiesReader(doc))
            {

            }

            public CustomReader(IPropertiesReader innerReader)
            {
                _innerReader = innerReader;
            }

            public bool MoveNext()
            {
                while (_innerReader.MoveNext())
                {
                    var token = _innerReader.Token;

                    switch (token.Type)
                    {
                        case PropertiesTokenType.Key:
                            if (!token.Text.Contains("list2"))
                            {
                                if (_innerReader.MoveNext() && _innerReader.Token.Type == PropertiesTokenType.Assigner)
                                    _innerReader.MoveNext();
                                break;
                            }

                            return true;
                        case PropertiesTokenType.Assigner:
                        case PropertiesTokenType.Value:
                            return true;
                        default:
                            continue;
                    }
                }

                return false;
            }

            public void Dispose()
            {
                _innerReader.Dispose();
            }
        }

        private class Player
        {
            public string Name { get; set; }
            public int Age;
            public string Friends { get; set; }
            public Dictionary<string, string> Parents { get; set; }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Name: {Name}");
                sb.AppendLine($"Age: ({Age.GetType().Name}) {Age}");
                sb.AppendLine($"Friends: {Friends}");
                sb.AppendLine($"Len: {Parents.Count}");
                WriteTree(Parents);
                return sb.ToString();
            }
        }

        public static readonly Stream SOURCE_STREAM = ToStream(SOURCE);
        public static readonly Stream SIMPLE_DOC_STREAM = ToStream(SIMPLE_SOURCE);
        static unsafe void Main(string[] args)
        {

            var re = new UnsafePropertiesReader(SOURCE);
            while (re.MoveNext())
            {
                switch (re.Token.Type)
                {
                    case PropertiesTokenType.Key:
                    case PropertiesTokenType.Assigner:
                        Console.Write(re.Token.Text);
                        break;
                    case PropertiesTokenType.Value:
                    case PropertiesTokenType.Comment:
                        Console.WriteLine(re.Token.Text);
                        break;
                }
            }

            ////Console.WriteLine(SOURCE);
            //// Setting for whether to align logical line
            //// ^ Beautify?
            ////


            ////Console.WriteLine("first----------------------------");
            //var provider = new ExpressionObjectProvider();
            ////IPropertiesReader obj = provider.Construct<IPropertiesReader>();
            //var r = provider.Construct<PropertiesReader>(new Type[] { typeof(TextReader), typeof(PropertiesReaderSettings) }, new object[] {new StringReader(SOURCE), null});
            //r.MoveNext();
            //Console.WriteLine(r.Token);
            //r.MoveNext();
            //Console.WriteLine(r.Token);
            //r.MoveNext();
            //Console.WriteLine(r.Token);

            //var token = reader.Token;
            //while (reader.MoveNext())
            //{
            //    token = reader.Token;

            //    switch (token.Type)
            //    {
            //        case PropertiesTokenType.Assigner:
            //        case PropertiesTokenType.Key:
            //            Console.Write(token.Text);
            //            break;

            //        case PropertiesTokenType.Value:
            //        case PropertiesTokenType.Comment:
            //            if(token.Type == PropertiesTokenType.Comment)
            //                Console.Write(((PropertiesReader)reader).CommentHandle + " ");
            //            Console.WriteLine(token.Text);
            //            break;
            //    }
            //}



            //Console.WriteLine("----------------------------------------------------------------------------------------------------");
            //StringBuilder sb = new StringBuilder();
            //var writer = new PropertiesWriter(new StringWriter(sb));
            //writer.TokenWritten += Writer_EventWritten;
            //writer.WriteComment("Chisen");
            //writer.WriteComment("Chisen 2");
            //writer.WriteKey("ChisenKey");
            //writer.WriteValue("ChisenKeyVal");
            //writer.WriteComment("Chisen Com after val");
            //writer.WriteKey("ChisenSecond");
            //writer.WriteValue("ChisenSecondVal");
            //writer.WriteKey("ChisenSecondAfterKey");
            //writer.WriteValue("ChisenSecondAfterKeyVal");
            //writer.WriteKey(" KeyStartw/whitespace");
            //writer.WriteValue(" valStartw/backwhitespace");
            //writer.WriteKey(" #EEEEZZZZZKeyStart#w/backwhitespaceLogical\n!LinesLol", false);
            //writer.WriteValue(" #EEEEKeyStartw/backwhitespaceLogical\n#Lines even more LOL", true);
            //writer.WriteComment("Chisen Com after val");
            //writer.WriteKey("#KeyStart#w/backwhitespaceLogical\n!LinesLol", true);
            //writer.WriteValue("!!###KeyStartw/backwhitespaceLogical\n#Lines even more LOL", true);
            //writer.WriteComment("Chisen Com after val");
            //writer.Dispose();
            //Console.WriteLine(sb);
            //Console.WriteLine("----------------------------------------------------------------------------------------------------");
            ////var e = fastJSON.JSON.Parse(JSON_SOURCE);
            //BenchmarkRunner.Run<BenchmarkTester>();
            Console.WriteLine("---------------------------------------------");
            var writer = new PropertiesWriter(Console.Out);
            writer.TokenWritten += (w, token) =>
            {
                if (token.Type == PropertiesTokenType.Comment)
                    Console.WriteLine("'" + token.Text + "'");
            };
            writer.Write(PropertiesToken.Comment("Chisen Good morning\r\n\f world! "));
            writer.Write(PropertiesToken.Comment("\nChisen lol"));
            writer.Write(PropertiesToken.Key("CHKey"));
            writer.Flush();

            Console.WriteLine("---------------=-=-=-=-=-=-=-=-=-=-=-=-=-------------------");

            var value = PropertiesSerializer.Deserialize<Dictionary<string, object>>(SERI_SOURCE);
            WriteTree(value);
            foreach (var node in value)
            {
                Console.WriteLine($"{node.Key} ++--++ {node.Value}");
            }
            var realValue = PropertiesSerializer.Deserialize<Dictionary<string, Dictionary<string, Dictionary<int, decimal>>>>(new CustomReader(SERI_SOURCE));
            WriteTree(realValue);

            var newValue = PropertiesSerializer.Deserialize<Dictionary<string, object>>(OTHER_SERI_SOURCE);
            WriteTree(newValue);

            var player = PropertiesSerializer.Deserialize<Player>(OTHER_SERI_SOURCE);
            Console.WriteLine(player);

            //var tree = composer.ReadTree(PropertiesDocument.Load(new PropertiesReader(SERI_SOURCE)));

            //WriteTree(tree);

            // PropertiesObject
            // PropertiesPrimitive
            // 
            // IPrimitiveSerializer/Converter
            // object? Deserialze(string source)
            //
            // IObjectSerializer/Converter
            // object? Deserialize(serializer, pObject, 
            //Console.WriteLine(re.TokenEnd);

            ////var parser = new PropertiesReader(new StringReader(SOURCE));
            ////while (parser.MoveNext())
            ////{
            ////    //Console.Write(parser.TokenStart + " - " + parser.TokenEnd);
            ////    switch (parser.Token.Type)
            ////    {
            ////        case PropertiesTokenType.Assigner:
            ////            Console.Write("\\" + parser.Token.Text);
            ////            break;
            ////        case PropertiesTokenType.Key:
            ////            Console.Write(parser.Token.Text);

            ////            if(parser.Token.Text == "key4")
            ////                Console.Write("  " + parser.TokenStart + " ::::::: " + parser.TokenEnd);
            ////            break;

            ////        case PropertiesTokenType.Comment:
            ////        case PropertiesTokenType.Text:
            ////            if (parser.Token.Type == PropertiesTokenType.Comment)
            ////                Console.Write("  " + parser.TokenStart + " ::::::: " + parser.TokenEnd);
            ////            Console.WriteLine("'" + parser.Token.Text + "'");
            ////            break;
            ////    }
            ////}
            //var doc = PropertiesDocument.Load(SOURCE);

            ////foreach(var prop in doc)
            ////{
            ////    Console.WriteLine(prop);
            ////}
            ////doc["welcome"] = " hello\\";

            ////Console.WriteLine();
            ////Console.WriteLine();
            ////doc.Save(Console.Out);
            ////Console.WriteLine(Console.OutputEncoding.EncodingName);
            ////Console.WriteLine(TimeZoneInfo.Local.DaylightName);
            ////Console.WriteLine(TimeZoneInfo.Local.DisplayName);
            ////Console.WriteLine(TimeZoneInfo.Local.StandardName);
            ////Console.WriteLine("----------------------------------------------------------------------------------------------------");

            ////Console.WriteLine("Real Length: " + SOURCE.Length);
            ////Console.WriteLine();
            ////Console.WriteLine();

            //IPropertiesReader parser = new PropertiesReader(new UnsafeStringReader(SOURCE));
            //PropertiesToken lastToken = default;
            //while (parser.MoveNext())
            //{
            //    lastToken = parser.Token;
            //    switch (lastToken.Type)
            //    {
            //        case PropertiesTokenType.Key:
            //        case PropertiesTokenType.Assigner:
            //            Console.Write( lastToken.Text);
            //            break;
            //        case PropertiesTokenType.Comment:
            //        case PropertiesTokenType.Value:
            //            Console.WriteLine(lastToken.Text);
            //            break;
            //    }
            //}


            //Console.WriteLine("PropertiesReader");
            //Console.WriteLine(lastToken);
            //Console.WriteLine("Start: " + parser.TokenStart);
            //Console.WriteLine("End: " + parser.TokenEnd);
            //Console.WriteLine();

            //parser = new UnsafePropertiesReader(SOURCE);
            //while (parser.MoveNext())
            //{
            //    lastToken = parser.Token;
            //}

            //Console.WriteLine("UnsafePropertiesReader");
            //Console.WriteLine(lastToken);
            //Console.WriteLine("Start: " + parser.TokenStart);
            //Console.WriteLine("End: " + parser.TokenEnd);
            //Console.WriteLine();

            //// TODO: This does not match
            //// ]arw;\sfl
            //// ]pgfa;d'
            //// Offser :/
            //// Normal reader is 1 off but UnsafeReader is like 11 off
            //Console.WriteLine("Len: " + SOURCE.Length);
            //string c = "\U0010FFFF";
            //Console.WriteLine(c.Length);
            //Console.WriteLine(c[0].ToString());
            //Console.WriteLine($"string c = \"\U0010FFFF\";");
            //Console.WriteLine('\a');
            //Console.WriteLine("-------------------");
            //Console.WriteLine(nameof(StreamMark) + sizeof(StreamMark));
            //Console.WriteLine(nameof(PropertiesToken) + sizeof(PropertiesToken));
            //Console.Read();
        }

        private static void WriteTree(PropertiesTreeNode node, int depth = 0)
        {
            for (int i = 0; i < depth; i++)
                Console.Write("  ");

            if (node is PropertiesObject objNode)
            {
                Console.WriteLine(objNode.Name ?? "<root>");
                depth++;
                foreach (var item in objNode)
                {
                    WriteTree(item, depth);
                }
            }
            else
            {
                var prop = node as PropertiesPrimitive;
                Console.WriteLine(prop.Name + "=" + prop.Value);
            }
        }

        private static void WriteTree(IDictionary node, int depth = 0)
        {
            foreach (DictionaryEntry item in node)
            {
                for (int i = 0; i < depth; i++)
                    Console.Write("  ");

                if (depth > 0)
                    Console.Write("- ");

                Console.Write($"({item.Key.GetType().Name}) {item.Key}" );
                if (item.Value is IDictionary dic)
                {
                    Console.WriteLine();
                    WriteTree(dic, depth + 1);
                }
                else
                {
                    Console.WriteLine($"= ({item.Value.GetType().Name}) {item.Value}");
                }
            }
        }

        private static void Writer_EventWritten(IPropertiesWriter writer, PropertiesTokenType tokenType, string? tokenValue)
        {
            Console.WriteLine($"Wrote: {tokenType}");
        }

        public static Stream ToStream(string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static Func<T> Creator<T>()
        {
            Type t = typeof(T);
            if (t == typeof(string))
                return Expression.Lambda<Func<T>>(Expression.Constant(string.Empty)).Compile();

            if (t.IsValueType)
                return Expression.Lambda<Func<T>>(Expression.New(t)).Compile();

            return null;
        }
    }

    public class BaseEvent
    {
        public event Func<object, string> BaseEvt;

        SubEvent evt;

        public BaseEvent(SubEvent evt)
        {
            this.evt = evt;
        }

        public void CallBase(out string out1, out string out2)
        {
            evt.SubEvt += BaseEvt;
            out1 = evt.Call();
            evt.SubEvt -= BaseEvt;
            out2 = evt.Call();
        }
    }

    public class SubEvent
    {
        public event Func<object, string> SubEvt;

        public string Call()
        {
            return SubEvt?.Invoke(this);
        }
    }

    [MinColumn]
    [MaxColumn]
    [MemoryDiagnoser]
    public class BenchmarkTester
    {
        //    private readonly FeatureTester tester = new FeatureTester();
        //    private readonly PropertiesSerializer serializer = new PropertiesSerializer();

        //    public BenchmarkTester()
        //    {
        //        Console.WriteLine("Created!");

        //        using (var fs = File.Open("hello.txt", FileMode.OpenOrCreate, FileAccess.Write))
        //        {
        //            using (var stream = new StreamWriter(fs))
        //            {
        //                stream.Write(Program.SOURCE);
        //                stream.Flush();
        //            }
        //        }
        //    }

        //    //[Benchmark]
        //    public void Kapjemimi_WRITE()
        //    {
        //        //JavaProperties props = new JavaProperties();

        //        ////            props.Add("key1", "value1");
        //        ////            props.Add("key2", "value2");
        //        ////            props.Add("key3", "value3");
        //        ////            props.Add("key\n4", "value\n4");
        //        ////            props.Add("\u006B\u0065\u00795", "\u0076\u0061\u006c\u0075\u00655");
        //        ////            props.Add("\\k\\e\\y\\6", "\\v\\a\\l\\u\\e\\6");
        //        ////            props.Add(": =", "\\colon\\space\\equal");

        //        //using var fs = File.Open("hello.txt", FileMode.Open, FileAccess.Read);
        //        ////            props.Store(fs, @"A comment line that starts with '#'.
        //        ////# This is a comment line having leading white spaces.
        //        ////! A comment line that starts with '!'.");

        //        //props.Load(fs);
        //        using var stream = new StringReader(Program.SOURCE);
        //        _buffer = new char[16];

        //        for (int i = 0; i < 16; i++)
        //        {
        //            int read = stream.Read();

        //            if (read < 0)
        //                break;

        //            _buffer[i] = (char)read;
        //        }
        //    }

        //    [Benchmark]
        //    public void DeseriRefl()
        //    {
        //        serializer.Settings.ObjectProvider = reflProvder;
        //        object? obj = serializer.Deserialize(FeatureTester.content);
        //    }

        //    [Benchmark]
        //    public void DeseriDynam()
        //    {
        //        serializer.Settings.ObjectProvider = dynamicProvder;
        //        object? obj = serializer.Deserialize(FeatureTester.content);
        //    }

        //    [Benchmark]
        //    public void DeseriExpr()
        //    {
        //        serializer.Settings.ObjectProvider = exprProvder;
        //        object? obj = serializer.Deserialize(FeatureTester.content);
        //    }

        //    // [Benchmark]
        //    public void NormalCtor()
        //    {
        //        new StreamMark(1, 1, 1);
        //    }

        //   // [Benchmark]
        //    public void ReflCtor()
        //    {
        //        reflProvder.Construct(typeof(StreamMark));
        //    }

        //    //[Benchmark]
        //    public void ExprCtor()
        //    {
        //        exprProvder.Construct(typeof(StreamMark));
        //    }

        //    char[] _buffer;

        //[Benchmark]
        //public void DynamicCtorRead()
        //{
        //    IPropertiesReader reader = new PropertiesReader(Program.SOURCE);
        //    while (!(reader.Read() is null))
        //    {
        //    }
        //}

        //[Benchmark]
        //public void DynamicCtor()
        //{
        //    //        //using var fs = File.Open("hello-pdn.properties", FileMode.OpenOrCreate, FileAccess.Write);
        //    //        //PropertiesWriter writer = new PropertiesWriter(fs);

        //    //        //var s = FeatureTester.content;

        //    StringBuilder output = new StringBuilder();
        //    PropertiesWriter writer = new PropertiesWriter(output);

        //    writer.Write(new DocumentStart());

        //    writer.Write(new Comment("A comment line that starts with '#'."));
        //    writer.Write(new Comment("This is a comment line having leading white spaces."));
        //    writer.Write(new Comment(CommentHandle.Exclamation, "A comment line that starts with '!'."));

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key("key1"));
        //    writer.Write(new ValueAssigner('='));
        //    writer.Write(new Text(" value1 "));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key("key2"));
        //    writer.Write(new ValueAssigner(':'));
        //    writer.Write(new Text("value2"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key("!key3"));
        //    writer.Write(new ValueAssigner(' '));
        //    writer.Write(new Text("value3"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key("!key!\n!4") { LogicalLines = true });
        //    writer.Write(new ValueAssigner('='));
        //    writer.Write(new Text(" value\n#4\n ") { LogicalLines = true });
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key(" \u006B\u0065\u00795"));
        //    writer.Write(new ValueAssigner('='));
        //    writer.Write(new Text(" \u0076\u0061\u006c\u0075\u00655"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key("\\k\\e\\y\\6"));
        //    writer.Write(new ValueAssigner('='));
        //    writer.Write(new Text("\\v\\a\\l\\u\\e\\6"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key(": ="));
        //    writer.Write(new ValueAssigner('='));
        //    writer.Write(new Text("!\\colon\\space\\equal"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new Comment("A comment line that starts with '#'."));
        //    writer.Write(new Comment("This is a comment line having leading white spaces."));
        //    writer.Write(new Comment(CommentHandle.Exclamation, "A comment line that starts with '!'."));

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key("key1"));
        //    writer.Write(new ValueAssigner('='));
        //    writer.Write(new Text(" value1 "));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key("key2"));
        //    writer.Write(new ValueAssigner(':'));
        //    writer.Write(new Text("value2"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key("!key3"));
        //    writer.Write(new ValueAssigner(' '));
        //    writer.Write(new Text("value3"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key("!key!\n!4") { LogicalLines = true });
        //    writer.Write(new ValueAssigner('='));
        //    writer.Write(new Text(" value\n#4\n ") { LogicalLines = true });
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key(" \u006B\u0065\u00795"));
        //    writer.Write(new ValueAssigner('='));
        //    writer.Write(new Text(" \u0076\u0061\u006c\u0075\u00655"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key("\\k\\e\\y\\6"));
        //    writer.Write(new ValueAssigner('='));
        //    writer.Write(new Text("\\v\\a\\l\\u\\e\\6"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key(": ="));
        //    writer.Write(new ValueAssigner('='));
        //    writer.Write(new Text("!\\colon\\space\\equal"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new DocumentEnd());

        //    Console.Write(output);

        //    Console.WriteLine("------------------- out ^^^^^^^^^^");
        //    IPropertiesReader reader = new PropertiesReader(output.ToString());

        //    while (reader.TryRead(out Core.Events.PropertiesEvent? e))
        //    {
        //        if (e is Core.Events.Key k)
        //        {
        //            Console.Write($"\"{k.Text}\"");
        //        }
        //        else if (e is Core.Events.ValueAssigner v)
        //        {
        //            Console.Write(v.Text);
        //        }
        //        else if (e is Core.Events.Text va)
        //        {
        //            Console.WriteLine($"'{va.Text}'");
        //        }
        //        //else if (e is Core.Events.PropertyStart ps)
        //        //{
        //        //    Console.WriteLine("PropStart");
        //        //}
        //        //else if (e is Core.Events.PropertyEnd pe)
        //        //{
        //        //    Console.WriteLine("PropEnd");
        //        //}
        //    }

        //    Console.WriteLine("------------------- out from read ^^^^^^^^^");


        //    //        //////////

        //    //        //StringBuilder sb = new StringBuilder();

        //    //        //using var fs = File.Open("hello.txt", FileMode.Open, FileAccess.Read);

        //    //        //{
        //    //        //    string text;

        //    //        //    using (var sr = new StreamReader(fs))
        //    //        //    {
        //    //        //        while ((text = sr.ReadLine()) != null)
        //    //        //            sb.Append(text);
        //    //        //    }
        //    //        //}


        //    //        //PropertiesReader reader = new PropertiesReader(Program.SOURCE);

        //    //        //while (reader.Read() != null)
        //    //        //    ;

        //    //        dynamicProvder.Construct(typeof(StreamMark));
        //}

        //    private IObjectProvider dynamicProvder = new DynamicObjectProvider();
        //    private IObjectProvider reflProvder = new ReflectionObjectProvider();
        //    private IObjectProvider exprProvder = new ExpressionObjectProvider();

        //[Benchmark]
        //public void ReadWithString()
        //{
        //    //JsonTextReader reader = new JsonTextReader(new StringReader(Program.JSON_SOURCE));
        //    //while (reader.Read())
        //    //{
        //    //}

        //    using(StringReader reader = new StringReader(Program.SOURCE))
        //    {
        //        string s;
        //        while((s = reader.ReadLine()) != null) {
        //            for(int i = 0; i < s.Length; i++) { }
        //        }
        //    }
        //}

        //[Benchmark]
        //public void ReadWithChar()
        //{
        //    JsonTextReader reader = new JsonTextReader(new StringReader(Program.JSON_SOURCE));
        //    while (reader.Read())
        //    {
        //    }

        //    using (StringReader reader = new StringReader(Program.SOURCE))
        //    {
        //        while (reader.Read() != -1)
        //        {

        //        }
        //    }
        //}

        //[Benchmark]
        //public void fastJson()
        //{
        //    fastJSON.JSON.Parse(Program.JSON_SOURCE);
        //}

        [Benchmark]
        public void WithString()
        {
            //PropertiesReader reader = new PropertiesReader(Program.SOURCE);

            using (var reader = new StringReader(Program.SOURCE))
            {
                while (reader.ReadLine() != null) ;
            }

        }

        [Benchmark]
        public void WithChar()
        {
            //PropertiesReader reader = new PropertiesReader(Program.SOURCE);

            using (var reader = new StringReader(Program.SOURCE))
            {
                while (reader.Read() != -1) ;

            }
        }
        readonly StringBuilder sb = new StringBuilder();

        //[Benchmark]
        //public void PDN_Writer_Token()
        //{
        //    sb.Length = 0;
        //    var writer = new PropertiesWriter(new StringWriter(sb));
        //    writer.Write(PropertiesTokenType.Comment, "Chisen 2");
        //    writer.Write(PropertiesTokenType.Comment, "Chisen");
        //    writer.Write(PropertiesTokenType.Key, "ChisenKey");
        //    writer.Write(PropertiesTokenType.Value, "ChisenKeyVal");
        //    writer.Write(PropertiesTokenType.Comment, "Chisen Com after val");
        //    writer.Write(PropertiesTokenType.Key, "ChisenSecond");
        //    writer.Write(PropertiesTokenType.Value, "ChisenSecondVal");
        //    writer.Write(PropertiesTokenType.Key, "ChisenSecondAfterKey");
        //    writer.Write(PropertiesTokenType.Value, "ChisenSecondAfterKeyVal");
        //    writer.Write(PropertiesTokenType.Key, " KeyStartw/whitespace");
        //    writer.Write(PropertiesTokenType.Value, " valStartw/backwhitespace");
        //    writer.Write(PropertiesTokenType.Key, "#KeyStart#w/backwhitespaceLogical\n!LinesLol");
        //    writer.Write(PropertiesTokenType.Value, "KeyStartw/backwhitespaceLogical\n#Lines even more LOL");
        //    writer.Write(PropertiesTokenType.Comment, "Chisen Com after val");
        //    writer.Dispose();
        //}

        //[Benchmark]
        //public void PDN_Writer()
        //{
        //    sb.Length = 0;
        //    var writer = new PropertiesWriter(new StringWriter(sb));
        //    writer.WriteComment("Chisen");
        //    writer.WriteComment("Chisen 2");
        //    writer.WriteKey("ChisenKey");
        //    writer.WriteValue("ChisenKeyVal");
        //    writer.WriteComment("Chisen Com after val");
        //    writer.WriteKey("ChisenSecond");
        //    writer.WriteValue("ChisenSecondVal");
        //    writer.WriteKey("ChisenSecondAfterKey");
        //    writer.WriteValue("ChisenSecondAfterKeyVal");
        //    writer.WriteKey(" KeyStartw/whitespace");
        //    writer.WriteValue(" valStartw/backwhitespace");
        //    writer.WriteKey("#KeyStart#w/backwhitespaceLogical\n!LinesLol", false);
        //    writer.WriteValue("KeyStartw/backwhitespaceLogical\n#Lines even more LOL", false);
        //    writer.WriteComment("Chisen Com after val");
        //    writer.Dispose();
        //}

        [Benchmark]
        public void PDN_Reader()
        {
            //PropertiesReader reader = new PropertiesReader(Program.SOURCE);

            var parser = new PropertiesReader(new StringReader(Program.SIMPLE_SOURCE));

            while (parser.MoveNext())
            {
            }
        }

        [Benchmark]
        public void PDN_UnsafeReader()
        {
            //PropertiesReader reader = new PropertiesReader(Program.SOURCE);

            var parser = new UnsafePropertiesReader(Program.SIMPLE_SOURCE);

            while (parser.MoveNext())
            {
            }
            //PropertiesDocument.Load(Program.SOURCE);
        }

        [Benchmark]
        public void PDN_DocReader()
        {
            //PropertiesReader reader = new PropertiesReader(Program.SOURCE);

            //var parser = new PropertiesReader(new UnsafeStringReader(Program.SOURCE));

            //while (parser.MoveNext())
            //{
            //}

            PropertiesDocument.Load(Program.SIMPLE_SOURCE);
        }

        //[Benchmark]
        //public void PDN_CacheReader()
        //{
        //    //using FileStream stream = File.OpenRead("C:\\Users\\alvyn\\source\\git-repos\\PropertiesDotNet\\PropertiesDotNet.Test\\myprop.txt");
        //    //PropertiesReader reader = new PropertiesReader(Program.SOURCE);

        //    var parser = new CachePropertiesReader(new StringReader(Program.SOURCE));

        //    while (parser.MoveNext())
        //    {
        //    }
        //}

        //[Benchmark]
        //public void PDN_StateReader()
        //{
        //    //PropertiesReader reader = new PropertiesReader(Program.SOURCE);

        //    var parser = new PropertiesStateReader(new StringReader(Program.SOURCE));

        //    while (parser.MoveNext())
        //    {
        //    }
        //}
        private static readonly Dictionary<string, string> _dic = new System.Collections.Generic.Dictionary<string, string>();
        private readonly JavaPropertyReader kReader =
            new JavaPropertyReader(_dic);
        [Benchmark]
        public void Kajibity()
        {
            //PropertiesReader reader = new PropertiesReader(Program.SOURCE);
            Program.SIMPLE_DOC_STREAM.Position = 0;
            kReader.Parse(Program.SIMPLE_DOC_STREAM);
        }

        //[Benchmark]
        //public void PDN_StateReader()
        //{
        //    //using FileStream stream = File.OpenRead("C:\\Users\\alvyn\\source\\git-repos\\PropertiesDotNet\\PropertiesDotNet.Test\\myprop.txt");
        //    //PropertiesReader reader = new PropertiesReader(Program.SOURCE);

        //    var parser = new PropertiesReader(new StringReader(Program.SOURCE));

        //    while (parser.MoveNext())
        //    {
        //    }
        //}
    }
}
