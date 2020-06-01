using FluentValidation;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace MediatRDemo.RequestHandlers
{
    public class Ping : IRequest<string>
    {
        public string ResponseMessage { get; set; }
    }

    public class PingHandler : IRequestHandler<Ping, string>
    {
        public Task<string> Handle(Ping request, CancellationToken cancellationToken)
        {
            return Task.FromResult(request.ResponseMessage);
        }
    }

    public class PingValidator : AbstractValidator<Ping>
    {
        public PingValidator()
        {
            RuleFor(x => x.ResponseMessage)
                .NotEmpty()
                .WithMessage("We need to know what you want from us")
                .DependentRules(() => 
            RuleFor(x => x.ResponseMessage.Length)
                .LessThanOrEqualTo(512)
                .WithMessage("We will not reply with more than 512 bytes"));
        }
    }
}