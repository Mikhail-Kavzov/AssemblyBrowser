using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace AssemblyBrowserLib
{
    public class ExtensionInfo
    {
        public ExtensionInfo(MethodInfo methodInfo, string signature)
        {
            Signature = signature;
            MethodInfo = methodInfo;
        }

        public MethodInfo MethodInfo { get; set; }
        public string Signature { get; set; }
    }
}