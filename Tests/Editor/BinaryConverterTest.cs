using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Yeast.Binary;
using Yeast.Memento;

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
            var converter = new BinaryConverter();
            var result = converter.Serialize(val);

            var val2 = converter.Deserialize(result);
            Assert.IsTrue(val.ValueEquals(val2));

            var result2 = converter.Serialize(val2);
            Assert.AreEqual(result, result2);
        }
    }
}
