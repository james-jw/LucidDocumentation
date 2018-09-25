using LucidDocumentation.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml.Linq;

namespace LucidDocumentation
{
    /// <summary>
    /// Core static class for building documentation. 
    /// </summary>
    public class DocumentationBuilder
    {
        private string _packageResolutionPath;

        /// <summary>
        /// Creates a DocumentationBuilder object for use in generating documentation for any number of assemblies.
        ///
        /// <code>
        ///   var docBuilder = new DocumentationBuilder("c:\packages\"); 
        /// </code>
        /// </summary>
        /// <param name="packageResolutionPath">The folder path to resolve any dependent nuget packages from.</param>
        public DocumentationBuilder(string packageResolutionPath)
        {
            _packageResolutionPath = packageResolutionPath;
        }

        /// <summary>
        /// Returns documentation in standard Markdown format for the assembly at the `assemblyPath` provided. The referenced assemblie's 
        /// generated documentation `.xml` file must be present in the same folder.
        /// 
        /// ###### Usage:
        /// <code>
        ///    var markdown = docBuilder.BuildDocumentation("c:\SampleAssembly.dll"); 
        /// </code>
        /// </summary>
        /// <param name="assemblyPath">The absolute path of the assembly to document.</param>
        /// <returns>Markdown formated documentation</returns>
        public string BuildDocumentation(string assemblyPath)
        {
            List<ClassDocumentation> classDocuments = new List<ClassDocumentation>();

            try
            {
                var assembly = Assembly.LoadFile(assemblyPath);
                var xml = XDocument.Load(assemblyPath.Replace(".dll", ".xml"));

                var memberInfos = xml.Descendants("member")
                    .Select(m => new MemberInfo(m, typeName => FindType(assembly, typeName)))
                    .ToArray();

                var builder = new StringBuilder();
                builder.AppendLine($"# {assembly.GetName().Name} `assembly`");

                GenerateLinkTree(memberInfos, builder);

                foreach(var member in memberInfos.Where(m => m.Type == MemberTypes.Class))
                {
                    var relatedType = FindType(assembly, member.FullName);
                    if (relatedType == null)
                        throw new Exception($"Unable to find type {member.FullName}.");

                    var doc = new ClassDocumentation() {
                        Name = member.Name,
                        Namespace = relatedType.Namespace,
                        Summary = ConvertSummary(member.Summary),
                        Interfaces = relatedType.GetInterfaces().Select(i => i.Name).ToArray()
                    };

                    foreach(var subItem in memberInfos.Where(m => m.ParentName == member.FullName))
                    {
                        DocumentationItem newItem = null;
                        if (subItem.Type == MemberTypes.Property)
                            newItem = new PropertyDocumentation();
                        else if (subItem.Type == MemberTypes.Function)
                        {
                            MethodDetails relatedMethod = subItem.GetMethod(relatedType); 
                            newItem = new MethodDocumentation()
                            {
                                Signature = relatedMethod.GetSignature(),
                                ReturnType = relatedMethod.ReturnType,
                                ReturnSummary = subItem.Returns,
                                Arguments = relatedMethod.Arguments.Select(a =>
                                {
                                    return new ArgumentDocumentation()
                                    {
                                        Name = a.Name,
                                        Type = a.ParameterType.Name,
                                        Required = !a.HasDefaultValue,
                                        DefaultValue = a.DefaultValue,
                                        Summary = subItem.Parameters.FirstOrDefault(p => p.Attribute("name").Value == a.Name)?.Value ?? "",
                                    };
                                }).ToList()
                            };
                        }

                        if(newItem != null)
                        {
                            newItem.Name = subItem.Name;
                            newItem.Summary = ConvertSummary(subItem.Summary);
                        }

                        doc.Items.Add(newItem);
                    }

                    builder.AppendLine(doc.ToMarkdown());
                }

                return builder.ToString();
            } catch (Exception e)
            {
                throw new Exception($"Unable to build documentaiton for '{assemblyPath}'. {e.Message}", e);
            }
        }

        private Type FindType(Assembly assemblyIn, string fullName)
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                var assemblyDirectory = assemblyIn.GetAssemblyDirectory();
                foreach (var file in Directory.EnumerateFiles(assemblyDirectory))
                {
                    if (file.EndsWith(".dll"))
                    {
                        assemblyIn = Assembly.LoadFile(file);
                        var assemblies = assemblyIn.GetReferencedAssemblies()
                            .Select(a => Assembly.Load(a.FullName));

                        foreach (var assembly in assemblies.Concat(new[] { assemblyIn }))
                        {
                            foreach (var type in assembly.GetTypes())
                            {
                                if (type.FullName == fullName)
                                    return type;
                            }
                        }
                    }

                }
            }
            catch (ReflectionTypeLoadException e)
            {
                Debug.WriteLine(e.Message);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= this.CurrentDomain_AssemblyResolve;
            }

            return null;
        }

        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var fileName = args.Name.Split(',').First();
            var filePath = FindFile(_packageResolutionPath, $"{fileName}.dll");

            return Assembly.LoadFile(filePath);
        }

        private static string FindFile(string searchDirectory, string searchPattern)
        {
            foreach(var dir in Directory.GetDirectories(searchDirectory, "*", SearchOption.AllDirectories))
            {
                var file = Directory.GetFiles(dir, searchPattern).FirstOrDefault();
                if(file != null)
                    return file;
            }

            return null;
        }

        private static void GenerateLinkTree(MemberInfo[] memberInfos, StringBuilder builder)
        {
            var namespaceGroups = from m in memberInfos
                        where m.Type == MemberTypes.Class
                        group m by m.ParentName into g
                        select g;

            foreach(var group in namespaceGroups)
            {
                builder.AppendLine($"- [{group.Key}](#{group.Key})");
                foreach (var clazz in group)
                {
                    builder.AppendLine($"   - [{clazz.Name}](#{clazz.Name})");
                }
            }

            builder.AppendLine();
        }

        private static string ConvertSummary(XElement summary)
        {
            var builder = new StringBuilder();
            foreach(var node in summary?.Nodes())
            {
                if (node is XText text)
                {
                    foreach (var line in text.Value.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.None))
                    {
                        var finalLine = line.Trim();
                        if(finalLine != string.Empty)
                            builder.AppendLine(finalLine);
                    }
                }
                else if (node is XElement element)
                {
                    if (element.Name == "code")
                    {
                        builder.AppendLine();
                        builder.AppendLine("```c#");
                        builder.AppendLine(element.Value.Trim());
                        builder.AppendLine("```");
                    }
                }
            }
            
            return builder.ToString();
        }
    }
}
