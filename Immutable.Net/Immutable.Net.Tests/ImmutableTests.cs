using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ImmutableNet.Tests
{
    public class ImmutableTests
    {
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

        [Fact]
        public void Test_That_Original_Object_Cannot_Be_Modified()
        {
            var testClass = new Immutable<TestClass>();
            var newTestClass = testClass.Modify(x => x.Test, 1);

            Assert.Equal(testClass.Get(x => x.Test), 0);
        }

        [Fact]
        public void Test_That_Returned_Object_Contains_Modifications()
        {
            var testClass = new Immutable<TestClass>();
            var newTestClass = testClass.Modify(x => x.Test, 2);

            Assert.Equal(newTestClass.Get(x => x.Test), 2);
        }

        [Fact]
        public void Test_That_Modifying_Nested_Object_Keeps_Original_Unmodified()
        {
            var testClass = new Immutable<TestClass>();
            var newTestClass = testClass.Modify(x => x.Data, 
                testClass.Get(x => x.Data)
                .Modify(x => x.Member, 2)
            );

            Assert.Equal(testClass.Get(x => x.Data.Get(y => y.Member)), 0);
        }

        [Fact]
        public void Test_That_Modifying_Nested_Returns_Modifications()
        {
            var testClass = new Immutable<TestClass>();
            var newTestClass = testClass.Modify(x => x.Data, 
                testClass.Get(x => x.Data)
                .Modify(x => x.Member, 2)
            );

            Assert.Equal(newTestClass.Get(x => x.Data.Get(y => y.Member)), 2);
        }

        [Fact]
        public void Test_Modify_With_Implicit_Conversion()
        {
            var testClass = (new Immutable<TestClass>()).Modify(x => x.Test, 2.0M);

            Assert.Equal(testClass.Get(x => x.Test), 2);
        }

        [Fact]
        public void Test_Modify_Nested_With_Implicit_Conversion()
        {
            var testClass = new Immutable<TestClass>();
            var newTestClass = testClass.Modify(x => x.Data,
                testClass.Get(x => x.Data)
                .Modify(x => x.Member, 2.0M)
            );

            Assert.Equal(newTestClass.Get(x => x.Data.Get(y => y.Member)), 2);
        }

        [Fact]
        public void Test_That_Builder_Modifies_Original_Instance()
        {
            var testClass = (new Immutable<TestClass>()).ToBuilder();
            testClass.Modify(x => x.Test = 2);

            Assert.Equal(testClass.Get(x => x.Test), 2);
        }

        [Fact]
        public void Test_That_ToBuilder_Returns_New_Instance()
        {
            var testClass = new Immutable<TestClass>();
            var builder = testClass.ToBuilder();

            Assert.NotSame(testClass, builder);
        }

        [Fact]
        public void Test_That_ToImmutable_Returns_New_Instance()
        {
            var testClass = new ImmutableBuilder<TestClass>();
            var immutable = testClass.ToImmutable();

            Assert.NotSame(testClass, immutable);
        }

        [Fact]
        public void Immutable_Microbenchmark()
        {
            var stopwatch = new Stopwatch();
            var testClass = new TestClass();
            var testClassImmutable = new Immutable<TestClass>();

            stopwatch.Start();
            for(var i = 0; i < 100000; i++)
            {
                testClass.Test = i;
            }
            stopwatch.Stop();

            var setterTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Reset();
            stopwatch.Start();
            for(var i = 0; i < 100000; i++)
            {
               testClassImmutable.Modify(x => x.Test, i);
            }
            stopwatch.Stop();

            var immutableTime = stopwatch.ElapsedMilliseconds;
        }

        [Fact]
        public void ImmutableBuilder_Microbenchmark()
        {
            var stopwatch = new Stopwatch();
            var testClass = new TestClass();
            var testClassImmutableBuilder = new ImmutableBuilder<TestClass>();

            stopwatch.Start();
            for (var i = 0; i < 100000; i++)
            {
                testClass.Test = i;
            }
            stopwatch.Stop();

            var setterTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Reset();

            testClassImmutableBuilder.Modify(x => x.Test = 1);

            stopwatch.Start();
            for (var i = 0; i < 100000; i++)
            {
                testClassImmutableBuilder.Modify(x => x.Test = 1);
            }
            stopwatch.Stop();

            var immutableTime = stopwatch.ElapsedMilliseconds;
        }

        [Fact]
        public void Test_Immutable_Serialization()
        {
            var testClass = (new Immutable<TestClass>()).Modify(x => x.Test, 1);
            string serialized = JsonConvert.SerializeObject(testClass);
            var newTestClass = JsonConvert.DeserializeObject<Immutable<TestClass>>(serialized);

            Assert.Equal(testClass.Get(x => x.Test), newTestClass.Get(x => x.Test));
        }

        [Fact]
        public void Test_Immutable_Protobuf_Serialization()
        {
            var testClass = (new Immutable<TestClass>()).Modify(x => x.Test, 1);

            var stream = new MemoryStream();
            Serializer.Serialize(stream, testClass);
            stream.Seek(0, SeekOrigin.Begin);

            var newTestClass = Serializer.Deserialize<Immutable<TestClass>>(stream);

            Assert.Equal(testClass.Get(x => x.Test), newTestClass.Get(x => x.Test));
        }

        [Fact]
        public void Test_That_ImmutableBuilder_Does_Not_Modify_Original()
        {
            var testClass = (new Immutable<TestClass>()).Modify(x => x.Test, 1);
            var testClassBuilder = testClass.ToBuilder().Modify(x => x.Test = 2);

            Assert.NotEqual(testClass.Get(x => x.Test), testClassBuilder.Get(x => x.Test));
        }

        [Fact]
        public void Test_That_Modify_Can_Alter_Converted_Nullables()
        {
            var testClass = new Immutable<TestNullableClass>();
            decimal? testDecimal = null;

            testClass = testClass.Modify(x => x.Nullable, testDecimal);

            Assert.Null(testClass.Get(x => x.Nullable));
        }
    }
}
