using Confluent.Kafka;
using KafkaAPIService.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace KafkaAPIService.Controllers
{
    [ApiController]
    public class PostsController : ControllerBase
    {
        private readonly ILogger<PostsController> _logger;
        static bool init = true;
        static HttpClient client = new HttpClient();
        static ProducerConfig config = new ProducerConfig{
            BootstrapServers = "kafka:9092"
        };
        public PostsController(ILogger<PostsController> logger)
        {
            _logger = logger;

            if (init)
            {
                client.BaseAddress = new Uri("http://db-controller:80");
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                init = false;
            }
        }
        [HttpPost, Route("[controller]/TestData")]
        public async Task<IActionResult> TestData()
        {
            for(int i = 0; i < 10; i++)
            {
                var tPost = new Posts
                {
                    Id = i+1,
                    Title = $"Test{i+1}",
                    Body = string.Concat(Enumerable.Repeat("Test", i + 1)),
                    Status = 0
                };
                await AddPosts(tPost);
            }
            return Ok();
        }

        [HttpPost, Route("[controller]/AddPosts")]
        public async Task<IActionResult> AddPosts([Bind("Id, Title, Body, Status")] Posts post)
        {
            post.Updated = DateTime.Now;

            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                var result = await producer.ProduceAsync("Posts", new Message<Null, string> { Value = JsonSerializer.Serialize(post) });
            }
            return Ok(JsonSerializer.Serialize(post));
        }

        [HttpGet, Route("[controller]/GetPosts/{id?}")]
        public IActionResult GetPosts(int? id)
        {
            var response = client.GetAsync($"Dataflow/GetPosts/{id}").Result;
            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content.ReadAsStringAsync().Result);
            }
            else
            {
                return BadRequest(response);
            }
        }

        [HttpGet, Route("[controller]/GetReport")]
        public IActionResult GetReport()
        {
            var response = client.GetAsync($"Dataflow/GetReport").Result;
            if (response.IsSuccessStatusCode)
            {
                return Ok(response.Content.ReadAsStringAsync().Result);
            }
            else
            {
                return BadRequest(response);
            }
        }
    }
}
