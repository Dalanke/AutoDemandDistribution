using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace NodesDemand
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        [DllImport("epanet2.dll", EntryPoint = "ENopen")]
        static extern int ENopen(string imporfile, string reportfile, string outputfile);
        [DllImport("epanet2.dll", EntryPoint = "ENclose")]
        static extern int ENclose();
        [DllImport("epanet2.dll", EntryPoint = "ENsolveH")]
        static extern int ENsolveH();
        [DllImport("epanet2.dll", EntryPoint = "ENsaveH")]
        static extern int ENsaveH();
        [DllImport("epanet2.dll", EntryPoint = "ENsetreport")]
        static extern int ENsetreport(string command);
        [DllImport("epanet2.dll", EntryPoint = "ENreport")]
        static extern int ENreport();
        [DllImport("epanet2.dll", EntryPoint = "ENgetcount")]
        static extern int ENgetcount(int countcode, out int count);
        [DllImport("epanet2.dll", EntryPoint = "ENgetlinkvalue")]
        static extern int ENgetlinkvalue(int index, int parameter_value, out float value);
        [DllImport("epanet2.dll", EntryPoint = "ENgetnodevalue")]
        static extern int ENgetnodevalue(int index, int parameter_value, out float value);
        [DllImport("epanet2.dll", EntryPoint = "ENsetlinkvalue")]
        static extern int ENsetlinkvalue(int index, int parameter_value, float value);
        [DllImport("epanet2.dll", EntryPoint = "ENsetnodevalue")]
        static extern int ENsetnodevalue(int index, int parameter_value, float value);
        [DllImport("epanet2.dll", EntryPoint = "ENgetlinktype")]
        static extern int ENgetlinktype(int index, out int typecode);
        [DllImport("epanet2.dll", EntryPoint = "ENgetnodetype")]
        static extern int ENgetnodetype(int index, out int typecode);
        [DllImport("epanet2.dll", EntryPoint = "ENgetlinkindex")]
        static extern int ENgetlinkindex(string id, out int index);
        [DllImport("epanet2.dll", EntryPoint = "ENgetnodeindex")]
        static extern int ENgetnodeindex(string id, out int index);
        [DllImport("epanet2.dll", EntryPoint = "ENgetlinknodes")]
        static extern int ENgetlinknodes(int index, out int fromnode,out int tonode);
        [DllImport("epanet2.dll", EntryPoint = "ENsaveinpfile")]
        static extern int ENsaveinpfile(string filepath);

        float[,] pipesinfo;
        double[] nodes_demand;
        float sum = 0;
        double[] nodes_elv;
        private void button1_Click(object sender, EventArgs e)
        {
            string fName = "";
            OpenFileDialog o1 = new OpenFileDialog();
            o1.Filter = "input files|*.INP|all files|*.*";
            o1.RestoreDirectory = true;
            if (o1.ShowDialog() == DialogResult.OK)
            {
                fName = o1.FileName;
            }
            int errorcode = ENopen(fName, fName.Replace(".inp",".rpt"), fName.Replace(".inp",".out"));
            if (errorcode > 0)
            {
                ENclose();
                MessageBox.Show("error code:" + errorcode.ToString());
            }
            else label1.Text = "Success! File path="+fName;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            int errorcode=ENclose();
            if (errorcode > 0)
            {
                MessageBox.Show("An error occur when exit, code:"+errorcode.ToString());
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            sum = 0;
            string[] Tpipes = textBox1.Text.Split(',');
            string[] OneWayPipes = textBox2.Text.Split(',');
            int[] T_index = new int[Tpipes.Length];
            for (int i = 0; i < Tpipes.Length; i++)
            {
                ENgetlinkindex(Tpipes[i], out T_index[i]);
            }
            int[] O_index = new int[OneWayPipes.Length];
            for (int i = 0; i < OneWayPipes.Length; i++)
            {
                ENgetlinkindex(OneWayPipes[i],out O_index[i]);
            }
            int links_num,link_typecode;
            ENgetcount(2, out links_num);
            pipesinfo = new float[links_num,2];
            for (int i = 1; i <= links_num; i++)
            {
                ENgetlinktype(i, out link_typecode);
                if (link_typecode == 1)
                {
                    float length = 0;
                    int b1 = Array.IndexOf(T_index, i);
                    int b2 = Array.IndexOf(O_index, i);
                    if (b1 != -1)
                    {
                        ENgetlinkvalue(i, 1, out length);
                        pipesinfo[i - 1, 0] = i;
                        pipesinfo[i - 1, 1] = 0;
                    }
                    else if (b2 != -1)
                    {
                        ENgetlinkvalue(i, 1, out length);
                        pipesinfo[i - 1, 0] = i;
                        pipesinfo[i - 1, 1] = length/2;
                    }
                    else
                    {
                        ENgetlinkvalue(i, 1, out length);
                        pipesinfo[i - 1, 0] = i;
                        pipesinfo[i - 1, 1] = length;
                    }
                }
                else
                {
                    pipesinfo[i - 1, 0] = i;
                    pipesinfo[i - 1, 1] = 0;
                }
            }
            for (int i = 0; i < links_num; i++)
            {
                sum += pipesinfo[i,1];
            }
            label2.Text = "Success, total length= "+sum.ToString();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            int nodes_num,links_num;
            ENgetcount(0, out nodes_num);
            ENgetcount(2, out links_num);
            nodes_demand = new double[nodes_num];
            double a =Convert.ToDouble(textBox3.Text)*1000/(sum*3600);
            int from_node, to_node;
            for (int i = 1; i <=links_num; i++)
            {
                if (pipesinfo[i-1,1]!=0)
                {
                    double half_de =Math.Round(pipesinfo[i-1, 1] * a/2,2);
                    ENgetlinknodes(i,out from_node,out to_node);
                    nodes_demand[from_node - 1] += half_de;
                    nodes_demand[to_node - 1] += half_de;
                }
            }
            for (int i = 0; i < nodes_demand.Length; i++)
            {
                listBox1.Items.Add("Node "+(i+1).ToString()+" "+nodes_demand[i]+" L/s");
            }
            button4.Enabled = true;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            string fName = "";
            SaveFileDialog s1 = new SaveFileDialog();
            s1.DefaultExt = "inp";
            s1.Filter= "input files|*.INP|all files|*.*";
            if (s1.ShowDialog()==DialogResult.OK)
            {
                fName = s1.FileName;
            }
            for (int i = 0; i < nodes_demand.Length; i++)
            {
                int ec = ENsetnodevalue(i+1,1,(float)nodes_demand[i]);
                if (ec > 0)
                    MessageBox.Show("Error, code: "+ec.ToString());
            }
            int errorcode = ENsaveinpfile(fName);
            if (errorcode > 0)
            {
                MessageBox.Show("Error, code: " + errorcode.ToString());
            }
            else
                MessageBox.Show("Success!");
            
        }

        private void button5_Click(object sender, EventArgs e)
        {
            listBox2.Items.Clear();
            double l = Math.Round(Convert.ToDouble(textBox4.Text),1);
            double h = Math.Round(Convert.ToDouble(textBox5.Text),1);
            int int_l = (int)(l * 10);
            int int_h = (int)(h * 10+1);
            Random r1 = new Random();
            int nodes_num;
            ENgetcount(0, out nodes_num);
            nodes_elv = new double[nodes_num];
            for (int i = 0; i < nodes_elv.Length; i++)
            {
                int tpcode = -1;
                ENgetnodetype(i+1,out tpcode);
                if (tpcode == 0)
                {
                    nodes_elv[i] =r1.Next(int_l, int_h)/10.0;
                    listBox2.Items.Add("Node" + (i + 1).ToString() + " " + nodes_elv[i].ToString() + "m");
                }
                else
                    nodes_elv[i] = 0;
            }

        }

        private void button6_Click(object sender, EventArgs e)
        {
            string fName = "";
            SaveFileDialog s1 = new SaveFileDialog();
            s1.DefaultExt = "inp";
            s1.Filter = "input files|*.INP|all files|*.*";
            if (s1.ShowDialog() == DialogResult.OK)
            {
                fName = s1.FileName;
            }
            for (int i = 0; i < nodes_elv.Length; i++)
            {
                int ec = ENsetnodevalue(i + 1, 0, (float)nodes_elv[i]);
                if (ec > 0)
                    MessageBox.Show("Error, code: " + ec.ToString());
            }
            int errorcode = ENsaveinpfile(fName);
            if (errorcode > 0)
            {
                MessageBox.Show("Error, code: " + errorcode.ToString());
            }
            else
                MessageBox.Show("Success!");
        }

        private void button7_Click(object sender, EventArgs e)
        {
            float head_low = float.Parse(textBox7.Text);
            float head_high = float.Parse(textBox8.Text);
            float v_low = float.Parse(textBox9.Text);
            float v_high = float.Parse(textBox10.Text);
            int nodes_num, links_num;
            ENgetcount(0, out nodes_num);
            ENgetcount(2, out links_num);                     // get number of links and nodes
            string[] Tpipes = textBox13.Text.Split(',');
            int[] T_index = new int[Tpipes.Length];
            for (int i = 0; i < Tpipes.Length; i++)
            {
                ENgetlinkindex(Tpipes[i], out T_index[i]);
            }                                                 // get index of transmission pipes
            Initialization s1 = new Initialization();
            s1.Set_length(links_num);
            int di_low = Convert.ToInt32(textBox11.Text);
            int di_up = Convert.ToInt32(textBox12.Text);
            bool flag = true;
            while (flag)
            {
                listBox3.Items.Clear();
                float[] p1 = s1.Gen_d(di_low, di_up);
                for (int i = 0; i < links_num; i++)        //set di
                {
                    int typecode = -1;
                    typecode = ENgetlinktype(i + 1, out typecode);
                    int b1 = Array.IndexOf(T_index, i+1);             //exclude the transmission pipes
                    if ((typecode == 1||typecode==0) && b1==-1)
                    {
                        ENsetlinkvalue(i + 1, 0, p1[i]);
                        listBox3.Items.Add("Pipe "+(i+1).ToString()+" "+p1[i].ToString()+"mm");
                        listBox3.Refresh();
                    }
                }
                ENsolveH();
                bool c1 = true;
                int tpc1;
                for (int i = 1; i <= nodes_num; i++)
                {
                    float tmp1;
                    ENgetnodetype(i, out tpc1);
                    if (tpc1==0)
                    {
                        ENgetnodevalue(i, 11, out tmp1);              //node pressure
                        if (tmp1>head_high||tmp1<head_low)
                        {
                            c1 = false;
                            break;
                        }
                    }
                }
                bool c2 = true;
                int tpc2;
                for (int i = 1; i <=links_num; i++)
                {
                    float tmp2;
                    ENgetlinktype(i,out tpc2);
                    int b1 = Array.IndexOf(T_index, i);       
                    if (tpc2==1&& b1==-1)                               //exculde transmission pipe
                    {
                        ENgetlinkvalue(i,9,out tmp2);                   // flow velocity
                        if (tmp2>v_high||tmp2<v_low)
                        {
                            c2 = false;
                            break;
                        }
                    }
                }
                if (c1&&c2)
                {
                    flag = false;
                }

            }
            
        }

        private void button8_Click(object sender, EventArgs e)
        {
            string fName = "";
            SaveFileDialog s1 = new SaveFileDialog();
            s1.DefaultExt = "inp";
            s1.Filter = "input files|*.INP|all files|*.*";
            if (s1.ShowDialog() == DialogResult.OK)
            {
                fName = s1.FileName;
            }
            int errorcode = ENsaveinpfile(fName);
            if (errorcode > 0)
            {
                MessageBox.Show("Error, code: " + errorcode.ToString());
            }
            else
                MessageBox.Show("Success!");
        }

        private void button9_Click(object sender, EventArgs e) // override the button7_click
        {
            float head_low = float.Parse(textBox7.Text);
            float head_high = float.Parse(textBox8.Text);
            float v_low = float.Parse(textBox9.Text);
            float v_high = float.Parse(textBox10.Text);
            int nodes_num, links_num;
            ENgetcount(0, out nodes_num);
            ENgetcount(2, out links_num);                     // get number of links and nodes
            string[] Tpipes = textBox13.Text.Split(',');
            int[] T_index = new int[Tpipes.Length];
            for (int i = 0; i < Tpipes.Length; i++)
            {
                ENgetlinkindex(Tpipes[i], out T_index[i]);
            }                                                 // get index of transmission pipes
            float[] pipe_diameter = new float[links_num];           // save pipes diameter into array 
            for (int i = 1; i <= links_num; i++)
            {
                ENgetlinkvalue(i, 0, out pipe_diameter[i - 1]);
            }
            Initialization s1 = new Initialization();
            s1.Set_length(links_num);
            int d_range = Convert.ToInt32(textBox6.Text);
            int d_min = Convert.ToInt32(textBox14.Text);
            bool flag = true;
            while (flag)
            {
                listBox3.Items.Clear();
                float[] p1 = s1.Gen_d(pipe_diameter,d_range,d_min);
                for (int i = 0; i < links_num; i++)        //set di
                {
                    int typecode = -1;
                    typecode = ENgetlinktype(i + 1, out typecode);
                    int b1 = Array.IndexOf(T_index, i + 1);             //exclude the transmission pipes
                    if ((typecode == 1 || typecode == 0) && b1 == -1)
                    {
                        ENsetlinkvalue(i + 1, 0, p1[i]);
                        listBox3.Items.Add("Pipe " + (i + 1).ToString() + " " + p1[i].ToString() + "mm");
                        listBox3.Refresh();
                    }
                }
                ENsolveH();
                bool c1 = true;
                int tpc1;
                for (int i = 1; i <= nodes_num; i++)
                {
                    float tmp1;
                    ENgetnodetype(i, out tpc1);
                    if (tpc1 == 0)
                    {
                        ENgetnodevalue(i, 11, out tmp1);              //node pressure
                        if (tmp1 > head_high || tmp1 < head_low)
                        {
                            c1 = false;
                            break;
                        }
                    }
                }
                bool c2 = true;
                int tpc2;
                for (int i = 1; i <= links_num; i++)
                {
                    float tmp2;
                    ENgetlinktype(i, out tpc2);
                    int b1 = Array.IndexOf(T_index, i);
                    if (tpc2 == 1 && b1 == -1)                               //exculde transmission pipe
                    {
                        ENgetlinkvalue(i, 9, out tmp2);                   // flow velocity
                        if (tmp2 > v_high || tmp2 < v_low)
                        {
                            c2 = false;
                            break;
                        }
                    }
                }
                if (c1 && c2)
                {
                    flag = false;
                }
            }
        }
    }
}
