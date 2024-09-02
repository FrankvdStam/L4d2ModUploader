using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using L4d2ModUploader.Util;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using Renci.SshNet;
using SteamWebAPI2.Interfaces;
using SteamWebAPI2.Utilities;

namespace L4d2ModUploader.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        public MainViewModel()
        {
            var webInterfaceFactory = new SteamWebInterfaceFactory(Settings.Instance.SteamApiKey);
            var steamUserInterface = webInterfaceFactory.CreateSteamWebInterface<SteamUser>();
            _steamRemoteStorage = webInterfaceFactory.CreateSteamWebInterface<SteamRemoteStorage>();

            RefreshCommand = new RelayCommand((o) => Refresh());
            UploadCommand = new RelayCommand(o => Upload());
            Refresh();

            Application.Current.DispatcherUnhandledException += (o, a) =>
            {
                a.Handled = true;
                SnackbarMessage(a.Exception.ToString());
            };
        }

        private SteamRemoteStorage _steamRemoteStorage;

        #region UI bindables

        public ObservableCollection<VpkViewModel> Vpks {
            get; 
            set; 
        } = new ObservableCollection<VpkViewModel>();
        
        public RelayCommand RefreshCommand
        {
            get => _refreshCommand;
            set => Set(ref _refreshCommand, value);
        }
        private RelayCommand _refreshCommand;

        public RelayCommand UploadCommand
        {
            get => _uploadCommand;
            set => Set(ref _uploadCommand, value);
        }
        private RelayCommand _uploadCommand;

        public Visibility UploadProgressVisibility
        {
            get => _uploadProgressVisibility;
            set => Set(ref _uploadProgressVisibility, value);
        }
        private Visibility _uploadProgressVisibility = Visibility.Collapsed;

        public int ProgressMax
        {
            get => _progressMax;
            set => Set(ref _progressMax, value);
        }
        private int _progressMax;

        public int ProgressValue
        {
            get => _progressValue;
            set => Set(ref _progressValue, value);
        }
        private int _progressValue;

        public SnackbarMessageQueue MessageQueue
        {
            get => _messageQueue;
            set => Set(ref _messageQueue, value);
        }
        private SnackbarMessageQueue _messageQueue = new SnackbarMessageQueue();
        #endregion

        #region Logic


        public void Refresh()
        {
            Task.Run(() =>
            {
                var addonsPath = Path.Combine(Settings.Instance.SteamL4d2RootPath, @"left4dead2\addons\workshop");

                var files = Directory.GetFiles(addonsPath, "*.vpk");
                var ids = files.Select(i => ulong.Parse(new FileInfo(i).Name.Replace(".vpk", string.Empty))).ToList();
                var results = Task.Run(() => _steamRemoteStorage.GetPublishedFileDetailsAsync((uint)ids.Count, ids)).Result;

                //Read vpk's from server
                using var sshClient = new SshClient(Settings.Instance.ScpHost, 22, Settings.Instance.ScpUser, Settings.Instance.ScpPassword);
                sshClient.Connect();
                var result = sshClient.RunCommand($"ls {Settings.Instance.HostL4d2RootPath}/addons");
                var uploaded = result.Result.Split("\n").Where(i => i.EndsWith(".vpk")).Select(i => ulong.Parse(i.Replace(".vpk", string.Empty))).ToList();

                App.Current.Dispatcher.Invoke(() =>
                {
                    Vpks.Clear();
                });

                foreach (var id in ids)
                {
                    var steamResult = results.Data.FirstOrDefault(i => i.PublishedFileId == id);
                    var vm = new VpkViewModel()
                    {
                        Id = id,
                        Uploaded = uploaded.Contains(id),
                        FilePath = Path.Combine(addonsPath, $"{id}.vpk"),
                        ImagePath = Path.Combine(addonsPath, $"{id}.jpg"),
                        Title = steamResult?.Title ?? "Title not found.",
                        Description = steamResult?.Description ?? "Description not found."
                    };

                    App.Current.Dispatcher.Invoke(() =>
                    {
                        Vpks.Add(vm);
                    });
                }
            });
        }


        public void Upload()
        {
            UploadProgressVisibility = Visibility.Visible;
            ProgressMax = Vpks.Count(i => i.Checked);
            ProgressValue = 0;

            Task.Run(() =>
            {
                try
                {
                    using var scpClient = new ScpClient(Settings.Instance.ScpHost, 22, Settings.Instance.ScpUser, Settings.Instance.ScpPassword);
                    scpClient.Connect();
                    var toUpload = Vpks.Where(i => i.Checked).ToList();

                    App.Current.Dispatcher.Invoke(() => { Vpks.Clear(); });

                    foreach (var vpk in toUpload)
                    {
                        using var stream = File.Open(vpk.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                        var destinationPath = @$"{Settings.Instance.HostL4d2RootPath}/addons/{vpk.Id}.vpk";
                        scpClient.Upload(stream, destinationPath);
                        ProgressValue++;
                    }
                }
                catch (Exception ex)
                {
                    SnackbarMessage(ex.ToString());
                    Debug.WriteLine(ex);
                }
                finally
                {
                    UploadProgressVisibility = Visibility.Collapsed;
                    RefreshCommand.Execute(null);
                }
            });
        }


        private void SnackbarMessage(string message)
        {
            MessageQueue.Enqueue(message);
        }

        #endregion




        public event PropertyChangedEventHandler? PropertyChanged;

        public void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
