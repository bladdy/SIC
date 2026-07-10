namespace SIC.Frontend.Services;

public class PageMetaService
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string Keywords { get; set; } = "";
    public string CanonicalUrl { get; set; } = "";
    public string OgImage { get; set; } = "";
    public string OgType { get; set; } = "website";
    public string SiteName { get; set; } = "SIC";
    public string TwitterCard { get; set; } = "summary_large_image";

    public event Action? OnChange;

    public void Set(
        string? title = null,
        string? description = null,
        string? image = null,
        string? keywords = null,
        string? canonicalUrl = null,
        string? ogType = null)
    {
        if (title != null) Title = title;
        if (description != null) Description = description;
        if (image != null) OgImage = image;
        if (keywords != null) Keywords = keywords;
        if (canonicalUrl != null) CanonicalUrl = canonicalUrl;
        if (ogType != null) OgType = ogType;

        OnChange?.Invoke();
    }
}