using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = true)]
public class HasDerivedClassAttribute : Attribute
{
    public Type DerivedType { get; private set; }
    public string Identifier { get; private set; }

    public HasDerivedClassAttribute(Type baseType, string identifier)
    {
        DerivedType = baseType;
        Identifier = identifier;
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class IsDerivedClassAttribute : Attribute
{
    public Type BaseType { get; private set; }

    public IsDerivedClassAttribute(Type baseType)
    {
        BaseType = baseType;
    }
}
