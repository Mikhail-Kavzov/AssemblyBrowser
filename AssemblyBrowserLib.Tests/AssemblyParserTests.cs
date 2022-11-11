using System.Collections.Generic;
using Xunit;

namespace AssemblyBrowserLib.Tests
{
    public class AssemblyParserTests
    {
        
        public static IEnumerable<object[]> TestMembers
        {
            get //namespace, class, members
            {
                yield return new object[]
                {
                    "{} AssemblyBrowserLib.Tests", "public class TestClass1", new List<string>
                    {
                        "protected internal Int32 ReturnInt(Boolean param1, Byte param2, Char param3)",
                        "private T ReturnAny<T>()"
                    }
                };
                yield return new object[]
                {
                    "{} System", "String", new List<string>
                    {
                        "public static Void Method1(this String str)",
                        "public static Void Method2(this String str, Boolean someValue)",
                        "public static String ReturnMethod(this String str)"
                    }
                };
                yield return new object[]
                {
                    "{} AssemblyBrowserLib.Tests", "internal class TestClass3", new List<string>
                    {
                        "public T Prop1 { public get; public set; }",
                        "public Int32 Prop2 { public get; private set; }",
                        "private Single Prop3 { private get; }",
                        "protected T ReturnT()"
                    }
                };
                yield return new object[]
                {
                    "{} AssemblyBrowserLib.Tests", "public interface ITestInterface", new List<string>
                    {
                        "public abstract Void HelloWorld()"
                    }
                };
                yield return new object[]
                {
                    "{} AssemblyBrowserLib.Tests", "public struct TestStruct", new List<string>
                    {
                        "public virtual Void HelloWorld()"
                    }
                };
                yield return new object[]
                {
                    "{} AssemblyBrowserLib.Tests", "public enum TestEnum", new List<string>
                    {
                        "Num1",
                        "Num2",
                        "Num3"
                    }
                };
            }
        }

        private static readonly string TestAssembly = "./AssemblyBrowserLib.Tests.dll";

        [Fact]
        public void CheckNamespaces()
        {
            List<string> namespacesTitles = new() //namespace definition
            {
                "{} AssemblyBrowserLib.Tests",
                "{} System",
            };
            
            var result = AssemblyParser.Parse(TestAssembly);
            Assert.True(namespacesTitles.TrueForAll(s => result.Children.Exists(node => node.FullSignature.Contains(s))));
        }
        
        [Fact]
        public void CheckTypeDefinition()
        {
            List<string> typesTitles = new()  //classes in TestClasses.cs
            {
                "public class AssemblyParserTests",
                "public class TestClass1",
                "public static class TestClass2",
                "public interface ITestInterface",
                "public struct TestStruct: ITestInterface",
                "public enum TestEnum",
                "internal class TestClass3<T>  where T: class, new()",
            };

            var result = AssemblyParser.Parse(TestAssembly);
            
            var namespaceNode = result.Children.Find(node => node.FullSignature == "{} AssemblyBrowserLib.Tests");

            Assert.NotNull(namespaceNode); //namespace !=null
            Assert.True(typesTitles.TrueForAll(s => namespaceNode.Children.Exists(node => node.FullSignature.Contains(s))));
        }
        
        [Theory]
        [MemberData(nameof(TestMembers))]
        public void CheckMembers(string namespaceName, string typeName, List<string> membersName)
        {
            var result = AssemblyParser.Parse(TestAssembly);
            var typeNode = result.Children.Find(namespaceNode => namespaceNode.FullSignature.Equals(namespaceName))
                ?.Children.Find(typeNode => typeNode.FullSignature.Contains(typeName));
            
            Assert.NotNull(typeNode);

            foreach (var memberName in membersName)
            {
                Assert.Contains(typeNode.Children,
                    memberNode => memberNode != null && memberNode.FullSignature.Contains(memberName));
            }
        }
    }
}