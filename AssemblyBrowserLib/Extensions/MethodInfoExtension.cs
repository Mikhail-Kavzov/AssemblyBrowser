using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyBrowserLib.Extensions
{
    public static class MethodInfoExtension
    {
        /// <summary>
        /// Gets string presentation of method modifiers
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns>string presentation of method modifiers</returns>
        public static string GetMethodModifiers(this MethodInfo methodInfo)
        {
            var builder = new StringBuilder();

            if (methodInfo.IsFamily)
            {
                builder.Append("protected ");
            }
            else if (methodInfo.IsAssembly)
            {
                builder.Append("internal ");
            }
            else if (methodInfo.IsFamilyOrAssembly)
            {
                builder.Append("protected internal ");
            }
            else if (methodInfo.IsFamilyAndAssembly)
            {
                builder.Append("private protected ");
            }
            else if (methodInfo.IsPrivate)
            {
                builder.Append("private ");
            }
            else if (methodInfo.IsPublic)
            {
                builder.Append("public ");
            }

            if (methodInfo.IsStatic)
            {
                builder.Append("static ");
            }

            if (methodInfo.IsAbstract)
            {
                builder.Append("abstract ");
            }
            else if (methodInfo.IsVirtual)
            {
                builder.Append("virtual ");
            }

            return builder.ToString();
        }
    }
}
