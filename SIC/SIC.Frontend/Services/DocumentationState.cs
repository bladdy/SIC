using SIC.Frontend.Models;

namespace SIC.Frontend.Services;

public class DocumentationState
{
    public DocumentationContext Context { get; private set; } = new();

    public event Action? OnChange;

    public void Set(DocumentationContext context)
    {
        Context = context;
        OnChange?.Invoke();
    }
}