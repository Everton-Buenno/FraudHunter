
using Microsoft.ML.Data;
namespace FraudHunter.Core.Models
{
    public class FraudPrediction
    {
        [ColumnName("Score")]
        public float Score { get; set; }

        [ColumnName("PredictedLabel")]
        public bool PredictedLabel { get; set; }
    }
}
