using System.Collections.Generic;
using System.Linq;

namespace Yeast.Memento
{
    public interface IMemento
    {
        public MementoType MementoType { get; }
    }

    public enum MementoType : byte
    {
        Null,
        String,
        Integer,
        Decimal,
        Bool,
        Array,
        Dict
    }

    public class NullMemento : IMemento
    {
        public MementoType MementoType => MementoType.Null;

        public NullMemento() { }

        public override bool Equals(object obj)
        {
            return obj is NullMemento;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public override string ToString()
        {
            return "NullMemento()";
        }
    }

    public class StringMemento : IMemento
    {
        public MementoType MementoType => MementoType.String;

        public string value;

        public StringMemento()
        {
            value = null;
        }

        public StringMemento(string value)
        {
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is StringMemento value && ((this.value == null && value.value == null) || this.value.Equals(value.value));
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return $"StringMemento(\"{value}\")";
        }
    }

    public class IntegerMemento : IMemento
    {
        public MementoType MementoType => MementoType.Integer;

        public long value;

        public IntegerMemento()
        {
            value = 0;
        }

        public IntegerMemento(long value)
        {
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is IntegerMemento value && this.value.Equals(value.value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return $"IntegerMemento({value})";
        }
    }

    public class DecimalMemento : IMemento
    {
        public MementoType MementoType => MementoType.Decimal;

        public double value;

        public DecimalMemento()
        {
            value = 0;
        }

        public DecimalMemento(double value)
        {
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is DecimalMemento value && this.value.Equals(value.value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return $"FloatMemento({value})";
        }
    }

    public class BoolMemento : IMemento
    {
        public MementoType MementoType => MementoType.Bool;

        public bool value;

        public BoolMemento()
        {
            value = false;
        }

        public BoolMemento(bool value)
        {
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is BoolMemento value && this.value.Equals(value.value);
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            return $"BooleanMemento({value})";
        }
    }

    public class ArrayMemento : IMemento
    {
        public MementoType MementoType => MementoType.Array;

        public IMemento[] value;

        public ArrayMemento()
        {
            value = null;
        }

        public ArrayMemento(List<IMemento> value)
        {
            this.value = value.ToArray();
        }

        public ArrayMemento(IMemento[] value)
        {
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is ArrayMemento value && ((this.value == null && value.value == null) || this.value.SequenceEqual(value.value));
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            var str = value == null ? "" : string.Join(", ", value.Select(v => v.ToString()));
            return $"ArrayMemento({str})";
        }
    }

    public class DictMemento : IMemento
    {
        public MementoType MementoType => MementoType.Dict;

        public Dictionary<string, IMemento> value;

        public DictMemento()
        {
            value = null;
        }

        public DictMemento(Dictionary<string, IMemento> value)
        {
            this.value = value;
        }

        public override bool Equals(object obj)
        {
            return obj is DictMemento value && ((this.value == null && value.value == null) || this.value.SequenceEqual(value.value));
        }

        public override int GetHashCode()
        {
            return value.GetHashCode();
        }

        public override string ToString()
        {
            var str = string.Join(", ", value.Select(v => $"{v.Key}: {v.Value}"));
            return $"MapMemento({str})";
        }
    }
}