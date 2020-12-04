using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MySchedulers
{

    /// <summary>
    /// Delegat za funkcije od kojih ce biti sacinjen svaki zadatak.
    /// Svaki funkcija u jednom zadatku mora da prima tri cjelobrojna argumenta i vraca cjelobrojan rezultat.
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="rez"></param>
    /// <returns></returns>
    public delegate int MethodDelegate(int firstOperand, int secondOperand, int result);
   
    /// <summary>
    /// Klasa kojom se vrsi preventivno rasporedjivanje procesa.
    /// </summary>
    public class PreemptiveScheduler 
    {
        /// <summary>
        /// Lista zadataka koji ce biti biti rasporedjeni na niti.
        /// </summary>
        public List<TaskToDo> listOfTasks = new List<TaskToDo>();
        /// <summary>
        /// Lista instanci klase MyThreadForPreemptiveScheduler na kojima ce biti izvrsivani zadaci.
        /// </summary>
        public List<MyThreadForPreemptiveScheduler> threadPool = new List<MyThreadForPreemptiveScheduler>();
        /// <summary>
        /// Broj niti koji ce biti na koristenju resporedjivacu.
        /// Ako se ekcplicitno ne navede druga vrijednost,podrazumijevana je 3.
        /// </summary>
        private int numOfThreads = 3;
        /// <summary>
        /// Mapa statusa resursa koji ce biti sinhronizovani pri rasporedjivanju.
        /// </summary>
        private Dictionary<Resurs, ObjectSatus> mapOfResursStatus = new Dictionary<Resurs, ObjectSatus>();
        /// <summary>
        /// Mapa statusa zadataka u svakom trenutku procesa rasporedjivanja.
        /// </summary>
        private Dictionary<TaskToDo, ObjectSatus> mapOfTaskStatus = new Dictionary<TaskToDo, ObjectSatus>();

        public PreemptiveScheduler()
        {
             
        }
        /// <summary>
        /// Metoda za dodavanje zadataka u listu zadataka listOfTasks i mapu mapOfTaskStatus.
        /// </summary>
        /// <param name="task"></param>
        public void addTask(TaskToDo task)
        {
            listOfTasks.Add(task);
            mapOfTaskStatus.Add(task,new ObjectSatus());
        }
        /// <summary>
        /// Metoda za eksplicitno navodjenje maksimalnog broja niti koje ce biti kreirane u procesu rasporedjivanja.
        /// </summary>
        /// <param name="i"></param>
        public void setNumOfThreads(int i)
        {
            numOfThreads = i;
        }
        /// <summary>
        /// Metoda kojom se navodi resurs koji ce biti sinhronizovano modifikovan pri izvrsavanju zadataka koji ga posjeduju.
        /// </summary>
        /// <param name="resurs"></param>
        public void syncResurs(Resurs resurs)
        {
            mapOfResursStatus.Add(resurs, new ObjectSatus());
        }
        /// <summary>
        /// Metoda kojom se pokrece rasporedjivanje
        /// </summary>
        public void work()
        {

            while (listOfTasks.Count != 0)
            {
                TaskToDo task = listOfTasks[0];
                listOfTasks.RemoveAt(0);
                if (task.listOfDelegates.Count != 0)
                {
                    
                    ObjectSatus taskStatus = mapOfTaskStatus[task];
                    if ((isResursFree(task.resurs) || task.ILockedResurs)  && !taskStatus.inUse)
                    {

                        ObjectSatus resursStatus = mapOfResursStatus.ContainsKey(task.resurs) ? mapOfResursStatus[task.resurs] : null;
                        
                        if ((threadPool.Count < numOfThreads) && isAllThreadsWork())
                        {

                            MyThreadForPreemptiveScheduler thread = new MyThreadForPreemptiveScheduler();
                            doContextSwitching(thread,resursStatus,taskStatus,task);
                            
                            listOfTasks.Add(task);
                            threadPool.Add(thread);
                            new Thread(thread.run).Start();
                            Thread.Sleep(50);
                        }
                        else
                        {
                           
                            MyThreadForPreemptiveScheduler thread = getFreeThread();
                            doContextSwitching(thread, resursStatus, taskStatus,task);

                            listOfTasks.Add(task);
                            thread.restartThread();
                            Thread.Sleep(50);

                        }

                    }
                    else
                    {
                        listOfTasks.Add(task);
                    }
                }
              
                

            }
            while (threadPool.Count != 0) {
                foreach(MyThreadForPreemptiveScheduler thread in threadPool)
                {
                    if (thread.inRun == false)
                    {
                        thread.terminateThread();
                        thread.restartThread();
                        threadPool.Remove(thread);
                        break;
                    }
                }
            }
            Console.WriteLine("Svi tredovi terminirani");


        }
        /// <summary>
        /// Metoda kojom se vrsi postavljanje resursa i potrebnih parametara za izvrsavanje zadatka na nekoj niti.
        /// </summary>
        /// <param name="thread"></param>
        /// <param name="resursStatus"></param>
        /// <param name="taskStatus"></param>
        /// <param name="task"></param>
        private void doContextSwitching(MyThreadForPreemptiveScheduler thread, ObjectSatus resursStatus, ObjectSatus taskStatus,TaskToDo task)
        {
            Console.WriteLine("Promjena na tredu: " + thread.id + " stavlja se task: " + task.id);

            if (resursStatus != null)
            {
                resursStatus.inUse = true;
                task.ILockedResurs = true;
            }
            thread.resursStatus = resursStatus;
            thread.taskStatus = taskStatus;
            
            taskStatus.inUse = true;
            thread.taskToDo = task;
        }
        /// <summary>
        /// Metoda koja provjerava da li su sve niti u threadPool-u pokrenute.
        /// </summary>
        private bool isAllThreadsWork()
        {
            bool flag = true;
            foreach(MyThreadForPreemptiveScheduler t in threadPool)
            {
                if (t.inRun == false)
                    flag = false;
            }
            return flag;
        }
        /// <summary>
        /// Metoda koja provjerava da li je prosljedjeni resurs zauzet od strane nekog zadatka koji se trenutno izvrsava.
        /// </summary>
        /// <param name="resurs"></param>
        /// <returns></returns>
        private bool isResursFree(Resurs resurs)
        {
            if (mapOfResursStatus.ContainsKey(resurs))
            {
                ObjectSatus status = mapOfResursStatus[resurs];
                return !status.inUse;
            }
            else
                return true;

        }
        /// <summary>
        /// Metoda koja vraca nit koja je trenutno u stanju cekanja i moze se iskoristiti za izvrsavanje zadatka.
        /// </summary>
        /// <returns></returns>
        private MyThreadForPreemptiveScheduler getFreeThread()
        {
            MyThreadForPreemptiveScheduler t = null;
            while (t==null)
            {
                foreach(MyThreadForPreemptiveScheduler thread in threadPool)
                {
                    if (!thread.inRun)
                    {
                        t = thread;
                        break;
                    }
                }
                
               
            }
            return t;
        }

    }
   /// <summary>
   /// Klasa u kojoj je implementiran rad niti u preventivnom rasporedjivanju zadataka.
   /// </summary>
    public class MyThreadForPreemptiveScheduler
    {
        /// <summary>
        /// Zadatak koji se izvrsava na niti.
        /// </summary>
        public TaskToDo taskToDo;
        /// <summary>
        /// Stanje niti (pokrenuta ili ne).
        /// </summary>
        public bool inRun = false;
        private static int staticId = 1;
        public int id = staticId++;
        private bool stop;
        /// <summary>
        /// Status resursa koji se obradjuje na niti.
        /// </summary>
        public ObjectSatus resursStatus;
        /// <summary>
        /// Stanje zadatka koji se izvrsava na niti.
        /// </summary>
        public ObjectSatus taskStatus;
        /// <summary>
        /// Vremenski interval koji odredjuje koliko dugo ce se jedan zadatak izvrsavati na niti prije promjene konteksta.
        /// </summary>
        private int timeInterval = 500;
        private TimeSpan elapsedTime;
        private bool flagEnd=false;
        private Stopwatch stopWatch = new Stopwatch();
        private MethodDelegate fun = null;
        private Random rnd = new Random();
        public MyThreadForPreemptiveScheduler()
        {
           
        }
        /// <summary>
        /// Metoda kojom je implementirana logika niti pri preventivnom izvrsavanju zadataka.
        /// </summary>
        public void run()
        {
            
            while (!flagEnd)
            {
                inRun = true;
                elapsedTime = new TimeSpan(0);
                stopWatch.Reset();
                stopWatch.Start();
                while (taskToDo.listOfDelegates.Count != 0 && elapsedTime.TotalMilliseconds<timeInterval)
                {
                    
                    fun = taskToDo.listOfDelegates[0];
                    taskToDo.listOfDelegates.RemoveAt(0);
                    taskToDo.resurs.result = fun(taskToDo.resurs.firstOperand, taskToDo.resurs.secondOperand, taskToDo.resurs.result);
                    int timeToSleep = rnd.Next(2, 5) * 100;
                    Console.WriteLine("Task: " + taskToDo.id + " se izvrsava na tredu: " + id + " i spavace " + timeToSleep+" trenutni rez: "+ taskToDo.resurs.result);
                    Thread.Sleep(timeToSleep);
                    elapsedTime = stopWatch.Elapsed;
                }
                
                if (resursStatus != null && taskToDo.listOfDelegates.Count == 0)
                {
                    resursStatus.inUse = false;
                    taskToDo.ILockedResurs = false;
                }
                taskStatus.inUse = false;
                inRun = false;
                stop = true;
                while (stop) { }

            }

            Console.WriteLine("Tred " + id + " je terminiran");

        }
        /// <summary>
        /// Metoda koja se poziva nakon promjene konteksta kako bi nit pocela sa izvrsavanjem zadatka koji je postavljen.
        /// </summary>
        public void restartThread()
        {
            stop = false;
        }
        /// <summary>
        /// Metoda za "Unistavanje" nit.
        /// </summary>
        public void terminateThread()
        {
            flagEnd = true;
        }
    }

    /// <summary>
    /// Klasa kojom se vrsi nepreventivno rasporedjivanje procesa-zadataka.
    /// </summary>
    public class NonPreemptiveScheduler
    {
        /// <summary>
        /// Lista instanci klase MyThreadForNonPreemptiveScheduler na kojima ce biti izvrsivani zadaci.
        /// </summary>
        public List<MyThreadForNonPreemptiveScheduler> threadPool = new List<MyThreadForNonPreemptiveScheduler>();
        /// <summary>
        /// Lista zadataka koji ce biti biti rasporedjeni na niti.
        /// </summary>
        public List<TaskToDo> listOfTasks = new List<TaskToDo>();
        /// <summary>
        /// Broj niti koji ce biti na koristenju resporedjivacu.
        /// Ako se ekcplicitno ne navede druga vrijednost,podrazumijevana je 3.
        /// </summary>
        private int numOfThreads = 3;
        /// <summary>
        /// Mapa statusa resursa koji ce biti sinhronizovani pri rasporedjivanju.
        /// </summary>
        private Dictionary<Resurs, ObjectSatus> mapOfResursStatus = new Dictionary<Resurs, ObjectSatus>();
        /// <summary>
        /// Metoda za dodavanje zadataka u listu zadataka listOfTasks.
        /// </summary>
        /// <param name="task"></param>
        public void addTask(TaskToDo task)
        {
            listOfTasks.Add(task);
        }
        /// <summary>
        /// Metoda za eksplicitno navodjenje maksimalnog broja niti koje ce biti kreirane u procesu rasporedjivanja.
        /// </summary>
        /// <param name="i"></param>
        public void setNumOfThreads(int i)
        {
            numOfThreads = i;
        }
        /// <summary>
        /// Metoda kojom se navodi resurs koji ce biti sinhronizovano modifikovan pri izvrsavanju zadataka koji ga posjeduju.
        /// </summary>
        /// <param name="resurs"></param>
        public void syncResurs(Resurs resurs)
        {
            mapOfResursStatus.Add(resurs, new ObjectSatus());
        }
        /// <summary>
        /// Metoda kojom se pokrece rasporedjivanje.
        /// </summary>
        public void work()
        {
            
            while (listOfTasks.Count != 0)
            {
                TaskToDo task= listOfTasks[0];
                listOfTasks.RemoveAt(0);
                if (isResursFree(task.resurs))
                {

                    ObjectSatus resursStatus = mapOfResursStatus.ContainsKey(task.resurs) ? mapOfResursStatus[task.resurs] : null;
                    if ((threadPool.Count < numOfThreads) && isAllThreadsWork())
                    {

                        MyThreadForNonPreemptiveScheduler thread = new MyThreadForNonPreemptiveScheduler(resursStatus);
                        threadPool.Add(thread);
                        thread.taskToDo = task;
                        
                        new Thread(thread.run).Start();
                        Thread.Sleep(100);
                    }
                    else
                    {
                        MyThreadForNonPreemptiveScheduler thread = getFreeThread();
                        thread.resursStatus = resursStatus;
                        thread.taskToDo = task;
                        thread.restartThread();
                        Thread.Sleep(100);

                    }

                }
                else
                {
                    listOfTasks.Add(task);
                }
                
            }
            while (threadPool.Count != 0)
            {
                foreach (MyThreadForNonPreemptiveScheduler thread in threadPool)
                {
                    if (thread.inRun == false)
                    {
                        thread.terminateThread();
                        thread.restartThread();
                        threadPool.Remove(thread);
                        break;
                    }
                }
            }
            Console.WriteLine("Svi tredovi terminirani");


        }
        /// <summary>
        /// Metoda koja provjerava da li su sve niti u threadPool-u pokrenute.
        /// </summary>
        private bool isAllThreadsWork()
        {
            bool flag = true;
            foreach (MyThreadForNonPreemptiveScheduler t in threadPool)
            {
                if (t.inRun == false)
                    flag = false;
            }
            return flag;
        }

        /// <summary>
        /// Metoda koja provjerava da li je prosljedjeni resurs zauzet od strane nekog zadatka koji se trenutno izvrsava.
        /// </summary>
        /// <param name="resurs"></param>
        /// <returns></returns>
        private bool isResursFree(Resurs resurs)
        {
            if (mapOfResursStatus.ContainsKey(resurs))
            {
                ObjectSatus status = mapOfResursStatus[resurs];
                return !status.inUse;
            }
            else
                return true;
            
        }
        /// <summary>
        /// Metoda koja vraca nit koja je trenutno u stanju cekanja i moze se iskoristiti za izvrsavanje zadatka.
        /// </summary>
        /// <returns></returns>
        private MyThreadForNonPreemptiveScheduler getFreeThread()
        {
            MyThreadForNonPreemptiveScheduler t = null;
            int index = 0;
            while (true)
            {
                if (!threadPool[index].inRun)
                {
                    t = threadPool[index];
                    break;
                }
                index = (index + 1) % threadPool.Count;                
            }
            return t;
        }

    
    }
    /// <summary>
    /// Klasa u kojoj je implementiran rad niti u nepreventivnom rasporedjivanju zadataka.
    /// </summary>
    public class MyThreadForNonPreemptiveScheduler
    {
        /// <summary>
        /// Zadatak koji se izvrsava na niti.
        /// </summary>
        public TaskToDo taskToDo;
        /// <summary>
        /// Stanje niti (pokrenuta ili ne).
        /// </summary>
        public bool inRun = false;
        private static int staticId=1;
        public int id = staticId++;
        private bool stop;
        /// <summary>
        /// Status resursa koji se obradjuje na niti.
        /// </summary>
        public ObjectSatus resursStatus;
        private MethodDelegate fun = null;
        private bool flagEnd = false;
        private Random rnd = new Random();
        public MyThreadForNonPreemptiveScheduler()
        {

        }
        public MyThreadForNonPreemptiveScheduler(ObjectSatus resursStatus)
        {
            this.resursStatus = resursStatus;
        }
        /// <summary>
        /// Metoda koja se poziva nakon promjene konteksta kako bi nit pocela sa izvrsavanjem zadatka koji je postavljen.
        /// </summary>
        public void restartThread()
        {
            stop = false;
        }
        /// <summary>
        /// Metoda kojom je implementirana logika niti pri nepreventivnom izvrsavanju zadataka.
        /// </summary>
        public void run() {
           
            
            while (!flagEnd)
            {
                inRun = true;
                if(resursStatus!=null)
                    resursStatus.inUse = true;
                while (taskToDo.listOfDelegates.Count != 0)
                {
                    fun = taskToDo.listOfDelegates[0];
                    taskToDo.listOfDelegates.RemoveAt(0);
                    taskToDo.resurs.result = fun(taskToDo.resurs.firstOperand, taskToDo.resurs.secondOperand, taskToDo.resurs.result);
                    int timeToSleep = rnd.Next(1, 5) * 500;
                    Console.WriteLine("Task: " + taskToDo.id + " se izvrsava na tredu: " + id + " i spavace " + timeToSleep + " trenutni stanje : " + taskToDo.resurs.result);
                    Thread.Sleep(timeToSleep);

                }
                if (resursStatus != null)
                    resursStatus.inUse = false;
                inRun = false;
                stop = true;
                while (stop) { }

            }
            Console.WriteLine("Tred " + id + " je terminiran");


        }
        /// <summary>
        /// Metoda za "Unistavanje" nit.
        /// </summary>
        internal void terminateThread()
        {
            flagEnd = true;
        }
    }
    /// <summary>
    /// Klasa kojoj se reprezentuje zadatak koji ce biti rasporedjen od strane oba tipa rasporedjivaca.
    /// </summary>
    public class TaskToDo
    {
        /// <summary>
        /// Objekata klase Resurs na kojim ce biti izvrsen zadatak
        /// </summary>
        public Resurs resurs;
        /// <summary>
        /// Lista svih delegata od kojih je sacinjen jedan zadatak.
        /// </summary>
        public List<MethodDelegate> listOfDelegates=new List<MethodDelegate>();
        private static int staticId = 1;
        public int id = staticId++;
        /// <summary>
        /// Podatak clan koji ukazuje na to da li je resur zaklucan od strane ovog zadatka.
        /// </summary>
        public bool ILockedResurs=false;
        public TaskToDo() { }
        /// <summary>
        /// Metoda za dodavanje delegata u jedan zadatak.
        /// </summary>
        /// <param name="md"></param>
        public void addDelegate(MethodDelegate md)
        {
            listOfDelegates.Add(md);

        }
        /// <summary>
        /// Metoda za dodavanje resursa nad kojim ce biti izvrsen zadatak.
        /// </summary>
        /// <param name="r"></param>
        public void addResurs(Resurs r)
        {
            resurs = r;
        }
    }
    /// <summary>
    /// Klasa kojom se reprezentuje resurs nad kojim ce se izvrsiti zadatak kojem je taj resurs dodijeljen.
    /// </summary>
    public class Resurs
    {
        /// <summary>
        /// Prvi operant u sklopu resursa.
        /// </summary>
        public int firstOperand = 0;
        /// <summary>
        /// Drugi operand u sklopu resursa.
        /// </summary>
        public int secondOperand = 0;
        /// <summary>
        /// Operant u kojem su pohranjeni medjurezultati i na kraju sam rezultat citavog zadata.
        /// </summary>
        public int result = 0;
        public Resurs(int firstOperand, int secondOperand)
        {
            this.firstOperand = firstOperand;
            this.secondOperand = secondOperand;
        }
        /// <summary>
        /// Metoda koja ispisuje vrijednost atributa resursa u trenutku kada je metoda pozvana.
        /// </summary>
        public void printsCurrentState()
        {
            Console.WriteLine("First operand: " + firstOperand + "second operand" + secondOperand + "current result" + result);
        }

    }
    /// <summary>
    /// Klasa kojom se reprezentuje stanje resursa i zadatka u procesu rasporedjivanja.
    /// </summary>
    public class ObjectSatus
    {
        public bool inUse = false;
    }
}
