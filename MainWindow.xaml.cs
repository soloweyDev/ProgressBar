using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ProgressBar
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Splash splash;

        public MainWindow()
        {
            InitializeComponent();
            Hide();

            CreateSplashScreenAsync(this);
        }

        private async Task CreateSplashScreenAsync(Window window)
        {
            DispatcherThread dt = await DispatcherThread.CreateAsync();
            Window w = await dt.Get(() =>
            {
                splash = new Splash();
                splash.Show();
                return splash;
            });

            await dt.Execute(() => splash.pbTest.Value += 10);
            Thread.Sleep(1000);
            await dt.Execute(() => splash.pbTest.Value += 10);
            Thread.Sleep(1000);
            await dt.Execute(() => splash.pbTest.Value += 10);
            Thread.Sleep(1000);
            await dt.Execute(() => splash.pbTest.Value += 10);
            Thread.Sleep(1000);
            await dt.Execute(() => splash.pbTest.Value += 10);
            Thread.Sleep(1000);
            await dt.Execute(() => splash.pbTest.Value += 10);
            Thread.Sleep(1000);
            await dt.Execute(() => splash.pbTest.Value += 10);
            Thread.Sleep(1000);
            await dt.Execute(() => splash.pbTest.Value += 10);
            Thread.Sleep(1000);

            await dt.Execute(() => w.Close());
            await dt.CloseAsync();
            dt.Dispose();

            Thread.Sleep(1000);
            window.Show();
        }
    }

    public class DispatcherThread : IDisposable
    {
        readonly Dispatcher dispatcher;
        readonly Thread thread;

        static public async Task<DispatcherThread> CreateAsync()
        {
            var waitCompletionSource = new TaskCompletionSource<DispatcherThread>();
            var thread = new Thread(() =>
            {
                waitCompletionSource.SetResult(new DispatcherThread());
                Dispatcher.Run();
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return await waitCompletionSource.Task;
        }

        private DispatcherThread()
        {
            dispatcher = Dispatcher.CurrentDispatcher;
            thread = Thread.CurrentThread;
        }

        public DispatcherOperation Execute(Action a) => dispatcher.InvokeAsync(a);
        public DispatcherOperation<T> Get<T>(Func<T> getter) => dispatcher.InvokeAsync(getter);

        public async Task CloseAsync()
        {
            var waitCompletionSource = new TaskCompletionSource<int>();
            EventHandler shutdownWatch = (sender, args) => waitCompletionSource.SetResult(0);
            dispatcher.ShutdownFinished += shutdownWatch;
            try
            {
                if (dispatcher.HasShutdownFinished)
                    return;
                dispatcher.BeginInvokeShutdown(DispatcherPriority.Normal);
                await waitCompletionSource.Task;
            }
            finally
            {
                dispatcher.ShutdownFinished -= shutdownWatch;
            }
        }

        public void Dispose()
        {
            dispatcher.InvokeShutdown();
            if (thread != Thread.CurrentThread)
                thread.Join();
        }
    }
}
