using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Oms.Framework.Threading;

namespace Oms.Framework.Tests.Threading
{
    [TestFixture]
    public class TaskDispatcherTests
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

        [Test]
        public async Task Queue_with_one_worker_runs_the_tasks_in_the_order_they_were_added()
        {
            using (var dispatcher = new TaskDispatcherQueue(1))
            {
                var order = new List<int>();
                var tasks = new List<Task>();
                for (var i = 0; i < 20; i++)
                {
                    var n = i;
                    tasks.Add(dispatcher.AddTask(() => order.Add(n)));
                }

                await Task.WhenAll(tasks).WaitAsync(Timeout);

                Assert.That(order, Is.EqualTo(System.Linq.Enumerable.Range(0, 20)));
            }
        }

        [Test]
        public async Task Stack_with_one_worker_runs_the_tasks_that_were_queued_while_it_was_busy_last_in_first_out()
        {
            using (var dispatcher = new TaskDispatcherStack(1))
            {
                var started = new ManualResetEventSlim();
                var release = new ManualResetEventSlim();
                var order = new List<int>();
                var blocker = dispatcher.AddTask(() => { started.Set(); release.Wait(Timeout); });
                Assert.That(started.Wait(Timeout), Is.True);
                var first = dispatcher.AddTask(() => order.Add(1));
                var second = dispatcher.AddTask(() => order.Add(2));

                release.Set();
                await Task.WhenAll(blocker, first, second).WaitAsync(Timeout);

                Assert.That(order, Is.EqualTo(new[] { 2, 1 }));
            }
        }

        [Test]
        public async Task A_task_that_throws_faults_its_task_and_does_not_stop_the_worker()
        {
            using (var dispatcher = new TaskDispatcherQueue(1))
            {
                var failing = dispatcher.AddTask(() => throw new InvalidOperationException("boom"));
                var ran = false;
                var next = dispatcher.AddTask(() => ran = true);

                Assert.ThrowsAsync<InvalidOperationException>(async () => await failing.WaitAsync(Timeout));
                await next.WaitAsync(Timeout);

                Assert.That(ran, Is.True);
            }
        }

        [Test]
        public async Task A_task_whose_token_is_already_cancelled_is_not_run()
        {
            using (var dispatcher = new TaskDispatcherQueue(1))
            {
                var cts = new CancellationTokenSource();
                cts.Cancel();
                var ran = false;

                var cancelled = dispatcher.AddTask(() => ran = true, cts.Token);

                Assert.ThrowsAsync<TaskCanceledException>(async () => await cancelled.WaitAsync(Timeout));
                await dispatcher.AddTask(() => { }).WaitAsync(Timeout);
                Assert.That(ran, Is.False);
            }
        }

        [Test]
        public async Task Several_workers_run_tasks_concurrently()
        {
            using (var dispatcher = new TaskDispatcherBag(2))
            {
                var barrier = new Barrier(2);
                var a = dispatcher.AddTask(() => Assert.That(barrier.SignalAndWait(Timeout), Is.True));
                var b = dispatcher.AddTask(() => Assert.That(barrier.SignalAndWait(Timeout), Is.True));

                await Task.WhenAll(a, b).WaitAsync(Timeout);
            }
        }
    }
}
