namespace SIC.Frontend.Models;

public class DocumentationContext
{
    public string? Title { get; set; }

    public List<BreadcrumbItem> Breadcrumbs { get; set; } = [];

    public List<TocItem> TocItems { get; set; } = [];

    public DocumentationPageLink? PreviousPage { get; set; }

    public DocumentationPageLink? NextPage { get; set; }
}

public class BreadcrumbItem
{
    public string Text { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}

public class TocItem
{
    public string Text { get; set; } = string.Empty;
    public string Anchor { get; set; } = string.Empty;
}

public class DocumentationPageLink
{
    public string Text { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}