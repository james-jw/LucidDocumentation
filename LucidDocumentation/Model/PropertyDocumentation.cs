using System.Text;

namespace LucidDocumentation
{
    internal class PropertyDocumentation : DocumentationItem
    {
        public bool IsStatic { get; set; }

        public bool HasSet { get; set; }
        public bool HasGet { get; set; }

        public string Type { get; set; }

        public override string ToMarkdown()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"#### {Name} `property`");
            builder.Append($"###### `{Name} {{ ");
            if (HasGet)
                builder.Append("get; ");

            if (HasSet)
                builder.Append("set; ");

            builder.AppendLine("}`");
            builder.AppendLine(Summary);
            return builder.ToString();
        }
    }
}
