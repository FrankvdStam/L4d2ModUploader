using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace L4d2ModUploader.ViewModels
{
    public class VpkViewModel : INotifyPropertyChanged
    {
        public ulong Id
        {
            get => _id;
            set => Set(ref _id, value);
        }
        private ulong _id;

        public bool Checked
        {
            get => _checked;
            set => Set(ref _checked, value);
        }
        private bool _checked;

        public bool Uploaded
        {
            get => _uploaded;
            set => Set(ref _uploaded, value);
        }
        private bool _uploaded;

        public string Title
        {
            get => _title;
            set => Set(ref _title, value);
        }
        private string _title;

        public string Description
        {
            get => _description;
            set => Set(ref _description, value);
        }
        private string _description;

        public string FilePath
        {
            get => _filePath;
            set => Set(ref _filePath, value);
        }
        private string _filePath;

        public string ImagePath
        {
            get => _imagePath;
            set => Set(ref _imagePath, value);
        }
        private string _imagePath;



        public event PropertyChangedEventHandler? PropertyChanged;

        public void Set<T>(ref T field, T value, [CallerMemberName] string propertyName = "")
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return;
            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
