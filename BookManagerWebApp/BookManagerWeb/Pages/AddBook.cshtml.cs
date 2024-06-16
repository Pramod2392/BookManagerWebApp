using BookManagerWeb.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using System.ComponentModel;
using System.Text.Json;

namespace BookManagerWeb.Pages
{
    public class AddBookModel : PageModel
    {
        private readonly IDownstreamApi _downstreamApi;
        private readonly ILogger<AddBookModel> _logger;
        public AddBookModel(ILogger<AddBookModel> logger, IDownstreamApi downstreamApi)
        {
            _downstreamApi = downstreamApi;
            _logger = logger;
        }

        [BindProperty()]
        public AddBook addBook { get; set; }
        public async Task OnGetAsync()
        {
            await Task.CompletedTask;
        }


        public async Task OnPostAsync()
        {            
            using MultipartFormDataContent content = new MultipartFormDataContent();
            var addBookJson = JsonSerializer.Serialize(addBook);

            await _downstreamApi.PostForUserAsync<string>("DownstreamApiBook", addBookJson);
            //_downstreamApi.
            using var response = await _downstreamApi.CallApiForUserAsync("DownstreamApiBook").ConfigureAwait(false);


            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var apiResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                ViewData["ApiResult"] = apiResult;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}: {error}");
            };            
        }
    }
}
