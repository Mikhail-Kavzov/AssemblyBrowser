using System;

namespace AssemblyBrowserLib.Tests
{
    public class TestClass1
    {
        protected internal int ReturnInt(bool param1, byte param2, char param3) => int.MaxValue;

        private T ReturnAny<T>() where T: new() => new();
    }

    public static class TestClass2
    {
        public static void Method1(this string str) => Console.WriteLine(str);

        public static void Method2(this string str, bool someValue) => Console.WriteLine(someValue.ToString(), str);

        public static string ReturnMethod(this string str) => str;
    }

    internal class TestClass3<T> where T : class, new()
    {
        public T Prop1 { get; set; }
        public int Prop2 { get; private set; }
        private float Prop3 { get; }

        protected T ReturnT() => new T();
    }

    public interface ITestInterface
    {
        public void HelloWorld();
    }

    public struct TestStruct: ITestInterface
    {
        public void HelloWorld() { }
    }

    public enum TestEnum
    {
        Num1,
        Num2,
        Num3
    }
}