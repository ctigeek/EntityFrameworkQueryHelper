using System;

namespace QueryHelper
{
    public class QueryOptions
    {
        private int limit = 200;
        public int Limit
        {
            get { return limit; }
            set
            {
                if (value > 200 || value < 0) limit = 200;
                limit = value;
            }
        }

        private int offset = 0;
        public int Offset
        {
            get { return offset;}
            set
            {
                if (value < 0) offset = 0;
                offset = value;
            }
        }

        public string OrderBy { get; set; }
        public bool OrderDesc { get; set; }
        public string Where { get; set; }
        public string Search { get; set; }
    }
}
