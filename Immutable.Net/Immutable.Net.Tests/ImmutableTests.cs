using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using NUnit.Framework;
using ProtoBuf;
using Shouldly;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnostics.Windows;
using BenchmarkDotNet.Running;

namespace ImmutableNet.Tests
{
    [TestFixture]
    [Config(typeof(BenchmarkConfig))]
    public class ImmutableTests
    {
        class BenchmarkConfig : ManualConfig
        {
            public BenchmarkConfig()
            {
                Add(new MemoryDiagnoser());
            }
        }

        [ProtoContract]
        class TestClass
        {
            [ProtoMember(1)]
            public long Test { get; set; }

            [ProtoMember(2)]
            public int Test2 { get; set; }

            [ProtoMember(3)]
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

        [ProtoContract]
        class TestClassMember
        {
            [ProtoMember(1)]
            public int Member { get; set; }

            [ProtoMember(2)]
            public int Member2 { get; set; }
        }

        class TestNullableClass
        {
            public int? Nullable { get; set; }
        }
        

        [Test]
        public void Test_That_Original_Object_Cannot_Be_Modified()
        {
            var testClass = new Immutable<TestClass>();
            var newTestClass = testClass.Modify(x => x.Test = 1);
            
            testClass.Get(x => x.Test).ShouldBe(0);
        }

        [Test]
        public void Test_That_Returned_Object_Contains_Modifications()
        {
            var testClass = new Immutable<TestClass>();
            var newTestClass = testClass.Modify(x => x.Test = 2);

            newTestClass.Get(x => x.Test).ShouldBe(2);
        }

        [Test]
        public void Test_That_Modify_Uses_Defined_Setter()
        {
            var testClass = new Immutable<TestWithSetter>();
            var newTestClass = testClass.Modify(x => x.Test = 2);

            newTestClass.Get(x => x.Test).ShouldBe(4);
        }

        [Test]
        public void Test_That_Modifying_Nested_Object_Keeps_Original_Unmodified()
        {
            var testClass = new Immutable<TestClass>();
            var newTestClass = testClass.Modify(x => x.Data = 
                testClass.Get(t => t.Data)
                .Modify(t => t.Member = 2)
            );

            testClass.Get(x => x.Data.Get(y => y.Member)).ShouldBe(0);
        }

        [Test]
        public void Test_That_Modifying_Nested_Returns_Modifications()
        {
            var testClass = new Immutable<TestClass>();
            var newTestClass = testClass.Modify(x => x.Data = 
                testClass.Get(t => t.Data)
                .Modify(t => t.Member = 2)
            );

            newTestClass.Get(x => x.Data.Get(y => y.Member)).ShouldBe(2);
        }

        [Test]
        public void Test_That_Builder_Modifies_Original_Instance()
        {
            var testClass = (new Immutable<TestClass>()).ToBuilder();
            testClass.Modify(x => x.Test = 2);

            testClass.Get(x => x.Test).ShouldBe(2);
        }

        [Test]
        public void Test_That_ToBuilder_Returns_New_Instance()
        {
            var testClass = new Immutable<TestClass>();
            var builder = testClass.ToBuilder();

            testClass.ShouldNotBe(builder.ToImmutable());
        }

        [Test]
        public void Test_That_ToImmutable_Returns_New_Instance()
        {
            var testClass = new ImmutableBuilder<TestClass>();
            var immutable = testClass.ToImmutable();

            testClass.ToImmutable().ShouldNotBe(immutable);
        }

        [Test]
        public void Test_Immutable_Serialization()
        {
            var testClass = (new Immutable<TestClass>()).Modify(x => x.Test = 1);
            var serialized = JsonConvert.SerializeObject(testClass);
            var newTestClass = JsonConvert.DeserializeObject<Immutable<TestClass>>(serialized);

            testClass.Get(x => x.Test).ShouldBe(newTestClass.Get(x => x.Test));
        }

        [Test]
        public void Test_Immutable_Protobuf_Serialization()
        {
            var testClass = (new Immutable<TestClass>()).Modify(x => x.Test = 1);

            var stream = new MemoryStream();
            Serializer.Serialize(stream, testClass);
            stream.Seek(0, SeekOrigin.Begin);

            var newTestClass = Serializer.Deserialize<Immutable<TestClass>>(stream);

            testClass.Get(x => x.Test).ShouldBe(newTestClass.Get(x => x.Test));
        }

        [Test]
        public void Test_That_ImmutableBuilder_Does_Not_Modify_Original()
        {
            var testClass = (new Immutable<TestClass>()).Modify(x => x.Test = 1);
            var testClassBuilder = testClass.ToBuilder().Modify(x => x.Test = 2);

            testClass.Get(x => x.Test).ShouldNotBe(testClassBuilder.Get(x => x.Test));
        }

        [Test]
        public void Test_That_Modify_Can_Alter_Converted_Nullables()
        {
            var testClass = new Immutable<TestNullableClass>();
            decimal? testDecimal = null;

            testClass = testClass.Modify(x => x.Nullable = (int?)testDecimal);

            testClass.Get(x => x.Nullable).ShouldBeNull();
        }
    }
}
