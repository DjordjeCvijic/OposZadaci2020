using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySchedulers;
namespace Zadatak1.Demo
{
    

    class Program
    {
        static void Main(string[] args)
        {
            NonPreemptiveScheduler scheduler = new NonPreemptiveScheduler();
            scheduler.setNumOfThreads(4);
            TaskToDo task1 = new TaskToDo();
            Resurs resurs1 = new Resurs(1, 2);
            task1.addResurs(resurs1);
            task1.addDelegate((int a, int b, int rez) =>  rez + a + b);
            task1.addDelegate((int a, int b, int rez) =>  rez + a * b);
            task1.addDelegate((int a, int b, int rez) =>  rez + a + a);
            task1.addDelegate((int a, int b, int rez) =>  rez + (int)Math.Pow((float)a, (float)b));
            scheduler.addTask(task1);

            TaskToDo task2 = new TaskToDo();
            Resurs resurs2 = new Resurs(3, 2);
            task2.addResurs(resurs2);
            task2.addDelegate((int a, int b, int rez) => rez * a + b);
            task2.addDelegate((int a, int b, int rez) => rez * a * b);
            task2.addDelegate((int a, int b, int rez) => rez * a + a);
            task2.addDelegate((int a, int b, int rez) => rez * (int)Math.Pow((float)a, (float)b));
            scheduler.addTask(task2);


            TaskToDo task3 = new TaskToDo();
            Resurs resurs3 = new Resurs(4, 2);
            task3.addResurs(resurs3);
            task3.addDelegate((int a, int b, int rez) => rez * a + b);
            task3.addDelegate((int a, int b, int rez) => rez * a * b);
            task3.addDelegate((int a, int b, int rez) => rez * a + a);
            task3.addDelegate((int a, int b, int rez) => rez * (int)Math.Pow((float)a, (float)b));
            scheduler.addTask(task3);

            scheduler.work();
            Console.ReadLine();
        }
    }

    
}
