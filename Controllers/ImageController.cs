using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace HAN.Demo.Controllers
{
    public class ImageController : Controller
    {
        IHttpClientFactory _httpClientFactory;

        public ImageController(IHttpClientFactory clientFactory)
        {
            _httpClientFactory = clientFactory;
        }

        public async Task<IActionResult> Index()
        {
            HttpClient client = _httpClientFactory.CreateClient();

            var response = await client.GetAsync("https://filestore.community.support.microsoft.com/api/images/8824176a-5025-4969-90a5-e4f339ceb3bc");
            
            byte[] image = await response.Content.ReadAsByteArrayAsync();

            return new FileContentResult(image, response.Content.Headers.ContentType.MediaType);
        }
    }
}