// Laura Bolduc
// Telephone Lab
// April 4th 2023
// Parallel and Distributed Processes

using System.Net;
using System.Net.Sockets;

public class Program {

    public static void BecomeClient(string serverIP, int port) {
        var client = new TcpClient();
        client.Connect(serverIP, port);
        var toServer = new StreamWriter(client.GetStream());
        var fromServer = new StreamReader(client.GetStream());
        Task.WaitAny(StartTasks(client));
        client.Close();
    }

    public static void BecomeServer(int port) {
        var server = new TcpListener(IPAddress.Any, port);
        server.Start();
        var client = server.AcceptTcpClient();
        var fromClient = new StreamReader(client.GetStream());
        var toClient = new StreamWriter(client.GetStream());
        Task.WaitAny(StartTasks(client));
        client.Close();
        server.Stop();
    }

    public static Task[] StartTasks(TcpClient client) {
        var stream = client.GetStream();
        var fromClient = new StreamReader(stream);
        var toClient = new StreamWriter(stream) {
            AutoFlush = true
        };
        var tr = Task.Run(() => {
            while(true) {
                var Text = fromClient.ReadLine();
                if(Text == null) {
                    break;
                }
                Console.WriteLine(Text); 
            }
        });
        var tw = Task.Run(() => {
            while(true) {
                var Text = Console.ReadLine();
                if(Text == null) {
                    break;
                }
                toClient.WriteLine(">>> {0}", Text); 
            }
        });
        return new Task[] {tr, tw};
        
    }

    public static void Main(string[] args) {
        if (args.Length == 1) {
            Console.WriteLine("Please enter all necessary feilds.");
        }
        var choice = args[0];
        if (choice == "server") {
            var two = Int32.Parse(args[1]);
            BecomeServer(two);
        } else if (choice == "client") {
            var two = args[1];
            var three = Int32.Parse(args[2]);
            BecomeClient(two, three);
        } else {
            Console.WriteLine("Please enter a valid first argument.");
        }
    }
}
