using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProbabilityApp
{
    class WorkerObject
    {
        private EndCondition endCondition;
        private uint amountOfOutcomes;
        public List<ResultObject> outcomes;
        public long[] unorderedOutcomes;
        public bool done = false;
        public bool abort = false;
        public DateTime startTime;
        public DateTime endTime;
        public uint x, y, z, w, t;

        public delegate void WorkerEventHandler(String s);
        public event WorkerEventHandler WorkerEvent;

        public void cleanup()
        {
            if (ExpManager.sharedInstance.orderedStorage)
            {
                outcomes.Clear();
                outcomes.TrimExcess();
            } else
            {
                unorderedOutcomes = null;
            }
            endCondition = null;
        }

        public void begin()
        {
            x = (uint) (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() >> 32);
            y = (uint) (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            z = (uint) (DateTimeOffset.UtcNow.ToUnixTimeSeconds() >> 32);
            w = (uint) (DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            startTime = DateTime.Now;
            switch (endCondition.condition)
            {
                case EndCondition.ConditionType.PATTERN:
                    {
                        beginPattern();
                        break;
                    }
                case EndCondition.ConditionType.REPETITIONS:
                    {
                        beginReps();
                        break;
                    }
                case EndCondition.ConditionType.TIME_LIMIT:
                    {
                        beginTime();
                        break;
                    }
            }
            endTime = DateTime.Now;
            done = true;
        }

        private void beginTime()
        {
            DateTime endTime = DateTime.UtcNow.AddSeconds((double) ExpManager.sharedInstance.endCondition.seconds);
            while (DateTime.UtcNow.CompareTo(endTime) <= 0)
            {
                storeResult(diceRoll());
            }
        }

        private void beginReps()
        {
            if (ExpManager.sharedInstance.orderedStorage)
            {
                outcomes = new List<ResultObject>((int)endCondition.repetitions / 16);
            }
            for (long i = 0; i < endCondition.repetitions; i++)
            {
                storeResult(diceRoll());
                if (abort) { break; }
            }
        }

        private void beginPattern()
        {
            long i = 0;
            int j = 0;
            uint k = 0;
            while (i < endCondition.patternRepetitions)
            {
                while (j < endCondition.pattern.Length)
                {
                    k = diceRoll();
                    storeResult(k);
                    if (abort) { break; }
                    if (k == endCondition.pattern[j] - 1)
                    {
                        j++;
                    } else
                    {
                        j = 0;
                    }
                }
                i++;
                if (abort) { break; }
            }
        }

        private uint diceRoll()
        {
            t = x ^ (x << 11);
            x = y; y = z; z = w;
            return ((w = w ^ (w >> 19) ^ t ^ (t >> 8))) % (amountOfOutcomes);
        }

        private void storeResult(uint result)
        {
            if (ExpManager.sharedInstance.orderedStorage)
            {
                if (outcomes.Count == 0)
                {
                    ResultObject r = new ResultObject
                    {
                        amount = 1,
                        roll = result
                    };
                    outcomes.Add(r);
                } else if (result == outcomes.Last().roll)
                {
                    ResultObject r = outcomes.Last();
                    outcomes.RemoveAt(outcomes.Count - 1);
                    ++r.amount;
                    outcomes.Add(r);
                } else
                {
                    ResultObject r = new ResultObject
                    {
                        amount = 1,
                        roll = result
                    };
                    try
                    {
                        outcomes.Add(r);
                    } catch (OutOfMemoryException)
                    {
                        outcomes.TrimExcess();
                        try
                        {
                            outcomes.Add(r);
                        } catch (OutOfMemoryException)
                        {
                            WorkerEvent.Invoke("Thread " + System.Threading.Thread.CurrentThread.Name + " ran out of memory and garbage collection failed. Aborting thread.");
                            abort = true;
                        }
                    }
                }
            } else
            {
                ++unorderedOutcomes[result];
            }
        }

        public WorkerObject(EndCondition e, int o)
        {
            endCondition = e;
            amountOfOutcomes = (uint) o;
            if (ExpManager.sharedInstance.orderedStorage)
            {
                outcomes = new List<ResultObject>();
            } else
            {
                unorderedOutcomes = new long[o];
            }
        }
    }

    public struct ResultObject
    {
        public uint roll;
        public byte amount;
    }
}
