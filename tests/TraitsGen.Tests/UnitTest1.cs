using System.Runtime.InteropServices;

namespace TraitsGen.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var ab = new AB();
            //ab.SampleAName = "1";
            //ab.SampleAValue = "2";
            ab.SampleBNumber = 1;
            ab.SampleBValue = 2;
            ab.A = "A";
            ab.B = "B";
        }
    }


    [Mixin<SampleA>]
    [Mixin<SampleB>]
    [Omit<SampleA>("SampleAName")]
    [Pick<SampleA>("A", "B")]
    public partial class AB
    {

    }

    public partial class SampleA
    {
        public string SampleAName { get; set; }

        public string SampleAValue { get; set; }
    }

    public class SampleB
    {
        public int SampleBNumber { get; set; }
        public int SampleBValue { get; set; }
    }

}