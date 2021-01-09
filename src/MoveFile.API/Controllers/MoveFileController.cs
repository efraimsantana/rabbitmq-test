using Microsoft.AspNetCore.Mvc;
using MoveFile.API.Model;
using RabbitMQ.Client;

namespace MoveFile.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MoveFileController : ControllerBase
    {
        private readonly IModel rabbitMQModel;

        public MoveFileController(IModel rabbitMqModel)
        {
            this.rabbitMQModel = rabbitMqModel;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Version v0.0.1");
        }

        [HttpPost]
        public IActionResult Post(FileViewModel file)
        {
            var serializedContent = System.Text.Json.JsonSerializer.Serialize(
                file,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true }
            );

            var messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(serializedContent);

            var prop = rabbitMQModel.CreateBasicProperties();

            rabbitMQModel.BasicPublish("", "s3", prop, messageBodyBytes);

            return Ok("Success");
        }
    }
}
