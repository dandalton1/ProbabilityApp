using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProbabilityApp
{
    public partial class EndConditionChooser : Form
    {
        public EndConditionChooser()
        {
            InitializeComponent();
            numericUpDown3.Maximum = ExpManager.sharedInstance.outcomes;
            radioButton1.Checked = ExpManager.sharedInstance.endCondition.condition == EndCondition.ConditionType.REPETITIONS;
            radioButton2.Checked = ExpManager.sharedInstance.endCondition.condition == EndCondition.ConditionType.TIME_LIMIT;
            radioButton3.Checked = ExpManager.sharedInstance.endCondition.condition == EndCondition.ConditionType.PATTERN;
            numericUpDown1.Value = ExpManager.sharedInstance.endCondition.repetitions;
            numericUpDown2.Value = ExpManager.sharedInstance.endCondition.seconds;
            numericUpDown3.Value = ExpManager.sharedInstance.endCondition.patternRepetitions;
            AcceptButton = button3;
            foreach (int i in ExpManager.sharedInstance.endCondition.pattern)
            {
                listBox1.Items.Add(i);
            }

            Console.WriteLine("END CONDITION CHOOSER INITIALIZED.");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Add((int)numericUpDown3.Value);
            ExpManager.sharedInstance.endCondition.addPatternItem((int)numericUpDown3.Value);
            Console.WriteLine("ADDED OUTCOME " + (int)numericUpDown3.Value + " TO SET");
            ExpManager.sharedInstance.endCondition.dispPattern();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Console.WriteLine("Selected index: " + (listBox1.SelectedIndex + 1));
            if (listBox1.SelectedIndex + 1 >= 0)
            {
                listBox1.Items.Remove(listBox1.SelectedItem);
                ExpManager.sharedInstance.endCondition.rmPatternItem(listBox1.SelectedIndex + 1);
                Console.WriteLine("REMOVED ITEM");
                ExpManager.sharedInstance.endCondition.dispPattern();
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            ExpManager.sharedInstance.endCondition.repetitions = (long)numericUpDown1.Value;

        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            ExpManager.sharedInstance.endCondition.seconds = numericUpDown2.Value;
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            ExpManager.sharedInstance.endCondition.condition = EndCondition.ConditionType.REPETITIONS;
            Console.WriteLine("End condition changed: repetitions");
        }

        private void radioButton2_CheckedChanged(object sender, EventArgs e)
        {
            ExpManager.sharedInstance.endCondition.condition = EndCondition.ConditionType.TIME_LIMIT;
            Console.WriteLine("End condition changed: time limit");
        }

        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            ExpManager.sharedInstance.endCondition.condition = EndCondition.ConditionType.PATTERN;
            Console.WriteLine("End condition changed: pattern");
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            ExpManager.sharedInstance.endCondition.patternRepetitions = (int) numericUpDown4.Value;
        }
    }
}
