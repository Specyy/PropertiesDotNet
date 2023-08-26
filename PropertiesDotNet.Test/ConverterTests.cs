using NUnit.Framework.Constraints;

namespace PropertiesDotNet.Test
{
    [TestFixture]
    public class ConverterTests
    {
        [Test]
        public void SystemTypeConverter_ShouldSerializeSystemTypes()
        {
            var serializer = new PropertiesSerializer();
            var converter = new SystemTypeConverter();

            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => converter.Serialize(serializer, typeof(string), "string"));
                Assert.DoesNotThrow(() => converter.Serialize(serializer, typeof(char), 'a'));
                Assert.DoesNotThrow(() => converter.Serialize(serializer, typeof(bool), true));
                Assert.DoesNotThrow(() => converter.Serialize(serializer, typeof(bool), false));
                Assert.DoesNotThrow(() => converter.Serialize(serializer, typeof(byte), 123));
                Assert.DoesNotThrow(() => converter.Serialize(serializer, typeof(sbyte), 123));
                Assert.DoesNotThrow(() => converter.Serialize(serializer, typeof(short), 123));
                Assert.DoesNotThrow(() => converter.Serialize(serializer, typeof(ushort), 123));
                Assert.DoesNotThrow(() => converter.Serialize(serializer, typeof(int), 123));
                Assert.DoesNotThrow(() => converter.Serialize(serializer, typeof(uint), 123));
                Assert.DoesNotThrow(() => converter.Serialize(serializer, typeof(long), 123));
                Assert.DoesNotThrow(() => converter.Serialize(serializer, typeof(ulong), 123));
                Assert.DoesNotThrow(() => converter.Serialize(serializer, typeof(float), 123));
                Assert.DoesNotThrow(() => converter.Serialize(serializer, typeof(double), 123));
                Assert.DoesNotThrow(() => converter.Serialize(serializer, typeof(decimal), 123));
                Assert.DoesNotThrow(() => converter.Serialize(serializer, typeof(DateTime), DateTime.Now));
                Assert.DoesNotThrow(() => converter.Serialize(serializer, typeof(Guid), Guid.NewGuid()));
                Assert.Throws<PropertiesException>(() => converter.Serialize(serializer, typeof(object), new object()));
            });
        }

        [Test]
        public void SystemTypeConverter_ShouldDeserializeSystemTypes()
        {
            var serializer = new PropertiesSerializer();
            var converter = new SystemTypeConverter();

            Assert.Multiple(() =>
            {
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(string), "string"));
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(char), "a"));
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(bool), "true"));
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(bool), "false"));
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(byte), "123"));
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(sbyte), "123"));
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(short), "123"));
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(ushort), "123"));
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(int), "123"));
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(uint), "123"));
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(long), "123"));
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(ulong), "123"));
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(float), "123f"));
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(double), "123d"));
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(decimal), "123m"));
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(DateTime), "Friday, 29 May 2015 05:50 AM"));
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(Guid), "0f8fad5b-d9cb-469f-a165-70867728950e"));
                Assert.Throws<PropertiesException>(() => converter.Deserialize(serializer, typeof(object), ""));
            });
        }

        [Test]
        [TestCase(typeof(string))]
        [TestCase(typeof(char))]
        [TestCase(typeof(bool))]
        [TestCase(typeof(byte))]
        [TestCase(typeof(sbyte))]
        [TestCase(typeof(short))]
        [TestCase(typeof(ushort))]
        [TestCase(typeof(int))]
        [TestCase(typeof(uint))]
        [TestCase(typeof(long))]
        [TestCase(typeof(ulong))]
        [TestCase(typeof(float))]
        [TestCase(typeof(double))]
        [TestCase(typeof(decimal))]
        [TestCase(typeof(DateTime))]
        [TestCase(typeof(Guid))]
        [TestCase(typeof(object))]
        public void SystemTypeConverter_ShouldAcceptSystemTypes(Type type)
        {
            bool accepts = new SystemTypeConverter().Accepts(type);
            Assert.That(type != typeof(object) ? accepts : !accepts);
        }

        [Test]
        public void ArrayConverter_ShouldAcceptArrays()
        {
            var converter = new ArrayConverter();
            Assert.Multiple(() =>
            {
                Assert.That(converter.Accepts(typeof(string[])));
                Assert.That(converter.Accepts(typeof(Guid[])));
                Assert.That(converter.Accepts(typeof(object[])));
                Assert.That(converter.Accepts(typeof(List<string>[])));
                Assert.That(!converter.Accepts(typeof(List<string>)));
                Assert.That(!converter.Accepts(typeof(ICollection<string>)));
            });
        }

        [Test]
        public void ArrayConverter_ShouldSerializeArrays()
        {
            var serializer = new PropertiesSerializer();
            var converter = new ArrayConverter();
            var data = "Hello World".ToCharArray();
            var badData = new List<char>(data);

            Assert.DoesNotThrow(() => converter.Serialize(serializer, data.GetType(), data, serializer.TreeComposer.CreateRoot()));
            Assert.Throws(new InstanceOfTypeConstraint(typeof(Exception)), () => converter.Serialize(serializer, badData.GetType(), badData, serializer.TreeComposer.CreateRoot()));
            Assert.Throws(new InstanceOfTypeConstraint(typeof(Exception)), () => converter.Serialize(serializer, data.GetType(), badData, serializer.TreeComposer.CreateRoot()));
        }

        [Test]
        public void ArrayConverter_ShouldDeserializeArrays()
        {
            var serializer = new PropertiesSerializer();
            var converter = new ArrayConverter();
            using var reader = new PropertiesReader(@"0=H
1=e
2=l
3=l
4=o
5=\ 
6=W
7=o
8=r
9=l
10=d");

            Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(char[]), serializer.TreeComposer.ReadObject(reader)));
            Assert.Throws(new InstanceOfTypeConstraint(typeof(Exception)), () => converter.Deserialize(serializer, typeof(List<char>), serializer.TreeComposer.ReadObject(reader)));
        }

        [Test]
        public void CollectionConverter_ShouldAcceptCollections()
        {
            var converter = new CollectionConverter();
            Assert.Multiple(() =>
            {
                Assert.That(converter.Accepts(typeof(List<string>)));
                Assert.That(converter.Accepts(typeof(LinkedList<string>)));
                Assert.That(converter.Accepts(typeof(HashSet<string>)));
                Assert.That(converter.Accepts(typeof(SortedSet<string>)));
                Assert.That(converter.Accepts(typeof(System.Collections.ArrayList)));
                Assert.That(converter.Accepts(typeof(List<List<string>[]>)));
            });
        }

        [Test]
        public void CollectionConverter_ShouldSerializeCollections()
        {
            var serializer = new PropertiesSerializer();
            var converter = new CollectionConverter();
            var data = new List<string>()
            {
                "foo",
                "bar",
                "baz",
                "qux",
            };

            var data1 = new LinkedList<string>();
            data1.AddLast("foo");
            data1.AddLast("bar");
            data1.AddLast("baz");
            data1.AddLast("qux");

            var data2 = new HashSet<string>()
            {
                "foo",
                "bar",
                "baz",
                "qux",
            };

            Assert.DoesNotThrow(() => converter.Serialize(serializer, data.GetType(), data, serializer.TreeComposer.CreateRoot()));
            Assert.DoesNotThrow(() => converter.Serialize(serializer, data1.GetType(), data1, serializer.TreeComposer.CreateRoot()));
            Assert.DoesNotThrow(() => converter.Serialize(serializer, data2.GetType(), data2, serializer.TreeComposer.CreateRoot()));
        }

        [Test]
        public void CollectionConverter_ShouldDeserializeCollections()
        {
            var serializer = new PropertiesSerializer();
            var converter = new CollectionConverter();
            string data = @"0=foo
10=bar
2=baz
3=qux";

            using (var reader = new PropertiesReader(data))
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(List<string>), serializer.TreeComposer.ReadObject(reader)));

            using (var reader = new PropertiesReader(data))
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(LinkedList<string>), serializer.TreeComposer.ReadObject(reader)));

            // HashSet<T> is currently not supported
        }
    }
}
