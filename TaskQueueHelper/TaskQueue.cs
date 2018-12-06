using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TaskQueueHelper
{
    public class TaskQueue
    {
        private SemaphoreSlim locker = new SemaphoreSlim(1);
        private Queue<TaskItem> queue = new Queue<TaskItem>();
        private Task loopTask = null;

        /// <summary>
        /// Task 삽입
        /// </summary>
        /// <param name="task">Task, Task<Task>(async)</param>
        /// <returns></returns>
        public Task Enqueue(Task task)
        {
            if (!(task is Task || task is Task<Task>))
            {
                throw new TaskQueueException("This method only accepts 'Task' and 'Task<Task>'. Use the 'Enqueue<TResult>' method.");
            }
            Task waiter = Unwrap(task);
            TaskItem taskItem = new TaskItem(task, waiter);
            Enqueue(taskItem);
            return waiter;
        }

        /// <summary>
        /// Task<T> 삽입
        /// </summary>
        /// <typeparam name="TResult">Task 결과 타입</typeparam>
        /// <param name="task">Task<T>, Task<Task<T>>(async)</param>
        /// <returns></returns>
        public Task<TResult> Enqueue<TResult>(Task task)
        {
            if (!(task is Task<TResult> || task is Task<Task<TResult>>))
            {
                throw new TaskQueueException("This method only accepts 'Task<TResult>' and 'Task<Task<TResult>>'. Use the 'Enqueue' method.");
            }
            Task<TResult> waiter = Unwrap<TResult>(task);
            TaskItem taskItem = new TaskItem(task, waiter);
            Enqueue(taskItem);
            return waiter;
        }

        private void Enqueue(TaskItem taskItem)
        {
            locker.Wait();
            queue.Enqueue(taskItem);
            if (loopTask == null)
            {
                loopTask = Loop();
            }
            locker.Release();
        }

        private Task Loop()
        {
            return Task.Run(async () =>
            {
                while (true)
                {
                    locker.Wait();
                    int count = queue.Count;
                    if (count > 0)
                    {
                        TaskItem taskItem = queue.Dequeue();
                        locker.Release();
                        taskItem.Starter.Start();
                        await taskItem.Waiter;
                    }
                    else
                    {
                        loopTask = null;
                        locker.Release();
                        break;
                    }

                }
            });
        }

        private Task Unwrap(Task task)
        {
            if (task is Task<Task>)
            {
                task = ((Task<Task>)task).Unwrap();
            }
            return task;
        }

        private Task<TResult> Unwrap<TResult>(Task task)
        {

            if (task is Task<Task<TResult>>)
            {
                task = ((Task<Task<TResult>>)task).Unwrap();
            }
            return (Task<TResult>)task;
        }

        private class TaskItem
        {
            public Task Starter { get; private set; }
            public Task Waiter { get; private set; }

            public TaskItem(Task starter, Task waiter)
            {
                Starter = starter;
                Waiter = waiter;
            }
        }

        public class TaskQueueException : Exception
        {
            public TaskQueueException(string message)
                : base(message)
            {

            }
        }
    }
}
