using System;
using System.Collections.Generic;
using System.Text;

namespace CSharp.UtilityTypes;

public static class SourceGenerationHelper
{
    public const string MixinAttribute = @"
namespace CSharp.UtilityTypes
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    public class MixinAttribute<T> : System.Attribute
    {
    }
}";

    public const string PickAttribute = @"
namespace CSharp.UtilityTypes
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    public class PickAttribute<T> : System.Attribute
    {
        public PickAttribute(params string[] names)
        {
         
        }
    }
}";

    public const string OmitAttribute = @"
namespace CSharp.UtilityTypes
{
    [System.AttributeUsage(System.AttributeTargets.Class, AllowMultiple = true)]
    public class OmitAttribute<T> : System.Attribute
    {
        public OmitAttribute(params string[] names)
        {
         
        }
    }
}";
}

