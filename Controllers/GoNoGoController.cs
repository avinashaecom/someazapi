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
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace AECOM.GNGApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/v1/[controller]")]
    public class GoNoGoController : ControllerBase
    {
        private readonly GoNoGoContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<GoNoGoController> _logger;
        public IConfiguration _configuration;


        // Flow or Not to Flow
        private readonly string _userName;
        private readonly string _userEmail;

        public GoNoGoController(GoNoGoContext context, IHttpContextAccessor httpContextAccessor, ILogger<GoNoGoController> logger, IConfiguration configuration)
        {
            _logger = logger;
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            // send back header with user identity
            string identityClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
            //var userInfo = _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == identityClaimType).Value;
            //_httpContextAccessor.HttpContext.Response.Headers.Append("x-gng-user-object-id", userInfo);


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

        [HttpGet()]
        public async Task<ActionResult<IEnumerable<GoNoGo>>> Get()
        {
            using (var _getAllGNGs = _context) // new GoNoGoContext())
            {

                var entityColl = await _getAllGNGs.GoNoGo.FromSqlRaw("EXECUTE dbo.GetAllGNGs").ToListAsync();
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
                        Error = "No GNG Records Found!"

                    };
                    return NotFound(entity);  //return Content("no GNG record found!");
                }
            }


        }

        [HttpGet("{Id}")]
        public async Task<ActionResult<GoNoGo>> Get(int Id)
        {
            if (Id == null)
            {
                return BadRequest();
            }
            using (var _getGNGsByopportunityId = _context) // new GoNoGoContext())
            {
                var entities = await _getGNGsByopportunityId.GoNoGo.FromSqlRaw("EXECUTE dbo.GetGNGsByID {0}", Id).ToListAsync();
                if (entities.Count != 1)
                {
                    //return NotFound();
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
                        Error = "No GNG Record with { Id = " +Id + " }  Found!"

                    };
                    return NotFound(entity);
                }
                else
                {
                    return entities[0];
                }
            }
        }

        /// <summary>
        /// Purpose: CREATE New GNG record 
        /// HttpVerb: POST
        /// Input: GNG Record as a Body of request 
        /// GNG Record MUST to have 
        ///   Id - int
        ///   CreatedBy - user email address as a string
        ///   FormData - JSON string equivalent of GNG Record [Refer PoerApp Screen#2]
        ///   GngStatusId - int 
        ///   Content-Type:Application/json
        /// Output: JSON for newly created GNG record 
        /// example: https://myserverurl/api/v1/gonogo
        /// </summary>
        /// /// 
        /// <param name="goNoGo">part of request body</param>
        /// <returns>HTTP Operation StatusStatus</returns>
        [HttpPost]
        public async Task<ActionResult<GoNoGoPost>> Post(GoNoGo goNoGo)
        {
            //_context.GoNoGo.Add(goNoGo);
            //await _context.SaveChangesAsync();

            if (!ModelState.IsValid)
            {


            }
            using (var _postContext = _context) // new GoNoGoContext())
            {
                goNoGo.CreatedBy = _userEmail;
                goNoGo.CreatedOn = DateTime.UtcNow;
                goNoGo.LastModifiedBy = _userEmail;
                goNoGo.LastModifiedOn = DateTime.UtcNow;

                _postContext.GoNoGo.Add(goNoGo);
                await _postContext.SaveChangesAsync();



                try
                {
                    var entity = _postContext.GoNoGoPost.FromSqlRaw("EXECUTE DBO.GetGNGsByID {0}", goNoGo.Id).ToList();

                    GoNoGoPost retPostRecord = new GoNoGoPost();

                    retPostRecord.Id = entity[0].Id;
                    retPostRecord.GngStatusValue = entity[0].GngStatusValue;
                    //retPostRecord.Gngstatus = retRecordStatus;
                    retPostRecord.GngstatusId = entity[0].GngstatusId;
                    retPostRecord.LastModifiedBy = entity[0].LastModifiedBy;
                    retPostRecord.CreatedBy = entity[0].CreatedBy;
                    retPostRecord.LastModifiedOn = entity[0].LastModifiedOn;
                    retPostRecord.OpportunityId = entity[0].OpportunityId;
                    retPostRecord.FormData = entity[0].FormData;
                    retPostRecord.CreatedOn = entity[0].CreatedOn;

                    //return entity[0];
                    return retPostRecord;
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GoNoGoExists(goNoGo.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        return Content("Not able to Fetch newly created GNG Record!");
                    }
                }

            }
        }

        /// <summary>
        /// Purpose: UPDATE GNG record for Id 
        /// HttpVerb: PUT
        /// Input: (MUST) Id in URI and actual GNG Record as a Body of request 
        /// GNG Record MUST to have Id 
        /// Content-Type:Application/json
        /// Output: Http Response Status 
        /// example: https://myserverurl/api/v1/gonogo/8
        /// </summary>
        /// <param name="Id">Part of URI</param>
        /// <param name="goNoGo">part of body</param>
        /// <returns>HTTP Operation StatusStatus</returns>
        [HttpPut("{Id}")]
        public async Task<IActionResult> Put(int Id, GoNoGo goNoGo)
        {


            if (Id != goNoGo.Id)
            {
                return BadRequest();
            }

            //_context.Entry(goNoGo).State = EntityState.Modified;
            using (var _putContext = _context) //  new GoNoGoContext())
            {
                try
                {
                    //Retrieve and Ensure Record Exist to Update
                    var entity = await _putContext.GoNoGo.FirstOrDefaultAsync(gngRec => gngRec.Id == goNoGo.Id);

                    if (entity != null)
                    {
                        //fill up entity with provided fields ONLY
                        if (goNoGo.OpportunityId != null)
                        {
                            entity.OpportunityId = goNoGo.OpportunityId;
                        }
                        ////if (goNoGo.CreatedBy != null)  // not required as created by is "NOT REQUIRED" as filled up by requesting user
                        ////{
                        ////    entity.CreatedBy = goNoGo.CreatedBy;
                        ////}
                        ///
                        //if (string.IsNullOrEmpty(_userEmail))
                        //{
                        //    entity.CreatedBy = _userEmail;
                        //}
                        if (goNoGo.FormData != null)
                        {
                            entity.FormData = goNoGo.FormData;
                        }
                        if (goNoGo.GngstatusId != null)
                        {
                            entity.GngstatusId = goNoGo.GngstatusId;
                        }
                        //if (string.IsNullOrEmpty(goNoGo.LastModifiedBy))
                        //{
                        //    return BadRequest("Bad Request - Missing LastModifiedBy Value!");
                        //}

                        //entity.LastModifiedBy = goNoGo.LastModifiedBy;
                        entity.LastModifiedBy = _userEmail;
                        entity.LastModifiedOn = DateTime.UtcNow;

                        _putContext.GoNoGo.Update(entity);
                        _putContext.SaveChanges();

                    }
                    else
                    {
                        var empptyEntity = new
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
                            Error = "No GNG Record with { Id = " + Id + " }  Found!"

                        };
                        return NotFound(empptyEntity);
                        //return BadRequest("No Record Id for Supplied OpportunityId Exist in the System!");
                    }

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GoNoGoExists(Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return Content("Status:OK - Updateded Seccessfully");
            //return NoContent();
        }

        [HttpGet("byOpportunityId/{opportunityId}")]
        public async Task<ActionResult<IEnumerable<GoNoGo>>> GetGoNoGobyOpportunityId(string opportunityId)
        {

            using (var _getAllGNGs = _context) // new GoNoGoContext())
            {

                var entityColl = await _getAllGNGs.GoNoGo.FromSqlRaw("EXECUTE dbo.GetAllGNGsByOppoID {0}", opportunityId).ToListAsync();
                if (entityColl.Count > 0)
                {
                    return entityColl;
                }
                else
                {
                    return Content("no GNG record found!");
                }
            }


        }

        private bool GoNoGoExists(int id)
        {
            return _context.GoNoGo.Any(e => e.Id == id);
        }
    }
}
