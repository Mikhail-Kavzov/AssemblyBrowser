using System.Collections.Generic;
using Xunit;

namespace AssemblyBrowserLib.Tests
{
    public class AssemblyParserTests
    {
        public static IEnumerable<object[]> TestLibsPaths
        {
            get
            {
                yield return new object[] { "./Moq.dll" };
                yield return new object[] { "./Newtonsoft.Json.dll" };
                yield return new object[] { "./AutoMapper.dll" };
                yield return new object[] { "./Castle.Core.dll" };
                yield return new object[] { "./AssemblyBrowserLib.Tests.dll" };
            }
        }
        
        public static IEnumerable<object[]> TestMembers
        {
            get
            {
                yield return new object[]
                {
                    "namespace AssemblyBrowserLib.Tests", "public class TestClass1", new List<string>
                    {
                        "protected internal Int32 ReturnInt(Boolean param1, Byte param2, Char param3)",
                        "private T ReturnAny<T>()"
                    }
                };
                yield return new object[]
                {
                    "namespace System", "String", new List<string>
                    {
                        "public static Void DoNothing(this String str)",
                        "public static Void DoNothing(this String str, Boolean isNothing)",
                        "public static String DoReturn(this String str)"
                    }
                };
                yield return new object[]
                {
                    "namespace AssemblyBrowserLib.Tests", "internal class TestClass3", new List<string>
                    {
                        "public T Prop1 { public get; public set; }",
                        "public Int32 Prop2 { public get; private set; }",
                        "private Single Prop3 { private get; }",
                        "protected T ReturnT()"
                    }
                };
                yield return new object[]
                {
                    "namespace AssemblyBrowserLib.Tests", "public interface ITestInterface", new List<string>
                    {
                        "public abstract Void HelloWorld()"
                    }
                };
                yield return new object[]
                {
                    "namespace AssemblyBrowserLib.Tests", "public struct TestStruct", new List<string>
                    {
                        "public virtual Void HelloWorld()"
                    }
                };
                yield return new object[]
                {
                    "namespace AssemblyBrowserLib.Tests", "public enum TestEnum", new List<string>
                    {
                        "Value1",
                        "Value2",
                        "Value3"
                    }
                };
            }
        }

        public static readonly string MainTestLibPath = "./AssemblyBrowserLib.Tests.dll";

        [Fact]
        public void Parse_ParsingAssembly_GotCorrectNamespaces()
        {
            List<string> namespacesTitles = new()
            {
                "namespace AssemblyBrowserLib.Tests",
                "namespace System",
            };
            
            var result = AssemblyParser.Parse(MainTestLibPath);
            Assert.True(namespacesTitles.TrueForAll(s => result.Children.Exists(node => node.FullSignature.Contains(s))));
        }
        
        [Fact]
        public void Parse_ParsingAssembly_GotCorrectTypes()
        {
            List<string> typesTitles = new()
            {
                "public class AssemblyParserTests",
                "public class TestClass1",
                "public static class TestClass2",
                "public interface ITestInterface",
                "public struct TestStruct",
                "public enum TestEnum"
            };

            var result = AssemblyParser.Parse(MainTestLibPath);
            
            var namespaceNode = result.Children.Find(node => node.FullSignature == "namespace AssemblyBrowserLib.Tests");

            Assert.NotNull(namespaceNode);
            Assert.True(typesTitles.TrueForAll(s => namespaceNode.Children.Exists(node => node.FullSignature.Contains(s))));
        }
        
        [Theory]
        [MemberData(nameof(TestMembers))]
        public void Parse_ParsingAssembly_GotCorrectMembers(string namespaceTitle, string typeTitle, List<string> membersTitles)
        {
            var result = AssemblyParser.Parse(MainTestLibPath);
            
            var typeNode = result.Children.Find(namespaceNode => namespaceNode.FullSignature.Equals(namespaceTitle))
                ?.Children.Find(typeNode => typeNode.FullSignature.Contains(typeTitle));
            
            Assert.NotNull(typeNode);

            foreach (var memberTitle in membersTitles)
            {
                Assert.Contains(typeNode.Children,
                    memberNode => memberNode != null && memberNode.FullSignature.Contains(memberTitle));
            }
        }
    }
}