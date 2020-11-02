using System;
using EmployeeChat.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Shared;
using Shared.Models;

namespace EmployeeChat.Extensions
{
    public static class Extensions
    {
        public static IApplicationBuilder MapHub(this IApplicationBuilder app, PathString path,
            ISocketHandler handler) =>
            app.Map(path, (_app) => _app.UseMiddleware<WebSocketMiddleware>(handler));
    }
}