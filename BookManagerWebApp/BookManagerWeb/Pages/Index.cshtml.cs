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

        [BindProperty(SupportsGet = true)]
        public string searchText { get; set; } = string.Empty;

        public IndexModel(ILogger<IndexModel> logger, IDownstreamApi downstreamApi)
        {
            _logger = logger;
            _downstreamApi = downstreamApi;                        
        }

        public async Task OnGetAsync()
        {
            //Check if the user has logged in for the first time.
            // If yes, call user api and add the user to the table
            var claims = HttpContext.User.Claims.ToList();            

            

            var isNewUser = claims.Find(x => x.Type.ToString().Contains("newUser"));

            if (isNewUser != null && isNewUser.Value.ToLower().Contains("true"))
            {
                // Call user api to add user to db
                var user = new User();

                foreach (var claim in claims)
                {
                    switch (claim.Type.ToString().ToLower()) 
                    {
                        case "name" when claim.Type.ToLower().Contains("name"): 
                            user.FirstName = claim.Value.Split(' ')[0]; 
                            break;

                        case "givenname" when claim.Type.ToLower().Contains("givenname"):
                            user.DisplayName = claim.Value;
                            break;

                        case "emails" when claim.Type.ToLower().Contains("emails"):
                            user.UserEmail = claim.Value.ToString();
                            break;

                        case "http://schemas.microsoft.com/identity/claims/objectidentifier" when claim.Type.ToLower().Contains("objectidentifier"):
                            user.Id = claim.Value.ToString();
                            break;
                    }
                }

                if (string.IsNullOrWhiteSpace(user.DisplayName))
                {
                    user.DisplayName = user.FirstName;
                }
                
                var requestJson = JsonSerializer.Serialize(user);
                var response = await _downstreamApi.PostForUserAsync<User,User>("DownstreamApiUser", user);
            }

            //var claim = claims.Find(x => x.Type.ToString().Contains("newUser"));

            // Get list of user books
            books = await _downstreamApi.GetForUserAsync<List<Book>>("DownstreamApiBook").ConfigureAwait(false);

            // search for the books starting with or contains keyword

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var searchTextLower = searchText.ToLower();
                books = books?.Where(x => x.Title.ToLower().Contains(searchTextLower)).ToList(); 
            }
        }

        public void OnPost()
        {
            Response.Redirect("/AddBook");
        }

        
    }
}
