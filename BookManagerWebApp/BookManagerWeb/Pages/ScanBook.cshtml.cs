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

        public async Task<IActionResult> OnPostAsync([FromBody] ScanBook request)
        {
            if (string.IsNullOrEmpty(request.ISBN))
            {
                return BadRequest(new { success = false, message = "Invalid ISBN" });
            }

            //
            bool isValid = IsValidISBN(request.ISBN);

            if (!isValid)
            {
                return BadRequest(new { success = false, message = "Invalid ISBN: Please try again." });
            }

            // Call Google Book API to get the details
            string apiKey = Convert.ToString(_configuration?["BookAPI:APIKey"]);
            string volumeUri = Convert.ToString(_configuration?["VolumeQueryUrl"]);
            string uri = $"{volumeUri}?q=isbn:{request.ISBN}";

            var response = await _httpClientGoogleManagerApiClient.GetAsync(uri);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                var deserializedObject = JsonSerializer.Deserialize<GoogleBookAPIResponseModel>(responseString, options);                

                string imageURI = deserializedObject?.Items[0]?.VolumeInfo?.ImageLinks?.Thumbnail;

                if (!string.IsNullOrWhiteSpace(imageURI))
                {
                    var imageDataResponse = await _httpClientGoogleBookImageApiClient.GetAsync(imageURI);

                    if (imageDataResponse.IsSuccessStatusCode)
                    {
                        var imageData = await imageDataResponse.Content.ReadAsStringAsync();
                    }
                }
            }

            await Task.CompletedTask;
            return new OkObjectResult(isValid);
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
    }
}
