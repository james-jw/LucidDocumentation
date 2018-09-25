namespace LucidDocumentation
{
    internal class ArgumentDocumentation : DocumentationItem
    {
        public string Type { get; set; }
        public bool Required { get; set; }
        public object DefaultValue { get; set; }

        public override string ToMarkdown()
        {
            return $"| {Name} | `{Type}` | {DefaultValue ?? ""} | {Summary} |";
        }
    }
}
