namespace TraitsGen.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {

        }

        [Traits<SampleB>]
        public partial class SampleA
        {
            public string Name { get; set; }

            public string Value { get; set; }
        }

        public class SampleB
        {
            public int SampleBNumber { get; set; }
            public int SampleBValue { get; set; }
        }
    }
}