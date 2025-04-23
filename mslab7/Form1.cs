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
        private int[] _samplesCount = { 50, 100, 1000, 100000, 1000000 };
        public Form1()
        {
            InitializeComponent();
            InitializeChart();
            InitializeDataGridView();
        }

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
            int x=3,y=3;
            foreach(int size in _samplesCount)
            {
                Chart chart = new Chart();
                chart.Titles.Add("Выборка из "+size);
                chart.Titles[0].Font = new Font("Arial", 10, FontStyle.Bold);
                chart.Location = new Point(x, y);
                chart.Size = new Size(359, 304);
                chart.Visible = true;
                ChartArea chartArea = new ChartArea();
                chart.ChartAreas.Add(chartArea);
                chart.ChartAreas[0].AxisX.Title = "z";
                chart.ChartAreas[0].AxisY.Title = "Плотность вероятности";
                Legend legend = new Legend();
                chart.Legends.Add(legend);
                panel1.Controls.Add(chart);
                // Генерация выборки
                double[] sample = GenerateSample(size);
                // Расчет статистик
                CalculateStatistics(sample);
                // Визуализация
                PlotResults(sample, chart);
                x += 359 + 5;
            }
        }

        private double[] GenerateSample(int n)
        {
            Random rand = new Random();
            double[] sample = new double[n];

            for (int i = 0; i < n; i++)
            {
                double u = rand.NextDouble();
                sample[i] = DistributionMath.InverseCumulativeDistribution(u);
            }

            return sample;
        }

        private void CalculateStatistics(double[] sample)
        {
            double mean = sample.Average();
            double variance = sample.Select(x => Math.Pow(x - mean, 2)).Sum() / (sample.Length - 1);

            // Проверка критерием Колмогорова
            Array.Sort(sample);
            double maxDiff = 0;

            for (int i = 0; i < sample.Length; i++)
            {
                double F_emp = (i + 1.0) / sample.Length;
                double F_theo = DistributionMath.CumulativeDistribution(sample[i]);
                double diff = Math.Abs(F_emp - F_theo);

                if (diff > maxDiff)
                    maxDiff = diff;
            }

            double lambda = maxDiff * Math.Sqrt(sample.Length);

            // Вывод результатов
            dataGridView1.Rows.Add( sample.Length ,mean, variance, lambda, lambda <= 1.22 ? "Да" : "Нет");
            //lblResults.Text = $"Результаты:\n" +
            //                 $"Мат. ожидание: {mean:F4}\n" +
            //                 $"Дисперсия: {variance:F4}\n" +
            //                 $"λ (Колмогоров): {lambda:F4}\n" +
            //                 $"Согласие: {(lambda <= 1.22 ? "Да" : "Нет")}";
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
            Series theory = new Series("Теория")
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
