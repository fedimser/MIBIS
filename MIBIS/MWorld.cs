//Class representing the MIBIS World - the main instance, containing all cells

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms; 


namespace MIBIS
{
   public class MWorld
    { 
        
    #region "Data"

        public int Size_x;                              //Width of the world
        public int Size_y;                              //Height of the world
        public static Random rnd = new Random();

        public List<MCell> Cells = new List<MCell>();           //Cells
        public List<MCell> Cells_To_Add = new List<MCell>();    //Cells to add on next step
        public List<MCell> CellsBuf = new List<MCell>();        //Buffer for rollback  


        public int LightLevel=3;                //Light level
        public int LightSupply;                 //Light which is received by cells
        public int RadiationLevel=0;            //Radiation level = probability of mutation of one bit;
        
        public Brush FillBrush = Brushes.White;

        static bool Cell_is_Dead(MCell x) { return !x.Living; }             
        static Predicate<MCell> removeCellPred = new Predicate<MCell>(Cell_is_Dead); //Predicate for removing of dead cells


        public int ID_Counter=0;                //Counter for unique naming of cells

#endregion

    #region "Statistics"
        //Information about state of the world:
        public int Cell_Count=0;        //Number of cells
        public double Birth_Rate;       //Derivative of Cell_count, divided to Cell_Count (percent/second)
        public float Total_Energy;      //Sum of energy of all cells (EU = Energy Unit)
        public float Avg_Energy;        //Average energy
        public float Energy_Deriv;      //(EU per milisecond)
        public int Total_Square = 0;    //Sum of squared radiuses of all cells
        public float Model_Time;        //Time from creating of the new population (s)

        private float[] Pop_history= new float[11]; //Last values of population - needed to count Birth_rate

        //Calculate all statistical values
        public void refreshStatistics(int dt)
        {
            Model_Time += dt;
            Cell_Count = Cells.Count();

            for (int i = 1; i <= 10; i++) Pop_history[i - 1] = Pop_history[i];
            Pop_history[10] = Cell_Count;
            
            if (Cell_Count == 0) Birth_Rate = 0;
            else Birth_Rate =  (100 * (Cell_Count-Pop_history[0]) / Cell_Count) / (10*dt/1000F);
             
            float TE = 0; 
            foreach (MCell i in Cells)
            { 
                TE+=i.Energy; 
            }

            Energy_Deriv = (TE - Total_Energy) / dt; 
            Total_Energy = TE;
            if (Cell_Count != 0) Avg_Energy = Total_Energy / Cell_Count; else Avg_Energy = 0;
        }


#endregion

    #region "Construcrors"
        public MWorld(int _x, int _y)
        {
            Size_x = _x;
            Size_y = _y;
        }

        public MWorld()
        {
            Size_x = 100;
            Size_y = 100;
        }
#endregion
       
    #region "Cells"

        //Creates population of Cells_count cells with same given DNA
        public void CreatePopulation(int Cells_count , MCell.DNA _DNA)
        {
            Clear();
             
            for (int i = 0; i < Cells_count; i++)
            {
                var t = new MCell(this, _DNA);
                t.Pos = new Point(rnd.Next(0, Size_x), rnd.Next(0, Size_y));
                t.Energy = t.BasicEnergy;
                AddCell(t);
            }

            SavePopulationToBuffer();
        }

        //Creates random population
        public void CreateRandomPopulation(int Cells_count , bool Same_Cells)
        {
            Clear();

            if (Same_Cells)
            {
                MCell.DNA tDNA = new MCell.DNA();
                for (int i = 0; i < Cells_count; i++)
                {
                    var t = new MCell(this,tDNA);
                    t.Pos = new Point(rnd.Next(0, Size_x), rnd.Next(0, Size_y));
                    t.Energy = t.BasicEnergy;
                    AddCell(t);
                }
            }
            else
            {
                for (int i = 0; i < Cells_count; i++)
                {
                    var t = new MCell(this);
                    t.Pos = new Point(rnd.Next(0, Size_x), rnd.Next(0, Size_y));
                    t.Energy = t.BasicEnergy;
                    AddCell(t);
                }
            }

            SavePopulationToBuffer();
        }

        //Removes all cells
        public void Clear()
        {
            Cells.Clear();
            Cells_To_Add.Clear();
            Total_Square = 0;
            Model_Time = 0;
        }

        //Adds one cell
        public void AddCell(MCell t)
        {
            Cells_To_Add.Add(t);
        }

        //Adds random cells to existing population
        public void AddCells(int count, bool Same)
        {
            if (Same)
            {
                MCell.DNA _DNA = new MCell.DNA();

                for (int i = 0; i < count; i++)
                {
                    var t = new MCell(this, _DNA);
                    t.Pos = new Point(rnd.Next(0, Size_x), rnd.Next(0, Size_y));
                    t.Energy = t.BasicEnergy;
                    AddCell(t);
                }
            }
            else
            {
                for (int i = 0; i < count; i++)
                {
                    var t = new MCell(this);
                    t.Pos = new Point(rnd.Next(0, Size_x), rnd.Next(0, Size_y));
                    t.Energy = t.BasicEnergy;
                    AddCell(t);
                }
            }
        }

        //Saves inital population
        private void SavePopulationToBuffer()
        {
            CellsBuf.Clear();
            foreach (MCell i in Cells) CellsBuf.Add(new MCell(i));
            foreach (MCell i in Cells_To_Add) CellsBuf.Add(new MCell(i));
        }

