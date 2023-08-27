namespace PropertiesDotNet.Test
{
    [TestFixture]
    public class ValueProviderTests
    {
        [Test]
        public void ReflectionValueProvider_ShouldProvidePropertyData()
        {
            TestValueProviderProperties(new ReflectionValueProvider());
        }

        [Test]
        public void ReflectionValueProvider_ShouldProvideFieldData()
        {
            TestValueProviderFields(new ReflectionValueProvider());
        }

        private void TestValueProviderProperties(IValueProvider provider)
        {
            var sample = new SampleClass();
            var publicProperty = typeof(SampleClass).GetProperty(nameof(SampleClass.PublicData))!;
            var privateProperty = typeof(SampleClass).GetProperty("PrivateData", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

            Assert.That(sample.PublicData, Is.Null);

            const string SAMPLE_DATA = "SampleData";

            provider.SetValue(sample, publicProperty, SAMPLE_DATA);
            Assert.That(sample.PublicData, Is.EqualTo(SAMPLE_DATA));
            Assert.That(provider.GetValue(sample, publicProperty), Is.EqualTo(SAMPLE_DATA));

            Assert.That(provider.GetValue(sample, privateProperty), Is.Null);

            provider.SetValue(sample, privateProperty, SAMPLE_DATA);
            Assert.That(provider.GetValue(sample, privateProperty), Is.EqualTo(SAMPLE_DATA));
        }

        private void TestValueProviderFields(IValueProvider provider)
        {
            var sample = new SampleClass();
            var publicField = typeof(SampleClass).GetField(nameof(SampleClass.PublicFieldData))!;
            var privateField = typeof(SampleClass).GetField("PrivateFieldData", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

            Assert.That(sample.PublicData, Is.Null);

            const string SAMPLE_DATA = "SampleData";

            provider.SetValue(sample, publicField, SAMPLE_DATA);
            Assert.That(sample.PublicFieldData, Is.EqualTo(SAMPLE_DATA));
            Assert.That(provider.GetValue(sample, publicField), Is.EqualTo(SAMPLE_DATA));

            Assert.That(provider.GetValue(sample, privateField), Is.Null);

            provider.SetValue(sample, privateField, SAMPLE_DATA);
            Assert.That(provider.GetValue(sample, privateField), Is.EqualTo(SAMPLE_DATA));
        }

        private class SampleClass
        {
            public string? PublicData { get; set; }
            public string? PublicFieldData;

            private string? PrivateData { get; set; }
            private string? PrivateFieldData;

            public SampleClass() { }
        }
    }
}