using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MediatRDemo.RequestHandlers;
using MediatR;

namespace MediatRDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PingController : Controller
    {
        private readonly IMediator _mediator;

        public PingController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public Task<string> Get()
        {
            return _mediator.Send(new Ping { ResponseMessage = "Pong!" });
        }

        [HttpPost]
        public Task<string> Post([FromBody]string responseMessage)
        {
            return _mediator.Send(new Ping { ResponseMessage = responseMessage });
        }
    }
}