
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AECOM.GNGApi.Models
{

    public class GLA
    {
        public string Action { get; set; }
        public string DueDate { get; set; }
        public string Factor { get; set; }
        public string PersonResponsible { get; set; }
        public string Status { get; set; }
    }

    public class FormData
    {
        public string AECOMRule { get; set; }
        public string AccountName { get; set; }
        public string ApproverDecision { get; set; }
        public string BL { get; set; }
        public string CaptureMgr { get; set; }
        public string ClientEvalCriteria { get; set; }
        public string CurrExcRate { get; set; }
        public string DateRFPExp { get; set; }
        public string DateRFPExpected { get; set; }
        public string EsitmatedRev { get; set; }
        public string EstimatedAwdDate { get; set; }
        public string FormStatus { get; set; }
        public List<GLA> GLA { get; set; }
        public string GNGAppr1 { get; set; }
        public string GNGAppr2 { get; set; }
        public string Getprb { get; set; }
        public string GoPrb { get; set; }
        public string Grossmargin { get; set; }
        public bool HasRiskAssmtCompleted { get; set; }
        public string IndProjNum { get; set; }
        public bool IsClientAccPlaninPlace { get; set; }
        public string LeadTime { get; set; }
        public string MeetsMPC { get; set; }
        public string MustWin { get; set; }
        public string OpportunityID { get; set; }
        public string OpportunityName { get; set; }
        public string Participants { get; set; }
        public string PrimaryDept { get; set; }
        public string PriorityNotes { get; set; }
        public string ProjDescrp { get; set; }
        public string PursuitBudgTotal { get; set; }
        public string PursuitLaborbudg { get; set; }
        public string PursuitODCbudg { get; set; }
        public string PursuitSubsbudg { get; set; }
        public string Rectype { get; set; }
        public string RequestorDecision { get; set; }
        public string ScoreifYes { get; set; }
        public string Sid { get; set; }
        public string StrategicBU { get; set; }
        public string StrategicPursuit { get; set; }
        public string submitterCmts { get; set; }
    }

    public class FDRootObject
    {
        public FormData[] formdata { get; set; }
    }
}
