using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LucidDocumentation
{
    internal class MethodDocumentation : DocumentationItem
    {
        public bool IsStatic { get; set; }
        public string ReturnType { get; set; }
        public string ReturnSummary { get; set; }

        public string Signature { get; set; }

        public List<ArgumentDocumentation> Arguments { get; set; } = new List<ArgumentDocumentation>();

        public override string ToMarkdown()
        {
            var builder = new StringBuilder();
            builder.AppendLine($"#### {Name} `method`")
                .AppendLine("```c#")
                .AppendLine($"{Signature}")
                .AppendLine("```");

            if(Arguments.Any())
            {
                builder.AppendLine("| Parameter | Type        | Default | Summary         |")
                       .AppendLine("|: --- :|: --------- :|: -------|: -------------- |");

                foreach (var argument in Arguments)
                    builder.AppendLine(argument.ToMarkdown());
            }

            if(ReturnType != null && ReturnType != "void")
            {
                builder.AppendLine($"###### Returns: `{ReturnType}`");
                if(ReturnSummary != null)
                    builder.AppendLine(ReturnSummary);
            }

            if (Summary != null) {
                builder.AppendLine("###### Detail:");
                builder.AppendLine(Summary);
            }

            builder.AppendLine("---");

            return builder.ToString();
        }
    }
}
