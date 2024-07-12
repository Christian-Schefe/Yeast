using System.Collections.Generic;
using System.Linq;

namespace Yeast.Ion
{
    public interface IIonValue
    {
        public IonType IonType { get; }
    }

    public enum IonType : byte
    {
        Null,
        String,
        Integer,
        Float,
        Boolean,
        Array,
        Map
    }

    public class NullValue : IIonValue
    {
        public IonType IonType => IonType.Null;

        public NullValue() { }

        public override bool Equals(object obj)
        {
            return obj is NullValue;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "NullValue()";
        }
    }

    public class StringValue : IIonValue
    {
        public IonType IonType => IonType.String;

        public string value;

        public StringValue()
        {
            value = null;
        }

        public StringValue(string value)
        {
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is StringValue value && ((this.value == null && value.value == null) || this.value.Equals(value.value));
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return $"StringValue(\"{value}\")";
        }
    }

    public class IntegerValue : IIonValue
    {
        public IonType IonType => IonType.Integer;

        public long value;

        public IntegerValue()
        {
            value = 0;
        }

        public IntegerValue(long value)
        {
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is IntegerValue value && this.value.Equals(value.value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return $"IntegerValue({value})";
        }
    }

    public class FloatValue : IIonValue
    {
        public IonType IonType => IonType.Float;

        public double value;

        public FloatValue()
        {
            value = 0;
        }

        public FloatValue(double value)
        {
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is FloatValue value && this.value.Equals(value.value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return $"FloatValue({value})";
        }
    }

    public class BooleanValue : IIonValue
    {
        public IonType IonType => IonType.Boolean;

        public bool value;

        public BooleanValue()
        {
            value = false;
        }

        public BooleanValue(bool value)
        {
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is BooleanValue value && this.value.Equals(value.value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return $"BooleanValue({value})";
        }
    }

    public class ArrayValue : IIonValue
    {
        public IonType IonType => IonType.Array;

        public IIonValue[] value;

        public ArrayValue()
        {
            value = null;
        }

        public ArrayValue(List<IIonValue> value)
        {
            this.value = value.ToArray();
        }

        public ArrayValue(IIonValue[] value)
        {
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is ArrayValue value && ((this.value == null && value.value == null) || this.value.SequenceEqual(value.value));
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            var str = value == null ? "" : string.Join(", ", value.Select(v => v.ToString()));
            return $"ArrayValue({str})";
        }
    }

    public class MapValue : IIonValue
    {
        public IonType IonType => IonType.Map;

        public Dictionary<string, IIonValue> value;

        public MapValue()
        {
            value = null;
        }

        public MapValue(Dictionary<string, IIonValue> value)
        {
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is MapValue value && ((this.value == null && value.value == null) || this.value.SequenceEqual(value.value));
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            var str = string.Join(", ", value.Select(v => $"{v.Key}: {v.Value}"));
            return $"MapValue({str})";
        }
    }
}