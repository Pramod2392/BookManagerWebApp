using BookManagerWeb.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace BookManagerWeb.Pages
{
    public class ScanBookModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClientGoogleManagerApiClient;
        private readonly HttpClient _httpClientGoogleBookImageApiClient;
        private readonly ILogger<ScanBookModel> _logger;
        public ScanBookModel(ILogger<ScanBookModel> logger, IHttpClientFactory httpClientFactory,
            IConfiguration configuration)
        {

            _logger = logger;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpClientGoogleManagerApiClient = _httpClientFactory.CreateClient("GoogleBookManagerApiClient");
            _httpClientGoogleBookImageApiClient = _httpClientFactory.CreateClient("GoogleBookImageAPIClient");

        }
        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync([FromBody] AddNewBookRequest request)
        {
            if (string.IsNullOrEmpty(request.isbn))
            {
                return BadRequest(new { success = false, message = "Invalid ISBN" });
            }

            //
            bool isValid = IsValidISBN(request.isbn);

            if (!isValid)
            {
                return BadRequest(new { success = false, message = "Invalid ISBN: Please try again." });
            }

            // Navigate to Add New Book page with the model data
            return RedirectToPage("/AddBook", new { Title = request.title, ImageUrl = request.imageSource });
        }

        public bool IsValidISBN(string isbn)
        {
            // Remove any hyphens or spaces
            isbn = isbn.Replace("-", "").Replace(" ", "");

            // Determine if it is ISBN-10 or ISBN-13
            if (isbn.Length == 13)
            {
                return IsValidISBN13(isbn);
            }
            else
            {
                // Invalid if it doesn't match ISBN-10 or ISBN-13 length
                return false;
            }
        }

        public bool IsValidISBN13(string isbn)
        {
            // Remove any hyphens or spaces from the ISBN
            isbn = isbn.Replace("-", "").Replace(" ", "");

            // Check if the length is 13
            if (isbn.Length != 13 || !isbn.All(char.IsDigit))
            {
                return false;
            }

            // Perform the ISBN-13 check digit validation
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                int digit = int.Parse(isbn[i].ToString());
                sum += (i % 2 == 0) ? digit : digit * 3;
            }

            // Calculate the check digit
            int checkDigit = 10 - (sum % 10);
            if (checkDigit == 10) checkDigit = 0;

            // Compare the calculated check digit with the 13th digit
            return checkDigit == int.Parse(isbn[12].ToString());
        }


        public async Task<JsonResult> OnGetData(string isbn)
        {
            GoogleBookAPIResponseModel output = new();
            string apiKey = Convert.ToString(_configuration?["GoogleBookAPI:APIKey"]);
            //string isbn = "9789355431356";
            string uri = $"books/v1/volumes?q=isbn:{isbn}&key={apiKey}";
            var response = await _httpClientGoogleManagerApiClient.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var deserializedObject = JsonSerializer.Deserialize<GoogleBookAPIResponseModel>(responseString, options);
                output = deserializedObject;

                string ImageURI = deserializedObject?.Items[0]?.VolumeInfo?.ImageLinks?.Thumbnail;
                
                // Redirect to another page with query string parameters
                string Title = output.Items[0].VolumeInfo.Title;

                var data = new { Title = Title, ImageSource = ImageURI };
                return new JsonResult(data);                
            }
            return new JsonResult(output);
            // Redirect to another page with query string parameters
            //Response.Redirect("/Popup");
        }
    }

    public class AddNewBookRequest
    {
        public string isbn { set; get; }
        public string title { get; set; }
        public string imageSource { get; set; } = string.Empty;
    }
}
