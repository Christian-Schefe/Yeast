using System.Collections.Generic;
using System.Linq;
using Yeast.Json;

namespace Yeast.Memento
{
    public interface IMemento
    {
        public MementoType MementoType { get; }
        public void Accept(IMementoVisitor visitor);
        public bool ValueEquals(IMemento other);
        public object GetValueAsObject();
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

        public override string ToString()
        {
            return "NullMemento()";
        }

        public bool ValueEquals(IMemento other)
        {
            return other is NullMemento;
        }

        public void Accept(IMementoVisitor visitor)
        {
            visitor.Visit(this);
        }

        public object GetValueAsObject() => null;
    }

    public class StringMemento : IMemento
    {
        public MementoType MementoType => MementoType.String;

        public string value;

        public StringMemento(string value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return $"StringMemento(\"{value}\")";
        }

        public bool ValueEquals(IMemento other)
        {
            return other is StringMemento str && str.value == value;
        }

        public void Accept(IMementoVisitor visitor)
        {
            visitor.Visit(this);
        }

        public object GetValueAsObject() => value;
    }

    public class IntegerMemento : IMemento
    {
        public MementoType MementoType => MementoType.Integer;

        public long value;

        public IntegerMemento(long value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return $"IntegerMemento({value})";
        }

        public bool ValueEquals(IMemento other)
        {
            if (other is DecimalMemento dec) return dec.value == value;
            else return other is IntegerMemento integer && integer.value == value;
        }

        public void Accept(IMementoVisitor visitor)
        {
            visitor.Visit(this);
        }

        public object GetValueAsObject() => value;
    }

    public class DecimalMemento : IMemento
    {
        public MementoType MementoType => MementoType.Decimal;

        public double value;

        public DecimalMemento(double value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return $"DecimalMemento({value})";
        }

        public bool ValueEquals(IMemento other)
        {
            if (other is IntegerMemento integer) return integer.value == value;
            else if (other is DecimalMemento dec)
            {
                if (double.IsNaN(dec.value) && double.IsNaN(value)) return true;
                else return dec.value == value;
            }
            else return false;
        }

        public void Accept(IMementoVisitor visitor)
        {
            visitor.Visit(this);
        }

        public object GetValueAsObject() => value;
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

        public override string ToString()
        {
            return $"BoolMemento({value})";
        }

        public bool ValueEquals(IMemento other)
        {
            return other is BoolMemento boolean && boolean.value == value;
        }

        public void Accept(IMementoVisitor visitor)
        {
            visitor.Visit(this);
        }

        public object GetValueAsObject() => value;
    }

    public class ArrayMemento : IMemento
    {
        public MementoType MementoType => MementoType.Array;

        public IMemento[] value;

        public ArrayMemento(List<IMemento> value)
        {
            this.value = value.ToArray();
        }

        public ArrayMemento(IMemento[] value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            var str = value == null ? "" : string.Join(", ", value.Select(v => v.ToString()));
            return $"ArrayMemento({str})";
        }

        public bool ValueEquals(IMemento other)
        {
            if (other is ArrayMemento arr && arr.value.Length == value.Length)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    if (!value[i].ValueEquals(arr.value[i])) return false;
                }
                return true;
            }
            return false;
        }

        public void Accept(IMementoVisitor visitor)
        {
            visitor.Visit(this);
        }

        public object GetValueAsObject()
        {
            var list = new List<object>();
            foreach (var val in value) list.Add(val.GetValueAsObject());
            return list;
        }
    }

    public class DictMemento : IMemento
    {
        public MementoType MementoType => MementoType.Dict;

        public Dictionary<string, IMemento> value;

        public DictMemento(Dictionary<string, IMemento> value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            var str = string.Join(", ", value.Select(v => $"{v.Key}: {v.Value}"));
            return $"DictMemento({str})";
        }

        public bool ValueEquals(IMemento other)
        {
            if (other is DictMemento dict && dict.value.Count == value.Count)
            {
                foreach (var pair in value)
                {
                    if (!dict.value.TryGetValue(pair.Key, out var val2) || !pair.Value.ValueEquals(val2)) return false;
                }
                return true;
            }
            return false;
        }

        public void Accept(IMementoVisitor visitor)
        {
            visitor.Visit(this);
        }

        public object GetValueAsObject()
        {
            var dict = new Dictionary<string, object>();
            foreach (var pair in value) dict[pair.Key] = pair.Value.GetValueAsObject();
            return dict;
        }
    }

    public interface IMementoVisitor
    {
        public void Visit(NullMemento memento);
        public void Visit(StringMemento memento);
        public void Visit(IntegerMemento memento);
        public void Visit(DecimalMemento memento);
        public void Visit(BoolMemento memento);
        public void Visit(ArrayMemento memento);
        public void Visit(DictMemento memento);
    }

    public interface IMementoVisitor<T> : IMementoVisitor
    {
        public T GetResult();
    }
}