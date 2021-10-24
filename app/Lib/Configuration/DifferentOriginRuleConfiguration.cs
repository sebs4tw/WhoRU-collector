using System;

namespace app.Lib.Configuration
{
    public class DifferentOriginRuleConfiguration
    {
        public uint MaxOriginCount {get;set;} = 2;
        public uint AnalysisThresholdMs {get;set;} = 5 * 60 * 1000;
    }
}