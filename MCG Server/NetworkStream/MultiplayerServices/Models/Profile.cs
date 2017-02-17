using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiplayerServices.Models
{
    public class Profile
    {
        private int _tokenPrizeIncrement = 100;

        public Profile()
        {
            Experience = 1;
            Tokens = 100;
            LastPrizeOn = DateTime.Now.ToUniversalTime();
            IncrementNextPriceOn();
            Inventory = new List<Inventory>();
            PrizeValue = 100;
        }

        public double Experience { get; set; }

        public int Tokens { get; set; }

        public DateTime? LastPrizeOn { get; set; }

        public DateTime? NextPrizeOn { get; set; }

        public List<Inventory> Inventory { get; set; }


        public int  PrizeValue { get; set; }

        public void ClaimTokens()
        {
            if (this.NextPrizeOn > DateTime.Now.ToUniversalTime()) throw new Exception("Unable to claim tokens. Invalid command");
            Tokens += _tokenPrizeIncrement;
            LastPrizeOn = DateTime.Now.ToUniversalTime();
            if (Experience < 5)
                Experience++;
            IncrementNextPriceOn();
        }

        //public void ClaimPrize()
        //{
        //    if (this.Tokens < _prizeValue) throw new Exception("Unable to claim prize. Invalid command");
        //    //Tokens += _tokenPrizeIncrement;
        //    //LastPrizeOn = DateTime.Now.ToUniversalTime();
        //    //if (Experience <= 1)
        //    //    Experience = 2;
        //    //IncrementNextPriceOn();
        //}

        public void IncrementNextPriceOn()
        {
            if (Experience <= 2)
                NextPrizeOn = LastPrizeOn.Value.AddMinutes(5);
            else if (Experience < 5)
                NextPrizeOn = LastPrizeOn.Value.AddMinutes(10);
            else if (Experience < 8)
                NextPrizeOn = LastPrizeOn.Value.AddMinutes(30);
            else if (Experience < 10)
                NextPrizeOn = LastPrizeOn.Value.AddHours(1);
            else if (Experience < 15)
                NextPrizeOn = LastPrizeOn.Value.AddHours(2);
            else if (Experience < 20)
                NextPrizeOn = LastPrizeOn.Value.AddHours(4);
            else
                NextPrizeOn = LastPrizeOn.Value.AddHours(8);

        }


    }



}
