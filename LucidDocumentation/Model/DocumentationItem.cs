namespace LucidDocumentation
{
    internal abstract class DocumentationItem
    {
        public string Name { get; set; }
        public string Summary { get; set; }
        public abstract string ToMarkdown();
    }
}
