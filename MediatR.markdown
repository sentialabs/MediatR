# Mediator pattern in C#

If you want to implement the Mediator pattern in C# you could of course implement the mediator and everything yourself. Another option is to use a standard library for this, a popular choice with us is [MediatR](https://github.com/jbogard/MediatR) since it's simple to implement and doesn't impose any rules on you. That's why it's tagline is "Simple, unambitious mediator implementation in .NET". In this blog post we'll take a quick look at what the mediator pattern is, and how to implement this with C#.

## What is the mediator pattern
While [Wikipedia](https://en.wikipedia.org/wiki/Mediator_pattern) probably explains it better than I can, let me give it a shot.

The mediator pattern is meant to split responsibilities between a caller and the callee. So instead of having an instance of a class and calling a method directly on it, you ask the mediator to do this for you. While this might sound a lot like doing dependency injection, it actually goes a step further. You don't even know the interface of the callee. This makes it even more decoupled than you'd have with dependency injection.

The result is that the objects are loosely coupled. They only know about the mediator object, not about each other. Besides the mediator object they'll likely also have some kind of data transfer object to send parameters for the callee.

For a comparison in the real world, imagine only being able to communicatie with someone or a department through their secretary. You give them a message (the data transfer object) and they pass it on to who you want to reach. Once there's an answer, the secretary will give it back to you. This way you have no knowledge of who actually answered it.

## How to implement it in C#
For this part I'm assuming you are starting from scratch. If you are implementing this in an already existing solution the steps should be the same, but the order of things in the  `Startup.cs` might be different. If you're just looking for a full example, head on over to [this GitHub repository](https://github.com/xudonax/MediatR). As we're mostly doing web application development here at Sentia, this example will be in the form of a web application, based on .NET Core 3.1. There is nothing that's stopping you from implementing something like this in a console application or a GUI application.

### Setting up the project
First things first, we'll make a new ASP.NET Core project. Because we don't really need either MVC or Web API for this example, we'll just use an empty ASP.NET Core project. This is created by the command `dotnet new web` in an empty directory. This will use the folder name as name for the project. On my machine, the output was as follows:

```
$ dotnet new web
The template "ASP.NET Core Empty" was created successfully.

Processing post-creation actions...
Running 'dotnet restore' on /home/sanne/MediatRDemo/MediatRDemo.csproj...
  Determining projects to restore...
  Restored /home/sanne/MediatRDemo/MediatRDemo.csproj (in 81 ms).

Restore succeeded.
```

In other words, we now have a new project named `MediatRDemo.csproj`. This is the project we'll use in this blog post. Of course all of this could also be done in your favorite IDE, be it Visual Studio or Jetbrains Rider.

Next up, we need to install `MediatR`. This is a [NuGet package](https://www.nuget.org/packages/MediatR/), so we'll install it using `dotnet add package MediatR`, giving us the following output:

```
$ dotnet add package MediatR
  Determining projects to restore...
  Writing /tmp/tmpZaLe9j.tmp
info : Adding PackageReference for package 'MediatR' into project '/home/sanne/MediatRDemo/MediatRDemo.csproj'.
info : Restoring packages for /home/sanne/MediatRDemo/MediatRDemo.csproj...
info :   GET https://api.nuget.org/v3-flatcontainer/mediatr/index.json
info :   OK https://api.nuget.org/v3-flatcontainer/mediatr/index.json 108ms
info :   GET https://api.nuget.org/v3-flatcontainer/mediatr/8.0.1/mediatr.8.0.1.nupkg
info :   OK https://api.nuget.org/v3-flatcontainer/mediatr/8.0.1/mediatr.8.0.1.nupkg 6ms
info : Installing MediatR 8.0.1.
info : Package 'MediatR' is compatible with all the specified frameworks in project '/home/sanne/MediatRDemo/MediatRDemo.csproj'.
info : PackageReference for package 'MediatR' version '8.0.1' added to file '/home/sanne/MediatRDemo/MediatRDemo.csproj'.
info : Committing restore...
info : Writing assets file to disk. Path: /home/sanne/MediatRDemo/obj/project.assets.json
log  : Restored /home/sanne/MediatRDemo/MediatRDemo.csproj (in 1.15 sec).
```

Great! So, now we've got MediatR installed, but this way it isn't as easy to use. To make it easier to use, we'll also install `MediatR.Extensions.Microsoft.DependencyInjection` the same way. After this, we need to add MediatR to the services collection. To do this, open up `Startup.cs` and add the code below to the `ConfigureServices(IServiceCollection services)` method:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddMediatR(typeof(Startup));
}
```

This makes it so you can get the `IMediator` interface from dependency injection in .NET Core. Don't forget to add `using MediatR;` to the top of the file if your IDE didn't prompt you for it. We're passing in the type in which MediatR should look for implementations of `IRequest` and `IRequestHandler`. In this case, that's the same project as the `Startup` class. In a bigger application, this might be another assembly.

Short recap of what we've done so far:

1. Create a new project
2. Add the MediatR and dependency injection packages
3. Add MediatR to the ServiceCollection using the dependency injection package

### Adding the MediatR objects

The next step is adding an `IRequest` and `IRequestHandler`. These represent a request to the mediator, and a way to handle this. Convention for MediatR is to have these two classes in the same file, which we'll follow for convenience. So, let's create a new folder named `RequestHandlers` which'll contain the new file. In this folder, create a file named `PingHandler.cs`. I'd advise doing this with your IDE so you'll have the correct namespace and some basic `using` statements. In this file, create two classes:

1. A class named `Ping` which implements the `MediatR.IRequest<string>` interface
2. A class named `PingHandler` which implements the `MediatR.IRequestHandler<Ping, string>`

The `IRequestHandler` interface takes one or two type arguments. If you give it just one, you cannot return a value. If you give it two, you can return a value. In the `Ping` class, add one public property of type `string` with the name `ResponseMessage`. In the `PingHandler` class, implement the interface and return the `ResponseMessage` from the request. Your code should now look like this:

```csharp
using System.Threading;
using System.Threading.Tasks;
using MediatR;

namespace MediatRDemo.RequestHandlers {
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
}
```

So, now we have both the request and the handler for that request. The handler will simply reply with the response message that was request. So, how do we call this? Simple! We'll add a controller that'll do the work for us. Before we get to that part though, we'll have to add controller support in the `Startup.cs`. To do this, add the line below to the `ConfigureServices` method:

```csharp
services.AddControllers()
```

Also, in the `Configure` method we need to modify the lambda function inside `app.UseEndpoints`. At the moment it'll always return the text "Hello World" when calling the website. Not what we want. Instead, we want to use controllers to handle the requests. To do this, modify it to look like this:

```csharp
app.UseEndpoints(endpoints => endpoints.MapControllers());
```

With that done, we're ready to create our controllers and have then work as well. Create a folder named `Controllers` in the root of your project and add an API controller named `PingController.cs`. Add a `private readonly IMediator _mediator` and initialize it from the constructor. Next, add a `Get` method and have it call `_mediator.Send(new Ping { ResponseMessage = "Pong!" })`. Your controller should look like this:

```csharp
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
    }
}
```

If you now run `dotnet run` in the root of the project, everything should compile and you should be able to access [http://localhost:5000/ping](http://localhost:5000/ping). This should show you the text you entered for the `ReponseMessage` in your controller. So far so good, but where are you going to put validation?

### Adding validations
A significant part of time in web applications is spend validating user input. You'll never now what the user will send you. To make this simpler we'll use FluentValidations. This is available as a [NuGet package](https://www.nuget.org/packages/FluentValidation.DependencyInjectionExtensions) as well. In this case, we're directly installing the dependency injection extensions, this will download and install FluentValidation as a dependency.

Install this with `dotnet add package FluentValidation.DependencyInjectionExtensions` in the root of your project. Next, we need to update the `ConfigureServices` to load the validators. Add the following line to that method, and don't forget to add `using FluentValidation;` at the top of the file.

```csharp
services.AddValidatorsFromAssemblyContaining<Startup>();
```

You need to pass in the assembly in which your validators are. In this case, it's the same as the `Startup` class, but just like with adding MediatR it might be somewhere else in a bigger project. So change it where necessary. 

We'll add validators to the same file as the `IRequest` and `IRequestHandler`, this'll keep everything nice and contained for this demo. In reality, you could have a completely different namespace or assembly for the validators. 

To add a validator, open the `RequestHandlers/PingHandler.cs` file again, and add a `PingValidator` class. This class must inherit from the `FluentValidation.AbstractValidator<Ping>` class. `Ping`, in this case, is referring to the `IRequest` we want to validate. As an example, we'll check that the `ResponseMessage` is not empty but also not longer than 512 characters. We'll do that by adding a constructor with these validations so it'll look like this:

```csharp
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
            .WithMessage("We will not reply with more than 512 characters"));
    }
}
```

Here, too, you must add the `using FluentValidation` line to the list of usings or it won't compile. So, now we have the `IRequest`, the `IRequestHandler` and the validation set up, but we still need to add something that'll call your validations on each request to MediatR. To do this, MediatR has something called `IPipelineBehavior`. This is an interface that gives you access to the MediatR pipeline, and using this we'll validate the incoming `IRequest`. There are a lot of examples for this online, but I'm using a combination from multiple sources that looks like this:

```csharp
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;

