//Class of MIBIS Main form
//This form contains the world and Main Menu

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace MIBIS
{
    public partial class Form1 : Form
    {
        public static MWorld wrld; 
        Form2 InfoForm;
        
        public Form1()
        { 
            InitializeComponent();
            wrld = new MWorld();

            xs = 0;
            ys = 0;
            scl = 1;
            
            InfoForm=new Form2();
            InfoForm.MainForm = this;
            InfoForm.Show();

            this.Location = new Point(0, 0);
            InfoForm.Location=new Point(this.Right, 0);
            FitWindowSize(); 
             

        }

    #region "graphics and mouse"
        static Bitmap WorldBitmap;
        static Graphics g; 

        static  float xs, ys,scl; 
        static bool now_move_mouse = false;
        static Point tp1;
        static Point p;

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                now_move_mouse = true;
                tp1 = GetCurPos();
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            RefreshScale();
            now_move_mouse = false;
        }

        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            RefreshScale();
            now_move_mouse = false;
        }

        private void pictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            float alpha = 1 + 0.1F*(e.Delta/120);
            if (scl*alpha < 0.7 || scl*alpha > 10) return;
            float t1=(1-(1/alpha))/scl;

            Point p = GetCurPos();
            xs += p.X * t1;
            ys += p.Y * t1;
            scl *= alpha;
        }

        private void pictureBox1_MouseEnter(object sender, EventArgs e)
        {
            pictureBox1.Focus();
        }

        //Draws world periodically
        private void timer1_Tick(object sender, EventArgs e)
        { 
            RefreshScale();
            if (pictureBox1.Width == 0) return;
            WorldBitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);

            DrawWorld();

            pictureBox1.Image = WorldBitmap;
            
        }

         

        //Draws world
        private static void DrawWorld()
        {            
            Matrix m = new Matrix();
            m.Scale(scl, scl);
            m.Translate(-xs, -ys);
            
             
            g = Graphics.FromImage(WorldBitmap);
            g.Transform = m;
            wrld.Draw(ref g);  
        }

        private static void DrawLoop()
        {
            while (true)
            {
                DrawWorld();
            }
        }
         

        void RefreshScale()
        {
            if (now_move_mouse)
            {
                Point p = GetCurPos();
                xs -= (p.X - tp1.X) / scl;
                ys -= (p.Y - tp1.Y) / scl;
                tp1 = p;
            }
        }

        //Return position of the cursor at the form
        Point GetCurPos()
        {
            p = Cursor.Position;
            p = pictureBox1.PointToClient(p);
            return p;
            //return new Point(p.X-pictureBox1.Location.X, p.Y-pictureBox1.Location.Y);
        }

        //Changes size of form to contain exactly all world in scale 1
        public void FitWindowSize()
        {
            int dx = this.Width - pictureBox1.Width;
            int dy = this.Height - pictureBox1.Height;
            this.Size = new Size(wrld.Size_x + dx, wrld.Size_y + dy);
            this.Location = new Point(0, 0);
            InfoForm.Location = new Point(this.Right, 0);
        }
        
        //Sets scale=1 and moves picture of world to the center
        private void resetScaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            scl = 1;
            xs = -(pictureBox1.Width - wrld.Size_x) / 2;
            ys = -(pictureBox1.Height - wrld.Size_y) / 2;
        }

        private void fitWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            FitWindowSize();
            scl = 1;
            xs = -(pictureBox1.Width - wrld.Size_x) / 2;
            ys = -(pictureBox1.Height - wrld.Size_y) / 2;
        }

        private void saveImageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd = new SaveFileDialog();
            sfd.Filter = "BMP|*.bmp";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                Bitmap bmp = new Bitmap(wrld.Size_x, wrld.Size_y);
                Graphics g = Graphics.FromImage(bmp);
                wrld.Draw(ref g);
                bmp.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
            }
        }

