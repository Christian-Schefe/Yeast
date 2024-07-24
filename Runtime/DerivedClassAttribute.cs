using System;

namespace Yeast
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, Inherited = false)]
    public class HasDerivedClassesAttribute : Attribute
    {
        public Type[] DerivedTypes { get; private set; }

        public HasDerivedClassesAttribute(params Type[] baseType)
        {
            DerivedTypes = baseType;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false)]
    public class IsDerivedClassAttribute : Attribute
    {
        public string Identifier { get; private set; }

        public IsDerivedClassAttribute(string identifier)
        {
            Identifier = identifier;
        }
    }
}