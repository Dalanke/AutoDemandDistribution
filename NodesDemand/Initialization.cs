using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodesDemand
{
    class Initialization
    {
        private int length;
        public void Set_length(int pipe_num)
        {
            length = pipe_num;
        }
        public float[] Gen_d(int lowerbound, int upperbound)
        {
            int b1 = lowerbound / 50;
            int b2 = upperbound / 50 + 1;
            float[] diameter = new float[length];
            for (int i = 0; i < length; i++)
            {
                Random rd = new Random(Guid.NewGuid().GetHashCode());
                diameter[i] = rd.Next(b1, b2) * 50;
            }
            return diameter;
        }
        public float[] Gen_d(float[] sample, int range, int min_di)  //overload, generate according to the sample
        {
            int b1 = -1 * range / 50;
            int b2 = range / 50 + 1;
            float[] diameter = new float[length];
            for (int i = 0; i < length; i++)
            {
                if (sample[i] == 0)
                {
                    diameter[i] = 0;
                }
                else
                {
                    Random rd = new Random(Guid.NewGuid().GetHashCode());
                    diameter[i] = rd.Next(b1, b2) * 50 + sample[i];
                    if (diameter[i] < min_di)
                    {
                        diameter[i] = min_di;
                    }
                }

            }
            return diameter;
        }

    }
}
