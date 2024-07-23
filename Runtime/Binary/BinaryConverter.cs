using System;
using System.Collections.Generic;
using Yeast.Memento;

namespace Yeast.Binary
{

    public class BinaryConversionException : Exception
    {
        public BinaryConversionException(string message) : base(message) { }
    }

    public class BinaryConverter
    {
        public IMemento Deserialize(byte[] value)
        {
            int offset = 0;
            return DeserializeInternal(value, ref offset);
        }

        private IMemento DeserializeInternal(byte[] value, ref int offset)
        {
            byte type = value[offset];
            offset += 1;

            if (type == (byte)MementoType.String)
            {
                string stringValue = DeserializeString(value, ref offset);
                return new StringMemento(stringValue);
            }
            else if (type == (byte)MementoType.Integer)
            {
                long integerValue = DeserializeLong(value, ref offset);
                return new IntegerMemento(integerValue);
            }
            else if (type == (byte)MementoType.Bool)
            {
                bool booleanValue = DeserializeBoolean(value, ref offset);
                return new BoolMemento(booleanValue);
            }
            else if (type == (byte)MementoType.Decimal)
            {
                double floatValue = DeserializeDouble(value, ref offset);
                return new DecimalMemento(floatValue);
            }
            else if (type == (byte)MementoType.Null)
            {
                return new NullMemento();
            }
            else if (type == (byte)MementoType.Array)
            {
                return DeserializeArray(value, ref offset);
            }
            else if (type == (byte)MementoType.Dict)
            {
                return DeserializeObject(value, ref offset);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private ArrayMemento DeserializeArray(byte[] value, ref int offset)
        {
            int length = DeserializeInt(value, ref offset);
            List<IMemento> values = new();
            for (int i = 0; i < length; i++)
            {
                values.Add(DeserializeInternal(value, ref offset));
            }
            return new ArrayMemento(values);
        }

        private DictMemento DeserializeObject(byte[] value, ref int offset)
        {
            int length = DeserializeInt(value, ref offset);
            Dictionary<string, IMemento> values = new();
            for (int i = 0; i < length; i++)
            {
                string key = DeserializeString(value, ref offset);
                values.Add(key, DeserializeInternal(value, ref offset));
            }
            return new DictMemento(values);
        }

        public byte[] Serialize(IMemento value)
        {
            List<byte> result = new() { (byte)value.MementoType };
            if (value is StringMemento stringValue)
            {
                SerializeString(stringValue.value, result);
            }
            else if (value is IntegerMemento integerValue)
            {
                result.AddRange(BitConverter.GetBytes(integerValue.value));
            }
            else if (value is BoolMemento booleanValue)
            {
                result.AddRange(BitConverter.GetBytes(booleanValue.value));
            }
            else if (value is DecimalMemento floatValue)
            {
                result.AddRange(BitConverter.GetBytes(floatValue.value));
            }
            else if (value is NullMemento)
            {
            }
            else if (value is ArrayMemento arrayValue)
            {
                result.AddRange(SerializeArray(arrayValue.value));
            }
            else if (value is DictMemento objectValue)
            {
                result.AddRange(SerializeObject(objectValue.value));
            }
            else
            {
                throw new NotImplementedException();
            }
            return result.ToArray();
        }

        private byte[] SerializeArray(IMemento[] values)
        {
            List<byte> bytes = new();
            bytes.AddRange(BitConverter.GetBytes(values.Length));
            foreach (var item in values)
            {
                bytes.AddRange(Serialize(item));
            }
            return bytes.ToArray();
        }

        private byte[] SerializeObject(Dictionary<string, IMemento> values)
        {
            List<byte> bytes = new();
            bytes.AddRange(BitConverter.GetBytes(values.Count));
            foreach (var item in values)
            {
                SerializeString(item.Key, bytes);
                bytes.AddRange(Serialize(item.Value));
            }
            return bytes.ToArray();
        }

        private void SerializeString(string str, List<byte> bytes)
        {
            var strBytes = System.Text.Encoding.UTF8.GetBytes(str);
            bytes.AddRange(BitConverter.GetBytes(strBytes.Length));
            bytes.AddRange(strBytes);
        }

        private string DeserializeString(byte[] value, ref int offset)
        {
            int length = DeserializeInt(value, ref offset);
            var str = System.Text.Encoding.UTF8.GetString(value, offset, length);
            offset += length;
            return str;
        }

        private int DeserializeInt(byte[] value, ref int offset)
        {
            int intValue = BitConverter.ToInt32(value, offset);
            offset += 4;
            return intValue;
        }

        private long DeserializeLong(byte[] value, ref int offset)
        {
            long longValue = BitConverter.ToInt64(value, offset);
            offset += 8;
            return longValue;
        }

        private bool DeserializeBoolean(byte[] value, ref int offset)
        {
            bool booleanValue = BitConverter.ToBoolean(value, offset);
            offset += 1;
            return booleanValue;
        }

        private double DeserializeDouble(byte[] value, ref int offset)
        {
            double doubleValue = BitConverter.ToDouble(value, offset);
            offset += 8;
            return doubleValue;
        }
    }
}
