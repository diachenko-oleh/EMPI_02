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
        static double alpha = 0.05;
        static int N;
        static void Main(string[] args)
        {
            List<int> nums = [];
            Console.WriteLine("Режим введення:\n1- Вручну\n2- Згенерувати вибiрку за нормальним законом розподiлу\n3- Згенерувати вибiрку за рiвномiрним законом розподiлу");
            int choice = int.Parse(Console.ReadLine());
            switch (choice)
            {
                case 1:
                    Console.WriteLine("Введiть числа від 0 до 20 через пробіл: (для нормальної роботи програми слiд ввести >30 чисел)");
                    string numLine = Console.ReadLine();
                    string[] S = numLine.Trim().Split();

                    foreach (var item in S)
                    {
                        int n = int.Parse(item);
                        if (n >= 0 && n <= 20) nums.Add(n);

                    }
                    N = nums.Count;
                    break;
                case 2:
                    Console.WriteLine("Введiть к-сть елементiв: ");
                    N = int.Parse(Console.ReadLine());
                    Console.WriteLine("Введiть мат. сподsвання: (за замовчуванням 10)");
                    double mu = 10;
                    string MU = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(MU))
                    {
                        mu = double.Parse(MU);
                    }
                    Console.WriteLine("Введiть вiдхилення: (за замовчуванням 5)");
                    double sigma = 5;
                    string SGM = Console.ReadLine();
                    if (!string.IsNullOrWhiteSpace(SGM))
                    {
                        sigma = double.Parse(SGM);
                    }
                    Normal normal = new Normal(mu, sigma);
                    for (int i = 0; i < N; i++)
                    {
                        double x = normal.Sample();
                        nums.Add((int)Math.Round(x));
                    }
                    break;
                case 3:
                    Console.WriteLine("Введiть к-сть елементiв: ");
                    N = int.Parse(Console.ReadLine());
                    var uniform = new ContinuousUniform(0, 20);
                    for (int i = 0; i < N; i++)
                    {
                        double x = uniform.Sample();
                        nums.Add((int)Math.Round(x)); 
                    }
                    break;
                default:
                    Main(args);
                    break;
            }
           
            Console.WriteLine("Введiть рiвень значущостi а: (за замовчуванням 0.05)");
            string A = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(A))
            {
                double a = double.Parse(A);
                alpha = a;
            }
           
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
            Console.WriteLine("\n");

            // ПЕРЕВІРКА НА НОРМАЛЬНИЙ РОЗПОДІЛ
            Console.WriteLine("-----ПЕРЕВIРКА НА НОРМАЛЬНИЙ РОЗПОДIЛ-----");
            Normal(nums,freq);

            // ПЕРЕВІРКА НА РІВНОМІРНИЙ РОЗПОДІЛ
            Console.WriteLine("-----ПЕРЕВІРКА НА РІВНОМІРНИЙ РОЗПОДІЛ-----");
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
            Console.WriteLine("X_середнє: " + Xav);

            //знаходимо σср

            double σ = 0;
            foreach (var item in nums)
            {
                σ += Math.Pow(item - Xav, 2);
            }
            σ = Math.Sqrt(σ / N);
            Console.WriteLine("Cереднє квадратичне вiдхилення вибiрки: " + σ+"\n");

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
            int k = E.Count - 1;

            Console.WriteLine("--- Висновок ---");
            Console.WriteLine("Рiвень значущостi a: " + alpha);
            Console.WriteLine("Кiлькiть ступенiв свободи k: " + k);
            double ksiCrit = ChiSquared.InvCDF(k, 1 - alpha);
            Console.WriteLine("Х^2 емпiричне: " + ksiSq);
            Console.WriteLine("Х^2 критичне: " + ksiCrit);
            if (ksiCrit > ksiSq) Console.WriteLine("Гiпотезу приймаємо");
            else Console.WriteLine("Гiпотезу вiдхиляємо");
            Console.WriteLine("--- -------- ---\n");
        }

        static void Uniform(List<int> nums, Dictionary<int, int> freq)
        {
            //знаходимо очікувані та спостережувані частоти

            Console.WriteLine("Кiлькiсть унiкальних елементiв " + freq.Count);
            int amountOfIntervals = freq.Count / 5;

            if (amountOfIntervals < 2)
            {
                amountOfIntervals = 3;
                Console.WriteLine("Вибірка не пiдходить до розбиття, тому встановлено 3 за замовчуванням");
            }
            int currInvertal = 1;
            Console.WriteLine("Подiлимо вибiрку на "+ amountOfIntervals+ " iнтервалiв");

            int maxElementsInInterval = freq.Count/amountOfIntervals;
            Console.WriteLine("Тодi кiлькiсть елементiв в iнтервалi " + maxElementsInInterval+"\n");

            Dictionary<int, double> E = [];
            Dictionary<int, double> O = [];
            
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

            Console.WriteLine("--- Висновок ---");
            Console.WriteLine("Рiвень значущостi a: " + alpha);
            Console.WriteLine("Кiлькiть ступенiв свободи k: " + k);
            double ksiCrit = ChiSquared.InvCDF(k, 1 - alpha);
            Console.WriteLine("Х^2 емпiричне: " + ksiSq);
            Console.WriteLine("Х^2 критичне: " + ksiCrit);
            if (ksiCrit > ksiSq) Console.WriteLine("Гiпотезу приймаємо");
            else Console.WriteLine("Гiпотезу вiдхиляємо");
            Console.WriteLine("--- -------- ---\n");
        }
    }
}