        //Rollback
        public void ReturnInitalPopulation()
        {
            Clear();
            foreach (MCell i in CellsBuf) Cells.Add(new MCell(i));
        }

        //Applies delete and add when modelling is off
        public void ApplyCellChanges()
        {
            //Add created cells 
            foreach (MCell i in Cells_To_Add) Cells.Add(i);
            Cells_To_Add.Clear();

            //Remove deleted cells
            Cells.RemoveAll(removeCellPred);
        }

        #endregion

    #region "Live"

        public int Mod_Interval = 50;//Basic interval of one step of modelling, ms 
    
        //Models life of the colony during Mod_Interval (ms)
        public void Live()
        {
            //Add cells created on last step
            foreach (MCell i in Cells_To_Add) Cells.Add(i);
            Cells_To_Add.Clear();

            //Calculate Light_Supply
            Total_Square = 0;
            foreach (MCell i in Cells) Total_Square += i.CellRadius * i.CellRadius;             
            float k = (Size_x * Size_y/ MCell.k4) / (Total_Square*3.1415926535F);
            if (k > 1) k = 1; 
            LightSupply = (int)(LightLevel * k);
             
            //Models living of all cells
            foreach (MCell i in Cells)
            {
                i.Live(Mod_Interval); 
                 
                //Models toroidal surface
                if (i.Pos.X < 0) i.Pos.X += Size_x;
                if (i.Pos.Y < 0) i.Pos.Y += Size_y;
                if (i.Pos.X > Size_x) i.Pos.X -= Size_x;
                if (i.Pos.Y > Size_y) i.Pos.Y -= Size_y;

            }

            //Remove died cells
             Cells.RemoveAll(removeCellPred);
        }
        #endregion

    #region "graphics"


         public  void Draw(ref Graphics g)
        { 
            bool AdvancedGraphics = true;       //Advanced ooptions of graphics

            g.Clear(Color.White);             
            g.DrawRectangle( Pens.Red, -1, -1, Size_x+1, Size_y+1);
            g.FillRectangle(FillBrush, 0, 0, Size_x, Size_y);           //Draw field

            //Draw all cells
            foreach (MCell i in Cells)
            {
                i.Draw(g);
            }

            if (AdvancedGraphics)
            {
                //Draw cells near border
                foreach (MCell i in Cells)
                {
                    if(i.Pos.X<i.CellRadius)i.DrawPhantom(g,Size_x,0);
                    if (i.Pos.X >Size_x- i.CellRadius) i.DrawPhantom(g, -Size_x,0);
                    if (i.Pos.Y< i.CellRadius) i.DrawPhantom(g, 0, Size_y);
                    if (i.Pos.Y >Size_y- i.CellRadius) i.DrawPhantom(g, 0, -Size_y);
                }

                //Fill cells out of the border
                g.FillRectangle(Brushes.White, -35, -35, 34, 70 + Size_y);
                g.FillRectangle(Brushes.White, -35, -35, 70 + Size_x, 34);
                g.FillRectangle(Brushes.White, Size_x + 1, -35, 34, 70 + Size_y);
                g.FillRectangle(Brushes.White, -35, Size_y + 1, 70 + Size_x, 34);
            }
        }

         
#endregion

    #region "Files"

        //Saves information about the world to file
        public void SaveToFile(string path)
        {
            string str = "";
            str += String.Format("E0={0}\nm1={1}\nm2={2}\nm3={3}\n", MCell.E0, MCell.m1, MCell.m2,MCell.m3);
            str += String.Format("k1={0}\nk2={1}\nk3={2}\nk4={3}\nk5={4}\nk6={5}\n", MCell.k1, MCell.k2, MCell.k3, MCell.k4, MCell.k5, MCell.k6);
            str += String.Format("Rmx={0}\n", MCell.Rmx);
            str += String.Format("Light={0}\nRadiation={1}\nWidth={2}\nHeight={3}\n", LightLevel, RadiationLevel, Size_x, Size_y);

            System.IO.File.WriteAllText(path,str);
        }

        //Loads information about the world from file
        public void LoadFromFile(string path)
        {
            var str = System.IO.File.ReadLines(path);
            foreach(string s in str)
            {
                var x = s.Split('=');
                if (x[0] == "E0") MCell.E0 = Convert.ToInt32(x[1]);
                if (x[0] == "m1") MCell.m1 = Convert.ToInt32(x[1]);
                if (x[0] == "m2") MCell.m2 = Convert.ToInt32(x[1]);
                if (x[0] == "m3") MCell.m3 = Convert.ToInt32(x[1]);
                if (x[0] == "k1") MCell.k1 = Convert.ToInt32(x[1]);
                if (x[0] == "k2") MCell.k2 = Convert.ToInt32(x[1]);
                if (x[0] == "k3") MCell.k3 = Convert.ToInt32(x[1]);
                if (x[0] == "k4") MCell.k4 = Convert.ToInt32(x[1]);
                if (x[0] == "k5") MCell.k5 = Convert.ToInt32(x[1]);
                if (x[0] == "k6") MCell.k6 = Convert.ToSingle(x[1]);
                if (x[0] == "Rmx") MCell.Rmx = Convert.ToInt32(x[1]);

                if (x[0] == "Light") LightLevel = Convert.ToInt32(x[1]);
                if (x[0] == "Radiation") RadiationLevel = Convert.ToInt32(x[1]);
                if (x[0] == "Width") Size_x = Convert.ToInt32(x[1]);
                if (x[0] == "Height") Size_y = Convert.ToInt32(x[1]);
            }
        }

#endregion

    }
}
