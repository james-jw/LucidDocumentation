using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LucidDocumentation
{
    internal class ClassDocumentation : DocumentationItem
    {
        public List<DocumentationItem> Items { get; set; } = new List<DocumentationItem>();
        public string Namespace { get; set; }
        public string[] Interfaces { get; internal set; }

        public override string ToMarkdown()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"## {Name} `class`")
               .AppendLine($"##### Namespace: `{Namespace}`");

            if (Interfaces.Any())
                builder.AppendLine($"##### Interfaces: {string.Join(",", Interfaces.Select(i => $"`{i}`"))}");

            builder.AppendLine(Summary)
               .AppendLine("---");

            var properties = Items.OfType<PropertyDocumentation>();
            if(properties.Any())
            {
                builder.AppendLine("### Properties")
                    .AppendLine();

                foreach(var property in properties)
                    builder.AppendLine(property.ToMarkdown());
            }

            var methods = Items.OfType<MethodDocumentation>();
            if(methods.Any())
            {
                builder.AppendLine();
                foreach(var method in methods)
                    builder.AppendLine(method.ToMarkdown());
            }

            return builder.ToString();
        }
    }
}
