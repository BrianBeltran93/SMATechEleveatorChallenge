using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SMATechEleveatorChallenge
{
    public class Elevator
    {
        public const int TravelTime = 3;
        private const int WaitTime = 1;

        public Elevator() 
        {
        }

        public async void MoveUp(Sensor sensor)
        {
            SetElevatorStateToMovingUp(sensor);
            await AscendFloors(sensor);
        }

        private static void SetElevatorStateToMovingUp(Sensor sensor)
        {
            sensor.CurrentDirectionTraveling = Sensor.TravelDirections.Up;
            sensor.CurrentMovementState = Sensor.MovementStates.Moving;
        }

        private async Task AscendFloors(Sensor sensor)
        {
            for (int i = sensor.CurrentFloor; i <= Sensor.MaxFloorLevel; i++)
            {
                sensor.CurrentFloor = i;

                if (sensor.IsValidDestinationFloor() || sensor.IsValidPickupFloor())
                {
                    StopAtFloor(sensor);
                    return;
                }
                else
                {
                    await TravelBetweenFloors(sensor);
                }
            }
        }


        public async void MoveDown(Sensor sensor)
        {
            SetElevatorStateToMovingDown(sensor);
            await DescendFloors(sensor);
        }

        private static void SetElevatorStateToMovingDown(Sensor sensor)
        {
            sensor.CurrentDirectionTraveling = Sensor.TravelDirections.Down;
            sensor.CurrentMovementState = Sensor.MovementStates.Moving;
        }

        private async Task DescendFloors(Sensor sensor)
        {
            for (int i = sensor.CurrentFloor; i >= Sensor.MinFloorLevel; i--)
            {
                sensor.CurrentFloor = i;

                if (sensor.IsValidDestinationFloor() || sensor.IsValidPickupFloor())
                {
                    StopAtFloor(sensor);
                    return;
                }
                else
                {
                    await TravelBetweenFloors(sensor);
                }
            }
        }


        public async void StopAtFloor(Sensor sensor)
        {
            sensor.CurrentMovementState = Sensor.MovementStates.Stopped;

            sensor.LogElevatorStop();

            sensor.SetCurrentFloorToFalse();

            Console.WriteLine($"Stopped at {sensor.CurrentFloor}.");

            sensor.RemovePeopleFromElevator();
            sensor.AddPeopleToElevator();

            await WaitAtStoppedFloor();

            sensor.CheckActiveFloors(this);
        }

        private static async Task WaitAtStoppedFloor()
        {
            await Task.Delay(TimeSpan.FromSeconds(WaitTime));
        }

        private async Task TravelBetweenFloors(Sensor sensor)
        {
            sensor.LogElevatorPass();
            await Task.Delay(TimeSpan.FromSeconds(TravelTime));
        }
    }
}
