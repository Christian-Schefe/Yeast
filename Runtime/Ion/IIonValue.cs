using System.Collections.Generic;

namespace Yeast.Ion
{
    public interface IIonValue { }

    public class NullValue : IIonValue
    {
        public NullValue() { }

        public override bool Equals(object obj)
        {
            return obj is NullValue;
        }

        public override int GetHashCode()
        {
            return 0;
        }
    }

    public class StringValue : IIonValue
    {
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
    }

    public class IntegerValue : IIonValue
    {
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
    }

    public class FloatValue : IIonValue
    {
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
    }

    public class BooleanValue : IIonValue
    {
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
    }

    public class ArrayValue : IIonValue
    {
        public List<IIonValue> value;

        public ArrayValue()
        {
            value = null;
        }

        public ArrayValue(List<IIonValue> value)
        {
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is ArrayValue value && ((this.value == null && value.value == null) || this.value.Equals(value.value));
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }

    public class MapValue : IIonValue
    {
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
            return obj is MapValue value && ((this.value == null && value.value == null) || this.value.Equals(value.value));
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }
    }
}