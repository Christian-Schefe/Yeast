using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Yeast.Binary;
using Yeast.Ion;

namespace Yeast.Test
{
    public class BinaryConverterTest
    {
        [Test]
        public void TestAll()
        {
            TestNull();
            TestIntegers();
            TestStrings();
            TestFloats();
            TestBooleans();
            TestArrays();
            TestMaps();
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
            var converter = new BinaryConverter();
            var result = converter.Serialize(val, new BinarySerializationSettings());

            //Debug.Log($"Binary: {string.Join(", ", result)}");
            var val2 = converter.Deserialize(result, new BinaryDeserializationSettings());
            Assert.AreEqual(val, val2);

            var result2 = converter.Serialize(val2, new BinarySerializationSettings());
            Assert.AreEqual(result, result2);
        }
    }
}
