# MediatR
Demo repository for ASP.NET Core and MediatR. This contains a bare mimimum implementation for a MediatR request handler
in ASP.NET Core. You can send a request to /ping as either a GET or a POST. If you send a GET, you'll get a standard
response back saying simply "Pong!". However, if you send a POST request you *have* to specify a response message. And
this response message cannot be `null` or longer than 512 characters. If you fail to meet those conditions, you'll get
a ValidationException back from the FluentValidation library. For more information, read the write-up at [Sentia Techblog](https://www.sentiatechblog.com/).

Most, if not all, of this code is based on the examples and demos as posted on the [MediatR Wiki](https://github.com/jbogard/MediatR/wiki)

