using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySchedulers;
namespace Zadatak1.Demo
{
    
    /// <summary>
    /// Klasa za testiranje oba tipa rasporedjivaca
    /// </summary>
    class Program
    {
        /// <summary>
        /// Rad sa rasporedjivacima:
        /// Nakon instanciranja zeljenog rasporedjivaca,metodom setNumOfThreads(int) se navodi maksimalan broj niti koji smije biti 
        /// kreiran tokom procesa rasporedjivanja
        /// Nakon toga se kreira instanca klase TaskToDo i pozivom odgovarajucih metoda se dodjeljuje instanca prethodno kreiranog resursa
        /// kao i sve funkcije koje cine taj zadatak.
        /// Potom se poziva metoda instance klase zeljenog reasporedjivaca kako bi se tako kompletiran zadatak dodao
        /// U opciji je i pozivanje metode syncResurs(Resurs) kojom se navodi koji resurs mora biti sinhronizovan prilikom izvrsavanja
        /// zadataka koji ga posjeduju.
        /// Pozivom metode work() se pokrece instancirani rasporedjivac
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
           
            
            PreemptiveScheduler preemptiveScheduler = new PreemptiveScheduler();
            preemptiveScheduler.setNumOfThreads(3);
            
            TaskToDo task1 = new TaskToDo();
            Resurs resurs1 = new Resurs(1, 2);
            task1.addResurs(resurs1);
            task1.addDelegate((int a, int b, int res) => res + a + b);
            task1.addDelegate((int a, int b, int res)=> res + a * b);
            task1.addDelegate((int a, int b, int res) => res + a / a);
            task1.addDelegate((int a, int b, int res) => res + (int)Math.Pow((float)a, (float)b));
            preemptiveScheduler.addTask(task1);

            TaskToDo task2 = new TaskToDo();
            //Resurs resurs2 = new Resurs(3, 2);
            task2.addResurs(resurs1);
            task2.addDelegate((int a, int b, int res) => res * a / b);
            task2.addDelegate((int a, int b, int res) => res * a * b);
            task2.addDelegate((int a, int b, int res) => res * a + a);
            task2.addDelegate((int a, int b, int res) => res * (int)Math.Pow((float)a, (float)b));

            preemptiveScheduler.addTask(task2);

            TaskToDo task3 = new TaskToDo();
            //Resurs resurs3 = new Resurs(3, 1);
            Resurs resurs2 = new Resurs(3, 2);
            task3.addResurs(resurs2);
            task3.addDelegate((int a, int b, int res) => res * a + b);
            task3.addDelegate((int a, int b, int res) => res * a * b);
            task3.addDelegate((int a, int b, int res) => res * a + a);
            task3.addDelegate((int a, int b, int res) => res + (int)Math.Pow((float)a, (float)b));
            preemptiveScheduler.addTask(task3);

            preemptiveScheduler.syncResurs(resurs1);

            preemptiveScheduler.work();



            /*
              NonPreemptiveScheduler scheduler = new NonPreemptiveScheduler();
            scheduler.setNumOfThreads(8);

            TaskToDo task1 = new TaskToDo();
            Resurs resurs1 = new Resurs(1, 2);
            task1.addResurs(resurs1);
            task1.addDelegate((int a, int b, int res) => res + a + b);
            task1.addDelegate((int a, int b, int res) => res + a * b);
            task1.addDelegate((int a, int b, int res) => res + a / a);
            task1.addDelegate((int a, int b, int res) => res + (int)Math.Pow((float)a, (float)b));
            scheduler.addTask(task1);

            TaskToDo task2 = new TaskToDo();
            Resurs resurs2 = new Resurs(3, 2);
            task2.addResurs(resurs1);
            task2.addDelegate((int a, int b, int res) => res * a - b);
            task2.addDelegate((int a, int b, int res) => res * a * b);
            task2.addDelegate((int a, int b, int res) => res * a + a);
            task2.addDelegate((int a, int b, int res) => res * (int)Math.Pow((float)a, (float)b));
            scheduler.addTask(task2);


            TaskToDo task3 = new TaskToDo();
            Resurs resurs3 = new Resurs(4, 2);
            task3.addResurs(resurs3);
            task3.addDelegate((int a, int b, int res) => res * a + b);
            task3.addDelegate((int a, int b, int res) => res * a * b);
            task3.addDelegate((int a, int b, int res) => res * a + a);
            task3.addDelegate((int a, int b, int res) => res + (int)Math.Pow((float)a, (float)b));
            scheduler.addTask(task3);

            TaskToDo task4 = new TaskToDo();
            Resurs resurs4 = new Resurs(5, 2);
            task4.addResurs(resurs4);
            task4.addDelegate((int a, int b, int res) => res * a + b);
            task4.addDelegate((int a, int b, int res) => res * a * b);
            task4.addDelegate((int a, int b, int res) => res * a + a);
            task4.addDelegate((int a, int b, int res) => res + (int)Math.Pow((float)a, (float)b));
            scheduler.addTask(task4);

            scheduler.syncResurs(resurs1);

            scheduler.work();
             */

            Console.ReadLine();
        }
    }

    
}
