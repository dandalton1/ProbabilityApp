using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProbabilityApp
{
    class ExpectedValueFinder
    {
        public static double eValue = 0;
        public static double Variance = 0;

        public static String find()
        {
            switch (ExpManager.sharedInstance.endCondition.condition)
            {
                case EndCondition.ConditionType.INVALID: {
                        return "Invalid end condition";
                    }
                case EndCondition.ConditionType.PATTERN:
                    {
                        return "" + (Math.Pow(ExpManager.sharedInstance.outcomes, ExpManager.sharedInstance.endCondition.pattern.Length * ExpManager.sharedInstance.endCondition.patternRepetitions));
                    }
                case EndCondition.ConditionType.TIME_LIMIT:
                    {
                        return "Dependant on amount of repetitions reached";
                    }
                case EndCondition.ConditionType.REPETITIONS:
                    {
                        eValue = ExpManager.sharedInstance.endCondition.repetitions * (1 / (double)ExpManager.sharedInstance.outcomes);
                        Variance = ExpManager.sharedInstance.endCondition.repetitions * (1 / (double)ExpManager.sharedInstance.outcomes) * (1 - (1 / (double)ExpManager.sharedInstance.outcomes));
                        return "" + ExpManager.sharedInstance.endCondition.repetitions * (1 / (double) ExpManager.sharedInstance.outcomes) + " (for each outcome)";
                    }
                default:
                    {
                        return "This is an error. (Switch reached default state)";
                    }
            }
        }

        public static double findDouble()
        {
            switch (ExpManager.sharedInstance.endCondition.condition)
            {
                case EndCondition.ConditionType.INVALID:
                    {
                        return -1;
                    }
                case EndCondition.ConditionType.PATTERN:
                    {
                        return ExpManager.sharedInstance.endCondition.pattern.Length * ExpManager.sharedInstance.endCondition.patternRepetitions * ExpManager.sharedInstance.outcomes;
                    }
                case EndCondition.ConditionType.TIME_LIMIT:
                    {
                        return 0;
                    }
                case EndCondition.ConditionType.REPETITIONS:
                    {
                        eValue = ExpManager.sharedInstance.endCondition.repetitions * (1 / (double)ExpManager.sharedInstance.outcomes);
                        Variance = ExpManager.sharedInstance.endCondition.repetitions * (1 / (double)ExpManager.sharedInstance.outcomes) * (1 - (1 / (double)ExpManager.sharedInstance.outcomes));
                        return ExpManager.sharedInstance.endCondition.repetitions * (1 / (double)ExpManager.sharedInstance.outcomes);
                    }
                default:
                    {
                        return -1;
                    }
            }
        }

        public static double findDouble(double reps)
        {
            switch (ExpManager.sharedInstance.endCondition.condition)
            {
                case EndCondition.ConditionType.INVALID:
                    {
                        return -1;
                    }
                case EndCondition.ConditionType.PATTERN:
                    {
                        return Math.Pow(ExpManager.sharedInstance.outcomes, ExpManager.sharedInstance.endCondition.pattern.Length * ExpManager.sharedInstance.endCondition.patternRepetitions);
                    }
                case EndCondition.ConditionType.TIME_LIMIT:
                    {
                        return -1;
                    }
                case EndCondition.ConditionType.REPETITIONS:
                    {
                        return reps * (1 / (double)ExpManager.sharedInstance.outcomes);
                    }
                default:
                    {
                        return -1;
                    }
            }
        }
    }
}
