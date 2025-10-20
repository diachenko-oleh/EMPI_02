using MathNet.Numerics;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Providers.LinearAlgebra;
using System.Collections.Concurrent;
using System.Globalization;
using System.Runtime.Intrinsics.X86;

namespace EMPI_02
{
    internal class Program
    {
        static double alpha = 0.01;
        static int N;
        static void Main(string[] args)
        {
            /*
            Console.WriteLine("enter nums by space:");
            string numLine = Console.ReadLine();
            string[] S = numLine.Trim().Split();
            List<int> nums = [];
            foreach (var item in S)
            {
               int n = int.Parse(item);
                if (n >= 0 && n <= 20) nums.Add(n);

            }
            Console.WriteLine("enter alpha:");
            double a = double.Parse(Console.ReadLine());
            alpha = a;
            */

            List<int> nums = [];
            Console.WriteLine("введiть к-сть елементiв: ");
            N = int.Parse(Console.ReadLine());
            for (int i = 0; i < N; i++)
            {
                Random rnd = new Random();
                nums.Add(rnd.Next(0, 21));
                
            }
            nums.Clear();
            var uniform = new ContinuousUniform(0, 21);
            for (int i = 0; i < N; i++)
            {
                double x = uniform.Sample(); // повертає double
                nums.Add((int)Math.Round(x)); // округлюємо до int
            }
            N = nums.Count;
            nums.Sort();
            Console.WriteLine("Вибiрка:");
            foreach (var item in nums)
            {
                Console.Write(item+" ");
            }
            Console.WriteLine("\nЧастоти кожного з елементiв:");
            Dictionary<int, int> freq = [];
            foreach (var item in nums)
            {
                if (freq.ContainsKey(item)) freq[item]++;
                else freq[item] = 1;
            }
            foreach (var item in freq)
            {
                Console.Write(item+" ");
            }
            Console.WriteLine();

            // ПЕРЕВІРКА НА НОРМАЛЬНИЙ РОЗПОДІЛ
            //Normal(nums,freq);



            // ПЕРЕВІРКА НА РІВНОМІРНИЙ РОЗПОДІЛ
            Uniform(nums, freq);
            




        }

        static void Normal(List<int> nums, Dictionary<int, int> freq)
        {
            //знаходимо Хср

            double Xav = 0;
            foreach (var item in nums)
            {
                Xav += item;
            }
            Xav /= N;
            Console.WriteLine("X середнє: " + Xav);

            //знаходимо σср

            double σ = 0;
            foreach (var item in nums)
            {
                σ += Math.Pow(item - Xav, 2);
            }
            σ = Math.Sqrt(σ / N);
            Console.WriteLine("Cереднє квадратичне вiдхилення вибiрки: " + σ);

            //знаходимо очікувані та спостережувані частоти 

            Dictionary<int, double> E = [];
            Dictionary<int, double> O = [];
            var normal = new Normal(Xav, σ);
            int ind = 1;
            double amountO = 0;
            double amountE = 0;
            foreach (var item in freq)
            {
                double Ei = (normal.CumulativeDistribution(item.Key + 0.5) - normal.CumulativeDistribution(item.Key - 0.5)) * N;
                amountO += item.Value;
                amountE += Ei;
                if (amountE >= 5)
                {
                    E[ind] = amountE;
                    O[ind] = amountO;
                    ind++;
                    amountO = 0;
                    amountE = 0;
                }
            }
            if (amountE > 0)
            {
                E[ind] = amountE;
                O[ind] = amountO;
            }
            Console.WriteLine("Iнтервали та їх очiкуванi частоти: \n[номер_iнтервалу, очiкувана частота]");
            foreach (var item in E)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("Iнтервали та їх спостережуванi частоти: \n[номер_iнтервалу, спостережувана частота]");
            foreach (var item in O)
            {
                Console.WriteLine(item);
            }

            //знаходимо χ² та кількість ступенів свободи
            double ksiSq = 0;
            for (int i = 0; i < E.Count; i++)
            {
                ksiSq += Math.Pow(O[i + 1] - E[i + 1], 2) / E[i + 1];
            }
            int k = E.Count - 2 - 1;
            Console.WriteLine("---");
            Console.WriteLine("Х^2 емпiричне: " + ksiSq);
            Console.WriteLine("Рiвень значущостi a: " + alpha);
            Console.WriteLine("Кiлькiть ступенiв свободи k: " + k);
            Console.WriteLine("---");
        }

        static void Uniform(List<int> nums, Dictionary<int, int> freq)
        {
            //знаходимо очікувані та спостережувані частоти

            Console.WriteLine("Кiлькiсть унiкальних елементiв " + freq.Count);
            int amountOfIntervals = freq.Count / 5;
            int currInvertal = 1;
            Console.WriteLine("Подiлимо вибiрку на "+ amountOfIntervals+ " iнтервалiв");

            int maxElementsInInterval = freq.Count/amountOfIntervals;
            Console.WriteLine("Тодi кiлькiсть елементiв в iнтервалi " + maxElementsInInterval);

            Dictionary<int, double> E = [];
            Dictionary<int, double> O = [];
            
            double amount = 0;
            List<int> interval = [];
            foreach (var item in freq)
            {
                interval.Add(item.Value);
                if (interval.Count > maxElementsInInterval)
                {
                    E[currInvertal] = (double)N / amountOfIntervals;
                    O[currInvertal] = interval.Sum();
                    if (currInvertal < amountOfIntervals) currInvertal++;
                    interval.Clear();
                    
                }
               
            }
            if (interval.Count > 0)
            {

                E[currInvertal] = (double)N / amountOfIntervals;
                O[currInvertal] = interval.Sum();
            }
          
            Console.WriteLine("Iнтервали та їх очiкуванi частоти: \n[номер_iнтервалу, очiкувана частота]");
            foreach (var item in E)
            {
                Console.WriteLine(item);
            }
            Console.WriteLine("Iнтервали та їх спостережуванi частоти: \n[номер_iнтервалу, спостережувана частота]");
            foreach (var item in O)
            {
                Console.WriteLine(item);
            }

            double ksiSq = 0;
            for (int i = 0; i < E.Count; i++)
            {
                ksiSq += Math.Pow(O[i + 1] - E[i + 1], 2) / E[i + 1];
            }
            int k = E.Count - 1;
            Console.WriteLine("---");
            Console.WriteLine("Х^2 емпiричне: " + ksiSq);
            Console.WriteLine("Рiвень значущостi a: " + alpha);
            Console.WriteLine("Кiлькiть ступенiв свободи k: " + k);
            Console.WriteLine("---");
        }
    }
}
