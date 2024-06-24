using BookManagerWeb.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Identity.Abstractions;
using System.ComponentModel;
using System.Net.Http.Json;
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
            try
            {
                using MultipartFormDataContent content = new MultipartFormDataContent();
                var addBookJson = JsonSerializer.Serialize(addBook);

                var requestParameters = new Dictionary<string, string>
                {
                    { "Title", addBook.Title },
                    { "CategoryId", addBook.CategoryId.ToString() },
                    { "Price", addBook.Price.ToString() }
                };
                
                var fileStream = new MemoryStream();
                await addBook.Image.CopyToAsync(fileStream);
                var streamContent = new StreamContent(fileStream);
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(addBook.Image.ContentType); // Change content type as per your image type
                content.Add(streamContent, "Image", addBook.Image.FileName); // "image" is the key for the image data

                var response = await _downstreamApi.CallApiForUserAsync("DownstreamApiBook",
                            options =>
                            {
                                options.HttpMethod = HttpMethod.Post.ToString();
                                options.CustomizeHttpRequestMessage = new Action<HttpRequestMessage>(x =>
                                {
                                    x.Content = content;
                                    x.RequestUri = new Uri($"https://localhost:7264/api/Books?Title={addBook.Title}&CategoryId={addBook.CategoryId}&Price={addBook.Price}");
                                });                                
                            });

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
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                throw;
            }           
        }
    }
}
