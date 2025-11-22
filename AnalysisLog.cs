using System.Runtime.Serialization;

namespace AtronSuiteApp
{
    // --- MODELO DE DADOS ---
    [DataContract]
    public class AnalysisLog
    {
        [DataMember] public string Date { get; set; }
        [DataMember] public string SizeFound { get; set; }
        [DataMember] public string TriggerType { get; set; }
    }
}