namespace MediatRDemo
{
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var context = new ValidationContext(request);
            var failures = _validators
                .Select(v => v.Validate(context))
                .SelectMany(result => result.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                throw new ValidationException(failures);
            }

            return next();
        }
    }
}
```

This'll get all the registered `IValidator<TRequest>` instances from the dependency injection framework, and then call all of them. If none of these give a failure, the request passes on to the next stage in the pipeline via the call to `next()`. But if there is a failure, we'll throw a new `ValidationException` containing a list of all failures. There are improvements to be had, for example if you have a lot of validators you want to call for each request you might want to call them in parallel.

To have MediatR call this `IPipelineBehavior` on each request, add the following line to the `ConfigureServices` method in the `Startup` class:

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
```

We're adding it as a transient here to make sure we don't run the wrong validators. With this done, all we need to do is give the outside world a way to trigger this validator. The current `Get` method on the controller isn't likely to trigger a failure on the validator because it has a constant string that's being sent. To make it a bit more interesting, let's add a `Post` method that'll accept an input and try to put it into the `ResponseMessage`. Add this to your `PingController`:

```csharp
[HttpPost]
public Task<string> Post([FromBody]string responseMessage)
{
    return _mediator.Send(new Ping { ResponseMessage = responseMessage });
}
```

If you now run the application again using `dotnet run`, it should still work just fine when doing a simple GET request via the browser. However, we can now also POST data to the controller and have it return it to us. Let's use curl to do some POST requests to the controller.

Let's start with something simple, something that'll surely pass the validator:
```
$ curl 
    --request POST \
    --header "Content-Type: application/json" \
    --data '"hello"' \
    http://localhost:5000
hello
```

This weird escaping around the `hello` is necessary, because otherwise it isn't a proper JSON string. The request should just return `hello` back to you. So far so good. What happens when we do the same request, but with an empty the `--data` argument? Let's try it:

```
$ curl 
    --request POST \
    --header "Content-Type: application/json" \
    --data '' \
    http://localhost:5000
{"type":"https://tools.ietf.org/html/rfc7231#section-6.5.1","title":"One or more validation errors occurred.","status":400,"traceId":"|b3bf985d-4b67db25a45cb80a.","errors":{"":["A non-empty request body is required."]}}
```

Yikes, that gives us an error. Note how it returns the message you told it to return in the 