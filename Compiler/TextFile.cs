using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Compiler
{
    public class TextFile : INotifyPropertyChanged
    {
        private string _text;
        private string _fileName;
        private string _filePath;
        private bool _isModified;

        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(); UpdateIsModified(); }
        }

        public string FileName
        {
            get => _fileName;
            set { _fileName = value; OnPropertyChanged(); }
        }

        public string FilePath
        {
            get => _filePath;
            set { _filePath = value; OnPropertyChanged(); UpdateIsModified(); }
        }

        public bool IsModified
        {
            get => _isModified;
            set { _isModified = value; OnPropertyChanged(); }
        }

        private string _originalText; // Для отслеживания изменений

        public TextFile(string fileName, string text = "", string filePath = null)
        {
            FileName = fileName;
            Text = text;
            _originalText = text;
            FilePath = filePath;
            IsModified = false;
        }

        private void UpdateIsModified()
        {
            IsModified = _originalText != Text || (string.IsNullOrEmpty(FilePath) && !string.IsNullOrEmpty(Text));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public void SaveCompleted()
        {
            _originalText = Text;
            IsModified = false;
        }
    }
}