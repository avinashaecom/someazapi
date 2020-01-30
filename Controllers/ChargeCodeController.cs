using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AECOM.GNGApi.Models;
using Microsoft.Extensions.Configuration;
using RestSharp;
using System.Security.Claims;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using Newtonsoft.Json;


namespace SecureGNGAPI.Controllers
{
    [Authorize]
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ChargeCodeController : ControllerBase
    {
        private readonly GoNoGoContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ChargeCodeController> _logger;
        private readonly string _userInfo;
        public IConfiguration _configuration;

        // Flow or Not to Flow
        private readonly string _userName;
        private readonly string _userEmail;

        public ChargeCodeController(GoNoGoContext context, IHttpContextAccessor httpContextAccessor, ILogger<ChargeCodeController> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;            
            _httpContextAccessor = httpContextAccessor;

            #region _UNWANTED 
            // send back header with user identity

            //string identityClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
            //var oid = _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == identityClaimType).Value;
            //var robotOid = "confi setting Values"
            //_httpContextAccessor.HttpContext.Response.Headers.Append("x-gng-user-name", userInfo);
            //_userInfo = _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == "name").Value;           


            //string identityClaimType = "http://schemas.microsoft.com/identity/claims/objectidentifier";
            //var oid = _httpContextAccessor.HttpContext.User.Claims.First(c => c.Type == identityClaimType).Value; //RobotOidSettings //c077ad23-10e8-4836-8cad-f622ef5a7881
            #endregion

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
            
            else if (_httpContextAccessor.HttpContext.User.Claims.Any(c => c.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier")) // Implementation for oid based claim - specifically for Robot 
            {
                string robotConfiguredOid =  _configuration["RobotOidSettings:oid"];
                if(!string.IsNullOrEmpty(robotConfiguredOid))
                {
                    string robotAzureOid = _httpContextAccessor.HttpContext.User.Claims.First(o => o.Type == "http://schemas.microsoft.com/identity/claims/objectidentifier").Value;
                    if (string.Compare(robotConfiguredOid, robotAzureOid) == 0)
                    {
                        _userEmail = "robot.application@aecom.com";
                        _userName = "Application, Robot";
                    }

                }

            }
           


        }

        

      
        [HttpPatch("{id}")]
        public async Task<IActionResult> PatchGoNoGo(int id, ChargeCodeReq goNoGo)
        {
            if (!string.IsNullOrEmpty(goNoGo.Action) && goNoGo.Action.ToLower() == "updatechargecode")
            {
                using (var _patchContext = _context)
                {
                    try
                    {
                        //Retrieve and Ensure Record Exist to Update
                        var entity = _patchContext.GoNoGo.FirstOrDefault(gngRec => gngRec.Id == id);

                        if (entity != null)
                        {
                            //fill up entity with provided fields ONLY
                            if (id != entity.Id)
                            {
                                return BadRequest();
                            }
                            else
                            {

                            }
                            if (string.IsNullOrEmpty(goNoGo.ChargeCode))
                            {
                                return BadRequest();
                            }
                            else
                            {
                                entity.ChargeCode = goNoGo.ChargeCode;
                            }
                            if (string.IsNullOrEmpty(goNoGo.ChargeCodeStatus))
                            {
                                return BadRequest();
                            }
                            else
                            {
                                entity.ChargeCodeStatus = goNoGo.ChargeCodeStatus;
                            }
                            

                            if (string.IsNullOrEmpty(_userEmail))
                            {
                                return BadRequest();
                            }
                            else
                            {
                                entity.LastModifiedBy = _userEmail;
                            }
                            if (string.IsNullOrEmpty(_userEmail))
                            {
                                return BadRequest();
                            }
                            else
                            {
                                entity.ChargeCodeStatusBy = _userEmail;
                            }


                            if (string.IsNullOrEmpty(goNoGo.ChargeCodeStatusMessage))
                            {
                                return BadRequest();
                            }
                            else
                            {
                                entity.ChargeCodeStatusMessage = goNoGo.ChargeCodeStatusMessage;
                            }
                            if (!goNoGo.ChargeCodeStatusTime.HasValue)
                            {
                             
                                entity.ChargeCodeStatusTime = DateTime.UtcNow;

                            }
                            else
                            {
                                entity.ChargeCodeStatusTime = goNoGo.ChargeCodeStatusTime;
                            }
                            entity.LastModifiedOn = DateTime.UtcNow;


                            _patchContext.GoNoGo.Update(entity);
                            _patchContext.SaveChanges();

                            if (!string.IsNullOrEmpty(entity.FormData))
                            {
                                int status = SendPostChargeCodeNotifications(entity.FormData, entity.ChargeCode);
                                if (status == 1)
                                {

                                    return Content("ChargeCode Update successful but GNG for given Chargecode is missing Sid!");
                                }
                                if (status == 2)
                                {

                                    return Content("ChargeCode Update successful but GNG for given Chargecode is missing Sid Value!");
                                }
                                if (status == 3)
                                {

                                    return Content("ChargeCode Update successful but SFDC Update Failed");
                                }
                                //if (status == 4)
                                //{

                                //    return Content("ChargeCode Update successful but SFDC Update Failed");
                                //}
                            }
                            else
                            {
                                return Content("ChargeCode Update successful but GNG FormData is empty!");
                            }

                              
                             



                        }
                        else
                        {
                            var chargerCodeEntity = new
                            {
                                Id = id,
                                Error = "Charge Code {" + id + "} not found!"
                                

                            };
                            return NotFound(chargerCodeEntity);
                        }
                        

                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!GoNoGoExists(goNoGo.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                return NoContent();
            }
            else
            {
                return BadRequest();
            }
        }

       

        

        private bool GoNoGoExists(int id)
        {
            return _context.GoNoGo.Any(e => e.Id == id);
        }

        private int SendPostChargeCodeNotifications(string formData, string chargeNumber)
        {

            string str1 = formData.Trim().Substring(1, formData.Length - 2);
            //JObject my_obj = JsonConvert.DeserializeObject<JObject>(str1);
            //var tobj = my_obj["Sid"];

            if(!str1.Contains("Sid"))
            {
                //return false;
                return 1; // Sid Kay not present against GID Id 
            }

            FormData rootObject = JsonConvert.DeserializeObject<FormData>(str1);

            string SFDCOppoId = rootObject.Sid;

            if (string.IsNullOrEmpty(SFDCOppoId))
            {
                //return false; // pass on reason of SFDCOppoId missing in GNG System
                return 2; // Sid Value either null or Empty
            }
            else
            {

                string flowURI = _configuration["FlowSettings:Uri_Dev"];
                var client = new RestClient(flowURI);
                var request = new RestRequest(Method.POST);
                request.AddHeader("Content-Type", "application/json");
                string jsonToSend = "{ \"id\": \"" + SFDCOppoId + "\" ," + "\"ChargeNumber\":\"" + chargeNumber + "\" " + "}";

                request.AddParameter("application/json; charset=utf-8", jsonToSend, ParameterType.RequestBody);
                request.RequestFormat = DataFormat.Json;

                IRestResponse response = client.Execute(request);

                if (!response.IsSuccessful)
                {
                    //return false;
                    return 3; // Failed SFDC Update
                }

                //return true;
                return 4; // Successful SFDC Update!
            }
        }

    }
}
