using Confluent.Kafka;
using KafkaAPIService.Models;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace KafkaAPIService.Controllers
{
    [ApiController]
    public class CommentsController : ControllerBase
    {

        private readonly ILogger<CommentsController> _logger;
        static bool init = true;
        static HttpClient client = new HttpClient();
        static ProducerConfig config = new ProducerConfig
        {
            BootstrapServers = "kafka:9092"
        };

        public CommentsController(ILogger<CommentsController> logger)
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
            int c = 1;
            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < i+1; j++)
                {
                    var tPost = new Comments
                    {
                        Id = c,
                        Author = $"Test{i + 1}-{j+1}",
                        Description = string.Concat(Enumerable.Repeat("Test", i + 1)),
                        Status = 0,
                        StatusMessage = "Active",
                        PostId = i + 1,
                    };
                    c++;
                    await AddComments(tPost);
                }
            }
            return Ok();
        }

        [HttpPost, Route("[controller]/AddComments")]
        public async Task<IActionResult> AddComments([Bind("Id, Author, Description, PostId, Status, StatusMessage")] Comments comment)
        {
            comment.Updated = DateTime.Now;
            using (var producer = new ProducerBuilder<Null, string>(config).Build())
            {
                var result = await producer.ProduceAsync("Comments", new Message<Null, string> { Value = JsonSerializer.Serialize(comment) });
            }
            return Ok(JsonSerializer.Serialize(comment));
        }

        [HttpGet, Route("[controller]/GetComments/{id?}")]
        public IActionResult GetComments(int? id)
        {
            var response = client.GetAsync($"Dataflow/GetComments/{id}").Result;
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