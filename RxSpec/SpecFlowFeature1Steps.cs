using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using TechTalk.SpecFlow;

namespace RxSpec
{
    [Binding]
    public class SpecFlowFeature1Steps
    {
        [Given(@"the directory (.*) is empty")]
        public void GivenTheDirectoryIsEmpty(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }
            foreach (var filename in Directory.EnumerateFiles(directoryPath))
            {
                File.Delete(filename);
            }

            var fileSytemWatcher = new FileSystemWatcher(directoryPath) { EnableRaisingEvents = true };
            var fileEvents = Observable.FromEventPattern<FileSystemEventHandler, FileSystemEventArgs>(h => fileSytemWatcher.Created += h, h => fileSytemWatcher.Created -= h);

            var replay = Observable.Replay(fileEvents);
            replay.Connect();
            ScenarioContext.Current.Set(replay as IObservable<EventPattern<FileSystemEventArgs>>);
        }

        [When(@"the file (.*) is copied to directory (.*)")]
        public void WhenTheFileIsCopiedToDirectory(string fileRelativePath, string destinationDirectoryPath)
        {

            File.Copy(fileRelativePath, Path.Combine(destinationDirectoryPath, Path.GetFileName(fileRelativePath)));

        }

        [Then(@"(.*) file is written to directory (.*) within (.*) seconds")]
        public void ThenFileIsWrittenToDirectoryWithinSeconds(int numFilesExpected, string directoryPath, int timeoutSeconds)
        {
            ScenarioContext.Current.Get<IObservable<EventPattern<FileSystemEventArgs>>>()
              .Where(f => f.EventArgs.ChangeType == WatcherChangeTypes.Created)
              .Select(f => new FileInfo(f.EventArgs.FullPath))
              .Where(f => f.DirectoryName == directoryPath)
              .Take(numFilesExpected)
              .Do(f => Debug.WriteLine(f.FullName))
              .Timeout(TimeSpan.FromSeconds(timeoutSeconds))
              .Wait();
        }
    }

    public static class ScenarioContextExtensions
    {
        private static readonly ConcurrentDictionary<string, Lazy<IObservable<FileSystemEventArgs>>> observeDirectories = new ConcurrentDictionary<string, Lazy<IObservable<FileSystemEventArgs>>>();

        public static IObservable<FileSystemEventArgs> ObserveDirectory(this ScenarioContext current, string directoryPath)
        {
            return observeDirectories.GetOrAdd(directoryPath, new Lazy<IObservable<FileSystemEventArgs>>(() =>
            {
                var fileSytemWatcher = new FileSystemWatcher(directoryPath) { EnableRaisingEvents = true };
                return Observable.FromEvent<FileSystemEventHandler, FileSystemEventArgs>(h => fileSytemWatcher.Created += h, h => fileSytemWatcher.Created -= h);
            })).Value;
        }
    }
}
