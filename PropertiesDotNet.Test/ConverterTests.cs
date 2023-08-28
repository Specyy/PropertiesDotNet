using System.Collections;
using System.Text;

using NUnit.Framework.Constraints;

namespace PropertiesDotNet.Test
{
    [TestFixture]
    public class ConverterTests
    {
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
            Assert.That(new SystemTypeConverter().Accepts(type), type != typeof(object) ? Is.True : Is.False);
        }

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
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(DateTime), "Sun., 27 Aug. 2023 22:37:31 GMT"));
                Assert.DoesNotThrow(() => converter.Deserialize(serializer, typeof(Guid), "0f8fad5b-d9cb-469f-a165-70867728950e"));
                Assert.Throws<PropertiesException>(() => converter.Deserialize(serializer, typeof(object), ""));
            });
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
                Assert.That(converter.Accepts(typeof(List<string>)), Is.False);
                Assert.That(converter.Accepts(typeof(ICollection<string>)), Is.False);
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
                Assert.That(converter.Accepts(typeof(ArrayList)));
                Assert.That(converter.Accepts(typeof(List<List<string>[]>)));
                Assert.That(converter.Accepts(typeof(List<List<string>[]>[])), Is.False);
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

        [Test]
        public void DictionaryConverter_ShouldAcceptDictionaries()
        {
            var converter = new DictionaryConverter();
            Assert.Multiple(() =>
            {
                Assert.That(converter.Accepts(typeof(Dictionary<string, object>)));
                Assert.That(converter.Accepts(typeof(SortedList<string, object>)));
                Assert.That(converter.Accepts(typeof(KeyValuePair<string, object>)), Is.False);
                Assert.That(converter.Accepts(typeof(DictionaryEntry)), Is.False);
            });
        }

        [Test]
        public void DictionaryConverter_ShouldSerializeDictionaries()
        {
            var serializer = new PropertiesSerializer();
            var converter = new DictionaryConverter();
            var data = new Dictionary<string, object>()
            {
                { "Hello", "World" },
                { "123", 456 },
                { "GUID", Guid.NewGuid() },
                { "Time", DateTime.Now },
                { "Array", new[] { 9, 8, 7 } },
                { "List", new List<char>("Example🌐List") },
            };

            Assert.DoesNotThrow(() => converter.Serialize(serializer, data.GetType(), data, serializer.TreeComposer.CreateRoot()));
        }

        [Test]
        public void DictionaryConverter_ShouldDeserializeDictionaries()
        {
            var serializer = new PropertiesSerializer();
            var converter = new DictionaryConverter();
            string data = @"Hello=World
123=456
GUID=5cff0cec-7396-4065-b90c-fca2ec71fd1e
Time=Sun., 27 Aug. 2023 22:37:31 GMT
Array.0=9
Array.1=8
Array.2=7
List.0=E
List.1=x
List.2=a
List.3=m
List.4=p
List.5=l
List.6=e
List.7=\u0020
List.8=L
List.9=i
List.10=s
List.11=t";

            static bool DicitonaryEqual<TKey, TValue>(Dictionary<TKey, TValue> source, Dictionary<TKey, TValue> other) where TKey : notnull
            {
                if (source.Count != other.Count)
                    return false;

                foreach (var entry in source)
                {
                    if (other.TryGetValue(entry.Key, out var value))
                    {
                        if (value is Dictionary<TKey, TValue> dic)
                        {
                            if (!DicitonaryEqual((entry.Value as Dictionary<TKey, TValue>)!, dic))
                                return false;
                        }
                        if (entry.Value is not null || value is not null)
                        {
                            bool equal = (entry.Value is IEquatable<TValue> eq && eq.Equals(value))
                               || (value is IEquatable<TValue> equate && equate.Equals(entry.Value))
                               || (entry.Value?.Equals(value) ?? Equals(entry.Value, value))
                               || (value?.Equals(value) ?? Equals(value, entry.Value));

                            if (!equal)
                                return false;
                        }

                    }
                    else
                    {
                        return false;
                    }
                }

                return true;
            }

            using var reader = new PropertiesReader(data);
            Dictionary<string, object> read = null!;
            Assert.DoesNotThrow(() => read = (Dictionary<string, object>)converter.Deserialize(serializer, typeof(Dictionary<string, object>), serializer.TreeComposer.ReadObject(reader))!);
            Assert.That(DicitonaryEqual(read, new Dictionary<string, object>()
            {
                { "Hello", "World" },
                { "123", "456" },
                { "GUID", Guid.Parse("5cff0cec-7396-4065-b90c-fca2ec71fd1e") },
                { "Time", DateTime.ParseExact("Sun., 27 Aug. 2023 22:37:31 GMT", System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.RFC1123Pattern,null) },
                { "Array", new Dictionary<string, object>()
                           {
                                { "0", "9" },
                                { "1", "8" },
                                { "2", "7" }
                           }

                },
                { "List", new Dictionary<string, object>()
                          {
                                { "0", "E" },
                                { "1", "x" },
                                { "2", "a" },
                                { "3", "m" },
                                { "4", "p" },
                                { "5", "l" },
                                { "6", "e" },
                                { "7", " " },
                                { "8", "L" },
                                { "9", "i" },
                                { "10", "s" },
                                { "11", "t" }
                          }
                },
            }), Is.True);
        }

        [Test]
        public void ObjectConverter_ShouldAcceptObjects()
        {
            var converter = new ObjectConverter();
            Assert.Multiple(() =>
            {
                Assert.That(converter.Accepts(typeof(object)));
                Assert.That(converter.Accepts(typeof(Dictionary<string, object>)));
                Assert.That(converter.Accepts(typeof(PropertiesDocument)));
                Assert.That(converter.Accepts(typeof(PropertiesProperty)));
                Assert.That(converter.Accepts(typeof(PropertiesTreeNode)), Is.False);
                Assert.That(converter.Accepts(typeof(string[])), Is.False);
            });
        }

        [Test]
        public void ObjectConverter_ShouldSerializeObjects()
        {
            var serializer = new PropertiesSerializer();
            var converter = new ObjectConverter()
            {
                AllowFields = true,
                MemberFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
            };
            var data = new SampleClass();

            using (var writer = new PropertiesWriter(Console.Out))
            {
                var root = serializer.TreeComposer.CreateRoot();
                converter.Serialize(serializer, data.GetType(), data, root);
                serializer.TreeComposer.WriteObject(root, writer);
            }

            Assert.DoesNotThrow(() => converter.Serialize(serializer, data.GetType(), data, serializer.TreeComposer.CreateRoot()));
        }

        [Test]
        public void ObjectConverter_ShouldDeserializeObjects()
        {
            var serializer = new PropertiesSerializer();
            var converter = new ObjectConverter()
            {
                AllowFields = true,
                MemberFlags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
            };
            var restrictedConverter = new ObjectConverter()
            {
                AllowFields = false,
                MemberFlags = System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance,
            };
            string data = @"PublicData=EditedPublicData
PrivateData=EditedPrivateData
PublicFieldData=EditedPublicFieldData
PrivateFieldData=EditedPrivateFieldData";

            using (var reader = new PropertiesReader(data))
                Assert.Throws<PropertiesException>(() => restrictedConverter.Deserialize(serializer, typeof(SampleClass), serializer.TreeComposer.ReadObject(reader)));

            using (var reader = new PropertiesReader(data))
            {
                SampleClass obj = null!;
                Assert.DoesNotThrow(() => obj = (SampleClass)converter.Deserialize(serializer, typeof(SampleClass), serializer.TreeComposer.ReadObject(reader))!);
                Assert.That(obj, Is.EqualTo(new SampleClass("EditedPublicData", "EditedPublicFieldData", "EditedPrivateData", "EditedPrivateFieldData")));
            }
        }

        [Theory]
        public void ObjectConverter_VerifySampleDeserialization()
        {
            var serializer = new PropertiesSerializer();
            var converter = new ObjectConverter();
            string data = @"
Name: Abe Lincoln
Age: 25
HeightInInches: 6.3333334922790527
Addresses.Home.Street: 2720 Sundown Lane
Addresses.Home.City: Kentucketsville
Addresses.Home.State: Calousiyorkida
Addresses.Home.Zip: 99978
Addresses.Work.Street: 1600 Pennsylvania Avenue NW
Addresses.Work.City: Washington
Addresses.Work.State: District of Columbia
Addresses.Work.Zip: 20500
";
            using var reader = new PropertiesReader(data);
            var person = (Person)converter.Deserialize(serializer, typeof(Person), serializer.TreeComposer.ReadObject(reader))!;

            Assert.Multiple(() =>
            {
                Assert.That(person.Name, Is.EqualTo("Abe Lincoln"));
                Assert.That(person.Age, Is.EqualTo(25));
                Assert.That(person.Height, Is.EqualTo(6.3333334922790527f));

                var homeAddress = person.Addresses["Home"];
                Assert.That(homeAddress.Street, Is.EqualTo("2720 Sundown Lane"));
                Assert.That(homeAddress.City, Is.EqualTo("Kentucketsville"));
                Assert.That(homeAddress.State, Is.EqualTo("Calousiyorkida"));
                Assert.That(homeAddress.Zip, Is.EqualTo(99978));

                var workAddress = person.Addresses["Work"];
                Assert.That(workAddress.Street, Is.EqualTo("1600 Pennsylvania Avenue NW"));
                Assert.That(workAddress.City, Is.EqualTo("Washington"));
                Assert.That(workAddress.State, Is.EqualTo("District of Columbia"));
                Assert.That(workAddress.Zip, Is.EqualTo(20500));
            });
        }

        [Theory]
        public void ObjectConverter_VerifySampleSerialization()
        {
            var serializer = new PropertiesSerializer();
            var converter = new ObjectConverter();
            var person = new Person()
            {
                Name = "Abe Lincoln",
                Age = 25,
                Height = 6.3333334922790527f,
                Addresses = new Dictionary<string, Address>()
                {
                    { "Home", new Address()
                        {
                            Street = "2720 Sundown Lane",
                            City = "Kentucketsville",
                            State = "Calousiyorkida",
                            Zip = 99978,
                        }
                    },
                    { "Work", new Address()
                        {
                            Street = "1600 Pennsylvania Avenue NW",
                            City = "Washington",
                            State = "District of Columbia",
                            Zip = 20500,
                        }
                    }
                }
            };
            var output = new StringBuilder();
            string expectedOutput = @"# The person's name
Name=Abe Lincoln
# The person's age
Age=25
# The person's height in inches
HeightInInches=6.3333335
# A list of addresses
Addresses.Home.Street=2720 Sundown Lane
# A list of addresses
Addresses.Home.City=Kentucketsville
# A list of addresses
Addresses.Home.State=Calousiyorkida
# A list of addresses
# The person's zip
# Also called zip code
Addresses.Home.Zip=99978
# A list of addresses
Addresses.Work.Street=1600 Pennsylvania Avenue NW
# A list of addresses
Addresses.Work.City=Washington
# A list of addresses
Addresses.Work.State=District of Columbia
# A list of addresses
# The person's zip
# Also called zip code
Addresses.Work.Zip=20500".TrimEnd(Environment.NewLine.ToCharArray());

            using (var writer = new PropertiesWriter(output))
            {
                var root = serializer.TreeComposer.CreateRoot();
                converter.Serialize(serializer, typeof(Person), person, root);
                serializer.TreeComposer.WriteObject(root, writer);
            }

            Assert.That(output.ToString().TrimEnd(Environment.NewLine.ToCharArray()), Is.EqualTo(expectedOutput));
            serializer.SerializeObject(person, Console.Out);
        }

