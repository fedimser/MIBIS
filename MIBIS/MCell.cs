//Class representing the MIBIS Cell - living unit

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace MIBIS
{
    public class MCell
    {
    #region "Data"
        public PointF Pos;      //Position of cell in the world
        public int CellID;      //Unique ID
      
         
       //Constants, given by user:
       public static int k1;            //energy from sun
       public static int k2;            //energy for live
       public static int k3;            //energy for move
       public static int k4;            //LightLevel/LightSupply
       public static int k5;            //min difference of radiuses for eating
       public static float k6;          //freq of dividing 
       public static int k7;            //resrved
        
       public static int m1;            //Brown moving
       public static int m2;            //Self moving
       public static int m3;            //reserved
       public static int E0;            //Basic level of energy(EU)
       public static int Rmx;           //Maximal radius


       public DNA Cell_DNA;

       //Properties, determined by DNA
       private Color CellColor;                 //Color of body
       private Color BorderColor;               //Color of border
       public int CellRadius;                   //Radius of cell
       private int CellBorderWidth;             //Width of border
       private Pen DrawPen;
       private Brush DrawBrush;
       private float v_max;                     //Maximal speed (px/ms)
       private int movement_confidence;         //Time of constant moving(ms)
       private int legs;                //Number of legs
       private float legs_len;          //Length of legs, in radiuses, [1,2];
       private float PhStEff;           //Effeciency
       public float BasicEnergy;        //E0*CellRadius
       public float DivideExcess;       // Can divide if Energy >=BasicEnergy*DivideExcess

        //Properties which are changing
        public float Energy;            //Energy of cell (EU) 
        public bool Living;             //Live or dead

        MWorld wrld;                    //Pointer to parent world
        static Random rnd = new Random();                     //Individual random generator

        #endregion

    #region "Constructors"
        //Creates cell with random DNA
        public MCell(MWorld _wrld)
        {
            _wrld.ID_Counter++;
            CellID = _wrld.ID_Counter;
            rnd = new Random(CellID);

            setRandomDNA();
            wrld = _wrld;
            Living = true; 
        }

        //Creates cell with given DNA
        public MCell(MWorld _wrld,DNA _DNA)
        {
            _wrld.ID_Counter++;
            CellID = _wrld.ID_Counter;
            Console.WriteLine(String.Format("new {0}",CellID) );
            rnd = new Random(CellID);

            Cell_DNA = new DNA(_DNA);
            applyDNA();
            wrld = _wrld;
            Living = true; 
        }

        //Copy constructor
        public MCell(MCell c)
        { 
            Pos=c.Pos; 
            CellID=c.CellID;  
            Cell_DNA=new DNA(c.Cell_DNA);
            applyDNA();
            Energy = c.Energy;             
            Living = c.Living;
            wrld = c.wrld; 
        }
#endregion
         
    #region "DNA"

        public class DNA
        {
           public const int DNA_Length = 11;
           public byte[] x = new byte[DNA_Length];
             
           public DNA()
           {
               MWorld.rnd.NextBytes(x);
           }

           public DNA(DNA t)
           {
               for (int i = 0; i < DNA_Length; i++) x[i] = t.x[i];
           }

        }

        //Sets to this cell random DNA
        public void setRandomDNA()
        {
            Cell_DNA = new DNA();
            MWorld.rnd.NextBytes(Cell_DNA.x);
            applyDNA();
        }

        //Randomly changes DNA
        public void MutateDNA()
        { 
            for (int i = 0; i < DNA.DNA_Length; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    int g = rnd.Next(10000);
                   // Console.WriteLine(string.Format("{0}",g));
                    if (g < wrld.RadiationLevel)
                    {
                        byte mut= (byte)(1 << j);
                        this.Cell_DNA.x[i] ^= mut;
                    }
                }
            }
            applyDNA();
        }
        
        //Calculates all genetical properties of cell
        public void applyDNA()
        {
            CellColor = Color.FromArgb(255, Cell_DNA.x[0], Cell_DNA.x[1], Cell_DNA.x[2]);
            BorderColor = Color.FromArgb(255, Cell_DNA.x[3], Cell_DNA.x[4], Cell_DNA.x[5]);
            CellRadius = (Cell_DNA.x[6] % Rmx)+1;   
            CellBorderWidth = (Cell_DNA.x[7] & 7);   //0b00000111

            DrawBrush = new SolidBrush(CellColor);
            DrawPen = new Pen(BorderColor,CellBorderWidth);
             
            v_max = Cell_DNA.x[8] *  1e-5F*m2;               // px/ms
            legs = Cell_DNA.x[8] / 12;
            legs_len = 1 + (Cell_DNA.x[9] / 255F);
            movement_confidence = (Cell_DNA.x[9]+5) * 100;     // ms
            ChangeDirection();
             
            PhStEff =  Cell_DNA.x[1] /256F;
            BasicEnergy = CellRadius * E0;
            DivideExcess = 1 +  (255- Cell_DNA.x[0]) * (k6/255);
        }

#endregion

    #region "Graphics"
        //Draws the cell
        public void Draw(Graphics g)
        {
            for (int i = 0; i < legs; i++)
            {
                float a = (i * 2 * 3.1415926535F / legs);
                g.DrawLine(DrawPen, Pos.X, Pos.Y, Pos.X + legs_len * CellRadius * (float)Math.Cos(a), Pos.Y + legs_len * CellRadius * (float)Math.Sin(a));
            }

            g.FillEllipse(DrawBrush, Pos.X - CellRadius, Pos.Y - CellRadius, 2 * CellRadius, 2 * CellRadius);
            g.DrawEllipse(DrawPen, Pos.X - CellRadius, Pos.Y - CellRadius, 2 * CellRadius, 2 * CellRadius);           
        }

        //Draws the cell with translation
        public void DrawPhantom(Graphics g, int dx, int dy)
        {
            for (int i = 0; i < legs; i++)
            {
                float a = (i * 2 * 3.1415926535F / legs);
                g.DrawLine(DrawPen, dx + Pos.X, dy + Pos.Y, dx + Pos.X + legs_len * CellRadius * (float)Math.Cos(a), dy + Pos.Y + legs_len * CellRadius * (float)Math.Sin(a));
            }

            g.FillEllipse(DrawBrush, dx+Pos.X - CellRadius, dy+Pos.Y - CellRadius, 2 * CellRadius, 2 * CellRadius);
            g.DrawEllipse(DrawPen,dx+ Pos.X - CellRadius, dy+Pos.Y - CellRadius, 2 * CellRadius, 2 * CellRadius);
        }
    #endregion

    #region "Life"

        PointF v_self= new PointF(0,0);     //vector of current self speed, px/ms
        float v_self_abs;                   //absolute value of self speed, px/ms

        //Models life of the cell during dt, [dt]=ms
        public void Live(int dt)
        {
            //Brown+self movement
            float tw= (0.001F*m1*dt/CellRadius);
            Pos.X += (-tw + 2*tw*(float)MWorld.rnd.NextDouble()) + v_self.X * dt;           
            Pos.Y += (-tw + 2 * tw * (float)MWorld.rnd.NextDouble()) + v_self.Y * dt;       
           
            //Randomly changes Direction of self movement 
            if (MWorld.rnd.Next(0, movement_confidence) < dt) { ChangeDirection();}

            //Calculate change of energy
            Energy += dt * (k1 * wrld.LightSupply * PhStEff - k2 * CellRadius - k3 * CellRadius*v_self_abs);

            //Die
            if (Energy <= 0) Living = false;

            //Divide
            if (Energy >= DivideExcess*BasicEnergy) Divide();

            //Eat smaller
            foreach (MCell i in wrld.Cells)
            {
                if (CellRadius - i.CellRadius > k5 && sqr(Pos.X - i.Pos.X) + sqr(Pos.Y - i.Pos.Y) < sqr(CellRadius))
                {
                    this.Energy += i.Energy;
                    i.Energy = 0;
                    i.Living = false;
                }
            }             
        }

        //Sets new direction
        void ChangeDirection()
        {
            float t = v_max * 0.707106F;
            v_self = new PointF(-t + 2 * t * (float)MWorld.rnd.NextDouble(), -t + 2 * t * (float)MWorld.rnd.NextDouble());
            v_self_abs = Abs(v_self);
        }

        //Creates new cell
        void Divide()
        {
            Energy = Energy / 2;
            var t = new MCell(this);
            t.MutateDNA(); 
            wrld.AddCell(t);
        }
         
        private static float Abs(PointF _p)
        {
            return (float) Math.Sqrt(_p.X * _p.X + _p.Y * _p.Y);
        }

        private float sqr(float x)
        {
            return x * x;
        }

#endregion

    }
}
