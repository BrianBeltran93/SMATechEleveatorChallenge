using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SMATechEleveatorChallenge
{
    public class Sensor
    {
        private int _NumberOfPeopleInElevator = 0;
        private int _currentWeight = 0;
        private readonly string _fullPath = AppDomain.CurrentDomain.BaseDirectory;
        private List<Person> _people;
        private readonly DateTime textFileName = DateTime.UtcNow.ToLocalTime();

        public bool[] Floors { get; set; }
        public int CurrentFloor {get; set;}
        public TravelDirections CurrentDirectionTraveling { get; set; }
        public MovementStates CurrentMovementState {get; set; }

        public const int MinFloorLevel = 1;
        public const int MaxFloorLevel = 12;
        private const int WeightLimit = 1000;
        private const int InElevator = 0;

        public enum TravelDirections
        {
            None,
            Up,
            Down
        }
        public enum MovementStates
        {
            Stopped,
            Moving
        }

        public Sensor() 
        {
            CurrentDirectionTraveling = TravelDirections.None;
            CurrentMovementState = MovementStates.Stopped;
            CurrentFloor = 1;
            _people = new List<Person>();
            Floors = new bool[MaxFloorLevel];
        }

        public void ReadElevatorButtonRequests(Elevator elevator)
        {
            string? userInput = null;
            while (userInput != "Q")
            {
                userInput = null;

                userInput = ReadUserInput(userInput);

                if (userInput == "Q")
                {
                    continue;
                }
                else if (userInput[userInput.Length - 1] == 'U' || userInput[userInput.Length - 1] == 'D')
                {
                    AttemptToReadPickupRequest(elevator, userInput);
                }
                else if (int.TryParse(userInput, out int buttonRequest))
                {
                    AddDestinationToPerson(buttonRequest, elevator);
                }
                else
                {
                    Console.WriteLine("Invalid input!");
                }
            }

            //Let elevator finish going to already requested floors before quitting
            while (CurrentDirectionTraveling != TravelDirections.None)   
            {
                ;
            }
        }

        private static string ReadUserInput(string? userInput)
        {
            while (userInput == null || userInput == "")
            {
                Console.WriteLine("Enter button request:");
                userInput = Console.ReadLine();

                if (userInput != null)
                {
                    userInput = userInput.Trim();
                }
            }
            userInput = userInput.ToUpper();
            return userInput;
        }

        private void AttemptToReadPickupRequest(Elevator elevator, string userInput)
        {
            string inputSubstring = userInput.Substring(0, userInput.Length - 1);

            if (int.TryParse(inputSubstring, out int floorNumber))
            {
                if (floorNumber > MaxFloorLevel || floorNumber < MinFloorLevel)
                {
                    Console.WriteLine("Invalid floor number!");
                }
                else
                {
                    LogValidFloorRequest(userInput);

                    Person person = CreatePerson(userInput);
                    AddPersonToList(person);

                    CheckIfApproachingRequestedFloor(person.PickupFloor);

                    if (CurrentDirectionTraveling == TravelDirections.None)
                    {
                        CheckActiveFloors(elevator);
                    }
                }
            }
            else
            {
                Console.WriteLine("Invalid Input!");
            }
        }

        private Person CreatePerson(string input)
        {
            Person person = new(input);
            return person;
        }

        public void AddPersonToList(Person person)
        {
            _people.Add(person);
            Console.WriteLine($"{_people.Count} total people using the elevator");
        }


        public async void CheckIfApproachingRequestedFloor(int requestedFloor)
        {
            if (CurrentMovementState == MovementStates.Stopped)
            {
                Floors[requestedFloor - 1] = true;
                return;
            }
            else if (IsAlreadyAscendingTowardsRequestedFloor(requestedFloor))
            {
                await Task.Delay(TimeSpan.FromSeconds(Elevator.TravelTime));
            }
            else if (IsAlreadyDescendingTowardsRequestedFloor(requestedFloor))
            {
                await Task.Delay(TimeSpan.FromSeconds(Elevator.TravelTime));
            }
            
            Floors[requestedFloor - 1] = true;
        }

        private bool IsAlreadyAscendingTowardsRequestedFloor(int requestedFloor)
        {
            return CurrentDirectionTraveling == TravelDirections.Up && requestedFloor == CurrentFloor + 1 && Floors[CurrentFloor] == false;
        }

        private bool IsAlreadyDescendingTowardsRequestedFloor(int requestedFloor)
        {
            return CurrentDirectionTraveling == TravelDirections.Down && requestedFloor == CurrentFloor - 1 && Floors[CurrentFloor - 2] == false;
        }


        public void CheckActiveFloors(Elevator elevator)
        {
            if (CurrentDirectionTraveling == TravelDirections.None && Floors[CurrentFloor - 1] == true && _currentWeight < WeightLimit)
            {
                elevator.StopAtFloor(this);
            }
            else if (CurrentDirectionTraveling == TravelDirections.Up || CurrentDirectionTraveling == TravelDirections.None)
            {
                if (HasActiveFloorsAbove())
                {
                    elevator.MoveUp(this);
                }
                else if (HasActiveFloorsBelow())
                {
                    elevator.MoveDown(this);
                }
                else
                {
                    CurrentDirectionTraveling = TravelDirections.None;
                    return;
                }
            }
            else if (CurrentDirectionTraveling == TravelDirections.Down)
            {
                if (HasActiveFloorsBelow())
                {
                    elevator.MoveDown(this);
                }
                else if (HasActiveFloorsAbove())
                {
                    elevator.MoveUp(this);
                }
                else
                {
                    CurrentDirectionTraveling = TravelDirections.None;
                    return;
                }
            }
        }


        private bool HasActiveFloorsAbove()
        {
            if (_currentWeight >= WeightLimit)
            {
                return HasActiveDestinationFloorsAbove();
            }
            else
            {
                for (int i = CurrentFloor; i < Floors.Length; i++)
                {
                    if (Floors[i] == true)
                    {
                        return true;
                    }
                }

                return false;
            }
            
        }

        private bool HasActiveDestinationFloorsAbove()
        {
            for (int i = CurrentFloor; i < Floors.Length; i++)
            {
                if (Floors[i] == true)
                {
                    for (int j = 0; j < _people.Count; i++)
                    {
                        if (i == _people[j].Destination)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool HasActiveFloorsBelow()
        {
            if (_currentWeight >= WeightLimit)
            {
                return HasActiveDestinationFloorsBelow();
            }
            else
            {
                for (int i = CurrentFloor - 2; i >= 0; i--)
                {
                    if (Floors[i] == true)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        private bool HasActiveDestinationFloorsBelow()
        {
            for (int i = CurrentFloor - 2; i >= 0; i--)
            {
                if (Floors[i] == true)
                {
                    for (int j = 0; j < _people.Count; i++)
                    {
                        if (i == _people[j].Destination)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }


        public void AddDestinationToPerson(int destination, Elevator elevator)
        {
            if (_people.Count == 0)
            {
                Console.WriteLine("There is nobody using the elevator!");
                return;
            }
            else if (destination > MaxFloorLevel || destination < MinFloorLevel)
            {
                Console.WriteLine("Invalid floor number!");
                return;
            }

            for (int i = 0; i < _people.Count; i++)
            {
                if (IsPersonInElevatorWithNoDestination(i))
                {
                    LogValidFloorRequest(destination.ToString());

                    _people[i].Destination = destination;

                    CheckIfApproachingRequestedFloor(destination);

                    if (CurrentDirectionTraveling == TravelDirections.None)
                    {
                        CheckActiveFloors(elevator);
                    }

                    return;
                }
            }

            Console.WriteLine("There is no one in the elevator in need of a destination!");
        }

        private bool IsPersonInElevatorWithNoDestination(int i)
        {
            return _people[i].PickupFloor == InElevator && _people[i].Destination == null;
        }


        public void AddPeopleToElevator()
        {
            for(int i = 0; i < _people.Count; i++)
            {
                if (_people[i].PickupFloor == CurrentFloor)
                {
                    _people[i].PickupFloor = InElevator;
                    _NumberOfPeopleInElevator++;
                    _currentWeight += Person.Weight;
                    CheckIfWeightLimitReached();
                    Console.WriteLine($"{_NumberOfPeopleInElevator} people inside the elevator");
                }
            }
        }

        private void CheckIfWeightLimitReached()
        {
            if (_currentWeight >= WeightLimit)
            {
                Console.WriteLine("Elevator reached weight limit!");
            }
        }

        public void RemovePeopleFromElevator()
        {
            for(int i = _people.Count - 1; i >= 0; i--)
            {
                if (_people[i].Destination == CurrentFloor)
                {
                    _people.RemoveAt(i);
                    _NumberOfPeopleInElevator--;
                    _currentWeight -= Person.Weight;
                    Console.WriteLine($"{_people.Count} total people are using the elevator\n{_NumberOfPeopleInElevator} people inside the elevator");
                }
            }
        }


        public bool IsValidPickupFloor()
        {
            if (Floors[CurrentFloor - 1] == false)
            {
                return false;
            }

            if (_currentWeight >= WeightLimit)
            {
                return false;
            }

            for (int i = 0; i < _people.Count; i++)
            {
                if (_people[i].PickupFloor != CurrentFloor)
                {
                    continue;
                }
                else if (IsHeadingInSameDirectionAsElevator(i))
                {
                    return true;
                }
                else if (CurrentDirectionTraveling == TravelDirections.Up && !HasActiveFloorsAbove())
                {
                    return true;
                }
                else if (CurrentDirectionTraveling == TravelDirections.Down && !HasActiveFloorsBelow())
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsHeadingInSameDirectionAsElevator(int i)
        {
            return _people[i].DesiredDirection == CurrentDirectionTraveling.ToString()[0];
        }


        public bool IsValidDestinationFloor()
        {
            if (Floors[CurrentFloor - 1] == false)
            {
                return false;
            }

            for (int i = 0; i < _people.Count; i++)
            {
                if (_people[i].Destination == CurrentFloor)
                {
                    return true;
                }
            }

            return false;
        }


        public void SetCurrentFloorToFalse()
        {
            Floors[CurrentFloor - 1] = false;
        }

        public void LogValidFloorRequest(string _floorRequest)
        {
            DateTime timeStamp = DateTime.UtcNow.ToLocalTime();
            File.AppendAllText($"{_fullPath}{textFileName.ToString("MMddyy_hhmmss")}.txt", $"[{timeStamp}] Floor Request: {_floorRequest}\n");
        }

        public void LogElevatorPass()
        {
            DateTime timeStamp = DateTime.UtcNow.ToLocalTime();
            File.AppendAllText($"{_fullPath}{textFileName.ToString("MMddyy_hhmmss")}.txt", $"[{timeStamp}] Elevator passed floor {CurrentFloor}\n");
        }

        public void LogElevatorStop()
        {
            DateTime timeStamp = DateTime.UtcNow.ToLocalTime();
            File.AppendAllText($"{_fullPath}{textFileName.ToString("MMddyy_hhmmss")}.txt", $"[{timeStamp}] Elevator stopped at floor {CurrentFloor}\n");
        }

    }



}
