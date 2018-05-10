using System;
using System.Threading;

namespace ProbabilityApp
{
    public class ExpManager
    {
        public enum OutputTypes
        {
            CSV =       0b00_0001,
            HTML =      0b00_0010,
            LATEX =     0b00_0100,
            TXT =       0b00_1000,
            CSVOrdered= 0b01_0000,
            INVALID =   0b10_0000
        }

        public static ExpManager sharedInstance = new ExpManager();

        public OutputTypes output = 0;
        public int outcomes = 2;
        public String folderPath = "";
        public int threads = 1;
        public int trials = 1;
        public bool orderedStorage = false;
        public EndCondition endCondition = new EndCondition();
        Results[] results;

        public event ExpEventHandler ExpNewNotification;
        public delegate void ExpEventHandler(String s);

        
        public void notify(String s)
        {
            ExpNewNotification.Invoke(s);
        }

        public void begin()
        {
            ExpNewNotification.Invoke("Experiment beginning.");
            results = new Results[trials];
            for (int j = 0; j < trials; j++)
            {
                WorkerObject[] workers = new WorkerObject[threads];
                Thread[] t = new Thread[threads - 1];
                for (int i = 0; i < threads; i++)
                {
                    if (i == 0)
                    {
                        workers[i] = new WorkerObject(new EndCondition(endCondition, (endCondition.repetitions / threads) + (endCondition.repetitions % threads)), outcomes);
                    }
                    else
                    {
                        workers[i] = new WorkerObject(new EndCondition(endCondition, endCondition.repetitions / threads), outcomes);
                    }
                    workers[i].WorkerEvent += new WorkerObject.WorkerEventHandler(notify);
                    if (i != threads - 1)
                    {
                        t[i] = new Thread(workers[i].begin);
                        t[i].Name = "" + (i + 1);
                        t[i].Start();
                    } else
                    {
                        workers[i].begin();
                    }
                }

                // wait for each other thread to finish
                bool done = false;
                while (!done)
                {
                    done = true;
                    foreach (WorkerObject w in workers) { done = done && w.done; }
                }

                // Interpret results
                results[j] = new Results(workers);
                results[j].interpret();
                

                workers = null;
                GC.Collect();

                notify("Finished trial " + (j + 1) + ".");
                Console.WriteLine("[EXPMGR] Finished trial " + (j + 1) + ".");
            }

            Filesaver f = new Filesaver(results);
            f.FilesaverNotification += new Filesaver.FilesaverNotificationHandler(notify);
            f.saveFiles();
            f.cleanup();
        }

        public ExpManager()
        {

        }
    }
}