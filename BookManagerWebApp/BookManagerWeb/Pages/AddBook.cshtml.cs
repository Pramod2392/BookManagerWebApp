using BookManagerWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Identity.Abstractions;
using System.ComponentModel;
using System.Net.Http.Json;
using System.Text.Json;
using static System.Net.Mime.MediaTypeNames;

namespace BookManagerWeb.Pages
{
    public class AddBookModel : PageModel
    {
        private readonly IDownstreamApi _downstreamApi;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AddBookModel> _logger;
        public AddBookModel(ILogger<AddBookModel> logger, IDownstreamApi downstreamApi, IConfiguration configuration)
        {
            _downstreamApi = downstreamApi;
            _configuration = configuration;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public AddBook addBook { get; set; }

        public SelectList CategoryItems { get; set; }

        public SelectList LanguageItems { get; set; }

        public string ImageURL {  get; set; }

        [BindProperty()]
        public int CategorySelectedItemId { get; set; } = 0;

        [BindProperty()]
        public int LanguageSelectedItemId { get; set; } = 0;
        public async Task OnGetAsync(string Title, string ImageUrl)
        {
            var categoryList = await _downstreamApi.GetForUserAsync<List<Category>>("DownstreamApiBook",
                opts =>
                {
                    //opts.BaseUrl = "https://localhost:7264/api/Books";
                    opts.RelativePath = "/GetCategories";
                });
            
            CategoryItems = new SelectList(categoryList, "Id", "Name");

            var languageList = await _downstreamApi.GetForUserAsync<List<Language>>("DownstreamApiBook",
                opts =>
                {
                    //opts.BaseUrl = "https://localhost:7264/api/Books";
                    opts.RelativePath = "/GetLanguages";
                });

            LanguageItems = new SelectList(languageList, "Id", "Name");

            addBook.Title = Title;
            ImageURL = ImageUrl;
        }


        public async Task OnPostAsync()
        {
            try
            {
                addBook.CategoryId = CategorySelectedItemId;
                addBook.LanguageId = LanguageSelectedItemId;
                using MultipartFormDataContent content = new MultipartFormDataContent();
                var addBookJson = JsonSerializer.Serialize(addBook);

                var requestParameters = new Dictionary<string, string>
                {
                    { "Title", addBook.Title },
                    { "CategoryId", addBook.CategoryId.ToString() },
                    { "Price", addBook.Price.ToString() },
                    { "LanguageId", addBook.LanguageId.ToString() }
                };

                using var fileStream = new MemoryStream();

                if (!string.IsNullOrWhiteSpace(ImageURL))
                {
                    using (var client = new HttpClient())
                    {
                        var imageBytes = client.GetByteArrayAsync(ImageURL).Result;

                        var stream = new MemoryStream(imageBytes);
                        addBook.Image = new FormFile(stream, 0, imageBytes.Length, "ImageFile", "filename.jpg");
                    }
                }

                await addBook.Image.CopyToAsync(fileStream);
                fileStream.Position = 0;
                var streamContent = new StreamContent(fileStream, Convert.ToInt32(addBook.Image.Length));

                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(addBook.Image.ContentType); // Change content type as per your image type
                content.Add(streamContent, "Image", addBook.Image.FileName); // "image" is the key for the image data
                var baseUrl = _configuration["DownstreamApiBook:BaseUrl"]?.ToString();

                var response = await _downstreamApi.CallApiForUserAsync("DownstreamApiBook",
                            options =>
                            {
                                options.HttpMethod = HttpMethod.Post.ToString();
                                options.CustomizeHttpRequestMessage = new Action<HttpRequestMessage>(x =>
                                {
                                    x.Content = content;
                                    x.RequestUri = new Uri($"{baseUrl}?Title={addBook.Title}&CategoryId={addBook.CategoryId}&Price={addBook.Price}&LanguageId={addBook.LanguageId}");
                                });                                
                            });

                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var apiResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    // Display success message to user and navigate back to homw screen
                  
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    throw new HttpRequestException($"Invalid status code in the HttpResponseMessage: {response.StatusCode}: {error}");
                };

                Response.Redirect("/Index");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error: {ex.Message}");
                throw;
            }           
        }
    }
}
