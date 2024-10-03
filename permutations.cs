// Laura Bolduc
// February 25th 2023
// Assignment: parallel permutations using thread pools

using System;
using System;
using System.Diagnostics;
using System.IO;

namespace permutation;
public class permutations {
    private static string? JKVStep(string start) {
        // attribution (in Python, 2022): Joshua K. Veltri, Mitre Corporation
        //
        // Summary: start is a string of "digits" (actually, chars, but okay).
        // The algorithm implemented below finds the NEXT "higher value" that
        // can be represented using the same set of digits. So, "21458" would
        // be followed by "21485" then "21548", "21584", "21845", "21854" ...
        // until the sequence finally reaches "85421." Basically, we are just
        // "counting up" but only using the given set of digits. If we begin
        // with, say, "12458", the algoritm will traverse ALL permutations of
        // the five digits, ordered from lowest "12358" to highest "85421." A
        // consequence of the algoritm is that any duplicate permutations are
        // avoided. Seeding with "11112" will correctly generate the series
        // "11121", "11211", "12111" and finally '21111."
    
        // This algorithm arranges the digits "one step" towards a final
        // organization that represents  the highest "number" that can be
        // realized with the digits. Ie., all digits in descending order:
        // 		d[0] >= d[1] >= d[2] ... >= d[n-1].
        // For example, 12345 will eventually be reordered to 54321 through
        // a series of 120 (5!) rearrangements:
        //	 12345, 12354, 12435, 12453, 12534, 12543, ... 54312, 54321.
        // Follow the comments embedded in the code to understand how this
        // accomplished.

        // In the following documentation, assume we are given a starting
        // organization of five digits: 32541. The next higher valued
        // organization of the sequence is 34125.
        //
        // We use a loop; the indexed value digits[i] will be illustrated
        // by (), so when i == 1, digits[i] == 2 in the original configuration,
        // illustrated as 3(2)541. Digits to the right of the indexed value
        // will be referred to as "less significant" digits: 541.
        
        char[] digits = start.ToArray();

        // absolutely overkill, but demo of useful nested procedures allowed
        // in C#. Oh, how these were missed in Java!
        //
        void Swap(int to, int with) {
        char tmp = digits[to];
        digits[to] = digits[with];
        digits[with] = tmp;
        }

        // In the final (highest valued) config., all digits descend left to right.
        //
        // Begin by finding the rightmost digit out of order, ie., d[i] < d[i+1].
        // For example, given 32541, 2 < 5, so '2' is out of "final" order.
        // Keep this notion of "final order" in mind: 32541 will be stepped to
        // be closer to 54321.) If there is *no* digit out of order, that means 
        // we were given digits in the final order, so nothing to do here.

        // In this for loop: maxRight represents the highest value to the right
        // of the ith digit (loop is running right to left!). If a digit is
        // smaller than maxRight, the configuration is not in final order: some
        // digit right of the indexed digit is larger, so must be left of the
        // indexed digit:  3(2)541

        // initialize maxRight to be the right-most digit...
        char maxRight = digits[digits.Length-1];
        for (var i = digits.Length-2; i >= 0; i--) {
        char digit = digits[i];
        if (digit < maxRight) {
            // less significant (rightward) digits index
            var lessSignificant = i + 1;

            // find the smallest digit right of the indexed value that
            // is greater than the indexed digit. Why that specifically?
            // Because we are reaching for next higher order! If we were
            // starting at 3(2)541, swapping 1 for 2 would move backwards in
            // the series; swapping 5 for 2 would skip forward over all
            // steps of the series that are 34wxy. 
            //
            var repNdx = -1;
            for (var j = lessSignificant; j < digits.Length; j++)
            // find smallest less significant digit greater "the" digit
            if (digits[j] > digit &&
                (repNdx == -1 || digits[j] < digits[repNdx]))
                repNdx = j;

            // swap the indexed digit with its located replacement
            //
            Swap (repNdx, i);

            // Recapping: we have just gone from 3(2)wxy to 3(4)wxy.
            // We want to have all of the less significant (rightward)
            // digits in the lowest valued configuration, so we will
            // simply sort everything right of the indexed position to
            // get to 3(4)125.
            //
            Array.Sort(digits, lessSignificant, digits.Length-lessSignificant); 

            // we are done--we have rearraged digits to reach the next
            // higher-valued configuration in the series. We began with
            // 3(2)541 and have reached 3(4)125. Well done!
            //
            return new String(digits);
        }

        // if we got to here--remember the loop?--then everthing to the right
        // of and including the indexed digit is in order... keep searching
        // leftward
        //
        maxRight = digit;
        }
        
        // all digits are descending from the beginning--we were given the
        // highest valued configuration as input! We're done.
        //
        return null;
    }

    // make a small factorial method here to be used for indexing the array
    public static int fact(int n) {
        int result = 1;
        while (n != 1) {
            result = result * n;
            n = n - 1;
        }

        return result;
    }   

