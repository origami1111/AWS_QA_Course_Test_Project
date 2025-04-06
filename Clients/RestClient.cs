using AWS_QA_Course_Test_Project.DTOs;
using Newtonsoft.Json;
using System.Net.Http.Headers;

namespace AWS_QA_Course_Test_Project.Clients
{
    public class RestClient
    {
        private readonly HttpClient _httpClient;

        public RestClient(string publicIpAddress)
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri($"http://{publicIpAddress}/api/");
            _httpClient.DefaultRequestHeaders.Accept.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<HttpResponseMessage> GetImageAsync(string imageId)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"image/file/{imageId}");
            return response;
        }

        public async Task<List<ImageResponseDTO>> GetImagesAsync()
        {
            HttpResponseMessage response = await _httpClient.GetAsync("image");
            response.EnsureSuccessStatusCode();

            string responseData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<ImageResponseDTO>>(responseData);
        }

        public async Task<ImageResponseDTO> GetImageMetadataAsync(string imageId)
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"image/{imageId}");
            string responseData = await response.Content.ReadAsStringAsync();

            ImageResponseDTO imageResponse;
            try
            {
                imageResponse = JsonConvert.DeserializeObject<ImageResponseDTO>(responseData);
            }
            catch (Exception)
            {
                imageResponse = null;
            }

            return imageResponse;
        }

        public async Task<PostImageResponseDTO> PostImageAsync(string filePath)
        {
            using (var content = new MultipartFormDataContent())
            {
                var fileContent = new ByteArrayContent(await System.IO.File.ReadAllBytesAsync(filePath));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("image/jpeg");
                content.Add(fileContent, "upfile", "image.jpg");

                HttpResponseMessage response = await _httpClient.PostAsync("image", content);
                response.EnsureSuccessStatusCode();

                string responseData = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<PostImageResponseDTO>(responseData);
            }
        }

        public async Task DeleteImageAsync(string imageId)
        {
            HttpResponseMessage response = await _httpClient.DeleteAsync($"image/{imageId}");
            response.EnsureSuccessStatusCode();
        }

        public async Task<HttpResponseMessage> PostNotificationAsync(string email)
        {
            return await _httpClient.PostAsync($"notification/{email}", null);
        }

        public async Task<List<SubscriptionResponseDTO>> GetNotificationsAsync()
        {
            HttpResponseMessage response = await _httpClient.GetAsync("notification");
            response.EnsureSuccessStatusCode();

            string responseData = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<SubscriptionResponseDTO>>(responseData);
        }

        public async Task<HttpResponseMessage> DeleteNotificationAsync(string email)
        {
            return await _httpClient.DeleteAsync($"notification/{email}");
        }
    }
}
