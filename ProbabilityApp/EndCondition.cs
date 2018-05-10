using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

public class EndCondition
{
    public enum ConditionType
    {
        REPETITIONS,
        PATTERN,
        TIME_LIMIT,
        INVALID
    }

    public ConditionType condition;
    public int[] pattern = new int[0];
    public long repetitions = 1000;
    public long patternRepetitions = 1;
    public decimal seconds = 60.0M;

    public void addPatternItem(int item)
    {
        int[] cp = new int[pattern.Length + 1];
        for (int i = 0; i < pattern.Length; i++)
        {
            cp[i] = pattern[i];
        }

        cp[cp.Length - 1] = item;

        pattern = cp;
    }

    public void rmPatternItem(int index)
    {
        Console.WriteLine("Index to be removed: " + index);
        if (pattern.Length == 0)
        {
            MessageBox.Show("ERROR! Cannot remove item from empty box!", "no box", MessageBoxButtons.OK, MessageBoxIcon.Error);
            Console.WriteLine("TRIED TO REMOVE NON EXISTENT ITEM!");
        } else
        {
            int[] cp = new int[pattern.Length - 1];
            bool flag = false;
            for (int i = 0; i < pattern.Length; i++)
            {
                if (i == index)
                {
                    flag = true;
                    continue;
                }
                if (flag)
                {
                    cp[i - 1] = pattern[i];
                } else
                {
                    cp[i] = pattern[i];
                }
            }
            pattern = cp;
        }
    }

    public void dispPattern()
    {
        Console.Write("Pattern: [");
        for (int i = 0; i < pattern.Length; i++)
        {
            Console.Write(pattern[i] + ", ");
        }
        Console.Write("]");
        Console.WriteLine();
    }

    public EndCondition() {
        condition = ConditionType.INVALID;
    }

    public EndCondition(EndCondition e, long o)
    {
        condition = e.condition;
        pattern = e.pattern;
        repetitions = o;            // Swap reps with o
        patternRepetitions = o;     // Also swap patternreps with o
        seconds = e.seconds;
    }

    public override String ToString()
    {
        String s = "";

        switch (condition)
        {
            case ConditionType.INVALID:
                {
                    s = "Invalid end condition";
                    break;
                }
            case ConditionType.PATTERN:
                {
                    s = "End on pattern, ";
                    s += " pattern repeats " + patternRepetitions + " times, ";
                    s += "pattern: [";
                    for (int i = 0; i < pattern.Length; i++)
                    {
                        s += pattern[i];
                        if (i != pattern.Length - 1)
                        {
                            s += ", ";
                        }
                    }
                    s += "]";
                    break;
                }
            case ConditionType.TIME_LIMIT:
                {
                    s = "End after " + seconds + " seconds.";
                    break;
                }
            case ConditionType.REPETITIONS:
                {
                    NumberFormatInfo n = (NumberFormatInfo) CultureInfo.CurrentCulture.NumberFormat.Clone();
                    n.NumberDecimalDigits = 0;
                    s = "End after " + repetitions.ToString("n", n) + " repetitions.";
                    break;
                }
            default:
                {
                    s = "Invalid end condition (error, not ConditionType.INVALID)";
                    break;
                }
        }

        return s;
    }
}