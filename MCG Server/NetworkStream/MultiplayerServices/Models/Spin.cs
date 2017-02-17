using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerServices.Models
{

    public static class SpinScorePostions
    {
        private static Dictionary<string, int> _scores = null;
        public static Dictionary<string, int> Scores 
        { 
            get 
            {
                if (_scores == null)
                {
                    _scores = new Dictionary<string, int>();
                    _scores.Add("5KIND", 5);
                    _scores.Add("UNIQUE", 4);
                    _scores.Add("4KIND", 3);
                    _scores.Add("FULLHOUSE", 2);
                    _scores.Add("3KIND", 1);
                }

                return _scores;

            } 
        }
    }

    public class Spin
    {
        public bool Stay { get; set; }
        public int Turn { get; set; }
        public int Value1 { get; set; }
        public int Value2 { get; set; }
        public int Value3 { get; set; }
        public int Value4 { get; set; }
        public int Value5 { get; set; }
        public string RoomId { get; set; }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3},{4}", 
                this.Value1 == 0 ? "?" : this.Value1.ToString(),
                this.Value2 == 0 ? "?" : this.Value2.ToString(),
                this.Value3 == 0 ? "?" : this.Value3.ToString(),
                this.Value4 == 0 ? "?" : this.Value4.ToString(),
                this.Value5 == 0 ? "?" : this.Value5.ToString()
                );
        }

        public int GetPositions()
        {
            var values = new List<int>() { this.Value1, this.Value2, this.Value3, this.Value4, this.Value5 };
            var sum = values.GroupBy(x => x)
                .Select(x => new { 
                    Count = x.Count(),
                    Value = x.Key
                }).ToList();

            if (sum.Count == 5) // all unique
                return SpinScorePostions.Scores["UNIQUE"];
            else if (sum.Count == 1) // all unique
                return SpinScorePostions.Scores["5KIND"];
            else if (sum.Count == 2 && sum.Any(x => x.Count == 4)) // all unique
                return SpinScorePostions.Scores["4KIND"];
            else if (sum.Count == 2 && sum.Any(x => x.Count == 3)) // all unique
                return SpinScorePostions.Scores["FULLHOUSE"];
            else if (sum.Any(x => x.Count == 3)) // all unique
                return SpinScorePostions.Scores["3KIND"];
            else
                return 0;
        }

    }
}
