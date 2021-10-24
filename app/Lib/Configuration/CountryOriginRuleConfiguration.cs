using System;
using System.Collections.Generic;

namespace app.Lib.Configuration
{
    public class CountryOriginRuleConfiguration
    {
        public HashSet<string> Trusted {get;set;} = new HashSet<string>{"Canada", "USA"};
        public HashSet<string> HighRisk {get;set;} = new HashSet<string>{"China", "Cuba", "Iran", "North Korea", "Russia", "Sudan", "Syria"};
    }
}