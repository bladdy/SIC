using Microsoft.AspNetCore.Components;

namespace SIC.Frontend.Shared
{
    public partial class Pagination
    {
        private List<PageModel> links = null!;

        [Parameter] public int CurrentPage { get; set; } = 1;
        [Parameter] public int TotalPages { get; set; }
        [Parameter] public int Radio { get; set; } = 10;
        [Parameter] public EventCallback<int> SelectedPage { get; set; }
        [Parameter] public bool IsHome { get; set; } = false;

        protected override void OnParametersSet()
        {
            links = [];
            links.Add(new PageModel
            {
                Page = CurrentPage - 1,
                Enable = CurrentPage != 1,
                Text = "Previous"
            });

            for (int i = 1; i <= TotalPages; i++)
            {
                if (TotalPages <= Radio)
                {
                    links.Add(new PageModel
                    {
                        Text = $"{i}",
                        Page = i,
                        Enable = i == CurrentPage,
                    });
                }

                if (TotalPages > Radio && i <= Radio && CurrentPage <= Radio)
                    {
                    links.Add(new PageModel
                    {
                        Text = $"{i}",
                        Page = i,
                        Enable = i == CurrentPage,
                    });
                }
                if (CurrentPage > Radio && i > CurrentPage - Radio && i <= CurrentPage)
                {
                    links.Add(new PageModel
                    {
                        Text = $"{i}",
                        Page = i,
                        Enable = i == CurrentPage,
                    });
                }
            }

            links.Add(new PageModel
            {
                Page = CurrentPage != TotalPages ? CurrentPage + 1 : CurrentPage,
                Enable = CurrentPage != TotalPages,
                Text = "Next"
            });
        }

        private async Task InternalSelectedPage(PageModel pageModel)
        {
            if (pageModel.Page == CurrentPage || pageModel.Page == 0)
            {
                return;
            }

            await SelectedPage.InvokeAsync(pageModel.Page);
        }

        private class OptionModel
        {
            public string Name { get; set; } = null!;
            public int Value { get; set; }
        }

        private class PageModel
        {
            public bool Enable { get; set; } = true;
            public int Page { get; set; }
            public string Text { get; set; } = null!;
        }
    }
}