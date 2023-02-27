using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMATechEleveatorChallenge
{
    public class Person
    {
        public int PickupFloor { get; set; }
        public int? Destination { get; set; }
        public char? DesiredDirection { get; set; }

        public const int Weight = 150;

        public Person(string pickupLocation)
        {
            GetFloorAndDirection(pickupLocation);
        }

        private void GetFloorAndDirection(string pickupLocation)
        {
            string floorString = "";
            for (int i = 0; i < pickupLocation.Length; i++)
            {
                if (Char.IsDigit(pickupLocation, i))
                {
                    floorString += pickupLocation[i];
                }
            }
            PickupFloor = int.Parse(floorString);
            DesiredDirection = Char.ToUpper(pickupLocation[pickupLocation.Length - 1]);
        }
    }
}
