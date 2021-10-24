using System;

namespace app.Lib.Model
{
    public class Origin  {
        public string Country;
        public string IPV4;
        public string IPV6;

        public Tuple<string,string,string> ToTuple()
        {
            return new Tuple<string, string, string>(Country,IPV4,IPV6);
        }

        public static Origin FromTuple(Tuple<string,string,string> tuple)
        {
            return new Origin{
                Country = tuple.Item1,
                IPV4 = tuple.Item2,
                IPV6 = tuple.Item3
            };
        }
    }
}