#endregion
         
    #region "play"  

        public int Real_Model_Time;                         //Time spent for modelling one step
        public bool ModellingOn = true;                     //Is now modelling turned on

        //Does one step of modelling
        private void timer2_Tick(object sender, EventArgs e)
        {
            DateTime t1 = DateTime.Now;
            wrld.Live();
            DateTime t2 = DateTime.Now;           
            Real_Model_Time = t2.Subtract(t1).Milliseconds;
        }


        private void controlPanelToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (controlPanelToolStripMenuItem.Checked)
            {
                InfoForm.Location = InfoForm.Location = new Point(this.Right, this.Top);
                InfoForm.Show();
                InfoForm.Focus();
                this.Focus();
            }
            else
            {
                InfoForm.Hide();
            }
        }

        //Starts or suspends modelling
        public void StartStop()
        {
            if (ModellingOn)
            {
                Real_Model_Time = 0;
                ModellingOn = false;
                timer2.Stop();
                
                InfoForm.timer1.Stop();
                InfoForm.pictureBox2.Image = MIBIS.Properties.Resources.pause;
            }
            else
            {
                ModellingOn = true;
                timer2.Start();
                InfoForm.timer1.Start();
                InfoForm.pictureBox2.Image = MIBIS.Properties.Resources.play;
            }
        }

        private void startStopToolStripMenuItem_Click(object sender, EventArgs e)
        {
            StartStop();
        }         
         
        #endregion

    #region "FULLSCREEN"

        bool FS_Mode = false;

        public void FullScreenMode()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            scl = 1;
            xs = -(this.Width - wrld.Size_x) / 2;
            ys = -(this.Height - wrld.Size_y) / 2; 
           
            
            pictureBox1.Size = this.Size;
             
            menuStrip1.Hide();
        }

        public void ExitFullScreenMode()
        {

            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.WindowState = FormWindowState.Normal;
            scl = 1;
            xs = 0;
            ys = 0;
            pictureBox1.Size = this.Size;
            menuStrip1.Show();
        }

        private void fullScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (FS_Mode)
            { ExitFullScreenMode();
            FS_Mode = false;
            }
            else
            {
                FullScreenMode();
                FS_Mode = true;
            }
        }

        #endregion

    #region "World"

        private void addCellsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wrld.AddCells((int)InfoForm.numericUpDown6.Value, InfoForm.checkBox1.Checked);
            if (!timer2.Enabled) wrld.ApplyCellChanges();
        }

        //Creates new world
        public void CreateNewWorld()
        {
            wrld = new MWorld();
            InfoForm.ApplySetings();
            InfoForm.ApplySetings();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            CreateNewWorld();
            FitWindowSize();
            if (!timer2.Enabled) wrld.ApplyCellChanges();
        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var sfd=new SaveFileDialog();
            sfd.Filter=("Text files|*.txt|Any file|*.*");
            if(sfd.ShowDialog()==DialogResult.OK)
            {
                wrld.SaveToFile(sfd.FileName);
            }
        }

        private void создатьToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            wrld.CreateRandomPopulation((int)InfoForm.numericUpDown6.Value, InfoForm.checkBox1.Checked);
            if (!timer2.Enabled) wrld.ApplyCellChanges();
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = ("Text files|*.txt|Any file|*.*");
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                { 
                    wrld.LoadFromFile(ofd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                    throw;
                }
                ShowSettings();
            }
        }

        private void clearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wrld.Clear();
            if (!timer2.Enabled) wrld.ApplyCellChanges();
        }

        //Shows all properties in the control panel
        private void ShowSettings()
        {
            InfoForm.numericUpDown12.Value = MCell.k1;
            InfoForm.numericUpDown1.Value = MCell.k2;
            InfoForm.numericUpDown2.Value = MCell.k3;
            InfoForm.numericUpDown3.Value = MCell.k4;
            InfoForm.numericUpDown9.Value = MCell.k5;
            InfoForm.numericUpDown5.Value = (decimal)MCell.k6;
            InfoForm.numericUpDown4.Value = MCell.E0;
            InfoForm.numericUpDown13.Value = MCell.m1;
            InfoForm.numericUpDown14.Value = MCell.m2;
            InfoForm.numericUpDown7.Value = wrld.LightLevel;
            InfoForm.numericUpDown8.Value = wrld.RadiationLevel;
        }

        private void returnInitalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            wrld.ReturnInitalPopulation();
            if (!timer2.Enabled) wrld.ApplyCellChanges();
        }

        #endregion

       

    }
}