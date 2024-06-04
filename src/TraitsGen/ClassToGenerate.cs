using System;
using System.Collections.Generic;
using System.Text;

namespace TraitsGen
{
    public class ClassToGenerate
    {
        public string Name { get; set; }

        public string NameSpace { get; set; }

        public Dictionary<string, string> Properties { get; set; }
    }
}
