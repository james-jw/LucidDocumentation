using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace LucidDocumentation.Extensions
{
    public static class AssemblyExtensions
    {
        public static string GetAssemblyDirectory(this Assembly assemblyIn)
        {
            string codeBase = assemblyIn.CodeBase;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
    }
}
