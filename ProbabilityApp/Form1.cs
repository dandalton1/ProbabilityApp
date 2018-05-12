using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Deployment.Application;
using System.Windows.Forms;

namespace ProbabilityApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            autoThreadLabel.Text = "The amount of logical cores in your system detected are: " + Environment.ProcessorCount;
            ExpManager.sharedInstance.threads = Environment.ProcessorCount;
            ExpManager.sharedInstance.ExpNewNotification += new ExpManager.ExpEventHandler(notify);
            numericUpDown2.Value = Environment.ProcessorCount;
            cpuLabel.Text = "CPU Name: " + ProcessorInfo.sharedInstance.CPUName;
            ctLabel.Text = "Cores/Threads: " + ProcessorInfo.sharedInstance.Cores + "C/" + ProcessorInfo.sharedInstance.Threads + "T";
            clockLabel.Text = "Clock Speed: " + ProcessorInfo.sharedInstance.MHz + " MHz";
            VersionLabel.Text = "Version 1.0.0.2 - By Daniel Dalton.";
            AcceptButton = button1;
            F_FormClosed(null, null);
        }

        private void threadedEValueUpdater()
        {
            Console.WriteLine("Finding EValue...");
            String eValue = ExpectedValueFinder.find();
            Console.WriteLine("EValue: " + eValue);
            ExpectedValueLabel.Text = "Expected value: " + eValue;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            ExpManager.sharedInstance.outcomes = (int)numericUpDown1.Value;
            Console.WriteLine("AMOUNT OF OUTCOMES: " + ExpManager.sharedInstance.outcomes);
            threadedEValueUpdater();
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("Experiment files will be stored in a subfolder with the current date/time.\n" +
                "For example, if the date was April 19, 2018, and the experiment was run at 1:34 PM,\n" +
                "With 42.3975 milliseconds following that minute,\n" +
                "The output would be placed in a subfolder called \"Thursday, April 19, 2018 133423975\".\n" +
                "This will allow us to quickly do multiple experiments, without having to choose different parent folders.\n\n" +
                "If multiple trials of this experiment are specified, just one folder will be created, and each trial\n" +
                "of the experiment will be concatenated into one document of type(s) specified.", "More info");
        }

        private void checkedListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            ExpManager.sharedInstance.output = 0;
            foreach (int i in checkedListBox1.CheckedIndices)
            {
                switch (i)
                {
                    case 0:
                        {
                            ExpManager.sharedInstance.output = ExpManager.sharedInstance.output | ExpManager.OutputTypes.CSVOrdered;
                            break;
                        }
                    case 1:
                        {
                            ExpManager.sharedInstance.output = ExpManager.sharedInstance.output | ExpManager.OutputTypes.CSV;
                            break;
                        }
                    case 2:
                        {
                            ExpManager.sharedInstance.output = ExpManager.sharedInstance.output | ExpManager.OutputTypes.HTML;
                            break;
                        }
                    case 3:
                        {
                            ExpManager.sharedInstance.output = ExpManager.sharedInstance.output | ExpManager.OutputTypes.LATEX;
                            break;
                        }
                    case 4:
                        {
                            ExpManager.sharedInstance.output = ExpManager.sharedInstance.output | ExpManager.OutputTypes.TXT;
                            break;
                        }
                    default:
                        {
                            ExpManager.sharedInstance.output = ExpManager.sharedInstance.output | ExpManager.OutputTypes.INVALID;
                            break;
                        }
                }
            }
            Console.WriteLine("OUTPUT SETTINGS (DEC): " + ExpManager.sharedInstance.output);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            ExpManager.sharedInstance.folderPath = folderBrowserDialog1.SelectedPath;
            label3.Text = "Folder to save information in: " + folderBrowserDialog1.SelectedPath;
            Console.WriteLine("FOLDER PATH: " + folderBrowserDialog1.SelectedPath);
        }

        private EndConditionChooser f;

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            f = new EndConditionChooser();
            f.Activate();
            f.Visible = true;
            f.FormClosed += F_FormClosed;
        }

        private void F_FormClosed(object sender, FormClosedEventArgs e)
        {
            threadedEValueUpdater();
            label1.Text = "End condition: " + ExpManager.sharedInstance.endCondition.ToString();
            if (ExpManager.sharedInstance.endCondition.condition == EndCondition.ConditionType.PATTERN)
            {
                numericUpDown2.Value = 1;
                numericUpDown2.Enabled = false;
                ExpManager.sharedInstance.threads = 1;
            } else
            {
                numericUpDown2.Enabled = true;
                numericUpDown2.Value = Environment.ProcessorCount;
                ExpManager.sharedInstance.threads = Environment.ProcessorCount;
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (f != null)
            {
                f.Close();
            }
            if (ExpManager.sharedInstance.endCondition.condition != EndCondition.ConditionType.INVALID)
            {
                notifyIcon1.Visible = true;
                Visible = false;
                ExpManager.sharedInstance.begin();
                Visible = true;
                notifyIcon1.Visible = false;
            }
        }

        private void notify(String args)
        {
            notifyIcon1.ShowBalloonTip(500, "ProbabilityApp", args, ToolTipIcon.Info);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            ExpManager.sharedInstance.orderedStorage = checkBox1.Checked;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            ExpManager.sharedInstance.trials = (int) numericUpDown3.Value;
        }
    }
}

public static class Prompt
{
    public struct PromptResult
    {
        public DialogResult dialogResult;
        public string response;
    }


    public static PromptResult ShowDialog(string text, string caption)
    {
        Form prompt = new Form()
        {
            Width = 500,
            Height = 150,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            Text = caption,
            StartPosition = FormStartPosition.CenterScreen,
            DialogResult = DialogResult.Cancel
        };
        Label textLabel = new Label() { Left = 50, Top = 20, Text = text, Width = 400, Height = 50 };
        TextBox textBox = new TextBox() { Left = 50, Top = 50, Width = 400 };
        Button confirmation = new Button() { Text = "Ok", Left = 350, Width = 100, Top = 70, DialogResult = DialogResult.OK };
        confirmation.Click += (sender, e) => { prompt.Close(); };
        prompt.Controls.Add(textBox);
        prompt.Controls.Add(confirmation);
        prompt.Controls.Add(textLabel);
        prompt.AcceptButton = confirmation;

        return new PromptResult
        {
            dialogResult = prompt.ShowDialog(),
            response = textBox.Text
        };
    }
}