#pragma warning disable CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        private class SampleClass : IEquatable<SampleClass>
#pragma warning restore CS0659 // Type overrides Object.Equals(object o) but does not override Object.GetHashCode()
        {
            public string? PublicData { get; set; } = "ExamplePublicData";
            public string? PublicFieldData = "ExamplePublicFieldData";

            private string? PrivateData { get; set; } = "ExamplePrivateData";
            private string? PrivateFieldData = "ExamplePrivateFieldData";

            public SampleClass() { }

            public SampleClass(string? publicData, string? publicFieldData, string? privateData, string? privateFieldData)
            {
                PublicData = publicData;
                PublicFieldData = publicFieldData;
                PrivateData = privateData;
                PrivateFieldData = privateFieldData;
            }

            public bool Equals(SampleClass? other)
            {
                return PublicData == other?.PublicData &&
                    PublicFieldData == other?.PublicFieldData &&
                    PrivateData == other?.PrivateData &&
                    PrivateFieldData == other?.PrivateFieldData;
            }

            public override bool Equals(object? obj) => Equals(obj as SampleClass);
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        private class Person
        {
            [PropertiesComment("The person's name")]
            public string Name { get; set; }

            [PropertiesComment("The person's age")]
            public int Age { get; set; }

            [PropertiesComment("The person's height in inches")]
            [PropertiesMember("HeightInInches")]
            public float Height { get; set; }

            [PropertiesMember(false)]
            public int NumberOfSiblings { get; set; }

            [PropertiesComment("A list of addresses")]
            public Dictionary<string, Address> Addresses { get; set; }
        }

        private class Address
        {
            public string Street { get; set; }
            public string City { get; set; }
            public string State { get; set; }

            [PropertiesMember(false)]
            public int UnitNumber { get; set; }

            [PropertiesComment("The person's zip")]
            [PropertiesComment("Also called zip code")]
            [PropertiesMember(typeof(long?))]
            public int Zip { get; set; }
        }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.oo
    }
}