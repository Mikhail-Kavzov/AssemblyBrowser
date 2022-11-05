using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyBrowserLib.Extensions
{
    public static class TypeExtension
    {
        /// <summary>
        ///Get string presentation of type modifiers
        /// </summary>
        /// <param name="type"></param>
        /// <returns>string presentation of type modifiers</returns>
        public static string GetTypeModifiers(this Type type)
        {
            var builder = new StringBuilder();

            if (type.IsPublic)
            {
                builder.Append("public ");
            }
            else if (type.IsNotPublic)
            {
                builder.Append("internal ");
            }

            if (type.IsClass)
            {
                if (type.IsAbstract && type.IsSealed)
                {
                    builder.Append("static ");
                }
                else if (type.IsAbstract)
                {
                    builder.Append("abstract ");
                }
                else if (type.IsSealed)
                {
                    builder.Append("sealed ");
                }

                builder.Append("class ");
            }
            else if (type.IsEnum)
            {
                builder.Append("enum ");
            }
            else if (type.IsInterface)
            {
                builder.Append("interface ");
            }
            else if (type.IsValueType && !type.IsPrimitive)
            {
                builder.Append("struct ");
            }

            return builder.ToString();
        }
    }
}
