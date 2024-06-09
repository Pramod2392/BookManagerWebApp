using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Web;
using System.Net;
using Microsoft.Identity.Abstractions;
using BookManagerWeb.Models;
using System.Text.Json;
using System.ComponentModel;

namespace BookManagerWeb.Pages
{
    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
    public class IndexModel : PageModel
    {
        private readonly IDownstreamApi _downstreamApi;
        private readonly ILogger<IndexModel> _logger;

        [BindProperty]
        public List<Book>? books { get; set; }

        public IndexModel(ILogger<IndexModel> logger, IDownstreamApi downstreamApi)
        {
            _logger = logger;
            _downstreamApi = downstreamApi;;
        }

        public async Task OnGetAsync()
        {
            books = await _downstreamApi.GetForUserAsync<List<Book>>("DownstreamApiBook").ConfigureAwait(false);
        }

        public void OnPost()
        {
            Response.Redirect("/AddBook");
        }

        
    }
}
