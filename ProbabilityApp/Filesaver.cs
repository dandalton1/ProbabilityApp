using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Globalization;

namespace ProbabilityApp
{
    class Filesaver
    {
        private Results[] resultObjects;

        public event FilesaverNotificationHandler FilesaverNotification;
        public delegate void FilesaverNotificationHandler(String s);

        private void saveOrderedCSV(string path)
        {
            try
            {
                StreamWriter s = File.CreateText(path);
                long k = 0;
                long trial = 1;
                if (ExpManager.sharedInstance.orderedStorage)
                {
                    foreach (Results r in resultObjects)
                    {
                        s.WriteLine("Trial " + trial);
                        if (r.expAborted) { s.WriteLine("Experiment was aborted early due to low memory."); }
                        foreach (WorkerObject w in r.workerObjects)
                        {
                            foreach (ResultObject ro in w.outcomes)
                            {
                                for (int i = 0; i < ro.amount; i++)
                                {
                                    s.WriteLine(ro.roll + 1);
                                    k++;
                                }
                            }
                        }
                    }
                }
                else
                {
                    foreach (Results r in resultObjects)
                    {
                        s.WriteLine("Trial " + trial);
                        if (r.expAborted) { s.WriteLine("Experiment was aborted early due to low memory."); }
                        for (long i = 0; i < r.unorderedResults.Length; i++)
                        {
                            for (long j = 0; j < r.unorderedResults[i]; j++)
                            {
                                s.WriteLine(i + 1);
                                k++;
                            }
                        }
                    }
                }
                FilesaverNotification.Invoke("Saved ordered CSV file.");
                Console.WriteLine("[FILESAVER] Wrote ordered CSV out to " + path + ".");
                Console.WriteLine("[FILESAVER] Ordered CSV lines: " + k);
                s.Flush();
                s.Close();
                s.Dispose();
            }
            catch (Exception e)
            {
                FilesaverNotification.Invoke("Could not save Ordered CSV file. Reason: " + e.Message);
                Console.WriteLine("[EXCEPTION] " + e.Message);
            }
        }

        private void saveCSV(string path)
        {
            try
            {
                StreamWriter s = File.CreateText(path);
                for (int i = 0; i < resultObjects.Length; i++)
                {
                    s.WriteLine("Trial " + (i + 1));
                    if (resultObjects[i].expAborted)
                    {
                        s.WriteLine("Experiment was aborted early due to low memory.");
                    }
                    for (int k = 0; k < ExpManager.sharedInstance.outcomes; k++)
                    {
                        s.WriteLine("Outcome " + (k + 1) + "," + resultObjects[i].unorderedResults[k]);
                    }
                    s.WriteLine("Repetitions," + resultObjects[i].repCount);
                    s.WriteLine("Time taken (ms)," + resultObjects[i].timeTaken);
                    s.WriteLine("Reps/second," + resultObjects[i].repsPerSecond);
                    s.WriteLine();
                }
                FilesaverNotification.Invoke("Saved CSV file.");
                Console.WriteLine("[FILESAVER] Wrote CSV out to " + path + ".");
                s.Flush();
                s.Close();
                s.Dispose();
            }
            catch (Exception e)
            {
                FilesaverNotification.Invoke("Could not save CSV file. Reason: " + e.Message);
                Console.WriteLine("[EXCEPTION] " + e.Message);
            }
        }

