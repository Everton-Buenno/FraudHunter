
using Microsoft.ML.Data;
namespace FraudHunter.Core.Models
{
    public class TransactionData
    {
        [LoadColumn(0)]
        public float Time { get; set; }

        [LoadColumn(1)] public float V1 { get; set; }
        [LoadColumn(2)] public float V2 { get; set; }
        [LoadColumn(3)] public float V3 { get; set; }
        [LoadColumn(4)] public float V4 { get; set; }
        [LoadColumn(10)] public float V10 { get; set; }
        [LoadColumn(14)] public float V14 { get; set; }

        [LoadColumn(29)]
        public float Amount { get; set; }

        [LoadColumn(30)]
        public float Class { get; set; } 
    }
}
