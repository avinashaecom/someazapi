using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace AECOM.GNGApi.Models
{
    public partial class GoNoGo
    {
        public int Id { get; set; }
       [Required]
        public string OpportunityId { get; set; }
        //[Required] - as _userEmail param will fill up and no need to have in request Body
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        [Required]
        public string FormData { get; set; }
       [Required]
        public int? GngstatusId { get; set; }

        public virtual GNGStatus Gngstatus { get; set; }
        
        public string ChargeCodeStatus { get; set; }
        public string ChargeCode { get; set; }
        
        public DateTime? ChargeCodeStatusTime { get; set; }
        public string ChargeCodeStatusBy { get; set; }

        public string ChargeCodeStatusMessage { get; set; }
    }

    public partial class GoNoGoPost
    {
        
        public int Id { get; set; }
        public string OpportunityId { get; set; }
        public string CreatedBy { get; set; }
        public string LastModifiedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public DateTime? LastModifiedOn { get; set; }
        public string FormData { get; set; }
        public int GngstatusId { get; set; }
        public string GngStatusValue { get; set; }
        public string ChargeCodeStatus { get; set; }
        public string ChargeCode { get; set; }       

        public DateTime? ChargeCodeStatusTime { get; set; }
        public string ChargeCodeStatusBy { get; set; }
        public string ChargeCodeStatusMessage { get; set; }

    }

    public partial class ChargeCodeReq
    {
        public int Id { get; set; }       
        public string Action { get; set; }
        public string ChargeCodeStatus { get; set; }
        public string ChargeCode { get; set; }
        public string ChargeCodeStatusMessage { get; set; }
        public DateTime? ChargeCodeStatusTime { get; set; }
        public string ChargeCodeStatusBy { get; set; }
    }
}
