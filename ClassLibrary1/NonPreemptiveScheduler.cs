using System;
using System.Collections.Generic;
using System.Threading;

namespace MySchedulers
{
    public delegate int MethodDelegate(int a, int b, int rez);

    public class NonPreemptiveScheduler
    {
        private List<TaskToDo> listOfTasks = new List<TaskToDo>();
        private List<MyThread> threadPool = new List<MyThread>();
        private int numOfThreads=3;
        private Dictionary<Resurs, ResursSatus> mapOfResurs = new Dictionary<Resurs, ResursSatus>();

        public void addTask(TaskToDo task)
        {
            listOfTasks.Add(task);
        }
        public void setNumOfThreads(int i)
        {
            numOfThreads = i;
        }
        public void syncResurs(Resurs resurs)
        {
            mapOfResurs.Add(resurs, new ResursSatus());
        }

        public void work()
        {
            
            while (listOfTasks.Count != 0)
            {
                TaskToDo task= listOfTasks[0];
                
                if(isResursFree(task.resurs))
                {
                   
                    ResursSatus status = mapOfResurs.ContainsKey(task.resurs) ? mapOfResurs[task.resurs] : null;
                    if (threadPool.Count < numOfThreads)
                    {
                       
                        MyThread thread = new MyThread(status);
                        threadPool.Add(thread);
                        thread.taskToDo = task;
                        listOfTasks.RemoveAt(0);
                        new Thread(thread.run).Start();
                        Thread.Sleep(100);
                    }
                    else
                    {
                        MyThread thread = getFreeThread();
                        thread.resursStatus = status;
                        thread.taskToDo = task;
                        listOfTasks.RemoveAt(0);
                        thread.stop = false;
                        Thread.Sleep(100);

                    }

                }
                else
                {
                    listOfTasks.RemoveAt(0);
                    listOfTasks.Add(task);//OVIM STAVLJAM NA KRAJ LISTE;
                }
                
            }//TREBA POGASITI SVE TREDOVE
            

        }

        private bool isResursFree(Resurs resurs)
        {
            if (mapOfResurs.ContainsKey(resurs))
            {
                ResursSatus status = mapOfResurs[resurs];
                return !status.inUse;
            }
            else
                return true;
            
        }

        private MyThread getFreeThread()
        {
            MyThread t = null;
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
    class MyThread
    {
        public TaskToDo taskToDo;
        public bool inRun = false;
        private static int staticId=1;
        public int id = staticId++;
        public bool stop;
        public ResursSatus resursStatus;
        public MyThread(ResursSatus resursStatus)
        {
            this.resursStatus = resursStatus;
        }
        public void run() {
           
            MethodDelegate fun = null;
            Random rnd = new Random();
            while (true)
            {
                inRun = true;
                if(resursStatus!=null)
                    resursStatus.inUse = true;
                while (taskToDo.listOfDelegates.Count != 0)
                {
                    fun = taskToDo.listOfDelegates[0];
                    taskToDo.listOfDelegates.RemoveAt(0);
                    taskToDo.resurs.rez = fun(taskToDo.resurs.a, taskToDo.resurs.b, taskToDo.resurs.rez);
                    int timeToSleep = rnd.Next(1, 10) * 500;
                    Console.WriteLine("Tred " + id + ":" + taskToDo.resurs.rez + " vrijeme " + timeToSleep + " task:" + taskToDo.id);
                    Thread.Sleep(timeToSleep);

                }
                if (resursStatus != null)
                    resursStatus.inUse = false;
                inRun = false;
                stop = true;
                while (stop) { }

            }
            
           

        }
    }

    public class TaskToDo
    {
        public Resurs resurs;
        public List<MethodDelegate> listOfDelegates=new List<MethodDelegate>();
        private static int staticId = 1;
        public int id = staticId++;
        public TaskToDo() { }
 
        public void addDelegate(MethodDelegate md)
        {
            listOfDelegates.Add(md);

        }
        public void addResurs(Resurs r)
        {
            resurs = r;
        }
    }

    public class Resurs
    {
        public int a = 0, b = 0, rez = 0;
        public Resurs(int a, int b)
        {
            this.a = a;
            this.b = b;
        }

    }
    public class ResursSatus
    {
        public bool inUse = false;
    }
}
