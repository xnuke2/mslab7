using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mslab7
{
    public static class DistributionMath
    {
        public static double Start = - Math.PI/2;
        public static double End =Math.PI/2;
        private static double E => Math.Pow(10, -DigitsCount);
        public static int DigitsCount { get; set; } = 5;

        // Исходная плотность распределения (ПРВ)
        public static double SourceDensity(double z)
        {
            return 0.5 * (1 / (1 + Math.Cos(z)));
        }        

        // Функция распределения (ФРВ)
        public static double CumulativeDistribution(double z)
        {

            return (Math.Tan(z/2)+1)*0.5;
        }

        // Обратная функция распределения
        public static double InverseCumulativeDistribution(double u)
        {

            return 2 * Math.Atan(2* u);
        }
        public static List<(double Value, double Probability)> GetValueProbabilityPairs(double[] sample)
        {
            var groups = sample
                .GroupBy(z => z)
                .Select(g => (Value: g.Key, Probability: (double)g.Count() / sample.Length))
                .OrderBy(x => x.Value)
                .ToList();

            return groups;
        }

        public static double CalculateExpectedValue(List<(double Value, double Probability)> pairs)
        {
            return pairs.Sum(pair => pair.Value * pair.Probability);
        }

        // Численное интегрирование для проверки нормировки
        public static double CalculateNormalizationConstant()
        {
            int steps = 10000;
            double sum = 0;
            double dz = Math.PI / steps;

            for (int i = 0; i < steps; i++)
            {
                double z = -Math.PI / 2 + i * dz;
                sum += SourceDensity(z) * dz;
            }

            return sum;
        }
        public static double TheoreticalVariance()
        {
            // Численный расчет теоретической дисперсии
            int steps = 100000;
            double sum = 0;
            double dz = Math.PI / steps;

            for (int i = 0; i < steps; i++)
            {
                double z = -Math.PI / 2 + i * dz;
                sum += z * z / (1 + Math.Cos(z)) * dz;
            }
            return sum / 2; // ≈0.467
        }
    }
}
