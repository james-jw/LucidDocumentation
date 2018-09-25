using LucidDocumentation.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace LucidDocumentation
{
    internal class MemberInfo
    {
        private XElement _node;

        public string Type { get; private set; }

        public string Name { get; private set; }

        public string ParentName { get; private set; } = null;

        public XElement Summary { get => _node.Descendants("summary").FirstOrDefault(); }

        public IEnumerable<XElement> Parameters { get => _node.Descendants("param"); }
        public string FullName { get; private set; }
        public string Returns { get; private set; }
        public Type[] Arguments { get; private set; }

        public MemberInfo(XElement info, Func<string, Type> typeResolver = null)
        {
            _node = info;

            var name = info.Attribute("name").Value;
            var parts = name.Split(new char[] { ':', ')', '(' }, StringSplitOptions.RemoveEmptyEntries);
            switch(parts[0])
            {
                case "T":
                    Type = MemberTypes.Class;
                    Name = parts[1].Split('.').Last();
                    break;
                case "M":
                    Type = MemberTypes.Function;
                    Name = parts[1].Split('.').Last();
                    Returns = info.Descendants("returns").FirstOrDefault()?.Value;
                    Arguments = parts.Last().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(t =>
                        {
                            var typeOut = System.Type.GetType(t);
                            if (typeOut == null && typeResolver != null)
                                typeOut = typeResolver(t);
                            return typeOut;
                        }).ToArray();
                    break;
                case "P":
                    Type = MemberTypes.Property;
                    Name = parts[1].Split('.').Last();
                    break;
            }

            ParentName = parts[1].Replace($".{Name}", "");
            FullName = parts[1];
        }

        public MethodDetails GetMethod(Type relatedType)
        {
            if(Name != "#ctor")
            {
                var method = relatedType.GetMethod(Name, Arguments);
                return new MethodDetails(method);
            } else
            {
                var constructor = relatedType.GetConstructor(Arguments);
                return new MethodDetails(constructor, relatedType);
            }
        }
    }

    public class MethodDetails
    {
        MethodInfo _method;

        public MethodDetails(ConstructorInfo constructor, Type relatedType)
        {
            Arguments = constructor.GetParameters();
            IsGenericMethod = constructor.IsGenericMethod;
            GenericArguments = new Type[0];
            IsFamily = constructor.IsFamily;
            IsPublic = constructor.IsPublic;
            IsPrivate = constructor.IsPrivate;
            IsStatic = constructor.IsStatic;
            Name = relatedType.Name;
        }

        public MethodDetails(MethodInfo method)
        {
            ReturnType = method.ReturnType?.Name ?? "void";
            Arguments = method.GetParameters();
            IsGenericMethod = method.IsGenericMethod;
            GenericArguments = method.GetGenericArguments();
            IsFamily = method.IsFamily;
            IsAssembly = method.IsAssembly;
            IsPrivate = method.IsPrivate;
            IsPublic = method.IsPublic;
            IsStatic = method.IsStatic;
            Name = method.Name;

            _method = method;
        }

        public string ReturnType { get; set; }
        public ParameterInfo[] Arguments { get; set; }
        public bool IsGenericMethod { get; internal set; }
        public Type[] GenericArguments { get; internal set; }
        public string Name { get; internal set; }
        public bool IsStatic { get; internal set; }
        public bool IsFamily { get; internal set; }
        public bool IsAssembly { get; internal set; }
        public bool IsPrivate { get; internal set; }
        public bool IsPublic { get; internal set; }

        public bool IsDefined(Type type, bool inherit)
        {
            return _method?.IsDefined(type, inherit) == true;
        }
    }
}
