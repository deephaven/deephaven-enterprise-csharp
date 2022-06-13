using System;
using Deephaven.OpenAPI.Client;

namespace Deephaven.OpenAPI.ManualTests
{
    public static class IDS_7473
    {
        /// <summary>
        /// We put this little sandbox program here as a "convenient" way for the developer to test the behavior
        /// of the client library as we bring the PQ up and down while the code is running. The code assumes a PQ called
        /// TickingQuery set up at the server with this script (the exact query is not especially important):
        ///   ticks = db.timeTable("00:00:03").tail(10).update("A=new Random().nextInt(100)").update("B=new Random().nextInt(2)")
        ///
        /// The following scenarios should have the following behavior:
        /// Scenario 1: PQ down the whole time.
        ///    Exception will thrown at "POTENTIAL THROW LINE 1", caught in Main. Program exits.
        ///
        /// Scenario 2: PQ up the whole time.
        ///    Program runs normally. Operator needs to manually hit "enter" a few times to finish the program.
        ///
        /// Scenario 3. PQ up at program start, but stopped at "READ LINE 1".
        ///    Operator runs until READ LINE 1
        ///    Operator stops the PQ from the console
        ///    Operator presses Enter on READ LINE 1
        ///    Exception will be thrown at "POTENTIAL THROW LINE 2", caught at "CATCH LINE 1".
        ///    Then operator presses enter at "READ LINE 2".
        ///    Exception thrown at "POTENTIAL THROW LINE 3"
        ///
        /// Scenario 4. PQ up at program start, but stopped at "READ LINE 1", but then restarted at "READ LINE 2"
        ///    Operator runs until READ LINE 1
        ///    Operator stops the PQ from the console, then presses Enter
        ///    Exception will be thrown at "POTENTIAL THROW LINE 2", caught at "CATCH LINE 1".
        ///    Operator starts the PQ again from the console, waits a moment
        ///    Operator presses Enter at "READ LINE 2".
        ///    Code continues running until end
        /// </summary>
        public static void CrashPQ(string host, string username, string password, string operateAs)
        {
            using (var client = OpenApi.Connect(host))
            {
                client.Login(username, password, operateAs);

                Console.WriteLine("*** THIS IS POTENTIAL THROW LINE 1");
                using (var workerSession = client.AttachWorkerByName("TickingQuery"))
                {
                    try
                    {
                        var scope = workerSession.QueryScope;
                        var table = scope.BoundTable("ticks");
                        var t2 = table.Preemptive(1000);
                        t2.OnTableUpdate += PrintUtils.ShowTableUpdate;
                        t2.SubscribeAll();

                        Console.WriteLine("*** THIS IS READ LINE 1. In Scenario 3 you would stop the PQ here. Then hit enter: ");
                        Console.ReadLine();
                        Console.WriteLine("*** THIS IS POTENTIAL THROW LINE 2 ***");
                        var t5 = scope.EmptyTable(10, new String[0], new String[0]).Update("X = 12");
                        PrintUtils.PrintTableData(t5);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"*** THIS IS CATCH LINE 1. Caught exception {e}");
                    }
                }
                Console.WriteLine("*** THIS IS READ LINE 2. In Scenario 4 (but not 3) you would restart the PQ here. Then hit enter: ");
                Console.ReadLine();
                Console.WriteLine("*** THIS IS POTENTIAL THROW LINE 3 ***");
                using (var workerSession = client.AttachWorkerByName("TickingQuery"))
                {
                    var scope = workerSession.QueryScope;
                    var table = scope.BoundTable("ticks");
                    var t2 = table.Preemptive(1000);
                    t2.OnTableUpdate += PrintUtils.ShowTableUpdate;
                    t2.SubscribeAll();

                    Console.WriteLine("*** THIS IS READ LINE 3 *** Please hit enter: ");
                    Console.ReadLine();
                }
            }
            Console.WriteLine("You have reached the end of the manual IDS-4743 test!");
        }
    }
}
