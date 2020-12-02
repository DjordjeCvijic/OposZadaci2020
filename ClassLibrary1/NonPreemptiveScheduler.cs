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
       
        public void addTask(TaskToDo task)
        {
            listOfTasks.Add(task);
        }
        public void setNumOfThreads(int i)
        {
            numOfThreads = i;
        }

        public void work()
        {

            while (listOfTasks.Count != 0)
            {
                TaskToDo task= listOfTasks[0];
                listOfTasks.RemoveAt(0);
                if (threadPool.Count < numOfThreads)
                {
                    MyThread thread = new MyThread();
                    threadPool.Add(thread);
                    thread.taskToDo = task;
                    new Thread(thread.run).Start();
                    Thread.Sleep(100);
                }
                else
                {
                    
                   
                        MyThread thread = getFreeThread();
                        thread.taskToDo = task;
                        thread.stop = false;
                        Thread.Sleep(100);
                    

                }
            }
            

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
        public void run() {
           
            MethodDelegate fun = null;
            Random rnd = new Random();
            while (true)
            {
                inRun = true;
                while (taskToDo.listOfDelegates.Count != 0)
                {
                    fun = taskToDo.listOfDelegates[0];
                    taskToDo.listOfDelegates.RemoveAt(0);
                    taskToDo.resurs.rez = fun(taskToDo.resurs.a, taskToDo.resurs.b, taskToDo.resurs.rez);
                    int timeToSleep = rnd.Next(1, 10) * 500;
                    Console.WriteLine("Tred " + id + ":" + taskToDo.resurs.rez + " vrijeme " + timeToSleep + " task:" + taskToDo.id);
                    Thread.Sleep(timeToSleep);

                }
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
}
