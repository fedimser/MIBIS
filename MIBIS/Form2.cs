//Class of form of MIBIS Control panel 

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MIBIS
{
    public partial class Form2 : Form
    {
        public Form1 MainForm;
        private Bitmap chart_bmp=new Bitmap(250,105);
        private int[] CC_history = new int[250]; 
        private int max_CC = 100;
        double ModelSpeedK;
        int c = 0;         //Counter of info refresh

        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            ApplySetings();
        }
        
        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            if (trackBar1.Value <= 10) ModelSpeedK = 0.1 * trackBar1.Value;
            else ModelSpeedK = trackBar1.Value - 9;

            label10.Text = "Speed " + ModelSpeedK.ToString();
            MainForm.timer2.Interval = (int)(Form1.wrld.Mod_Interval / ModelSpeedK);
        }
         
        private void button2_Click(object sender, EventArgs e)
        {
            Form1.wrld.CreateRandomPopulation((int)numericUpDown6.Value,checkBox1.Checked);
            if (!MainForm.timer2.Enabled) Form1.wrld.ApplyCellChanges();
        } 

        private void numericUpDown7_ValueChanged(object sender, EventArgs e)
        {
            Form1.wrld.LightLevel = (int)numericUpDown7.Value;
            Form1.wrld.FillBrush = new SolidBrush(Color.FromArgb(255, 255, 255, 255 - Form1.wrld.LightLevel));
        }

        private void numericUpDown8_ValueChanged(object sender, EventArgs e)
        {
            Form1.wrld.RadiationLevel = (int)numericUpDown8.Value;
        }
         

        private void Form2_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            MainForm.controlPanelToolStripMenuItem.Checked = false;
            this.Hide();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MainForm.CreateNewWorld();
            MainForm.FitWindowSize();
            if (!MainForm.timer2.Enabled) Form1.wrld.ApplyCellChanges();
        }

        private void numericUpDown12_ValueChanged(object sender, EventArgs e)
        {
             MCell.k1= (int) numericUpDown12.Value;
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            MCell.k2 = (int)numericUpDown1.Value;
        }

        private void numericUpDown2_ValueChanged(object sender, EventArgs e)
        {
            MCell.k3 = (int)numericUpDown2.Value;
        }

        private void numericUpDown3_ValueChanged(object sender, EventArgs e)
        {
            MCell.k4 = (int)numericUpDown3.Value;
        }

        private void numericUpDown9_ValueChanged(object sender, EventArgs e)
        {
            MCell.k5 = (int)numericUpDown9.Value;
        }

        private void numericUpDown5_ValueChanged(object sender, EventArgs e)
        {
            MCell.k6 = (int)numericUpDown5.Value;
        }

        private void numericUpDown4_ValueChanged(object sender, EventArgs e)
        {
            MCell.E0 = (int)numericUpDown4.Value;
        }

        private void numericUpDown13_ValueChanged(object sender, EventArgs e)
        {
            MCell.m1 = (int)numericUpDown13.Value; 
        }

        private void numericUpDown14_ValueChanged(object sender, EventArgs e)
        {
            MCell.m2 = (int)numericUpDown14.Value; 
        }

        private void numericUpDown15_ValueChanged(object sender, EventArgs e)
        {
            MCell.m3 = (int)numericUpDown15.Value; 
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            Form1.wrld.AddCells((int)numericUpDown6.Value, checkBox1.Checked);
            if (!MainForm.timer2.Enabled) Form1.wrld.ApplyCellChanges();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Form1.wrld.Clear();
        }

        private void numericUpDown16_ValueChanged(object sender, EventArgs e)
        {
            MCell.Rmx = (int)numericUpDown16.Value;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            MainForm.StartStop();
        }

        private void numericUpDown17_ValueChanged(object sender, EventArgs e)
        {
            Form1.wrld.Mod_Interval = (int)numericUpDown17.Value;
            MainForm.timer2.Interval = (int)(Form1.wrld.Mod_Interval / ModelSpeedK);
        }

        //Applies values at the form to the world
        public void ApplySetings()
        {
            MCell.k1 = (int)numericUpDown12.Value;
            MCell.k2 = (int)numericUpDown1.Value;
            MCell.k3 = (int)numericUpDown2.Value;
            MCell.k4 = (int)numericUpDown3.Value;
            MCell.k5 = (int)numericUpDown9.Value;
            MCell.k6 = (int)numericUpDown5.Value;
            MCell.E0 = (int)numericUpDown4.Value;
            MCell.m1 = (int)numericUpDown13.Value;
            MCell.m2 = (int)numericUpDown14.Value;
            MCell.m3 = (int)numericUpDown15.Value;
            MCell.Rmx = (int)numericUpDown16.Value;

            Form1.wrld.RadiationLevel = (int)numericUpDown8.Value;
            Form1.wrld.LightLevel = (int)numericUpDown7.Value;
            Form1.wrld.Size_x = (int)numericUpDown10.Value;
            Form1.wrld.Size_y = (int)numericUpDown11.Value;
            Form1.wrld.FillBrush = new SolidBrush(Color.FromArgb(255, 255, 255, 255 - Form1.wrld.LightLevel));
        }
        
        //Refreshes all information, builds chart
        private void timer1_Tick(object sender, EventArgs e)
        {
            c++;

            Form1.wrld.refreshStatistics(timer1.Interval);
            int CC = Form1.wrld.Cell_Count;
            double BR = Form1.wrld.Birth_Rate;
            if (BR == double.NaN) BR = 0;
            label8.Text = CC.ToString();
            label9.Text = BR.ToString("F2");
            label21.Text = Form1.wrld.Avg_Energy.ToString("F0");
            label23.Text = Form1.wrld.Energy_Deriv.ToString("F0");
            label24.Text = Form1.wrld.Total_Square.ToString();
            label27.Text = (Form1.wrld.Model_Time / 1000).ToString("F0");
            label26.Text = String.Format("{0}x{1}", Form1.wrld.Size_x, Form1.wrld.Size_y);
            label31.Text = String.Format("{0}/{1}", MainForm.Real_Model_Time, MainForm.timer2.Interval);

            max_CC = 1;
            for (int i = 0; i < 199; i++)
            {
                CC_history[i] = CC_history[i + 1];
                if (CC_history[i] > max_CC) max_CC = CC_history[i];
            }

            if (MainForm.Real_Model_Time > MainForm.timer2.Interval)
            {
                pictureBox3.Visible = ((c & 1) == 0);
                if (checkBox2.Checked && (MainForm.Real_Model_Time > 2 * MainForm.timer2.Interval) && MainForm.ModellingOn)
                {
                    MainForm.StartStop();
                    MessageBox.Show("Overload");
                    pictureBox3.Hide();
                }

            }
            else if (pictureBox3.Visible) pictureBox3.Hide();

            CC_history[199] = CC;
            Graphics g = Graphics.FromImage(chart_bmp);
            g.Clear(Color.Black);

            for (int i = 0; i < 199; i++)
            {
                g.DrawLine(Pens.Green, i, 100, i, 100 - 100 * CC_history[i] / max_CC);
            }
            pictureBox1.Image = chart_bmp;
            panel1.BackColor = GetIndicateColor(Form1.wrld.Birth_Rate / 20);
            panel2.BackColor = GetIndicateColor(Form1.wrld.Energy_Deriv / (1000 * CC + 1));
        }
         
        //Gets color from Red(value=-1) to Green(+1)
        Color GetIndicateColor(double value)        //value \in [-1,1]
        {
            if (value < -1) value = -1;
            if (value > 1) value = 1;
            byte z = (byte)((value + 1) * 127);
            return Color.FromArgb(255, 255 - z, z, 0);
        }
    }
}
