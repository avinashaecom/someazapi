using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AECOM.GNGApi.Models;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.Configuration;



namespace AECOM.GNGApi.Controllers
{   [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class OpportunityController : ControllerBase
    {
        private readonly GoNoGoContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GoNoGoController> _logger;
        public IConfiguration _configuration;

        // Flow or Not to Flow
        private readonly string _userName;
        private readonly string _userEmail;

        public OpportunityController(GoNoGoContext context, IHttpContextAccessor httpContextAccessor, ILogger<GoNoGoController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            // send back header with user identity
            string identityClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
            //var userInfo = _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == identityClaimType).Value;
            //_httpContextAccessor.HttpContext.Response.Headers.Append("x-gng-user-object-id", userInfo);

            #region _UNWANTED
            ////if (_httpContextAccessor.HttpContext.Request.QueryString.HasValue)
            ////{
            ////    string callerIdentityBase64 = _httpContextAccessor.HttpContext.Request.Query["callerIdentity"];
            ////    var callerIdentityBytes = System.Convert.FromBase64String(callerIdentityBase64);
            ////    string callIdentityStr = System.Text.Encoding.UTF8.GetString(callerIdentityBytes);
            ////    var callIdentityJObject = JObject.Parse(callIdentityStr);
            ////    _userEmail = callIdentityJObject.SelectToken("email").Value<string>();
            ////    _userName = callIdentityJObject.SelectToken("displayName").Value<string>();
            ////}
            ////else
            ////{
            ////    _userName = _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == "name").Value;
            ////    _userEmail = _httpContextAccessor.HttpContext.User.Claims.First(email => email.Type == "preferred_username").Value;
            ////}
            #endregion _UNWANTED

            _configuration = configuration;


            if (_httpContextAccessor.HttpContext.Request.QueryString.HasValue) // Implementation for Flow Based callerIdentity
            {
                string callerIdentityBase64 = _httpContextAccessor.HttpContext.Request.Query["callerIdentity"];
                var callerIdentityBytes = System.Convert.FromBase64String(callerIdentityBase64);
                string callIdentityStr = System.Text.Encoding.UTF8.GetString(callerIdentityBytes);
                var callIdentityJObject = JObject.Parse(callIdentityStr);
                _userEmail = callIdentityJObject.SelectToken("email").Value<string>();
                _userName = callIdentityJObject.SelectToken("displayName").Value<string>();
            }
            else if (_httpContextAccessor.HttpContext.User.Claims.Any(c => c.Type == "name")) // Implementaion for Azure User Based Claims
            {
                _userName = _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == "name").Value;
                _userEmail = _httpContextAccessor.HttpContext.User.Claims.First(email => email.Type == "preferred_username").Value;
            }

            else if (_httpContextAccessor.HttpContext.User.Claims.Any(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")) // Implementation for oid based claim - specifically for PowerApp 
            {
                string powerAppConfiguredOid = _configuration["PowerappOidSettings:oid"];
                if (!string.IsNullOrEmpty(powerAppConfiguredOid))
                {
                    string powerAppAzureOid = _httpContextAccessor.HttpContext.User.Claims.First(o => o.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                    if (string.Compare(powerAppConfiguredOid, powerAppAzureOid) == 0)
                    {
                        _userEmail = "powerapp.application@aecom.com";
                        _userName = "Application, PowerApp";
                    }

                }

            }


        }

        [HttpGet("{opportunityId}/GoNoGo")]
        public async Task<ActionResult<IEnumerable<GoNoGoPost>>> GetGoNoGo(string opportunityId)
        {
            if (string.IsNullOrEmpty(opportunityId))
            {
                return BadRequest();
            }
            using (var _getGNGsByopportunityId = _context) // new GoNoGoContext())
            {

                var entityColl = _getGNGsByopportunityId.GoNoGoPost.FromSqlRaw("EXECUTE dbo.GetAllGNGsByOppoID {0}", opportunityId).ToList();
                if (entityColl.Count > 0)
                {
                    return entityColl;
                }
                else
                {
                    var entity = new
                    {
                        Id = 0,
                        OpportunityId = "",
                        CreatedBy = "",
                        LastModifiedBy = "",
                        CreatedOn = "",
                        LastModifiedOn = "",
                        FormData = "",
                        GngstatusId = 1,
                        GngStatusValue = "",
                        Error = "No GNG Record with { opportunityId = " + opportunityId + " }  Found!"

                    };
                    return NotFound(entity);
                    return Content("no GNG record found!");
                }
            }

            //return await _context.GoNoGo.ToListAsync();
        }

        private bool GoNoGoExists(int id)
        {
            return _context.GoNoGo.Any(e => e.Id == id);
        }
    }
}
