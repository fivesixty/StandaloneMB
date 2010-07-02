using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace StandaloneMB
{
    public static class QueuedBackgroundWorker
    {
        public static void QueueWorkItem<Tin>(
            Queue<QueueItem<Tin>> queue,
            Tin inputArgument,
            Action<Tin> doWork)
        {
            if (queue == null) throw new ArgumentNullException("queue");

            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = false;
            bw.WorkerSupportsCancellation = false;
            bw.DoWork += (sender, args) =>
            {
                if (doWork != null)
                {
                    doWork((Tin)args.Argument);
                    Console.WriteLine(args.Argument);
                }
            };
            bw.RunWorkerCompleted += (sender, args) =>
            {
                Console.WriteLine(DateTime.Now.Ticks);
                queue.Dequeue();
                if (queue.Count > 0)
                {
                    QueueItem<Tin> nextItem = queue.Peek();
                    nextItem.BackgroundWorker.RunWorkerAsync(nextItem.Argument);
                }
            };

            queue.Enqueue(new QueueItem<Tin>(bw, inputArgument));
            if (queue.Count == 1)
            {
                QueueItem<Tin> nextItem = queue.Peek();
                nextItem.BackgroundWorker.RunWorkerAsync(nextItem.Argument);
            }
        }

    }

    public static class BackgroundWorkerHelper
    {
        public static void DoWork<Tin>(
            Tin inputArgument,
            Action<Tin> doWork)
        {
            BackgroundWorker bw = new BackgroundWorker();
            bw.WorkerReportsProgress = false;
            bw.WorkerSupportsCancellation = false;
            bw.DoWork += (sender, args) =>
            {
                if (doWork != null)
                {
                    doWork((Tin)args.Argument);
                    Console.WriteLine(args.Argument);
                }
            };
            bw.RunWorkerCompleted += (sender, args) =>
            {
                Console.WriteLine(DateTime.Now.Ticks);
            };
            bw.RunWorkerAsync(inputArgument);
        }
    }

    public class DoWorkArgument<T>
    {
        public DoWorkArgument(T argument)
        {
            this.Argument = argument;
        }
        public T Argument { get; private set; }
    }

    public class QueueItem<Tin>
    {
        public QueueItem(BackgroundWorker backgroundWorker, Tin argument)
        {
            this.BackgroundWorker = backgroundWorker;
            this.Argument = argument;
        }

        public Tin Argument { get; private set; }
        public BackgroundWorker BackgroundWorker { get; private set; }
    }

}
