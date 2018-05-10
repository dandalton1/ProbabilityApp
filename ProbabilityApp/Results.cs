using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ProbabilityApp
{
    class Results
    {
        public WorkerObject[] workerObjects;
        public long repCount = 0;
        public bool orderedStorage;
        public double timeTaken = 0.0;
        public double repsPerSecond = 0.0;
        public double eValue;
        public double[] eValueDeviance;
        public double[] eValueDeviancePercent;
        public double sigma;
        public long[] unorderedResults;
        public bool expAborted = false;

        public long getRepCount() { return repCount; }

        public void cleanup()
        {
            foreach (WorkerObject w in workerObjects)
            {
                w.cleanup();
            }
            workerObjects = null;
        }

        private void calcRepCount()
        {
            if (orderedStorage)
            {
                repCount = 0;
                foreach (WorkerObject w in workerObjects)
                {
                    foreach (ResultObject r in w.outcomes)
                    {
                        repCount += r.amount;
                    }
                }
            } else
            {
                repCount = 0;
                foreach (WorkerObject w in workerObjects)
                {
                    foreach (long i in w.unorderedOutcomes)
                    {
                        repCount += i;
                    }
                }
            }
            Console.WriteLine("[RESULTS] RepCount: " + repCount);
        }

        private void calcTimeTaken()
        {
            timeTaken = 0.0;
            foreach (WorkerObject w in workerObjects)
            {
                timeTaken += w.endTime.Subtract(w.startTime).TotalMilliseconds;
            }
            timeTaken /= workerObjects.Length;
            Console.WriteLine("[RESULTS] Time taken: " + timeTaken + " ms");
        }

        private void calcRPS()
        {
            repsPerSecond = repCount / (timeTaken / 1000.0);
            Console.WriteLine("[RESULTS] Reps per second: " + repsPerSecond);
        }

        private void calcUnorderedResults()
        {
            unorderedResults = new long[ExpManager.sharedInstance.outcomes];
            for (int i = 0; i < unorderedResults.Length; i++) { unorderedResults[i] = 0; }
            if (orderedStorage)
            {
                for (long i = 0; i < workerObjects.LongLength; i++)
                {
                    foreach (ResultObject r in workerObjects[i].outcomes)
                    {
                        unorderedResults[r.roll] += r.amount;
                    }
                }
            }
            else
            {
                for (long i = 0; i < workerObjects.LongLength; i++)
                {
                    for (int j = 0; j < workerObjects[i].unorderedOutcomes.Length; j++)
                    {
                        unorderedResults[j] += workerObjects[i].unorderedOutcomes[j];
                    }
                }
            }
        }

        private void calcRepEValueDeviance()
        {
            eValue = ExpectedValueFinder.findDouble(repCount);
            sigma = Math.Sqrt(ExpectedValueFinder.Variance);
            eValueDeviance = new double[ExpManager.sharedInstance.outcomes];
            eValueDeviancePercent = new double[ExpManager.sharedInstance.outcomes];
            for (int i = 0; i < unorderedResults.Length; i++)
            {
                eValueDeviance[i] = eValue - unorderedResults[i];
                eValueDeviancePercent[i] = eValueDeviance[i] / eValue;
                eValueDeviancePercent[i] *= 100.0;
            }
        }

        private void chkAborted()
        {
            foreach (WorkerObject w in workerObjects)
            {
                expAborted = expAborted || w.abort;
            }
        }

        public void interpret()
        {
            orderedStorage = ExpManager.sharedInstance.orderedStorage; // a fancy alias
            calcRepCount();
            calcTimeTaken();
            calcRPS();
            calcUnorderedResults();
            chkAborted();
            if (ExpManager.sharedInstance.endCondition.condition == EndCondition.ConditionType.REPETITIONS)
            {
                calcRepEValueDeviance();
            }
        }

        public Results(WorkerObject[] workers)
        {
            workerObjects = workers;
        }
    }
}