        private void saveHTML(string path)
        {
            try
            {
                StreamWriter s = File.CreateText(path);
                Random rand = new Random(); // trust me i'm not using this for RNG for the experiments
                byte[] color = new byte[3];
                rand.NextBytes(color);
                NumberFormatInfo n = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                NumberFormatInfo nFloat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                n.NumberDecimalDigits = 0;
                s.WriteLine("<!DOCTYPE html>\n" +
                    "\n" +
                    "<html>\n" +
                    "<head>\n" +
                    "<title>" +
                    "Result File" +
                    "</title>\n" +
                    "<style>\n" +
                    "body {\n" +
                    "background-color: rgb(" + (color[0] * 1.5) + "," + (color[1] * 1.5) + "," + (color[2] * 1.5) + ");\n" +
                    "color: rgb(" + (color[0] * 0.5) + "," + (color[1] * 0.5) + "," + (color[2] * 0.5) + ");\n" +
                    "text-align: center;\n" +
                    "font-family: Arimo, sans-serif;\n" +
                    "}\n" +
                    "table {" +
                    "border: 1px solid black;" +
                    "margin: auto;" +
                    "}" +
                    "th {" +
                    "font-weight: bold;" +
                    "}" +
                    "hr {" +
                    "width: 66vw;" +
                    "}" +
                    "td, th {" +
                    "padding-left: 5vw;" +
                    "padding-right: 5vw;" +
                    "}" +
                    "</style>" +
                    "<link href=\"https://fonts.googleapis.com/css?family=Arimo\" rel=\"stylesheet\">" +
                    "</head>\n" +
                    "<body>" +
                    "<h1>Result of " + ExpManager.sharedInstance.trials + " Trials of a Computer-Generated Probability Experiment With " + ExpManager.sharedInstance.outcomes + " Outcomes, and End Condition: " + ExpManager.sharedInstance.endCondition.ToString() + "</h1>\n");
                if (ExpManager.sharedInstance.endCondition.condition == EndCondition.ConditionType.REPETITIONS)
                {
                    long totalReps = 0;
                    decimal totalTimeTaken = 0.0M;
                    decimal repsPerSec = 0.0M;
                    for (int i = 0; i < resultObjects.Length; i++)
                    {
                        totalReps += resultObjects[i].repCount;
                        totalTimeTaken += (decimal)resultObjects[i].timeTaken;
                    }
                    if (totalTimeTaken != 0)
                    {
                        repsPerSec = (totalReps * 1000) / totalTimeTaken;
                    }
                    else
                    {
                        repsPerSec = decimal.MaxValue;
                    }
                    s.WriteLine("<p>Total experiment repetitions: " + totalReps.ToString("n", n) + "</p>");
                    s.WriteLine("<p>Time taken for all trials, total, in HH:MM:SS format: " + TimeSpan.FromMilliseconds((double)totalTimeTaken).ToString() + "</p>");
                    s.WriteLine("<p>Average amount of repetitions per second: " + repsPerSec.ToString("n", nFloat) + "</p>");
                    s.WriteLine("<p>Ordered storage was " + ((ExpManager.sharedInstance.orderedStorage) ? "used" : "not used") + " in this experiment.</p>");
                    s.WriteLine("<p>CPU Info: " + ProcessorInfo.sharedInstance.CPUName + "; " + ProcessorInfo.sharedInstance.Cores + " cores/" + ProcessorInfo.sharedInstance.Threads + " threads, running at " + ProcessorInfo.sharedInstance.MHz + " MHz.</p>");
                    s.WriteLine("<p>" + ExpManager.sharedInstance.threads + " threads were used in this experiment.</p>");
                    if (resultObjects.Length != 1)
                    {
                        double[] deviations = { 0, 0, 0 };
                        double min = 0;
                        double max = 0;
                        for (int i = 0; i < resultObjects.Length; i++)
                        {
                            if (resultObjects[i].unorderedResults[0] > max)
                            {
                                max = resultObjects[i].unorderedResults[0];
                            }
                            else if (resultObjects[i].repCount < min)
                            {
                                min = resultObjects[i].unorderedResults[0];
                            }
                        }
                        foreach (Results r in resultObjects)
                        {
                            for (double i = 1; i < 4; i++)
                            {
                                if (Math.Abs(r.eValueDeviance[0] / r.sigma) < i)
                                {
                                    deviations[(int)i - 1]++;
                                }
                            }
                        }
                        for (int i = 0; i < 3; i++)
                        {
                            deviations[i] /= resultObjects.Length;
                        }
                        s.WriteLine("<br />"); // insert a newline before this info
                        s.WriteLine("<p>Percent of values of outcome 1 that were less than one standard deviation away: " + (deviations[0] * 100.0).ToString("n", nFloat) + "%</p>");
                        s.WriteLine("<p>Percent of values of outcome 1 that were less than two standard deviations away: " + (deviations[1] * 100.0).ToString("n", nFloat) + "%</p>");
                        s.WriteLine("<p>Percent of values of outcome 1 that were less than three standard deviations away: " + (deviations[2] * 100.0).ToString("n", nFloat) + "%</p>");
                    }
                    s.WriteLine("<h1>Results by Trial</h1>");
                    for (int i = 0; i < resultObjects.Length; i++)
                    {
                        s.WriteLine("<h2>Trial " + (i + 1) + "</h2>");
                        s.WriteLine("<p>Trial " + (i + 1) + " with end condition of: " + ExpManager.sharedInstance.endCondition.ToString() + "</p>");
                        if (resultObjects[i].expAborted)
                        {
                            s.WriteLine("<div class=\"warning\">\n" +
                                "<p>NOTICE: Experiment was aborted early due to low memory.</p>\n" +
                                "<p>To avoid this notice going forward: Either don't save with ordered storage, set the experiment to less repetitions, or upgrade your computer's RAM to handle the amount of experiments you're doing.</p>" +
                                "</div>");
                        }
                        s.WriteLine("<p>Expected value of x: " + resultObjects[i].eValue.ToString("n", nFloat) + "</p>");
                        s.WriteLine("<p>Variance of x: " + ExpectedValueFinder.Variance.ToString("n", nFloat) + "</p>");
                        s.WriteLine("<p>One standard deviation in this experiment: " + resultObjects[i].sigma.ToString("n", nFloat) + "</p>");
                        s.WriteLine("<table>");
                        s.WriteLine("<tr><th>Outcome</th><th>Amount of results</th><th>Deviation</th><th>Percent deviation</th><th>Deviation</th>");
                        for (int k = 0; k < ExpManager.sharedInstance.outcomes; k++)
                        {
                            s.WriteLine("<tr><td>" + (k + 1) + "</td><td>" + resultObjects[i].unorderedResults[k].ToString("n", n) + "</td><td>" + resultObjects[i].eValueDeviance[k].ToString("n", nFloat) + "</td><td>" + resultObjects[i].eValueDeviancePercent[k] + "%</td><td>" + (resultObjects[i].eValueDeviance[k] / resultObjects[i].sigma).ToString("n", nFloat) + "</td></tr>");
                        }
                        s.WriteLine("</table>");
                        s.WriteLine("<p>Repetitions: " + resultObjects[i].repCount.ToString("n", n) + "</p>");
                        s.WriteLine("<p>Time taken: " + (resultObjects[i].timeTaken / 1000.0).ToString("n", nFloat) + " seconds</p>");
                        s.WriteLine("<p>Reps/second: " + resultObjects[i].repsPerSecond.ToString("n", nFloat) + " repetitions per second</p><hr />");
                        s.WriteLine();
                    }
                    s.WriteLine("<h1>Deviations of outcome 1 from expected value as percentages</h1>");
                    for (int i = 0; i < resultObjects.Length; i++)
                    {
                        s.WriteLine("<p>" + resultObjects[i].eValueDeviancePercent[0] + "%</p>");
                    }
                }
                else if (ExpManager.sharedInstance.endCondition.condition == EndCondition.ConditionType.TIME_LIMIT)
                {
                    long totalReps = 0;
                    double totalTimeTaken = 0.0;
                    double repsPerSec = 0.0;
                    for (int i = 0; i < resultObjects.Length; i++)
                    {
                        totalReps += resultObjects[i].repCount;
                        totalTimeTaken += resultObjects[i].timeTaken;
                    }
                    if (totalTimeTaken != 0)
                    {
                        repsPerSec = (totalReps * 1000) / totalTimeTaken;
                    }
                    else
                    {
                        repsPerSec = double.PositiveInfinity;
                    }
                    double eValueReps = totalReps / resultObjects.Length;
                    s.WriteLine("<p>Total experiment repetitions: " + totalReps.ToString("n", n) + "</p>");
                    s.WriteLine("<p>Time taken for all trials, total, in HH:MM:SS format: " + TimeSpan.FromMilliseconds((double)totalTimeTaken).ToString() + "</p>");
                    s.WriteLine("<p>Average amount of repetitions per second: " + repsPerSec.ToString("n", nFloat) + "</p>");
                    s.WriteLine("<p>Ordered storage was " + ((ExpManager.sharedInstance.orderedStorage) ? "used" : "not used") + " in this experiment.</p>");
                    s.WriteLine("<p>CPU Info: " + ProcessorInfo.sharedInstance.CPUName + "; " + ProcessorInfo.sharedInstance.Cores + " cores/" + ProcessorInfo.sharedInstance.Threads + " threads, running at " + ProcessorInfo.sharedInstance.MHz + " MHz.</p>");
                    s.WriteLine("<p>" + ExpManager.sharedInstance.threads + " threads were used in this experiment.</p>");
                    double sigmaReps = 0;
                    double sigmaRPS = 0;
                    if (resultObjects.Length != 1)
                    {
                        double[] deviationsRPS = { 0, 0, 0 };
                        double[] deviationsReps = { 0, 0, 0 };
                        double minRPS = resultObjects[0].repsPerSecond;
                        double maxRPS = resultObjects[0].repsPerSecond;
                        double minReps = resultObjects[0].repCount;
                        double maxReps = resultObjects[0].repCount;
                        for (int i = 0; i < resultObjects.Length; i++)
                        {
                            sigmaReps += (resultObjects[i].repCount - eValueReps) * (resultObjects[i].repCount - eValueReps);
                            sigmaRPS += (resultObjects[i].repsPerSecond - repsPerSec) * (resultObjects[i].repsPerSecond - repsPerSec);
                            if (resultObjects[i].repCount > maxReps)
                            {
                                maxReps = resultObjects[i].repCount;
                            }
                            else if (resultObjects[i].repCount < minReps)
                            {
                                minReps = resultObjects[i].repCount;
                            }
                            if (resultObjects[i].repsPerSecond > maxRPS)
                            {
                                maxRPS = resultObjects[i].repsPerSecond;
                            }
                            else if (resultObjects[i].repsPerSecond < minRPS)
                            {
                                minRPS = resultObjects[i].repsPerSecond;
                            }
                        }
                        sigmaReps /= (resultObjects.Length - 1);
                        sigmaRPS /= (resultObjects.Length - 1);
                        sigmaReps = Math.Sqrt(sigmaReps);
                        sigmaRPS = Math.Sqrt(sigmaRPS);
                        s.WriteLine("<br />"); // insert a newline before this info
                        s.WriteLine("<p>Standard deviation of repetitions per trial: " + sigmaReps.ToString("n", nFloat) + "</p>");
                        s.WriteLine("<p>Maximum repetitions per trial: " + maxReps.ToString("n", nFloat) + "</p>");
                        s.WriteLine("<p>Minimum repetitions per trial: " + minReps.ToString("n", nFloat) + "</p>");
                        s.WriteLine("<br />"); // insert a newline before this info
                        s.WriteLine("<p>Standard deviation of repetitions per second: " + sigmaRPS.ToString("n", nFloat) + "</p>");
                        s.WriteLine("<p>Maximum repetitions per second: " + maxRPS.ToString("n", nFloat) + "</p>");
                        s.WriteLine("<p>Minimum repetitions per second: " + minRPS.ToString("n", nFloat) + "</p>");
                        foreach (Results r in resultObjects)
                        {
                            for (double i = 1; i < 4; i++)
                            {
                                if (Math.Abs(((eValueReps - r.repCount) / sigmaReps)) < i)
                                {
                                    deviationsReps[(int)i - 1]++;
                                }
                                if (Math.Abs((repsPerSec - r.repsPerSecond) / sigmaRPS) < i)
                                {
                                    deviationsRPS[(int)i - 1]++;
                                }
                            }
                        }
                        for (int i = 0; i < 3; i++)
                        {
                            deviationsReps[i] /= resultObjects.Length;
                            deviationsRPS[i] /= resultObjects.Length;
                        }
                        s.WriteLine("<br />"); // insert a newline before this info
                        s.WriteLine("<p>Percent of trial rep values that were less than one standard deviation away: " + (deviationsReps[0] * 100.0).ToString("n", nFloat) + "% </p>");
                        s.WriteLine("<p>Percent of trial rep values that were less than two standard deviations away: " + (deviationsReps[1] * 100.0).ToString("n", nFloat) + "% </p>");
                        s.WriteLine("<p>Percent of trial rep values that were less than three standard deviations away: " + (deviationsReps[2] * 100.0).ToString("n", nFloat) + "% </p>");
                        s.WriteLine("<br />"); // insert a newline before this info
                        s.WriteLine("<p>Percent of reps/sec values that were less than one standard deviation away: " + (deviationsRPS[0] * 100.0).ToString("n", nFloat) + "% </p>");
                        s.WriteLine("<p>Percent of reps/sec values that were less than two standard deviations away: " + (deviationsRPS[1] * 100.0).ToString("n", nFloat) + "% </p>");
                        s.WriteLine("<p>Percent of reps/sec values that were less than three standard deviations away: " + (deviationsRPS[2] * 100.0).ToString("n", nFloat) + "% </p>");
                        s.WriteLine("<br />");
                    }
                    s.WriteLine("<p>Ordered storage was " + ((ExpManager.sharedInstance.orderedStorage) ? "used" : "not used") + " in this experiment.</p>");
                    s.WriteLine("<p>CPU Info: " + ProcessorInfo.sharedInstance.CPUName + "; " + ProcessorInfo.sharedInstance.Cores + " cores/" + ProcessorInfo.sharedInstance.Threads + " threads, running at " + ProcessorInfo.sharedInstance.MHz + " MHz</p>");
                    s.WriteLine(ExpManager.sharedInstance.threads + " threads were used in this experiment.</p>");
                    s.WriteLine("<hr />");
                    for (int i = 0; i < resultObjects.Length; i++)
                    {
                        s.WriteLine("<h2>Trial " + (i + 1) + "</h2>");
                        s.WriteLine("<p>Trial " + (i + 1) + " with end condition of: " + ExpManager.sharedInstance.endCondition.ToString() + "</p>");
                        if (resultObjects[i].expAborted)
                        {
                            s.WriteLine("<div class=\"warning\">\n" +
                                "<p>NOTICE: Experiment was aborted early due to low memory.</p>\n" +
                                "<p>To avoid this notice going forward: Either don't save with ordered storage, set the experiment to less repetitions, or upgrade your computer's RAM to handle the amount of repetitions you're doing. </p>");
                        }
                        s.WriteLine("<p>Expected value of x, Variance of x, and standard deviation of x for each experiment cannot be calculated, as it is indeterminate how many repetitions will be done within " + ExpManager.sharedInstance.endCondition.seconds.ToString("n", n) + " seconds. </p>");
                        s.WriteLine("<table>");
                        s.WriteLine("<tr><th>Outcome</th><th>Amount of results</th></tr>\n");
                        for (int k = 0; k < ExpManager.sharedInstance.outcomes; k++)
                        {
                            s.WriteLine("<tr><td>" + (k + 1) + "</td><td>" + resultObjects[i].unorderedResults[k].ToString("n", n) + "</td></tr>");
                        }
                        s.WriteLine("</table>");
                        s.WriteLine("<p>Repetitions: " + resultObjects[i].repCount.ToString("n", n) + "</p>");
                        s.WriteLine("<p>Time taken: " + (resultObjects[i].timeTaken / 1000.0).ToString("n", nFloat) + " seconds</p>");
                        s.WriteLine("<p>Reps/second: " + resultObjects[i].repsPerSecond.ToString("n", nFloat) + " repetitions per second</p>");
                        s.WriteLine("<p>Deviance from mean amount of repetitions: " + (eValueReps - resultObjects[i].repCount).ToString("n", nFloat) + " </p>");
                        s.WriteLine("<p>Deviance from mean amount of reps/second: " + (repsPerSec - resultObjects[i].repsPerSecond).ToString("n", nFloat) + " </p>");
                        s.WriteLine("<p>Standard deviations from mean amount of repetitions: " + ((eValueReps - resultObjects[i].repCount) / sigmaReps).ToString("n", nFloat) + " </p>");
                        s.WriteLine("<p>Standard deviations from mean amount of reps/second: " + ((repsPerSec - resultObjects[i].repsPerSecond) / sigmaRPS).ToString("n", nFloat) + " </p>");
                        s.WriteLine();
                    }
                }
                else if (ExpManager.sharedInstance.endCondition.condition == EndCondition.ConditionType.PATTERN)
                {
                    long totalReps = 0;
                    double totalTimeTaken = 0.0;
                    double repsPerSec = 0.0;
                    for (int i = 0; i < resultObjects.Length; i++)
                    {
                        totalReps += resultObjects[i].repCount;
                        totalTimeTaken += resultObjects[i].timeTaken;
                    }
                    if (totalTimeTaken != 0)
                    {
                        repsPerSec = (totalReps * 1000) / totalTimeTaken;
                    }
                    else
                    {
                        repsPerSec = double.PositiveInfinity;
                    }
                    double eValueReps = totalReps / resultObjects.Length;
                    s.WriteLine("<p>Total experiment repetitions: " + totalReps.ToString("n", n) + "</p>");
                    s.WriteLine("<p>Time taken for all trials, total, in HH:MM:SS format: " + TimeSpan.FromMilliseconds((double)totalTimeTaken).ToString() + "</p>");
                    s.WriteLine("<p>Average amount of repetitions per trial: " + eValueReps.ToString("n", nFloat) + " </p>");
                    s.WriteLine("<p>Average amount of repetitions per second: " + repsPerSec.ToString("n", nFloat) + "</p>");
                    s.WriteLine("<p>Ordered storage was " + ((ExpManager.sharedInstance.orderedStorage) ? "used" : "not used") + " in this experiment.</p>");
                    s.WriteLine("<p>CPU Info: " + ProcessorInfo.sharedInstance.CPUName + "; " + ProcessorInfo.sharedInstance.Cores + " cores/" + ProcessorInfo.sharedInstance.Threads + " threads, running at " + ProcessorInfo.sharedInstance.MHz + " MHz.</p>");
                    s.WriteLine("<p>" + ExpManager.sharedInstance.threads + " threads were used in this experiment.</p>");
                    double sigmaReps = 0;
                    if (resultObjects.Length != 1)
                    {
                        double[] deviationsReps = { 0, 0, 0 };
                        double minReps = 0;
                        double maxReps = 0;
                        for (int i = 0; i < resultObjects.Length; i++)
                        {
                            sigmaReps += (resultObjects[i].repCount - eValueReps) * (resultObjects[i].repCount - eValueReps);
                            if (resultObjects[i].repCount > maxReps)
                            {
                                maxReps = resultObjects[i].repCount;
                            }
                            else if (resultObjects[i].repCount < minReps)
                            {
                                minReps = resultObjects[i].repCount;
                            }
                        }
                        sigmaReps /= (resultObjects.Length - 1);
                        sigmaReps = Math.Sqrt(sigmaReps);
                        s.WriteLine("<p>Standard deviation of repetitions per trial: " + sigmaReps.ToString("n", nFloat) + "</p>");
                        s.WriteLine("<p>Maximum repetitions per trial: " + maxReps.ToString("n", nFloat) + "</p>");
                        s.WriteLine("<p>Minimum repetitions per trial: " + minReps.ToString("n", nFloat) + "</p>");
                        foreach (Results r in resultObjects)
                        {
                            for (double i = 1; i < 4; i++)
                            {
                                if (Math.Abs(((eValueReps - r.repCount) / sigmaReps)) < i)
                                {
                                    deviationsReps[(int)i - 1]++;
                                }
                            }
                        }
                        for (int i = 0; i < 3; i++)
                        {
                            deviationsReps[i] /= resultObjects.Length;
                        }
                        s.WriteLine("<br />"); // insert a newline before this info
                        s.WriteLine("<p>Percent of trial rep values that were $<\\pm1\\sigma$ away: " + (deviationsReps[0] * 100.0).ToString("n", nFloat) + "% </p>");
                        s.WriteLine("<p>Percent of trial rep values that were $<\\pm2\\sigma$ away: " + (deviationsReps[1] * 100.0).ToString("n", nFloat) + "% </p>");
                        s.WriteLine("<p>Percent of trial rep values that were $<\\pm3\\sigma$ away: " + (deviationsReps[2] * 100.0).ToString("n", nFloat) + "% </p>");
                        s.WriteLine("<br />");
                    }
                    s.WriteLine("<h1>Results by trial</h1>");
                    for (int i = 0; i < resultObjects.Length; i++)
                    {
                        s.WriteLine("<h2>Trial " + (i + 1) + "</h2>");
                        s.WriteLine("<p>Trial " + (i + 1) + " with end condition of: " + ExpManager.sharedInstance.endCondition.ToString() + "</p>");
                        if (resultObjects[i].expAborted)
                        {
                            s.WriteLine("<div class=\"warning\">\n" +
                                "<p>NOTICE: Experiment was aborted early due to low memory. </p>\n" +
                                "<p>To avoid this notice going forward: Either don't save with ordered storage, set the experiment to less repetitions, or upgrade your computer's RAM to handle the amount of experiments you're doing. </p>");
                        }
                        s.WriteLine("<table>");
                        s.WriteLine("<tr><th>Outcome</th><th>Amount of results</th></tr>");
                        for (int k = 0; k < ExpManager.sharedInstance.outcomes; k++)
                        {
                            s.WriteLine("<tr><td>" + (k + 1) + "</td><td>" + resultObjects[i].unorderedResults[k].ToString("n", n) + "</td></tr>");
                        }
                        s.WriteLine("</table>");
                        s.WriteLine("<p>Repetitions: " + resultObjects[i].repCount.ToString("n", n) + "</p>");
                        s.WriteLine("<p>Time taken: " + (resultObjects[i].timeTaken / 1000.0).ToString("n", nFloat) + " seconds</p>");
                        s.WriteLine("<p>Reps/second: " + resultObjects[i].repsPerSecond.ToString("n", nFloat) + " repetitions per second</p>");
                        s.WriteLine("<p>Deviance from mean amount of repetitions: " + (eValueReps - resultObjects[i].repCount).ToString("n", nFloat) + " </p>");
                        s.WriteLine("<p>Standard deviations from mean amount of repetitions: " + ((eValueReps - resultObjects[i].repCount) / sigmaReps).ToString("n", nFloat) + " </p>");
                        s.WriteLine();
                    }
                }
                if (ExpManager.sharedInstance.orderedStorage)
                {
                    s.WriteLine("<h1>Ordered results</h1>");
                    for (int i = 0; i < resultObjects.Length; i++)
                    {
                        s.WriteLine("</h2>Trial " + (i + 1) + "</h2>");
                        s.Write("<p>");
                        foreach (WorkerObject w in resultObjects[i].workerObjects)
                        {
                            foreach (ResultObject ro in w.outcomes)
                            {
                                for (int j = 0; j < ro.amount; j++)
                                {
                                    s.Write(ro.roll + 1);
                                    s.Write(" ");
                                }
                            }
                        }
                        s.Write("</p>");
                    }
                }
                s.WriteLine("</body>" +
                    "</html>");
                FilesaverNotification.Invoke("Saved HTML file.");
                Console.WriteLine("[FILESAVER] Wrote HTML out to " + path + ".");
                s.Flush();
                s.Close();
                s.Dispose();
            } catch (Exception e)
            {
                FilesaverNotification.Invoke("Could not save HTML file. Reason: " + e.Message);
                Console.WriteLine("[EXCEPTION] " + e.Message);
            }
        }

