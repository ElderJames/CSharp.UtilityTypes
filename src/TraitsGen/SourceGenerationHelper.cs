using System;
using System.Collections.Generic;
using System.Text;

namespace TraitsGen;

public static class SourceGenerationHelper
{
    public const string Attribute = @"
namespace TraitsGen
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    public class TraitsAttribute<T> : System.Attribute
    {
    }
}";

  
}

