using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Yeast.Memento;
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
            Test(new NullMemento());
        }

        [Test]
        public void TestIntegers()
        {
            Test(new IntegerMemento(0));
            Test(new IntegerMemento(1));
            Test(new IntegerMemento(-1));
            Test(new IntegerMemento(624562456));
            Test(new IntegerMemento(-624562456));
            Test(new IntegerMemento(long.MaxValue));
            Test(new IntegerMemento(long.MinValue));
        }

        [Test]
        public void TestStrings()
        {
            Test(new StringMemento("Hello World!"));
            Test(new StringMemento(""));
            Test(new StringMemento(" "));
            Test(new StringMemento("1234567890"));
            Test(new StringMemento("Hello\nWorld!"));
            Test(new StringMemento("Hello\t\0\r\\\"World!"));
            Test(new StringMemento("\0"));
            Test(new StringMemento("\x01"));
            Test(new StringMemento("\0\a\b\f\r\t\n\v\u1234"));
            Test(new StringMemento("\n"));
            Test(new StringMemento("\r"));
            Test(new StringMemento("\t"));
            Test(new StringMemento("\b"));
            Test(new StringMemento("\\"));
            Test(new StringMemento("\""));
        }

        [Test]
        public void TestFloats()
        {
            Test(new DecimalMemento(0));
            Test(new DecimalMemento(1));
            Test(new DecimalMemento(-1));
            Test(new DecimalMemento(3.14159f));
            Test(new DecimalMemento(-3.14159f));
            Test(new DecimalMemento(3457457457.14159d));
            Test(new DecimalMemento(-3457457457.14159d));
            Test(new DecimalMemento(1E20));
            Test(new DecimalMemento(-1E20));
            Test(new DecimalMemento(float.MaxValue));
            Test(new DecimalMemento(float.MinValue));

            Test(new IntegerMemento(1_000_000_000_000_000));
            Test(new DecimalMemento(1_000_000_000_000_000_000_000f));
            Test(new DecimalMemento(-1_000_000_000_000_000_000_000d));
            Test(new DecimalMemento(double.Epsilon));
            Test(new DecimalMemento(double.MaxValue));
            Test(new DecimalMemento(double.NaN));
            Test(new DecimalMemento(double.NegativeInfinity));
            Test(new DecimalMemento(double.PositiveInfinity));

            TestParse("1E20", new DecimalMemento(1e20));
        }

        [Test]
        public void TestBooleans()
        {
            Test(new BoolMemento(true));
            Test(new BoolMemento(false));
        }

        [Test]
        public void TestArrays()
        {
            Test(new ArrayMemento(new List<IMemento>()));
            Test(new ArrayMemento(new List<IMemento> { new IntegerMemento(1) }));
            Test(new ArrayMemento(new List<IMemento> { new IntegerMemento(1), new IntegerMemento(2) }));
            Test(new ArrayMemento(new List<IMemento> { new IntegerMemento(1), new IntegerMemento(2), new IntegerMemento(3) }));
            Test(new ArrayMemento(new List<IMemento> { new IntegerMemento(1), new StringMemento("hello"), new DecimalMemento(3.141d), new IntegerMemento(4) }));
            Test(new ArrayMemento(new List<IMemento> {
                new ArrayMemento(new List<IMemento> { new IntegerMemento(1), new IntegerMemento(2) }),
                new ArrayMemento(new List<IMemento> { new StringMemento("hello"), new IntegerMemento(2) }),
                new ArrayMemento(new List<IMemento> { new IntegerMemento(1), new NullMemento(), new NullMemento() })
            }));

            var bigList = new List<IMemento>();
            for (int i = 0; i < 1000; i++) bigList.Add(new IntegerMemento(i));
            Test(new ArrayMemento(bigList));
        }

        [Test]
        public void TestMaps()
        {
            Test(new DictMemento(new Dictionary<string, IMemento>()));
            Test(new DictMemento(new Dictionary<string, IMemento> { { "f1", new IntegerMemento(1) } }));
            Test(new DictMemento(new Dictionary<string, IMemento> {
                { "f1", new IntegerMemento(1) },
                { "f2", new IntegerMemento(2) },
                { "this is a key", new DictMemento(new Dictionary<string, IMemento> { { "f1", new IntegerMemento(1) } }) }
            }));
        }

        private void Test(IMemento val)
        {
            var converter = new JsonConverter();
            var result = converter.Serialize(val, new JsonSerializationSettings() { indentSize = 0, prettyPrint = false });

            //Debug.Log($"Json: {result}");
            var val2 = converter.Deserialize(result, new JsonDeserializationSettings());

            Assert.AreEqual(val, val2);

            var result2 = converter.Serialize(val2, new JsonSerializationSettings() { indentSize = 0, prettyPrint = false });
            Assert.AreEqual(result, result2);

        }

        private void TestParse(string val, IMemento expected)
        {
            var converter = new JsonConverter();
            var ionValue = converter.Deserialize(val, new JsonDeserializationSettings());

            Assert.AreEqual(expected, ionValue);
        }
    }
}