        private void saveTXT(string path)
        {
            try
            {
                StreamWriter s = File.CreateText(path);
                for (int i = 0; i < resultObjects.Length; i++)
                {
                    s.WriteLine("Trial " + (i + 1));
                    if (resultObjects[i].expAborted)
                    {
                        s.WriteLine("Experiment was aborted early due to low memory.");
                    }
                    for (int k = 0; k < ExpManager.sharedInstance.outcomes; k++)
                    {
                        s.WriteLine("Outcome " + (k + 1) + "\t" + resultObjects[i].unorderedResults[k]);
                    }
                    s.WriteLine("Repetitions\t" + resultObjects[i].repCount);
                    s.WriteLine("Time taken (ms)\t" + resultObjects[i].timeTaken);
                    s.WriteLine("Reps/second\t" + resultObjects[i].repsPerSecond);
                    s.WriteLine();
                }
                FilesaverNotification.Invoke("Saved TXT file.");
                Console.WriteLine("[FILESAVER] Wrote TXT out to " + path + ".");
                s.Flush();
                s.Close();
                s.Dispose();
            }
            catch (Exception e)
            {
                FilesaverNotification.Invoke("Could not save CSV file. Reason: " + e.Message);
                Console.WriteLine("[EXCEPTION] " + e.Message);
            }
        }

