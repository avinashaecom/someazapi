using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AECOM.GNGApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("/api/v1/[controller]")]
    public class PingController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<PingController> _logger;

        public PingController(IHttpContextAccessor httpContextAccessor, ILogger<PingController> logger)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public class PingResponse
        {
            public DateTime Date { get; set; }
            public IEnumerable<KeyValuePair<string,string>> Claims { get; set; }
        }

        [HttpGet]
        public IEnumerable<PingResponse> Get()
        {
            // send back header with user identity
            string identityClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
            var userInfo = _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == identityClaimType).Value;
            _httpContextAccessor.HttpContext.Response.Headers.Append("x-gng-user-object-id", userInfo);

            return Enumerable.Range(1, 1).Select(index => new PingResponse
            {
                Date = DateTime.Now,
                Claims = _httpContextAccessor.HttpContext.User.Claims.Select(c => new KeyValuePair<string, string>(c.Type, c.Value)).ToArray()
            })
            .ToArray();
        }
    }
}
