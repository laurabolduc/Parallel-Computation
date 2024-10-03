// Laura Bolduc
// Spring 2023 March 30th
// Parallel and Distributed proc
// Assignment 2: intersection

using System.Diagnostics;
using System.Collections.Concurrent;

// make a wrapper class to start everything

public class Wrapper {
    public enum Direction {
            NorthSouth,
            EastWest
    }
    // make a stopwatch
    static Stopwatch clock = new Stopwatch();

    // create the parking lot, static to access it everywhere
    private static ConcurrentQueue<Vehicle>? parkingLot = new ConcurrentQueue<Vehicle>();

    // random generator
    Random rand = new Random();

    // make a new intersection instance
    private static Intersection? theInter;

    // the wrapper constructor makes an intersection and populates the parkinglot
    // also makes the pilots
    // takes in the command line args, number of vehicles, pilots and a seed value
    public Wrapper(int n, int p, int seed) {
        // make the new intersection
        theInter = new Intersection();
        // start the clock for the whole program
        clock.Start();
        // start switching the light using the light switch method
        theInter.lightSwitch();
        
        // populate the parking lot with cars
        rand = new Random(seed);
        // make how every many cars you need, assign a random value for direction and put them in the parking lot
        for (int i = 0; i < n; i++) {
            if (rand.Next(0, 2) == 0) {
                // random 0 == northsouth
                Vehicle temp = new Vehicle(i, Direction.NorthSouth);
                parkingLot!.Enqueue(temp);
                
            } else {
                // the number is 1
                // 1 is eastwest direction
                Vehicle temp = new Vehicle(i, Direction.EastWest);
                parkingLot!.Enqueue(temp);
               
            }
        }

        // make however many pilots you need
        // each pilot is a thread that implements my big method
        // assign each pilot an id number
        for (int i = 0; i < p; i++) {
            Pilot hi = new Pilot(i); 
        }
    }

    // vehicle class
    public class Vehicle {

        // vehicle direction
        public Direction d {
            get;
            set;
        }
        
        // make a vehicleNumber
        public int vehicleNumber {
            get;
        }
        
        // make a constructor for vehicles that takes a number and direction
        
        public Vehicle(int n, Direction dir) {
            vehicleNumber = n;
            d = dir;
        }
    }

    // class intersection

    public class Intersection {  
        // make seperate streets
        public ConcurrentQueue<Vehicle> NSroad;

        public ConcurrentQueue<Vehicle> EWroad;

        // make a light direction
        public Direction lightDirection {
            get;
            set;
        }

        // in the constructor, initialize the roads, set the light starting north south
        public Intersection() {
            NSroad = new ConcurrentQueue<Vehicle>(); // N/S road

            EWroad = new ConcurrentQueue<Vehicle>(); // E/W road

            lightDirection = Direction.NorthSouth;
            
        }
        
        // light switch method uses a single thread to switch the light back and forth forever
        // used in the wrapper constructor when we made the intersection
        public void lightSwitch() {
            // single seperate thread
            Thread light_thread = new Thread((Object? args) => {
                // forever
                while (true) {
                    // if the direction is north south switch to east west and sleep 1000ms
                    if (lightDirection == Direction.NorthSouth) {
                        Thread.Sleep(1000); 
                        lightDirection = Direction.EastWest;
                        // print out the light changed
                        Console.WriteLine("Changed flow of direction to E/W at {0}s", (clock.ElapsedMilliseconds / 10));
                        
                    } else {
                        // else the direction was east west, switch it to north south and sleep 1000ms
                        Thread.Sleep(1000);
                        lightDirection = Direction.NorthSouth;
                        // print out that the light changed
                        Console.WriteLine("Changed flow of direction to N/S at {0}s", (clock.ElapsedMilliseconds / 10));
                        
                    }
                    // aquire the lock and pulse all to let the cars in the intersection know that there was a change, 
                    // and that they need to WAKE UP
                    lock(theInter!) {
                       Monitor.PulseAll(theInter!);
                    }
                    
                }
            });
            // start the single thread
            light_thread.Start();
        }
    }

    // an occupied flag that is true if a car is crossing the intersection
    // this is in the wrapper so everyone can see it
    public static bool occupied = false;
    

    // pilot class
    public class Pilot {

        // every pilot has an id
        public int pilotID {
            get;
        }
 
        // constructor 
        // every pilot has an id and is a thread that does the work of leaveLot
        // once a pilot is made it knows what to do
        public Pilot(int id) {
            pilotID = id;
            ThreadPool.QueueUserWorkItem(leaveLot);
        }

        // leave lot of the bulk of my do stuff
        // doesnt really take in an argument
        public void leaveLot(object? args) {
            // continue while the lot has not been emptied
            while (parkingLot!.Count > 0) {
                lock(this) {
                    // lock the pilots
                    // try to dequeue a car, saved as Vehicle car
                    Vehicle car;
                    parkingLot.TryDequeue(out car!);
                    // make a now and after time, save the now time now since that's the time
                    // the car was removed from the parking lot
                    var now = clock.ElapsedMilliseconds / 10;
                    // after will be saved later
                    long after;
                    // then check what direction the car needs to head in and add it to it's respective queue
                    if (car.d == Direction.NorthSouth) {
                        theInter!.NSroad.Enqueue(car);
                        // call the cross intersection method using the current car and queue
                        crossIntersection(car, theInter!.NSroad);
                        // now the car has crossed, save the after time
                        after = clock.ElapsedMilliseconds / 10;
                    } else {
                        // do the same thing but for the EW queue
                        theInter!.EWroad.Enqueue(car);
                        crossIntersection(car, theInter!.EWroad);
                        after = clock.ElapsedMilliseconds / 10;
                    }
                    // report!!
                    Console.WriteLine("Pilot {0} vehicle {1} left queue at {2}s crossed {3} at {4}s.", pilotID, car.vehicleNumber, now, car.d, after);
                    
                }
            }       
        }

        // cross intersection method does the 3 valid checks
        public void crossIntersection(Vehicle v, ConcurrentQueue<Vehicle> cq) {
            // this time we lock the intersection 
            lock(theInter!) {
                
                Vehicle car;
                // while the first car in the queue we're looking at is not the car we are currently dealing with
                // or while the intersection is occupied
                // or while the vehicle direction is not the way the light is pointing...
                while((cq.TryPeek(out car!) && v.vehicleNumber != car.vehicleNumber) || occupied || v.d != theInter!.lightDirection) {
                    // monitor.wait!!
                    Monitor.Wait(theInter!);

                }
                // breaking out of the while means all three are false, (which is good, the car can go)
                // dequeue the car and change the occupied flag, now the car is in the intersection
                cq!.TryDequeue(out car!);
                occupied = true;
                // give up lock
            }
            // sleep for 100ms to simulate 10s of the car crossing the intersection
            Thread.Sleep(100);

            // re-aquire the lock
            lock(theInter!) {
                // car has crossed the intersection now so change the occupied flag and pulseall to wake up the 
                // other waiting cars
                occupied = false;
                Monitor.PulseAll(theInter!);
            }
        }
    }
    
    // main is simple, just take in the arguments and make a new wrapper instance
    // making a wrapper starts everything for us, populates the parking lot, makes an intersection, starts the clock,
    // starts the light changing and makes the pilots which do the work
    public static void Main(String[] args) {
        // number of cars to cross the intersection
        var n = Int32.Parse(args[0]);

        // number of pilots driving the cars across the intersection
        var p = Int32.Parse(args[1]);

        // a seed value for testing
        var seed = Int32.Parse(args[2]);

        Wrapper gorham2 = new Wrapper(n, p, seed);

    }
}