        private void saveLaTeX(string path)
        {
            try
            {
                StreamWriter s = File.CreateText(path);
                s.WriteLine("\\documentclass{article}\n" +
                    "\\usepackage{amssymb}\n" +
                    "\\begin{document}\n");
                NumberFormatInfo n = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                NumberFormatInfo nFloat = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();
                n.NumberDecimalDigits = 0;
                Prompt.PromptResult promptResult = Prompt.ShowDialog("You need to enter your name so that the generated LaTeX output can have your name on it.", "Please enter your name.");
                s.WriteLine("\\begin{center}\n" +
                    "\\title{Result of " + ExpManager.sharedInstance.trials + " Trials of a Computer-Generated Probability Experiment With " + ExpManager.sharedInstance.outcomes + " Outcomes, and End Condition: " + ExpManager.sharedInstance.endCondition.ToString() + "}\n" +
                    "\\author{" + ((promptResult.dialogResult == System.Windows.Forms.DialogResult.OK) ? promptResult.response : Environment.UserName + "\\thanks{Author name generated from User Name on the machine running it. This may be incorrect.}") + "}\n" +
                    "\\date{" + DateTime.Now.ToLongDateString() + ", " + DateTime.Now.ToLongTimeString() + "}\n" +
                    "\\maketitle\n" +
                    "\\pagebreak\n" +
                    "\\tableofcontents\n" +
                    "\\pagebreak\n" +
                    "\\section*{Aggregate data}\n");
                if (ExpManager.sharedInstance.endCondition.condition == EndCondition.ConditionType.REPETITIONS)
                {
                    long totalReps = 0;
                    decimal totalTimeTaken = 0.0M;
                    decimal repsPerSec = 0.0M;
                    for (int i = 0; i < resultObjects.Length; i++)
                    {
                        totalReps += resultObjects[i].repCount;
                        totalTimeTaken += (decimal)resultObjects[i].timeTaken;
                    }
                    if (totalTimeTaken != 0)
                    {
                        repsPerSec = (totalReps * 1000) / totalTimeTaken;
                    }
                    else
                    {
                        repsPerSec = decimal.MaxValue;
                    }
                    s.WriteLine("Total experiment repetitions: " + totalReps.ToString("n", n) + "\\\\");
                    s.WriteLine("Time taken for all trials, total, in HH:MM:SS format: " + TimeSpan.FromMilliseconds((double)totalTimeTaken).ToString() + "\\\\");
                    s.WriteLine("Average amount of repetitions per second: " + repsPerSec.ToString("n", nFloat) + "\\\\");
                    s.WriteLine("Ordered storage was " + ((ExpManager.sharedInstance.orderedStorage) ? "used" : "not used") + " in this experiment.\\\\");
                    s.WriteLine("CPU Info: " + ProcessorInfo.sharedInstance.CPUName + "; " + ProcessorInfo.sharedInstance.Cores + " cores/" + ProcessorInfo.sharedInstance.Threads + " threads, running at " + ProcessorInfo.sharedInstance.MHz + " MHz.\\\\");
                    s.WriteLine(ExpManager.sharedInstance.threads + " threads were used in this experiment.\\\\");
                    if (resultObjects.Length != 1)
                    {
                        double[] deviations = { 0, 0, 0 };
                        double min = 0;
                        double max = 0;
                        for (int i = 0; i < resultObjects.Length; i++)
                        {
                            if (resultObjects[i].unorderedResults[0] > max)
                            {
                                max = resultObjects[i].unorderedResults[0];
                            }
                            else if (resultObjects[i].repCount < min)
                            {
                                min = resultObjects[i].unorderedResults[0];
                            }
                        }
                        foreach (Results r in resultObjects)
                        {
                            for (double i = 1; i < 4; i++)
                            {
                                if (Math.Abs(r.eValueDeviance[0] / r.sigma) < i)
                                {
                                    deviations[(int)i - 1]++;
                                }
                            }
                        }
                        for (int i = 0; i < 3; i++)
                        {
                            deviations[i] /= resultObjects.Length;
                        }
                        s.WriteLine("\\hfill \\break"); // insert a newline before this info
                        s.WriteLine("Percent of values of outcome 1 that were $<\\pm1\\sigma$ away: " + (deviations[0] * 100.0).ToString("n", nFloat) + "\\% \\\\");
                        s.WriteLine("Percent of values of outcome 1 that were $<\\pm2\\sigma$ away: " + (deviations[1] * 100.0).ToString("n", nFloat) + "\\% \\\\");
                        s.WriteLine("Percent of values of outcome 1 that were $<\\pm3\\sigma$ away: " + (deviations[2] * 100.0).ToString("n", nFloat) + "\\% \\\\");
                    }
                    s.WriteLine("\\end{center}\n" +
                        "\\pagebreak\n" +
                        "\\section{Results by trial}");
                    for (int i = 0; i < resultObjects.Length; i++)
                    {
                        s.WriteLine("\\subsection{Trial " + (i + 1) + "}");
                        s.WriteLine("Trial " + (i + 1) + " with end condition of: " + ExpManager.sharedInstance.endCondition.ToString() + "\\\\");
                        if (resultObjects[i].expAborted)
                        {
                            s.WriteLine("\\\\\n" +
                                "NOTICE: Experiment was aborted early due to low memory. \\\\\n" +
                                "To avoid this notice going forward: Either don't save with ordered storage, set the experiment to less repetitions, or upgrade your computer's RAM to handle the amount of experiments you're doing. \\\\");
                        }
                        s.WriteLine("$\\mathbb{E}$[x] = " + resultObjects[i].eValue.ToString("n", nFloat) + " \\\\");
                        s.WriteLine("Variance of x: " + ExpectedValueFinder.Variance.ToString("n", nFloat) + " \\\\");
                        s.WriteLine("$\\sigma$ in this experiment: " + resultObjects[i].sigma.ToString("n", nFloat) + " \\\\");
                        s.WriteLine("\\begin{tabular}{l|l|l|l|l}");
                        s.WriteLine("\\textit{Outcome} &\\textit{Amount of results} &\\textit{Deviation} &\\textit{Percent deviation} &\\textit{$\\sigma$ deviance} \\\\\n" +
                            "\\hline");
                        for (int k = 0; k < ExpManager.sharedInstance.outcomes; k++)
                        {
                            s.WriteLine((k + 1) + "&" + resultObjects[i].unorderedResults[k].ToString("n", n) + "&" + resultObjects[i].eValueDeviance[k].ToString("n", nFloat) + "&" + resultObjects[i].eValueDeviancePercent[k] + "\\%&" + (resultObjects[i].eValueDeviance[k] / resultObjects[i].sigma).ToString("n", nFloat) + "\\\\");
                        }
                        s.WriteLine("\\end{tabular}\n" +
                            "\\\\"); // force a newline in the document, that way it won't do some idiotic bullshit like move the repetitions text to the right of the table for no odd reason
                        s.WriteLine("Repetitions: " + resultObjects[i].repCount.ToString("n", n) + "\\\\");
                        s.WriteLine("Time taken: " + (resultObjects[i].timeTaken / 1000.0).ToString("n", nFloat) + " seconds\\\\");
                        s.WriteLine("Reps/second: " + resultObjects[i].repsPerSecond.ToString("n", nFloat) + " repetitions per second\\\\");
                        s.WriteLine();
                    }
                    s.WriteLine("\\section{Deviations of outcome 1 from expected value as percentages}");
                    for (int i = 0; i < resultObjects.Length; i++)
                    {
                        s.WriteLine(resultObjects[i].eValueDeviancePercent[0] + "\\% ");
                    }
                } else if (ExpManager.sharedInstance.endCondition.condition == EndCondition.ConditionType.TIME_LIMIT)
                {
                    long totalReps = 0;
                    double totalTimeTaken = 0.0;
                    double repsPerSec = 0.0;
                    for (int i = 0; i < resultObjects.Length; i++)
                    {
                        totalReps += resultObjects[i].repCount;
                        totalTimeTaken += resultObjects[i].timeTaken;
                    }
                    if (totalTimeTaken != 0)
                    {
                        repsPerSec = (totalReps * 1000) / totalTimeTaken;
                    } else
                    {
                        repsPerSec = double.PositiveInfinity;
                    }
                    double eValueReps = totalReps / resultObjects.Length;
                    s.WriteLine("Total experiment repetitions: " + totalReps.ToString("n", n) + "\\\\");
                    s.WriteLine("Time taken for all trials, total, in HH:MM:SS format: " + TimeSpan.FromMilliseconds(totalTimeTaken).ToString() + "\\\\");
                    s.WriteLine("Average amount of repetitions per second: " + repsPerSec.ToString("n", nFloat) + "\\\\");
                    s.WriteLine("Average amount of repetitions per trial: " + eValueReps.ToString("n", nFloat) + "\\\\");
                    double sigmaReps = 0;
                    double sigmaRPS = 0;
                    if (resultObjects.Length != 1)
                    {
                        double[] deviationsRPS = { 0, 0, 0 };
                        double[] deviationsReps = { 0, 0, 0 };
                        double minRPS = resultObjects[0].repsPerSecond;
                        double maxRPS = resultObjects[0].repsPerSecond;
                        double minReps = resultObjects[0].repCount;
                        double maxReps = resultObjects[0].repCount;
                        for (int i = 0; i < resultObjects.Length; i++)
                        {
                            sigmaReps += (resultObjects[i].repCount - eValueReps) * (resultObjects[i].repCount - eValueReps);
                            sigmaRPS += (resultObjects[i].repsPerSecond - repsPerSec) * (resultObjects[i].repsPerSecond - repsPerSec);
                            if (resultObjects[i].repCount > maxReps)
                            {
                                maxReps = resultObjects[i].repCount;
                            } else if (resultObjects[i].repCount < minReps)
                            {
                                minReps = resultObjects[i].repCount;
                            }
                            if (resultObjects[i].repsPerSecond > maxRPS)
                            {
                                maxRPS = resultObjects[i].repsPerSecond;
                            } else if (resultObjects[i].repsPerSecond < minRPS)
                            {
                                minRPS = resultObjects[i].repsPerSecond;
                            }
                        }
                        sigmaReps /= (resultObjects.Length - 1);
                        sigmaRPS /= (resultObjects.Length - 1);
                        sigmaReps = Math.Sqrt(sigmaReps);
                        sigmaRPS = Math.Sqrt(sigmaRPS);
                        s.WriteLine("\\hfill \\break"); // insert a newline before this info
                        s.WriteLine("$\\sigma$ repetitions per trial: " + sigmaReps.ToString("n", nFloat) + "\\\\");
                        s.WriteLine("Maximum repetitions per trial: " + maxReps.ToString("n", nFloat) + "\\\\");
                        s.WriteLine("Minimum repetitions per trial: " + minReps.ToString("n", nFloat) + "\\\\");
                        s.WriteLine("\\hfill \\break"); // insert a newline before this info
                        s.WriteLine("$\\sigma$ of repetitions per second: " + sigmaRPS.ToString("n", nFloat) + "\\\\");
                        s.WriteLine("Maximum repetitions per second: " + maxRPS.ToString("n", nFloat) + "\\\\");
                        s.WriteLine("Minimum repetitions per second: " + minRPS.ToString("n", nFloat) + "\\\\");
                        foreach (Results r in resultObjects)
                        {
                            for (double i = 1; i < 4; i++)
                            {
                                if (Math.Abs(((eValueReps - r.repCount) / sigmaReps)) < i)
                                {
                                    deviationsReps[(int)i - 1]++;
                                }
                                if (Math.Abs((repsPerSec - r.repsPerSecond) / sigmaRPS) < i)
                                {
                                    deviationsRPS[(int)i - 1]++;
                                }
                            }
                        }
                        for (int i = 0; i < 3; i++)
                        {
                            deviationsReps[i] /= resultObjects.Length;
                            deviationsRPS[i] /= resultObjects.Length;
                        }
                        s.WriteLine("\\hfill \\break"); // insert a newline before this info
                        s.WriteLine("Percent of trial rep values that were $<\\pm1\\sigma$ away: " + (deviationsReps[0] * 100.0).ToString("n", nFloat) + "\\% \\\\");
                        s.WriteLine("Percent of trial rep values that were $<\\pm2\\sigma$ away: " + (deviationsReps[1] * 100.0).ToString("n", nFloat) + "\\% \\\\");
                        s.WriteLine("Percent of trial rep values that were $<\\pm3\\sigma$ away: " + (deviationsReps[2] * 100.0).ToString("n", nFloat) + "\\% \\\\");
                        s.WriteLine("\\hfill \\break"); // insert a newline before this info
                        s.WriteLine("Percent of reps/sec values that were $<\\pm1\\sigma$ away: " + (deviationsRPS[0] * 100.0).ToString("n", nFloat) + "\\% \\\\");
                        s.WriteLine("Percent of reps/sec values that were $<\\pm2\\sigma$ away: " + (deviationsRPS[1] * 100.0).ToString("n", nFloat) + "\\% \\\\");
                        s.WriteLine("Percent of reps/sec values that were $<\\pm3\\sigma$ away: " + (deviationsRPS[2] * 100.0).ToString("n", nFloat) + "\\% \\\\");
                        s.WriteLine("\\hfill \\break");
                    }
                    s.WriteLine("Ordered storage was " + ((ExpManager.sharedInstance.orderedStorage) ? "used" : "not used") + " in this experiment.\\\\");
                    s.WriteLine("CPU Info: " + ProcessorInfo.sharedInstance.CPUName + "; " + ProcessorInfo.sharedInstance.Cores + " cores/" + ProcessorInfo.sharedInstance.Threads + " threads, running at " + ProcessorInfo.sharedInstance.MHz + " MHz\\\\");
                    s.WriteLine(ExpManager.sharedInstance.threads + " threads were used in this experiment.\\\\");
                    s.WriteLine("\\end{center}\n" +
                        "\\pagebreak");
                    for (int i = 0; i < resultObjects.Length; i++)
                    {
                        s.WriteLine("\\subsection{Trial " + (i + 1) + "}");
                        s.WriteLine("Trial " + (i + 1) + " with end condition of: " + ExpManager.sharedInstance.endCondition.ToString() + "\\\\");
                        if (resultObjects[i].expAborted)
                        {
                            s.WriteLine("\\\\\n" +
                                "NOTICE: Experiment was aborted early due to low memory. \\\\\n" +
                                "To avoid this notice going forward: Either don't save with ordered storage, set the experiment to less repetitions, or upgrade your computer's RAM to handle the amount of repetitions you're doing. \\\\");
                        }
                        s.WriteLine("$\\mathbb{E}$[x], Variance of x, and $\\sigma$ for each experiment cannot be calculated, as it is indeterminate how many repetitions will be done within " + ExpManager.sharedInstance.endCondition.seconds.ToString("n", n) + " seconds. \\\\");
                        s.WriteLine("\\begin{tabular}{l|l}");
                        s.WriteLine("\\textit{Outcome} &\\textit{Amount of results} \\\\\n" +
                            "\\hline");
                        for (int k = 0; k < ExpManager.sharedInstance.outcomes; k++)
                        {
                            s.WriteLine((k + 1) + "&" + resultObjects[i].unorderedResults[k].ToString("n", n) + "\\\\");
                        }
                        s.WriteLine("\\end{tabular}\n" +
                            "\\\\"); // force a newline in the document, that way it won't do some idiotic bullshit like move the repetitions text to the right of the table for no odd reason
                        s.WriteLine("Repetitions: " + resultObjects[i].repCount.ToString("n", n) + "\\\\");
                        s.WriteLine("Time taken: " + (resultObjects[i].timeTaken / 1000.0).ToString("n", nFloat) + " seconds\\\\");
                        s.WriteLine("Reps/second: " + resultObjects[i].repsPerSecond.ToString("n", nFloat) + " repetitions per second\\\\");
                        s.WriteLine("Deviance from mean amount of repetitions: " + (eValueReps - resultObjects[i].repCount).ToString("n", nFloat) + " \\\\");
                        s.WriteLine("Deviance from mean amount of reps/second: " + (repsPerSec - resultObjects[i].repsPerSecond).ToString("n", nFloat) + " \\\\");
                        s.WriteLine("$\\sigma$ from mean amount of repetitions: " + ((eValueReps - resultObjects[i].repCount) / sigmaReps).ToString("n", nFloat) + " \\\\");
                        s.WriteLine("$\\sigma$ from mean amount of reps/second: " + ((repsPerSec - resultObjects[i].repsPerSecond) / sigmaRPS).ToString("n", nFloat) + " \\\\");
                        s.WriteLine();
                    }
                } else if (ExpManager.sharedInstance.endCondition.condition == EndCondition.ConditionType.PATTERN)
                {
                    long totalReps = 0;
                    double totalTimeTaken = 0.0;
                    double repsPerSec = 0.0;
                    for (int i = 0; i < resultObjects.Length; i++)
                    {
                        totalReps += resultObjects[i].repCount;
                        totalTimeTaken += resultObjects[i].timeTaken;
                    }
                    if (totalTimeTaken != 0)
                    {
                        repsPerSec = (totalReps * 1000) / totalTimeTaken;
                    }
                    else
                    {
                        repsPerSec = double.PositiveInfinity;
                    }
                    double eValueReps = totalReps / resultObjects.Length;
                    s.WriteLine("Total experiment repetitions: " + totalReps.ToString("n", n) + "\\\\");
                    s.WriteLine("Time taken for all trials, total, in HH:MM:SS format: " + TimeSpan.FromMilliseconds((double)totalTimeTaken).ToString() + "\\\\");
                    s.WriteLine("Average amount of repetitions per trial: " + eValueReps.ToString("n", nFloat) + " \\\\");
                    s.WriteLine("Average amount of repetitions per second: " + repsPerSec.ToString("n", nFloat) + "\\\\");
                    s.WriteLine("Ordered storage was " + ((ExpManager.sharedInstance.orderedStorage) ? "used" : "not used") + " in this experiment.\\\\");
                    s.WriteLine("CPU Info: " + ProcessorInfo.sharedInstance.CPUName + "; " + ProcessorInfo.sharedInstance.Cores + " cores/" + ProcessorInfo.sharedInstance.Threads + " threads, running at " + ProcessorInfo.sharedInstance.MHz + " MHz.\\\\");
                    s.WriteLine(ExpManager.sharedInstance.threads + " threads were used in this experiment.\\\\");
                    double sigmaReps = 0;
                    if (resultObjects.Length != 1)
                    {
                        double[] deviationsReps = { 0, 0, 0 };
                        double minReps = 0;
                        double maxReps = 0;
                        for (int i = 0; i < resultObjects.Length; i++)
                        {
                            sigmaReps += (resultObjects[i].repCount - eValueReps) * (resultObjects[i].repCount - eValueReps);
                            if (resultObjects[i].repCount > maxReps)
                            {
                                maxReps = resultObjects[i].repCount;
                            }
                            else if (resultObjects[i].repCount < minReps)
                            {
                                minReps = resultObjects[i].repCount;
                            }
                        }
                        sigmaReps /= (resultObjects.Length - 1);
                        sigmaReps = Math.Sqrt(sigmaReps);
                        s.WriteLine("$\\sigma$ repetitions per trial: " + sigmaReps.ToString("n", nFloat) + "\\\\");
                        s.WriteLine("Maximum repetitions per trial: " + maxReps.ToString("n", nFloat) + "\\\\");
                        s.WriteLine("Minimum repetitions per trial: " + minReps.ToString("n", nFloat) + "\\\\");
                        foreach (Results r in resultObjects)
                        {
                            for (double i = 1; i < 4; i++)
                            {
                                if (Math.Abs(((eValueReps - r.repCount) / sigmaReps)) < i)
                                {
                                    deviationsReps[(int)i - 1]++;
                                }
                            }
                        }
                        for (int i = 0; i < 3; i++)
                        {
                            deviationsReps[i] /= resultObjects.Length;
                        }
                        s.WriteLine("\\hfill \\break"); // insert a newline before this info
                        s.WriteLine("Percent of trial rep values that were $<\\pm1\\sigma$ away: " + (deviationsReps[0] * 100.0).ToString("n", nFloat) + "\\% \\\\");
                        s.WriteLine("Percent of trial rep values that were $<\\pm2\\sigma$ away: " + (deviationsReps[1] * 100.0).ToString("n", nFloat) + "\\% \\\\");
                        s.WriteLine("Percent of trial rep values that were $<\\pm3\\sigma$ away: " + (deviationsReps[2] * 100.0).ToString("n", nFloat) + "\\% \\\\");
                        s.WriteLine("\\hfill \\break");
                    }
                    s.WriteLine("\\end{center}\n" +
                        "\\pagebreak\n" +
                        "\\section{Results by trial}");
                    for (int i = 0; i < resultObjects.Length; i++)
                    {
                        s.WriteLine("\\subsection{Trial " + (i + 1) + "}");
                        s.WriteLine("Trial " + (i + 1) + " with end condition of: " + ExpManager.sharedInstance.endCondition.ToString() + "\\\\");
                        if (resultObjects[i].expAborted)
                        {
                            s.WriteLine("\\\\\n" +
                                "NOTICE: Experiment was aborted early due to low memory. \\\\\n" +
                                "To avoid this notice going forward: Either don't save with ordered storage, set the experiment to less repetitions, or upgrade your computer's RAM to handle the amount of experiments you're doing. \\\\");
                        }
                        s.WriteLine("\\begin{tabular}{l|l}");
                        s.WriteLine("\\textit{Outcome} &\\textit{Amount of results}\\\\\n" +
                            "\\hline");
                        for (int k = 0; k < ExpManager.sharedInstance.outcomes; k++)
                        {
                            s.WriteLine((k + 1) + "&" + resultObjects[i].unorderedResults[k].ToString("n", n) + "\\\\");
                        }
                        s.WriteLine("\\end{tabular}\n" +
                            "\\\\"); // force a newline in the document, that way it won't do some idiotic bullshit like move the repetitions text to the right of the table for no odd reason
                        s.WriteLine("Repetitions: " + resultObjects[i].repCount.ToString("n", n) + "\\\\");
                        s.WriteLine("Time taken: " + (resultObjects[i].timeTaken / 1000.0).ToString("n", nFloat) + " seconds\\\\");
                        s.WriteLine("Reps/second: " + resultObjects[i].repsPerSecond.ToString("n", nFloat) + " repetitions per second\\\\");
                        s.WriteLine("Deviance from mean amount of repetitions: " + (eValueReps - resultObjects[i].repCount).ToString("n", nFloat) + " \\\\");
                        s.WriteLine("$\\sigma$ from mean amount of repetitions: " + ((eValueReps - resultObjects[i].repCount) / sigmaReps).ToString("n", nFloat) + " \\\\");
                        s.WriteLine();
                    }
                }
                if (ExpManager.sharedInstance.orderedStorage)
                {
                    s.WriteLine("\\section{Ordered results}");
                    for (int i = 0; i < resultObjects.Length; i++)
                    {
                        s.WriteLine("\\subsection{Trial " + (i + 1) + "}");
                        foreach (WorkerObject w in resultObjects[i].workerObjects)
                        {
                            foreach (ResultObject ro in w.outcomes)
                            {
                                for (int j = 0; j < ro.amount; j++)
                                {
                                    s.Write(ro.roll + 1);
                                    s.Write(" ");
                                }
                            }
                        }
                        s.WriteLine("\\\\");
                    }
                }
                s.WriteLine("\\end{document}");
                FilesaverNotification.Invoke("Saved LaTeX file.");
                Console.WriteLine("[FILESAVER] Wrote LaTeX out to " + path + ".");
                s.Flush();
                s.Close();
                s.Dispose();
            }
            catch (Exception e)
            {
                FilesaverNotification.Invoke("Could not save CSV file. Reason: " + e.Message);
                Console.WriteLine("[EXCEPTION] " + e.Message);
            }
        }

