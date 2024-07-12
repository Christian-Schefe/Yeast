using System;
using System.Collections.Generic;
using Yeast.Ion;

namespace Yeast.Binary
{
    public struct ToBinarySettings
    {

    }

    public struct FromBinarySettings
    {

    }

    public class BinaryConversionException : Exception
    {
        public BinaryConversionException(string message) : base(message) { }
    }

    public class BinaryConverter : IFromConverter<byte[], IIonValue, FromBinarySettings, BinaryConversionException>, IIntoConverter<IIonValue, byte[], ToBinarySettings, BinaryConversionException>
    {
        private FromBinarySettings fromBinarySettings = new();
        private ToBinarySettings toBinarySettings = new();

        public bool TryFrom(byte[] value, out IIonValue result, FromBinarySettings settings, out BinaryConversionException exception)
        {
            fromBinarySettings = settings;
            try
            {
                int offset = 0;
                result = Deserialize(value, ref offset);
                exception = null;
                return true;
            }
            catch (BinaryConversionException e)
            {
                result = null;
                exception = e;
                return false;
            }
        }

        public bool TryInto(IIonValue value, out byte[] result, ToBinarySettings settings, out BinaryConversionException exception)
        {
            toBinarySettings = settings;
            try
            {
                result = Serialize(value);
                exception = null;
                return true;
            }
            catch (BinaryConversionException e)
            {
                result = null;
                exception = e;
                return false;
            }
        }

        private IIonValue Deserialize(byte[] value, ref int offset)
        {
            byte type = value[offset];
            offset += 1;

            if (type == (byte)IonType.String)
            {
                string stringValue = DeserializeString(value, ref offset);
                return new StringValue(stringValue);
            }
            else if (type == (byte)IonType.Integer)
            {
                long integerValue = DeserializeLong(value, ref offset);
                return new IntegerValue(integerValue);
            }
            else if (type == (byte)IonType.Boolean)
            {
                bool booleanValue = DeserializeBoolean(value, ref offset);
                return new BooleanValue(booleanValue);
            }
            else if (type == (byte)IonType.Float)
            {
                double floatValue = DeserializeDouble(value, ref offset);
                return new FloatValue(floatValue);
            }
            else if (type == (byte)IonType.Null)
            {
                return new NullValue();
            }
            else if (type == (byte)IonType.Array)
            {
                return DeserializeArray(value, ref offset);
            }
            else if (type == (byte)IonType.Map)
            {
                return DeserializeObject(value, ref offset);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private ArrayValue DeserializeArray(byte[] value, ref int offset)
        {
            int length = DeserializeInt(value, ref offset);
            List<IIonValue> values = new();
            for (int i = 0; i < length; i++)
            {
                values.Add(Deserialize(value, ref offset));
            }
            return new ArrayValue(values);
        }

        private MapValue DeserializeObject(byte[] value, ref int offset)
        {
            int length = DeserializeInt(value, ref offset);
            Dictionary<string, IIonValue> values = new();
            for (int i = 0; i < length; i++)
            {
                string key = DeserializeString(value, ref offset);
                values.Add(key, Deserialize(value, ref offset));
            }
            return new MapValue(values);
        }

        private byte[] Serialize(IIonValue value)
        {
            List<byte> result = new() { (byte)value.IonType };
            if (value is StringValue stringValue)
            {
                SerializeString(stringValue.value, result);
            }
            else if (value is IntegerValue integerValue)
            {
                result.AddRange(BitConverter.GetBytes(integerValue.value));
            }
            else if (value is BooleanValue booleanValue)
            {
                result.AddRange(BitConverter.GetBytes(booleanValue.value));
            }
            else if (value is FloatValue floatValue)
            {
                result.AddRange(BitConverter.GetBytes(floatValue.value));
            }
            else if (value is NullValue)
            {
            }
            else if (value is ArrayValue arrayValue)
            {
                result.AddRange(SerializeArray(arrayValue.value));
            }
            else if (value is MapValue objectValue)
            {
                result.AddRange(SerializeObject(objectValue.value));
            }
            else
            {
                throw new NotImplementedException();
            }
            return result.ToArray();
        }

        private byte[] SerializeArray(IIonValue[] values)
        {
            List<byte> bytes = new();
            bytes.AddRange(BitConverter.GetBytes(values.Length));
            foreach (var item in values)
            {
                bytes.AddRange(Serialize(item));
            }
            return bytes.ToArray();
        }

        private byte[] SerializeObject(Dictionary<string, IIonValue> values)
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
