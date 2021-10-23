using System;

namespace app.Lib.Model
{
    public class Origin : IEquatable<Origin> {
        public string Country;
        public string IPV4;
        public string IPV6;

        public bool Equals(Origin other)
        {
            return Country == other.Country && IPV4 == other.IPV4 && IPV6 == other.IPV6;
        }
    }
}