        public void saveFiles()
        {
            try
            {
                if (ExpManager.sharedInstance.output != ExpManager.OutputTypes.INVALID && ExpManager.sharedInstance.output != 0b00000)
                {
                    // actually save files
                    string p = Path.GetFullPath(ExpManager.sharedInstance.folderPath); // will throw exception if path is non-existent
                    string folderPath = Path.Combine(p, DateTime.Now.Month.ToString() + DateTime.Now.Day.ToString() + DateTime.Now.Year.ToString() + DateTime.Now.Hour.ToString() + DateTime.Now.Minute.ToString() + DateTime.Now.Second.ToString() + DateTime.Now.Millisecond.ToString() + "");
                    Directory.CreateDirectory(folderPath);
                    Console.WriteLine("[FILESAVER] Created directory " + folderPath + ".");

                    if ((ExpManager.sharedInstance.output & ExpManager.OutputTypes.CSVOrdered) != 0)
                    {
                        saveOrderedCSV(Path.Combine(folderPath, "Ordered Results.csv"));
                    }
                    if ((ExpManager.sharedInstance.output & ExpManager.OutputTypes.CSV) != 0)
                    {
                        saveCSV(Path.Combine(folderPath, "Results.csv"));
                    }
                    if ((ExpManager.sharedInstance.output & ExpManager.OutputTypes.TXT) != 0)
                    {
                        saveTXT(Path.Combine(folderPath, "Results.txt"));
                    }
                    if ((ExpManager.sharedInstance.output & ExpManager.OutputTypes.LATEX) != 0)
                    {
                        saveLaTeX(Path.Combine(folderPath, "Results.tex"));
                    }
                    if ((ExpManager.sharedInstance.output & ExpManager.OutputTypes.HTML) != 0)
                    {
                        saveHTML(Path.Combine(folderPath, "Results.html"));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("[EXCEPTION] " + e.Message);
            }
        }

        public void cleanup()
        {
            foreach (Results r in resultObjects) { r.cleanup(); }
        }

        public Filesaver(Results[] r)
        {
            resultObjects = r;
        }
    }
}
