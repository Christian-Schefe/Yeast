using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Yeast.Ion;
using Yeast.Json;

namespace Yeast.Test
{
    public class JsonConverterTest
    {
        [Test]
        public void TestSpecialStringsStringify()
        {
            Assert.AreEqual("\"\\0\"", JSON.Stringify("\0"));
            Assert.AreEqual("\"\\u0001\"", JSON.Stringify("\x01"));
            Assert.AreEqual("\"\\n\"", JSON.Stringify("\n"));
            Assert.AreEqual("\"\\r\"", JSON.Stringify("\r"));
            Assert.AreEqual("\"\\t\"", JSON.Stringify("\t"));
            Assert.AreEqual("\"\\b\"", JSON.Stringify("\b"));
            Assert.AreEqual("\"\\\\\"", JSON.Stringify("\\"));
            Assert.AreEqual("\"\\\"\"", JSON.Stringify("\""));
        }

        [Test]
        public void TestNull()
        {
            Test(new NullValue());
        }

        [Test]
        public void TestIntegers()
        {
            Test(new IntegerValue(0));
            Test(new IntegerValue(1));
            Test(new IntegerValue(-1));
            Test(new IntegerValue(624562456));
            Test(new IntegerValue(-624562456));
            Test(new IntegerValue(long.MaxValue));
            Test(new IntegerValue(long.MinValue));
        }

        [Test]
        public void TestStrings()
        {
            Test(new StringValue("Hello World!"));
            Test(new StringValue(""));
            Test(new StringValue(" "));
            Test(new StringValue("1234567890"));
            Test(new StringValue("Hello\nWorld!"));
            Test(new StringValue("Hello\t\0\r\\\"World!"));
            Test(new StringValue("\0"));
            Test(new StringValue("\x01"));
            Test(new StringValue("\0\a\b\f\r\t\n\v\u1234"));
            Test(new StringValue("\n"));
            Test(new StringValue("\r"));
            Test(new StringValue("\t"));
            Test(new StringValue("\b"));
            Test(new StringValue("\\"));
            Test(new StringValue("\""));
        }

        [Test]
        public void TestFloats()
        {
            Test(new FloatValue(0));
            Test(new FloatValue(1));
            Test(new FloatValue(-1));
            Test(new FloatValue(3.14159f));
            Test(new FloatValue(-3.14159f));
            Test(new FloatValue(3457457457.14159d));
            Test(new FloatValue(-3457457457.14159d));
            Test(new FloatValue(1E20));
            Test(new FloatValue(-1E20));
            Test(new FloatValue(float.MaxValue));
            Test(new FloatValue(float.MinValue));

            Test(new IntegerValue(1_000_000_000_000_000));
            Test(new FloatValue(1_000_000_000_000_000_000_000f));
            Test(new FloatValue(-1_000_000_000_000_000_000_000d));
            Test(new FloatValue(double.Epsilon));
            Test(new FloatValue(double.MaxValue));
            Test(new FloatValue(double.NaN));
            Test(new FloatValue(double.NegativeInfinity));
            Test(new FloatValue(double.PositiveInfinity));

            TestParse("1E20", new FloatValue(1e20));
        }

        [Test]
        public void TestBooleans()
        {
            Test(new BooleanValue(true));
            Test(new BooleanValue(false));
        }

        [Test]
        public void TestArrays()
        {
            Test(new ArrayValue(new List<IIonValue>()));
            Test(new ArrayValue(new List<IIonValue> { new IntegerValue(1) }));
            Test(new ArrayValue(new List<IIonValue> { new IntegerValue(1), new IntegerValue(2) }));
            Test(new ArrayValue(new List<IIonValue> { new IntegerValue(1), new IntegerValue(2), new IntegerValue(3) }));
            Test(new ArrayValue(new List<IIonValue> { new IntegerValue(1), new StringValue("hello"), new FloatValue(3.141d), new IntegerValue(4) }));
            Test(new ArrayValue(new List<IIonValue> {
                new ArrayValue(new List<IIonValue> { new IntegerValue(1), new IntegerValue(2) }),
                new ArrayValue(new List<IIonValue> { new StringValue("hello"), new IntegerValue(2) }),
                new ArrayValue(new List<IIonValue> { new IntegerValue(1), new NullValue(), new NullValue() })
            }));

            var bigList = new List<IIonValue>();
            for (int i = 0; i < 1000; i++) bigList.Add(new IntegerValue(i));
            Test(new ArrayValue(bigList));
        }

        [Test]
        public void TestMaps()
        {
            Test(new MapValue(new Dictionary<string, IIonValue>()));
            Test(new MapValue(new Dictionary<string, IIonValue> { { "f1", new IntegerValue(1) } }));
            Test(new MapValue(new Dictionary<string, IIonValue> {
                { "f1", new IntegerValue(1) },
                { "f2", new IntegerValue(2) },
                { "this is a key", new MapValue(new Dictionary<string, IIonValue> { { "f1", new IntegerValue(1) } }) }
            }));
        }

        private void Test(IIonValue val)
        {
            var converter = new JsonConverter();
            var result = converter.Serialize(val, new JsonSerializationSettings() { indentSize = 0, prettyPrint = false });

            //Debug.Log($"Json: {result}");
            var val2 = converter.Deserialize(result, new JsonDeserializationSettings());

            Assert.AreEqual(val, val2);

            var result2 = converter.Serialize(val2, new JsonSerializationSettings() { indentSize = 0, prettyPrint = false });
            Assert.AreEqual(result, result2);

        }

        private void TestParse(string val, IIonValue expected)
        {
            var converter = new JsonConverter();
            var ionValue = converter.Deserialize(val, new JsonDeserializationSettings());

            Assert.AreEqual(expected, ionValue);
        }
    }
}
