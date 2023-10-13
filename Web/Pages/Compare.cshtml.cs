using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Web.Pages
{
    public class CompareModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string? Url { get; set; }
        public void OnGet()
        {
        }
    }
}
