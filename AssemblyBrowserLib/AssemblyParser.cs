using AssemblyBrowserLib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace AssemblyBrowserLib
{

    public static class AssemblyParser
    {
        public static AssemblyNode Parse(string assemblyFilePath)
        {
            var assembly = Assembly.LoadFrom(assemblyFilePath);

            var result = new AssemblyNode("Root");

            var types = assembly.GetTypes();
            var typesByNamespaces = types.ToLookup(grouping => grouping.Namespace);
            var extensionMethodsInfos = new List<ExtensionInfo>();

            foreach (var typesByNamespace in typesByNamespaces) //namespace
            {
                var namespaceNode = new AssemblyNode($"{{}} {typesByNamespace.Key}");

                foreach (var type in typesByNamespace) //type
                {
                    var typeNode = new AssemblyNode(GetTypeDeclaration(type),
                        GetTypeMembers(type, extensionMethodsInfos));
                    namespaceNode.Children.Add(typeNode);
                }

                result.Children.Add(namespaceNode);
            }
            //extensions
            AddExtensionMethods(result, extensionMethodsInfos);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="extensionMethodsInfos"></param>
        /// <returns>Representation of type members</returns>
        private static List<AssemblyNode> GetTypeMembers(Type type, List<ExtensionInfo> extensionMethodsInfos)
        {
            var members = new List<AssemblyNode>();

            //get all declared types
            var membersByType = type.GetMembers(BindingFlags.Instance | BindingFlags.Public |
                                                BindingFlags.NonPublic | BindingFlags.Static |
                                                BindingFlags.DeclaredOnly);

            foreach (var member in membersByType)
            {
                if (member is FieldInfo fieldInfo) //field
                {
                    members.Add(new AssemblyNode(GetFieldDeclaration(fieldInfo)));
                }
                else if (member is PropertyInfo propertyInfo) //property
                {
                    members.Add(new AssemblyNode(GetPropertyDeclaration(propertyInfo)));
                }
                else if (member is MethodInfo methodInfo) //method
                {
                    var methodSignature = GetMethodSignature(methodInfo); //method signature

                    //Extension methods have ExtensionAttribute in an assembly when compiling
                    if (methodInfo.IsDefined(typeof(ExtensionAttribute), false))
                    {
                        extensionMethodsInfos.Add(new ExtensionInfo(methodInfo, methodSignature));
                    }
                    else
                    {
                        members.Add(new AssemblyNode(methodSignature));
                    }
                }
            }
            return members;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootNode">root of the tree</param>
        /// <param name="extensionMethodsInfos">list of extensions</param>
        private static void AddExtensionMethods(AssemblyNode rootNode, List<ExtensionInfo> extensionMethodsInfos)
        {
            foreach (var extensionMethodInfo in extensionMethodsInfos)
            {
                // [0] parameter - this object
                var paramType = extensionMethodInfo.MethodInfo.GetParameters()[0].ParameterType;

                //child nodes of the root - namespaces
                var namespaceNode = paramType.Namespace == null ? null :
                    rootNode.Children.FirstOrDefault(node => node.FullSignature.EndsWith(paramType.Namespace));

                // if it's not global namespace
                if (namespaceNode != null)
                {
                    var typeDeclaration = GetTypeDeclaration(paramType);

                    var typeNode = namespaceNode.Children.FirstOrDefault(node => node.FullSignature == typeDeclaration);

                    if (typeNode != null)
                    {
                        typeNode.Children.Add(new AssemblyNode(extensionMethodInfo.Signature));
                    }
                    else
                    {
                        var extTypeNode = new AssemblyNode(typeDeclaration);
                        extTypeNode.Children.Add(new AssemblyNode(extensionMethodInfo.Signature));
                        namespaceNode.Children.Add(extTypeNode);
                    }
                }
                else
                {
                    var extNamespaceNode = new AssemblyNode($"{{}} {paramType.Namespace}");
                    var extTypeNode = new AssemblyNode(
                        GetTypeDeclaration(paramType));

                    extTypeNode.Children.Add(new AssemblyNode(extensionMethodInfo.Signature));
                    extNamespaceNode.Children.Add(extTypeNode);

                    rootNode.Children.Add(extNamespaceNode);
                }
            }
        }


        /// <summary>
        /// Get type declaration
        /// </summary>
        /// <param name="type"></param>
        /// <returns>full declaration of type</returns>
        private static string GetTypeDeclaration(Type type)
        {
            var builder = new StringBuilder();

            //define modifiers + type name
            builder.Append($"{type.GetTypeModifiers()}{RemoveDotNetGenericRepresentation(type.Name)}");

            var constraints = "";

            if (type.IsGenericType)
            {
                builder.Append(GetGenericArguments(type.GetGenericArguments(), out constraints));
            }

            var parents = GetTypeParents(type);

            if (parents != "") //ex SomeClass:ParentClass, ISomeInterface, ICollectionInterface<T>
            {
                builder.Append($": {parents} ");
            }

            if (constraints != "") //ex :where T: class
            {
                builder.Append($" {constraints}");
            }

            return builder.ToString();
        }

        private static string RemoveDotNetGenericRepresentation(string genericName)
        {
            int pos;
            while ((pos = genericName.IndexOf("`")) != -1)
            {
                genericName = genericName.Remove(pos, 2);
            }
            return genericName;
        }

        private static string GetTypeParents(Type type)
        {
            var parents = new List<string>();

            //value types has base classes: enum for enums, value type for values
            // null - if interface or object class
            if (type.BaseType != null && (type.BaseType is not object) && !type.IsValueType)
            {
                var parent = RemoveDotNetGenericRepresentation(type.BaseType.Name);
                //get parameters if it's generic
                if (type.BaseType.IsGenericType) //example: BaseCollection<T1,T2>
                {
                    parent += GetVariableGenericArguments(type.BaseType.GetGenericArguments());
                }

                parents.Add(parent);
            }

            var interfacesTypes = type.GetInterfaces();

            foreach (var interfaceType in interfacesTypes) //example: ISomeInterface
            {
                var parent = RemoveDotNetGenericRepresentation(interfaceType.Name);

                if (interfaceType.IsGenericType)//example: IEnumerable<T>
                {
                    parent += GetVariableGenericArguments(interfaceType.GetGenericArguments());
                }

                parents.Add(parent);
            }

            return string.Join(", ", parents);
        }

        private static string GetFieldDeclaration(FieldInfo fieldInfo)
        {
            var builder = new StringBuilder();

            //extension method for fieldInfo, gets modifiers (public, protected, private protected etc...)
            builder.Append($"{fieldInfo.GetFieldModifiers()}" +
                $"{RemoveDotNetGenericRepresentation(fieldInfo.FieldType.Name)} ");

            //if generic - add <T1,T2,...>
            if (fieldInfo.FieldType.IsGenericType)
            {
                builder.Append($"{GetVariableGenericArguments(fieldInfo.FieldType.GetGenericArguments())} ");
            }

            builder.Append($"{fieldInfo.Name}");

            return builder.ToString();
        }

        /// <summary>
        /// Gets property declaration
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <returns>Property declaration</returns>
        private static string GetPropertyDeclaration(PropertyInfo propertyInfo)
        {
            var builder = new StringBuilder();

            var getModifiers = "";
            var setModifiers = "";
            if (propertyInfo.GetMethod != null) //if property has getter
            {
                getModifiers = propertyInfo.GetMethod.GetMethodModifiers();
            }

            if (propertyInfo.SetMethod != null) // if property has setter
            {
                setModifiers = propertyInfo.SetMethod.GetMethodModifiers();
            }
            // type of property
            builder.Append($"{getModifiers}" +
                $"{RemoveDotNetGenericRepresentation(propertyInfo.PropertyType.Name)}");
            //<T>
            if (propertyInfo.PropertyType.IsGenericType)
            {
                builder.Append(GetVariableGenericArguments(propertyInfo.PropertyType.GetGenericArguments()));
            }
            // {modifier get; modifier set;}
            builder.Append($" {propertyInfo.Name} {{ ");
            if (getModifiers != "")
            {
                builder.Append($"{getModifiers}get; ");
            }

            if (setModifiers != "")
            {
                builder.Append($"{setModifiers}set; ");
            }

            builder.Append('}');

            return builder.ToString();
        }

        /// <summary>
        /// Method signature
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        private static string GetMethodSignature(MethodInfo methodInfo)
        {
            var builder = new StringBuilder();
            //modifiers + return type
            //example: public static int Name
            builder.Append($"{methodInfo.GetMethodModifiers()}" +
                $"{RemoveDotNetGenericRepresentation(methodInfo.ReturnType.Name)} ");

            //append <T,...> if is generic return type
            if (methodInfo.ReturnType.IsGenericType)
            {
                builder.Append($"{GetVariableGenericArguments(methodInfo.ReturnType.GetGenericArguments())} ");
            }
            //method name
            builder.Append($"{RemoveDotNetGenericRepresentation(methodInfo.Name)}");

            var constraints = "";
            // if method is generic - append to name <T,...>
            if (methodInfo.IsGenericMethod)
            {
                builder.Append(GetGenericArguments(methodInfo.GetGenericArguments(), out constraints));
            }
            //method parameters
            builder.Append($"({GetMethodParameters(methodInfo)})");

            //append constraints, example: where T: class
            if (constraints != "")
            {
                builder.Append(constraints);
            }

            return builder.ToString();
        }

        private static string GetGenericArguments(Type[] genericArgumentsTypes, out string constraints)
        {
            //list of constraints
            var constraintsList = new List<string>();
            // list of arguments names
            var genericArguments = new List<string>();

            foreach (var genericArgumentType in genericArgumentsTypes)
            {
                genericArguments.Add(RemoveDotNetGenericRepresentation(genericArgumentType.Name));

                if (genericArgumentType.IsGenericParameter)
                {
                    var genericParameterConstraints = GetGenericArgumentConstraints(genericArgumentType);
                    // if there are constraints
                    if (genericParameterConstraints != "")
                    {
                        constraintsList.Add(genericParameterConstraints);
                    }
                }
            }

            constraints = constraintsList.Count > 0 ? $" where {string.Join(",", constraintsList)}" : "";

            return $"<{string.Join(", ", genericArguments)}>";
        }

        private static string GetGenericArgumentConstraints(Type genericArgument)
        {
            var constraints = new List<string>();

            //constraints of parameters
            var genericParameterConstraints =
                genericArgument.GetGenericParameterConstraints();

            foreach (var typeConstraint in genericParameterConstraints)
            {
                constraints.Add(typeConstraint.Name);
            }

            //flags of constraints
            var genericParameterAttributes = genericArgument.GenericParameterAttributes;
            //mask for all constraints
            var attributes = genericParameterAttributes &
                             GenericParameterAttributes.SpecialConstraintMask;

            if (attributes != GenericParameterAttributes.None) //None- no constraints
            {
                //class restriction
                if ((attributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0)
                {
                    constraints.Add("class");
                }

                //not null restriction
                if ((attributes &
                     GenericParameterAttributes.NotNullableValueTypeConstraint) != 0)
                {
                    constraints.Add("notnull");
                }

                //default public constructor restriction
                if ((attributes &
                     GenericParameterAttributes.DefaultConstructorConstraint) != 0)
                {
                    constraints.Add("new()");
                }
            }

            return constraints.Count > 0 ? $"{genericArgument.Name}: {string.Join(", ", constraints)}" : "";
        }

        private static string GetVariableGenericArguments(Type[] genericArgumentsTypes)
        {
            var genericArguments = new List<string>();

            foreach (var genericArgumentType in genericArgumentsTypes)
            {
                StringBuilder genericArgument = new(RemoveDotNetGenericRepresentation
                    (genericArgumentType.Name));
                if (genericArgumentType.IsGenericType) //recursion if type is also generic
                {
                    genericArgument.Append(
                        GetVariableGenericArguments(genericArgumentType.GetGenericArguments()));
                }

                genericArguments.Add(genericArgument.ToString());
            }

            return $"<{string.Join(", ", genericArguments)}>";
        }

        private static string GetMethodParameters(MethodInfo methodInfo)
        {
            var parameters = new List<string>();

            var parametersInfos = methodInfo.GetParameters();
            // if is extension method - add this
            if (methodInfo.IsDefined(typeof(ExtensionAttribute), false))
            {
                if (parametersInfos[0].ParameterType.IsGenericType)
                {
                    parameters.Add($"this " +
                        $"{RemoveDotNetGenericRepresentation(parametersInfos[0].ParameterType.Name)} " +
                        $"{GetVariableGenericArguments(parametersInfos[0].ParameterType.GetGenericArguments())}");
                }
                else
                {
                    parameters.Add($"this {parametersInfos[0].ParameterType.Name} {parametersInfos[0].Name}");
                }

                for (var i = 1; i < parametersInfos.Length; i++)
                {
                    if (parametersInfos[i].ParameterType.IsGenericType)
                    {
                        parameters.Add($"" +
                            $"{RemoveDotNetGenericRepresentation(parametersInfos[i].ParameterType.Name)} " +
                            $"{GetVariableGenericArguments(parametersInfos[i].ParameterType.GetGenericArguments())}");
                    }
                    else
                    {
                        parameters.Add($"{parametersInfos[i].ParameterType.Name} {parametersInfos[i].Name}");
                    }
                }
            }
            else // if non-extension method
            {
                foreach (var parameterInfo in parametersInfos)
                {
                    if (parameterInfo.ParameterType.IsGenericType)
                    {
                        parameters.Add($"" +
                            $"{RemoveDotNetGenericRepresentation(parameterInfo.ParameterType.Name)} " +
                            $"{GetVariableGenericArguments(parameterInfo.ParameterType.GetGenericArguments())}");
                    }
                    else
                    {
                        parameters.Add($"{parameterInfo.ParameterType.Name} {parameterInfo.Name}");
                    }
                }
            }

            return parameters.Count > 0 ? string.Join(", ", parameters) : "";
        }
    }
}