    // this version of GetPermutations uses a list
    public static List<String> GetPermuationsList (string permuteThis) {
        // create a new list of mres
        var mres = new List<ManualResetEvent>();
        // turn the given string into an array of characters so that we can sort it alphabetically
        var permChars = permuteThis.ToArray();  // permChars is char[] 
        // alpabitically sorted array
        Array.Sort(permChars); 
        // turn it back to string now so it can be passed to JKVstep
        var perm = new String(permChars);
        // empty list that will be the final list we return
        var perms = new List<string>();
        // empty List of lists to add each threads perm lists to
        // doing this makes the perms in alphabetical order
        var total = new List<List<string>>();

        // the for loop is making a new thread for each letter in the given string
        // each letter is the head once and the rest are the tail and will be passed to JKVstep
        for(var i = 0; i < perm.Length; i++) {
            // create an mre
            var mre = new ManualResetEvent(false); 
            // set a head
            var head = perm[i];
            // make the rest of the string the tail
            var tail = perm.Substring(0, i) + perm.Substring(i + 1);
            // create a temp list to add the perms from one thread to
            var temp = new List<string>();
            // add the perms from one head and tail to the list
            total.Add(temp);
            // add the mre to the list of mres
            mres.Add(mre);
            // QUWI
            ThreadPool.QueueUserWorkItem((object? args) => {
                (var head, var tail, var mre, var temp) = (ValueTuple<char, string, ManualResetEvent, List<string>>)args!;
            do {
                // first add the original head and tail to the list
                temp.Add(head + tail);
                // then pass just the tail to JKVstep and resave it to tail
                tail = JKVStep(tail);
            // only continue while tail is not null 
            } while (tail != null);
            // Set mre
            ((ManualResetEvent)(mre!)).Set(); 
            }, (head, tail, mre, temp));
        }
        // wait for all threads to finish
        WaitHandle.WaitAll(mres.ToArray());
    
        // loop through each list in the list of lists
        // add each list to the total list we made at the beginning
        // now that will be in order!
        foreach (var temp in total) {
            perms.AddRange(temp);
        }
        // return the total list
        return perms;
        
    }

    // this version of GetPermutations uses an array
    
    public static Array GetPermuationsArray (string permuteThis) {
        // create a new list of mres
        var mres = new List<ManualResetEvent>();
        // turn the given string into an array of characters so that we can sort it alphabetically
        var permChars = permuteThis.ToArray();  // permChars is char[] 
        // alpabitically sorted array
        Array.Sort(permChars); // alpabitically sorted array
        // turn it back to string now so it can be passed to JKVstep
        var perm = new String(permChars); 
        // empty array of strings that will hold the permutations
        // init this to how many permutations we will have (length of the string factorial)
        string[] total = new string[fact(permuteThis.Length)];

        // the for loop is making a new thread for each letter in the given string
        // each letter is the head once and the rest are the tail and will be passed to JKVstep
        for(int i = 0; i < perm.Length; i++) {
            // create an mre
            var mre = new ManualResetEvent(false);
            // set a head 
            var head = perm[i];
            // set the rest of the string to tail
            var tail = perm.Substring(0, i) + perm.Substring(i + 1);
            // set an index to keep track of where we are in the array
            var indx = i * fact(tail.Length);
            // now add the mre to the list of mres
            mres.Add(mre);
            // QUWI
            ThreadPool.QueueUserWorkItem((object? args) => {
                (var head, var tail, var mre, var indx) = (ValueTuple<char, string, ManualResetEvent, int>)args!;
            do {
                // at our current indx in the array, set it to the head + tail
                total[indx] = head + tail;
                // pass the tail to JKVstep
                tail = JKVStep(tail);
                // increase the index 
                indx++;
                // only while tail is not null
            } while (tail != null);
            // Set the mre
            ((ManualResetEvent)(mre!)).Set(); 
            }, (head, tail, mre, indx));
        
        }
        // wait for all threads to finish
        WaitHandle.WaitAll(mres.ToArray());

        // return the array
        return total;
        
    }
    

    public static void Main (string[] args) {
        // create a new stopwatch
        var timer = new Stopwatch();
        // set show to be a boolean value
        var show = 0; 
        // check to make sure that if there is a third agrument, it says "show"
        if (args.Length == 3){
            if(args[2] != "show"){
                // if the third arg is not "show" tell the user to try again
                Console.WriteLine(args[2] + " is not a valid third agrument. Please enter a valid third argument");
                System.Environment.Exit(0);
            }
            // if the third agrument was "show", set the boolean value to 1
            show = 1; // true
        }
        
        // make sure the user entered a type 
        if (args.Length < 2) {
            Console.WriteLine("Please enter a second argument (list or array).");
            System.Environment.Exit(0);
        }
    
        // check if first argument is a valid type
        if(args[1] != "array" && args[1]!= "list"){
            Console.WriteLine("Please enter a valid second argument (list or array).");
            System.Environment.Exit(0);
        }

        // if the main gets to here, this means we have a valid second argument and we know 
        // whether to print the perms or not
        // if the second argument is array:
        if (args[1] == "array"){
            // time our method
            timer.Start();
            // save to results
            var results = GetPermuationsArray(args[0]);
            timer.Stop();
            Console.WriteLine("Using the array method:");
            // if show == 1 then we print the perms
            if (show == 1) {
                foreach(string i in results) {
                    Console.WriteLine(i);
                }
            }
        }
        // if we get here we have already ruled out the second argument being invalid
        // and the second argument being array, so it's list
        if (args[1] == "list"){
            // time our method
            timer.Start();
            // save to results
            var results = GetPermuationsList(args[0]);
            timer.Stop();
            Console.WriteLine("Using the list method:");
            // if show == 1 then we print the perms
            if (show == 1) {
                foreach(string i in results) {
                    Console.WriteLine(i);
                }
            }
        }
        // print out the time it took
        Console.WriteLine(timer.Elapsed.TotalSeconds+"s"); 
    }
    
}