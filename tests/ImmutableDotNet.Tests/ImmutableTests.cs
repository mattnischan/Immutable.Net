using ImmutableDotNet.Serialization.Newtonsoft;
using Newtonsoft.Json;
using Shouldly;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xunit;

namespace ImmutableNet.Tests
{
    public class ImmutableTests
    {
        class TestClass
        {
            public long Test { get; set; }

            public int Test2 { get; set; }

            public Immutable<TestClassMember> Data { get; set; }

            public TestClass()
            {
                Data = new Immutable<TestClassMember>();
            }
        }

        class TestWithSetter
        {
            private int _test;
            public int Test
            {
                get
                {
                    return _test;
                }
                set
                {
                    _test = value * 2;
                }
            }
        }

        class TestClassMember
        {
            public int Member { get; set; }

            public int Member2 { get; set; }
        }

        class TestNullableClass
        {
            public int? Nullable { get; set; }
        }
        

        [Fact]
        public void Test_That_Original_Object_Cannot_Be_Modified()
        {
            var testClass = new Immutable<TestClass>();
            var newTestClass = testClass.Modify(x => x.Test = 1);
            
            testClass.Get(x => x.Test).ShouldBe(0);
        }

        [Fact]
        public void Test_That_Returned_Object_Contains_Modifications()
        {
            var testClass = new Immutable<TestClass>();
            var newTestClass = testClass.Modify(x => x.Test = 2);

            newTestClass.Get(x => x.Test).ShouldBe(2);
        }

        [Fact]
        public void Test_That_Modify_Uses_Defined_Setter()
        {
            var testClass = new Immutable<TestWithSetter>();
            var newTestClass = testClass.Modify(x => x.Test = 2);

            newTestClass.Get(x => x.Test).ShouldBe(4);
        }

        [Fact]
        public void Test_That_Modifying_Nested_Object_Keeps_Original_Unmodified()
        {
            var testClass = new Immutable<TestClass>();
            var newTestClass = testClass.Modify(x => x.Data = 
                testClass.Get(t => t.Data)
                .Modify(t => t.Member = 2)
            );

            testClass.Get(x => x.Data.Get(y => y.Member)).ShouldBe(0);
        }

        [Fact]
        public void Test_That_Modifying_Nested_Returns_Modifications()
        {
            var testClass = new Immutable<TestClass>();
            var newTestClass = testClass.Modify(x => x.Data = 
                testClass.Get(t => t.Data)
                .Modify(t => t.Member = 2)
            );

            newTestClass.Get(x => x.Data.Get(y => y.Member)).ShouldBe(2);
        }

        [Fact]
        public void Test_That_Builder_Modifies_Original_Instance()
        {
            var testClass = (new Immutable<TestClass>()).ToBuilder();
            testClass.Modify(x => x.Test = 2);

            testClass.Get(x => x.Test).ShouldBe(2);
        }

        [Fact]
        public void Test_That_ToBuilder_Returns_New_Instance()
        {
            var testClass = new Immutable<TestClass>();
            var builder = testClass.ToBuilder();

            testClass.ShouldNotBe(builder.ToImmutable());
        }

        [Fact]
        public void Test_That_ToImmutable_Returns_New_Instance()
        {
            var testClass = new ImmutableBuilder<TestClass>();
            var immutable = testClass.ToImmutable();

            testClass.ToImmutable().ShouldNotBe(immutable);
        }

        [Fact]
        public void Test_That_ImmutableBuilder_Does_Not_Modify_Original()
        {
            var testClass = (new Immutable<TestClass>()).Modify(x => x.Test = 1);
            var testClassBuilder = testClass.ToBuilder().Modify(x => x.Test = 2);

            testClass.Get(x => x.Test).ShouldNotBe(testClassBuilder.Get(x => x.Test));
        }

        [Fact]
        public void Test_That_Modify_Can_Alter_Converted_Nullables()
        {
            var testClass = new Immutable<TestNullableClass>();
            decimal? testDecimal = null;

            testClass = testClass.Modify(x => x.Nullable = (int?)testDecimal);

            testClass.Get(x => x.Nullable).ShouldBeNull();
        }

        [Fact]
        public void Test_That_JSON_NET_Serialization_Is_Correct()
        {
            var testClass = new Immutable<TestClassMember>();
            testClass = testClass.Modify(x => x.Member = 1);

            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ImmutableJsonConverter());

            var json = JsonConvert.SerializeObject(testClass, settings);
            json.ShouldBe(@"{""Member"":1,""Member2"":0}");
        }

        [Fact]
        public void Test_That_JSON_NET_Deserialization_Is_Correct()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ImmutableJsonConverter());

            var json = @"{""Member"":1,""Member2"":0}";
            var immutable = JsonConvert.DeserializeObject<Immutable<TestClassMember>>(json, settings);

            immutable.Get(x => x.Member).ShouldBe(1);
            immutable.Get(x => x.Member2).ShouldBe(0);
        }
    }
}
