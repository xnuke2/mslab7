using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace mslab7
{
    public partial class Form1 : Form
    {
        private int[] _samplesCount = {50,100,1000,  1000000 };
        public Form1()
        {
            InitializeComponent();
            InitializeChart();
            InitializeDataGridView();
        }
        Random rand = new Random();
        private void InitializeChart()
        {
            //chart1.Series.Clear();
            //chart1.ChartAreas[0].AxisX.Title = "z";
            //chart1.ChartAreas[0].AxisY.Title = "Плотность вероятности";
            chart2.Series.Clear();
            chart2.ChartAreas[0].AxisX.Title = "x";
            chart2.ChartAreas[0].AxisY.Title = "y";
            Series theory = new Series("f()")
            {
                ChartType = SeriesChartType.Line,
                Color = System.Drawing.Color.Coral,
                BorderWidth = 2
            };

            for (double z = DistributionMath.Start; z <= DistributionMath.End; z += 0.01)
                theory.Points.AddXY(z, DistributionMath.SourceDensity(z));
            
            chart2.Series.Add(theory);
            chart3.Series.Clear();
            chart3.ChartAreas[0].AxisX.Title = "x";
            chart3.ChartAreas[0].AxisY.Title = "y";

            theory = new Series("F()")
            {
                ChartType = SeriesChartType.Line,
                Color = System.Drawing.Color.Brown,
                BorderWidth = 2
            };

            for (double z = DistributionMath.Start; z <= DistributionMath.End; z += 0.01)
                theory.Points.AddXY(z, DistributionMath.CumulativeDistribution(z));
            chart3.Series.Add(theory);
        }

        private void InitializeDataGridView()
        {
            dataGridView1.Columns.Clear();
            dataGridView1.Columns.Add("Size", "Размер выборки");
            dataGridView1.Columns.Add("MO", "Мат. ожидание");
            dataGridView1.Columns.Add("D", "Дисперсия");
            dataGridView1.Columns.Add("λ", "λ (Колмогоров)");
            dataGridView1.Columns.Add("agree", "Согласие");
            dataGridView1.Rows.Add("теоретическая", 0, DistributionMath.TheoreticalVariance());
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            InitializeDataGridView();
            panel1.Controls.Clear();
            if (checkBox1.Checked)
                rand = new Random();
            int x=3,y=3;
            foreach(int size in _samplesCount)
            {
                Chart chart = new Chart();
                chart.Titles.Add("Выборка из "+size);
                chart.Titles[0].Font = new Font("Arial", 10, FontStyle.Bold);
                chart.Location = new Point(x, y+ 304+5);
                chart.Size = new Size(359, 304);
                chart.Visible = true;
                ChartArea chartArea = new ChartArea();
                chart.ChartAreas.Add(chartArea);
                //chart.ChartAreas[0].AxisX.Title = "z";
                //chart.ChartAreas[0].AxisY.Title = "Плотность вероятности";
                Legend legend = new Legend();
                chart.Legends.Add(legend);
                panel1.Controls.Add(chart);
                // Генерация выборки
                double[] sample = GenerateSample(size);
                // Расчет статистик
                CalculateStatistics(sample,chart);
                // Визуализация
                chart = new Chart();
                chart.Titles.Add("Выборка из " + size);
                chart.Titles[0].Font = new Font("Arial", 10, FontStyle.Bold);
                chart.Location = new Point(x, y);
                chart.Size = new Size(359, 304);
                chart.Visible = true;
                chartArea = new ChartArea();
                chart.ChartAreas.Add(chartArea);
                chart.ChartAreas[0].AxisX.Title = "z";
                chart.ChartAreas[0].AxisY.Title = "Плотность вероятности";
                legend = new Legend();
                chart.Legends.Add(legend);
                panel1.Controls.Add(chart);
                PlotResults(sample, chart);
                x += 359 + 5;
            }
        }

        private double[] GenerateSample(int n)
        {

            double[] sample = new double[n];

            for (int i = 0; i < n; i++)
            {
                double u = rand.NextDouble()-0.5;
                sample[i] = DistributionMath.InverseCumulativeDistribution(u);
            }

            return sample;
        }

        private void CalculateStatistics(double[] sample, Chart chart1)
        {
            
            var pairs = DistributionMath.GetValueProbabilityPairs(sample);
            double mean = DistributionMath.CalculateExpectedValue(pairs);
            double variance = sample.Select(x => Math.Pow(x - mean, 2)).Sum() / (sample.Length - 1);

            // Проверка критерием Колмогорова
            Array.Sort(sample);
            double maxDiff1 = 0; 
            double maxDiff2 = 0;

            for (int i = 0; i < sample.Length; i++)
            {
                double F_emp = (i + 1.0) / sample.Length; // F^*(x_i)
                double F_theo = DistributionMath.CumulativeDistribution(sample[i]); // F(x_i)

                // Вариант 1: F^* выше F
                double diff1 = Math.Abs(F_emp - F_theo);
                if (diff1 > maxDiff1) maxDiff1 = diff1;

                // Вариант 2: F^* ниже F (используем предыдущее F^*)
                if (i > 0)
                {
                    double F_emp_prev = i / (double)sample.Length; // F^*(x_{i-1})
                    double diff2 = Math.Abs(F_theo - F_emp_prev);
                    if (diff2 > maxDiff2) maxDiff2 = diff2;
                }
            }

            double delta_p = Math.Max(maxDiff1, maxDiff2);
            double lambda = delta_p * Math.Sqrt(sample.Length);

            // Вывод результатов
            dataGridView1.Rows.Add( sample.Length ,mean, variance, lambda, lambda <= 1.22 ? "Да" : "Нет");
            // Теоретическая F(x)
            Series theoryCDF = new Series("F(x)")
            {
                ChartType = SeriesChartType.Line,
                Color = Color.Red
            };
            for (double z = -Math.PI / 2; z <= Math.PI / 2; z += 0.01)
            {
                theoryCDF.Points.AddXY(z, DistributionMath.CumulativeDistribution(z));
            }
            chart1.Series.Add(theoryCDF);

            // Эмпирическая F^*(x)
            Series empCDF = new Series("F^*(x)")
            {
                ChartType = SeriesChartType.StepLine,
                Color = Color.Blue
            };
            Array.Sort(sample);
            for (int i = 0; i < sample.Length; i++)
            {
                empCDF.Points.AddXY(sample[i], (i + 1.0) / sample.Length);
            }
            chart1.Series.Add(empCDF);

        }

        private void PlotResults(double[] sample,Chart chart1)
        {
            chart1.Series.Clear();

            // Гистограмма
            Series hist = new Series("Выборка")
            {
                ChartType = SeriesChartType.Column,
                BorderWidth = 1
            };

            int bins = 20;
            double min = DistributionMath.Start;
            double max = DistributionMath.End;
            double binSize = (max - min) / bins;

            for (int i = 0; i < bins; i++)
            {
                double lower = min + i * binSize;
                double upper = lower + binSize;
                int count = sample.Count(x => x >= lower && x < upper);
                hist.Points.AddXY(
                    (lower + upper) / 2,
                    count / ((double)sample.Length * binSize)
                );
            }
            chart1.Series.Add(hist);

            // Теоретическая кривая
            Series theory = new Series("Теоритическая")
            {
                ChartType = SeriesChartType.Line,
                Color = System.Drawing.Color.Red,
                BorderWidth = 2
            };

            for (double z = min; z <= max; z += 0.01)
            {
                theory.Points.AddXY(z, DistributionMath.SourceDensity(z));
            }
            chart1.Series.Add(theory);


        }
    }
}
