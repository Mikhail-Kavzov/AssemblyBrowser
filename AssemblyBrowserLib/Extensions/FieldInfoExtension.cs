using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyBrowserLib.Extensions
{
    public static class FieldInfoExtension
    {
        /// <summary>
        /// Gets string presentation of field modifiers
        /// </summary>
        /// <param name="fieldInfo"></param>
        /// <returns>string presentation of field modifiers</returns>
        public static string GetFieldModifiers(this FieldInfo fieldInfo)
        {
            var builder = new StringBuilder();

            if (fieldInfo.IsFamily)
            {
                builder.Append("protected ");
            }
            else if (fieldInfo.IsAssembly)
            {
                builder.Append("internal ");
            }
            else if (fieldInfo.IsFamilyOrAssembly)
            {
                builder.Append("protected internal ");
            }
            else if (fieldInfo.IsFamilyAndAssembly)
            {
                builder.Append("private protected ");
            }
            else if (fieldInfo.IsPrivate)
            {
                builder.Append("private ");
            }
            else if (fieldInfo.IsPublic)
            {
                builder.Append("public ");
            }

            if (fieldInfo.IsStatic)
            {
                builder.Append("static ");
            }
            if (fieldInfo.IsLiteral)
            {
                builder.Append("const ");
            }
            else if (fieldInfo.IsInitOnly)
            {
                builder.Append("readonly ");
            }

            return builder.ToString();
        }
    }
}
