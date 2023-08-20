namespace PropertiesDotNet.Test
{
    [TestFixture]
    public class ObjectProviderTests
    {
        [Test]
        public void ReflectionObjectProvider_ShouldCallDifferentCtors()
        {
            TestObjectProvider(new ReflectionObjectProvider());
        }

        [Test]
        public void ExpressionObjectProvider_ShouldCallDifferentCtors()
        {
            TestObjectProvider(new ExpressionObjectProvider());
        }

        [Test]
        public void DynamicObjectProvider_ShouldCallDifferentCtors()
        {
            TestObjectProvider(new DynamicObjectProvider());
        }

        private void TestObjectProvider(IObjectProvider provider)
        {
            var sample = provider.Construct<SampleClass>();
            Assert.That(sample.Data, Is.Null);

            sample = provider.Construct<SampleClass>("SampleData");
            Assert.That(sample.Data, Is.EqualTo("SampleData"));

            Assert.Throws<PropertiesException>(() => provider.Construct<SampleClass>(10));
            provider.ConstructorFlags |= System.Reflection.BindingFlags.NonPublic;

            sample = provider.Construct<SampleClass>(10);
            Assert.That(sample.Data, Is.EqualTo(10));
        }

        private class SampleClass
        {
            public object? Data { get; set; }

            public SampleClass() { }
            public SampleClass(string data) { Data = data; }
            private SampleClass(int data) { Data = data; }
        }
    }
}
