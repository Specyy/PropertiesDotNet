using System.Reflection;

namespace PropertiesDotNet.Test
{
    [TestFixture]
    public class TreeComposerTests
    {
        public static readonly string ObjectFile;

        static TreeComposerTests()
        {
            string assemblyPath = Directory.GetParent(Assembly.GetExecutingAssembly().Location)!.FullName;
            string testDataDir = Path.Combine(assemblyPath, "data");

            ObjectFile = Path.Combine(testDataDir, "object.properties");
        }

        [Test]
        public void PropertiesTreeComposer_ShouldReadDocumentObjects()
        {
            using var reader = new PropertiesReader(ObjectFile);
            var composer = new PropertiesTreeComposer();

            var @object = composer.ReadObject(reader);

            Assert.Multiple(() =>
            {
                Assert.That(@object.ChildCount, Is.EqualTo(4));
                Assert.That(@object.DeepChildCount, Is.EqualTo(17));

                Assert.That(@object.Contains("one-level-key"));
                Assert.That(@object.Contains("two-level"));
                Assert.That(@object.Contains("three"));
                Assert.That(@object.Contains("a"));

                Assert.That(@object.GetValue("one-level-key"), Is.EqualTo("one-level-value"));
                Assert.That(@object.GetObject("two-level").GetValue("key"), Is.EqualTo("two-level.value"));
                Assert.That(@object.GetObject("two-level").GetValue("key1"), Is.EqualTo("two-level.value1"));
                Assert.That(@object.GetObject("three").GetObject("level").GetValue("key"), Is.EqualTo("three.level.value"));
                Assert.That(@object.GetObject("a").GetObject("very").GetObject("nested").GetObject("key").GetObject("that")
                    .GetObject("creates").GetObject("a").GetObject("lot").GetObject("of").GetValue("objects"), Is.EqualTo("value"));
            });
        }

        [Test]
        public void PropertiesTreeComposer_ShouldWriteDocumentObjects()
        {
            using var sw = new StringWriter();
            using var writer = new PropertiesWriter(sw);
            var composer = new PropertiesTreeComposer();
            var @object = composer.CreateRoot();

            @object.AddComment("document-level comment 1");
            @object.AddComment("document-level comment 2");

            @object.AddPrimitive("one-level-key", "one-level-value");

            @object.AddObject("two-level").AddPrimitive("key", "two-level.value").AddComment("two-level.key comment");
            @object.GetObject("two-level").AddPrimitive("key1", "two-level.value1").AddComment("two-level.key1 comment");
            @object.GetObject("two-level").AddComment("two-level comment");

            @object.AddObject("three").AddObject("level").AddPrimitive("key", "three.level.value");
            @object.AddObject("a").AddObject("very").AddObject("nested").AddObject("key").AddObject("that")
                   .AddObject("creates").AddObject("a").AddObject("lot").AddObject("of").AddPrimitive("objects", "value");

            composer.WriteObject(@object, writer);

            Assert.That(sw.ToString().TrimEnd(Environment.NewLine.ToCharArray()), 
                Is.EqualTo(@"# document-level comment 1
# document-level comment 2
one-level-key=one-level-value
# two-level comment
# two-level.key comment
two-level.key=two-level.value
# two-level comment
# two-level.key1 comment
two-level.key1=two-level.value1
three.level.key=three.level.value
a.very.nested.key.that.creates.a.lot.of.objects=value"));
        }
    }
}
