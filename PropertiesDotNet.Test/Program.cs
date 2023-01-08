using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using PropertiesDotNet.Core;

namespace PropertiesDotNet.Test
{
    public class Program
    {
        /// <summary>
        /// 
        /// </summary>
        internal const string SOURCE =
            @"
# A comment line that starts with '#'.
# This is a comment line having leading white spaces.
# A comment line that starts with '#'.
# This is a comment line having leading white spaces.
# A comment line that starts with '#'.
# This is a comment line having leading white spaces.
# A comment line that starts with '#'.
# This is a comment line having leading white spaces.
! A comment line that starts with '#'.
# This is a comment line having leading white spaces.
///! A comment line that starts with '!'.
\
key\r\n1=value1
  key2 :       value2
    key3             value3
! A comment line that starts with '#'.
# This is a comment line having leading white spaces.
# A comment line that starts with '#'.
# This is a comment line having leading white spaces.
# A comment line that starts with '#'.
# This is a comment line having leading white spaces.
# A comment line that starts with '#'.
# This is a comment line having leading white spaces.
key\
  4=value\
    4
\u006B\u0065\u00795=\u0076\u0061\u006c\u0075\u00655
\k\e\y\6=\v\a\lu\e\6

\:\ \== \\colon\\space\\equal";

        static unsafe void Main(string[] args)
        {
            Console.WriteLine(SOURCE);
            // Setting for whether to align logical line
            // ^ Beautify?
            //

            IPropertiesReader reader = new PropertiesReader(SOURCE);
            //reader.Read();
            //reader.Read();
            //reader.Read();
            //reader.Read();

            Console.WriteLine("first----------------------------");

            while (reader.MoveNext())
            {
                switch (reader.Token.Type)
                {
                    case PropertiesTokenType.Key:
                        Console.Write($"\"{reader.Token.Value}\"");
                        break;
                    case PropertiesTokenType.Assigner:
                        Console.Write($"{reader.Token.Value}");
                        break;
                    case PropertiesTokenType.Value:
                        Console.WriteLine($"\"{reader.Token.Value}\"");
                        break;
                    case PropertiesTokenType.Comment:
                        Console.WriteLine($"{((PropertiesReader)reader).CommentHandle} {reader.Token.Value}");
                        break;
                }
            }

            //BenchmarkRunner.Run<BenchmarkTester>();
            Console.WriteLine("-------------------");
            //new BenchmarkTester().DynamicCtor();
            Console.Read();
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
        //    writer.Write(new Value(" value1 "));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key("key2"));
        //    writer.Write(new ValueAssigner(':'));
        //    writer.Write(new Value("value2"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key("!key3"));
        //    writer.Write(new ValueAssigner(' '));
        //    writer.Write(new Value("value3"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key("!key!\n!4") { LogicalLines = true });
        //    writer.Write(new ValueAssigner('='));
        //    writer.Write(new Value(" value\n#4\n ") { LogicalLines = true });
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key(" \u006B\u0065\u00795"));
        //    writer.Write(new ValueAssigner('='));
        //    writer.Write(new Value(" \u0076\u0061\u006c\u0075\u00655"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key("\\k\\e\\y\\6"));
        //    writer.Write(new ValueAssigner('='));
        //    writer.Write(new Value("\\v\\a\\l\\u\\e\\6"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key(": ="));
        //    writer.Write(new ValueAssigner('='));
        //    writer.Write(new Value("!\\colon\\space\\equal"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new Comment("A comment line that starts with '#'."));
        //    writer.Write(new Comment("This is a comment line having leading white spaces."));
        //    writer.Write(new Comment(CommentHandle.Exclamation, "A comment line that starts with '!'."));

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key("key1"));
        //    writer.Write(new ValueAssigner('='));
        //    writer.Write(new Value(" value1 "));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key("key2"));
        //    writer.Write(new ValueAssigner(':'));
        //    writer.Write(new Value("value2"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key("!key3"));
        //    writer.Write(new ValueAssigner(' '));
        //    writer.Write(new Value("value3"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key("!key!\n!4") { LogicalLines = true });
        //    writer.Write(new ValueAssigner('='));
        //    writer.Write(new Value(" value\n#4\n ") { LogicalLines = true });
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key(" \u006B\u0065\u00795"));
        //    writer.Write(new ValueAssigner('='));
        //    writer.Write(new Value(" \u0076\u0061\u006c\u0075\u00655"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key("\\k\\e\\y\\6"));
        //    writer.Write(new ValueAssigner('='));
        //    writer.Write(new Value("\\v\\a\\l\\u\\e\\6"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new PropertyStart());
        //    writer.Write(new Key(": ="));
        //    writer.Write(new ValueAssigner('='));
        //    writer.Write(new Value("!\\colon\\space\\equal"));
        //    writer.Write(new PropertyEnd());

        //    writer.Write(new DocumentEnd());

        //    Console.Write(output);

        //    Console.WriteLine("------------------- out ^^^^^^^^^^");
        //    IPropertiesReader reader = new PropertiesReader(output.ToString());

        //    while (reader.TryRead(out Core.Events.PropertiesEvent? e))
        //    {
        //        if (e is Core.Events.Key k)
        //        {
        //            Console.Write($"\"{k.Value}\"");
        //        }
        //        else if (e is Core.Events.ValueAssigner v)
        //        {
        //            Console.Write(v.Value);
        //        }
        //        else if (e is Core.Events.Value va)
        //        {
        //            Console.WriteLine($"'{va.Value}'");
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
    }
}
