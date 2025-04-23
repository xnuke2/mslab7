using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mslab7
{
    public class Randomiser
    {
        private Random _random = new Random();

        private double E => Math.Pow(10, -DigitsCount);

        public double Start { get; set; } = -2 * Math.PI;

        public double End { get; set; } = Math.PI *3/2;

        public int DigitsCount { get; set; } = 5;

        public double f(double z)
        {
            return 1/(1+Math.Cos(z));
        }

        public double F(double z)
        {
            return Math.Tan(z/2);
        }

        public double XF(double z)
        {
            return 2 * z * Math.Log(z) - 2 * z;
        }

        public double X2F(double z)
        {
            return Math.Log(z) * Math.Pow(z, 2) - (Math.Pow(z, 2) / 2);
        }

        /// <summary>
        /// Метод половинного деления для нахождения обратной функции
        /// </summary>
        /// <param name="p">Вероятность от 0 до 1</param>
        public double HalvingMethod(double p)
        {
            double xStart = Start, xEnd = End;
            double x0 = 0;

            while (Math.Abs(xEnd - xStart) > E)           
            {
                x0 = (xEnd + xStart) / 2;
                double Fx0 = F(x0); // Вычисляем F(x0)

                if (Math.Abs(Fx0 - p) <= E)
                    break;

                if ((F(xStart) - p) * (Fx0 - p) < 0)
                    xEnd = x0;
                else
                    xStart = x0;
            }

            return Math.Round(x0, DigitsCount);
        }

        public double NextValue()
        {
            double u = _random.NextDouble();

            return HalvingMethod(u);
        }

        public void Reset()
        {
            _random = new Random();
        }
    }
}
