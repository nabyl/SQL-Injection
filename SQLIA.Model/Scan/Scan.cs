using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLIA.Model
{
    public partial class Scan
    {
        public void CaculateStatistics()
        {
            //run statistics for the scan 

           this.TotalBenign =  this.ScanEntries.Where(i => i.ActualPossiblity == false).Count();
           this.TotalMalicious = this.ScanEntries.Where(i => i.ActualPossiblity == true).Count();

           this.TruePositive = this.ScanEntries.Where(i => i.InjectionAttackPossible == true && i.ActualPossiblity == true).Count();
           this.TrueNegative = this.ScanEntries.Where(i => i.InjectionAttackPossible == false && i.ActualPossiblity == false).Count();

           this.FalsePositive = this.ScanEntries.Where(i => i.InjectionAttackPossible == true && i.ActualPossiblity == false).Count();
           this.FalseNegative = this.ScanEntries.Where(i => i.InjectionAttackPossible == false && i.ActualPossiblity == true).Count();

           this.DetectionRate = ((double)this.TruePositive / (double)(this.TruePositive + this.FalseNegative))* 100;;
           this.DetectionAccuracy = ((double)(this.TruePositive + this.TrueNegative) / (double)(this.TruePositive + this.TrueNegative + this.FalsePositive + this.FalseNegative))* 100;;

           this.FalsePositiveRate = ((double)this.FalsePositive / (double)(this.TruePositive + this.FalsePositive))* 100;
           this.FalseNegativeRate = ((double)this.FalseNegative / (double)(this.TrueNegative + this.FalseNegative)) * 100;


        }
    }
}
