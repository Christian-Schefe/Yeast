using System;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
public class HasDerivedClassesAttribute : Attribute
{
    public Type[] DerivedTypes { get; private set; }

    public HasDerivedClassesAttribute(params Type[] baseType)
    {
        DerivedTypes = baseType;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class IsDerivedClassAttribute : Attribute
{
    public string Identifier { get; private set; }

    public IsDerivedClassAttribute(string identifier)
    {
        Identifier = identifier;
    }
}
