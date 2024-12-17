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
    [AuthorizeForScopes(ScopeKeySection = "DownstreamApiBook:Scopes")]
    public class IndexModel : PageModel
    {
        private readonly IDownstreamApi _downstreamApi;
        private readonly IConfiguration _configuration;
        private readonly ILogger<IndexModel> _logger;

        [BindProperty]
        public PagedBook? pagedBooks { get; set; }

        [BindProperty(SupportsGet = true)]
        public string searchText { get; set; } = string.Empty;

        public int PageNumber = 1;
        public int PageSize = 3;
        public int TotalNumberOfBooks { get; set; }
        public bool HasPreviousPage;
        public bool HasNextPage;        

        public IndexModel(ILogger<IndexModel> logger, IDownstreamApi downstreamApi, IConfiguration configuration)
        {
            _logger = logger;
            _downstreamApi = downstreamApi;
            this._configuration = configuration;
        }

        public async Task OnGetAsync(int? pageNumber)
        {
            //Check if the user has logged in for the first time.
            // If yes, call user api and add the user to the table

            _logger.LogInformation("Inside OnGetAsync() method");

            int pageNo = Convert.ToInt32(pageNumber);

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
            if (pageNo >= 1)
            {
                this.PageNumber = pageNo;
            }
            else
            {
                this.PageNumber = 1;
            }
            
            var baseUrl = _configuration["DownstreamApiBook:BaseUrl"]?.ToString();
            // Get list of user books
            pagedBooks = await _downstreamApi.GetForUserAsync<PagedBook>("DownstreamApiBook",
                                    options =>
                                    {
                                        options.CustomizeHttpRequestMessage = new Action<HttpRequestMessage>(x =>
                                        {
                                            x.RequestUri = new Uri($"{baseUrl}?PageSize={PageSize}&PageNumber={this.PageNumber}&searchText={searchText}");
                                        });
                                    });

            HasNextPage = (int)Math.Ceiling((double)pagedBooks?.TotalCount / PageSize) > PageNumber ?  true : false;
            HasPreviousPage = PageNumber > 1 ? true : false;
        }

        public void OnPost()
        {
            Response.Redirect("/ScanBook");
        }

        public async Task OnGetSearch()
        {
            this.PageNumber = 1;
            var baseUrl = _configuration["DownstreamApiBook:BaseUrl"]?.ToString();
            // Get list of user books
            pagedBooks = await _downstreamApi.GetForUserAsync<PagedBook>("DownstreamApiBook",
                                    options =>
                                    {
                                        options.CustomizeHttpRequestMessage = new Action<HttpRequestMessage>(x =>
                                        {
                                            x.RequestUri = new Uri($"{baseUrl}?PageSize={PageSize}&PageNumber={this.PageNumber}&searchText={searchText}");
                                        });
                                    });

            HasNextPage = (int)Math.Ceiling((double)pagedBooks?.TotalCount / PageSize) > PageNumber ? true : false;
            HasPreviousPage = PageNumber > 1 ? true : false;
        }
    }
}
