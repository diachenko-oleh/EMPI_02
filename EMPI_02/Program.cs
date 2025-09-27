using System.Collections.Concurrent;
using System.Runtime.Intrinsics.X86;

namespace EMPI_02
{
    internal class Program
    {
        static int N = 10;
        static double k = 0.4;
        static double b = 1.2;
        static void Main(string[] args)
        {
            Dictionary<int,double> points = [];
            for (int i = 1; i <= N+10; i++)
            {
                points[i] = Math.Round(k * i + b,2);
            }
            Console.WriteLine("вихiднi данi:");
            foreach (var item in points)
            {
                Console.WriteLine("x: "+item.Key+"  y: "+item.Value);
            }

            double sigma = Math.Sqrt(N / 5);
            double[] noise = new double[N + 10];
            Random rnd = new Random();
            for (int i = 0; i < N+10; i++)
            {
                double u1 = 1.0 - rnd.NextDouble(); 
                double u2 = 1.0 - rnd.NextDouble();
                double z0 = Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
                noise[i] = z0 * sigma;
            }

            Console.WriteLine("данi з \"шумом\":");
            for (int i = 0; i < N + 10; i++)
            {
                points[i + 1] = Math.Round(points[i + 1]+noise[i],2);
            }
            foreach (var item in points)
            {
                Console.WriteLine("x: " + item.Key + "  y: " + item.Value);
            }

            double sumX = 0;
            double sumSqX = 0;
            double sumY = 0;
            double sumSqY = 0;
            double sumXmultY = 0;
            for (int i = 1; i <= N+10; i++)
            {
                sumX += i;
                sumY += points[i];
                sumSqX+=(i * i);
                sumSqY+=(points[i] * points[i]);
                sumXmultY+=(i * points[i]);
            }
            double avX = sumX / (N + 10);
            double avY = sumY / (N + 10);
            double avSqX = sumSqX / (N + 10);
            double avSqY = sumSqY / (N + 10);
            double avXmultY = sumXmultY/ (N + 10);
            Console.WriteLine("середнє значення Х = " + avX);
            Console.WriteLine("середнє значення Y = " + avY);
            Console.WriteLine("середнє значення Х^2 = " + avSqX);
            Console.WriteLine("середнє значення Y^2 = " + avSqY);
            Console.WriteLine("середнє значення XY = " + avXmultY);
            double Sx = Math.Sqrt(avSqX - Math.Pow(avX, 2));
            double Sy = Math.Sqrt(avSqY - Math.Pow(avY, 2));
            Console.WriteLine("середнi квадратичнi вiдхилення:");
            Console.WriteLine("Sx = "+Sx);
            Console.WriteLine("Sy = "+Sy);
            double r = (avXmultY - (avX*avY)) / (Sx * Sy);
            Console.WriteLine("вибiрковий коефiцiєнт кореляцiї:");
            Console.WriteLine("r = "+r);
            double a = Math.Round(r * (Sy / Sx),3);
            double B = Math.Round(avY - (a * avX),3);
            Console.WriteLine("коефiцiєнт а = "+a);
            Console.WriteLine("коефiцiєнт b = "+B);
            Console.WriteLine("кiнцеве рiвняння:");
            Console.WriteLine($"y={a}x+{B}");
        }
    }
}
