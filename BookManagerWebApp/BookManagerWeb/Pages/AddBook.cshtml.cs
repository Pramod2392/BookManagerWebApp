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
            //AddBook addBook = new AddBook() { Title = "The Monk who sold his Ferrari", Price = 200, CategoryId = 6 };
            //using FileStream fileStream = new FileStream("./Images/the-monk-who-sold-his-ferrari-1000x1000.png", FileMode.Open);
            //addBook.Image = new FormFile(fileStream, 0, fileStream.Length, "the-monk-who-sold-his-ferrari-1000x1000", "the-monk-who-sold-his-ferrari-1000x1000") { Headers = new HeaderDictionary(), ContentType = "image/png", ContentDisposition = "form-data; name=\"Image\"; filename=\"the-monk-who-sold-his-ferrari-1000x1000.png\"" };

            //using MultipartFormDataContent content = new MultipartFormDataContent();
            //var addBookJson = JsonSerializer.Serialize(addBook);

            //await _downstreamApi.PostForUserAsync<string>("DownstreamApi", "");
            ////_downstreamApi.
            //using var response = await _downstreamApi.CallApiForUserAsync("DownstreamApi").ConfigureAwait(false);


            //if (response.StatusCode == System.Net.HttpStatusCode.OK)
            //{
            //    var apiResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            //    ViewData["ApiResult"] = apiResult;                
            //}
            //else
            //{
            //    var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            //    throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}: {error}");
            //};

            await Task.CompletedTask;
        }
    }
}
