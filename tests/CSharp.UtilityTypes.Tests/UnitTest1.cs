
namespace CSharp.UtilityTypes.Tests
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
            ab.A = true;
            //ab.B = "B";

            var a = new SampleARemoveValue
            {
                SampleAValue = "1",
            };
        }
    }


    [Mixin<SampleA>]
    [Mixin<SampleB>]
    [Omit<SampleA>("SampleAName")]
    [Pick<SampleA>("A")]
    public partial class AB
    {

    }

    [Pick<SampleA>("SampleAValue")]
    public partial class SampleARemoveValue { }

    public partial class SampleA
    {
        public string SampleAName { get; set; }

        public string SampleAValue { get; set; }

        public bool A { get; set; }

        public bool B { get; set; }
    }

    public class SampleB
    {
        public int SampleBNumber { get; set; }
        public int SampleBValue { get; set; }
    }

    public class Todo
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public bool Completed { get; set; }
    }

    [Pick<Todo>("Title", "Completed")]
    public partial class TodoPreview { }
}