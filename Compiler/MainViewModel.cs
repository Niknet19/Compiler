using Compiler;
using Microsoft.Win32;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Windows;
using System.IO;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Diagnostics;

namespace Compiler
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<TextFile> _files;
        private TextFile _selectedFile;
        private string _resultText;
        private double _editorFontSize;
        private double _resultFontSize;

        public ObservableCollection<TextFile> Files
        {
            get => _files;
            set { _files = value; OnPropertyChanged(); }
        }

        public TextFile SelectedFile
        {
            get => _selectedFile;
            set
            {
                _selectedFile = value;
                OnPropertyChanged();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string ResultText
        {
            get => _resultText;
            set { _resultText = value; OnPropertyChanged(); }
        }

        public double EditorFontSize
        {
            get => _editorFontSize;
            set { _editorFontSize = value; OnPropertyChanged(); }
        }

        public double ResultFontSize
        {
            get => _resultFontSize;
            set { _resultFontSize = value; OnPropertyChanged(); }
        }

        public ICommand NewCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand IncreaseFontSizeCommand { get; }
        public ICommand DecreaseFontSizeCommand { get; }
        public ICommand ResetFontSizeCommand { get; }
        public ICommand CloseTabCommand { get; }

        public ICommand HelpCommand { get; } 
        public ICommand AboutCommand { get; }

        public MainViewModel()
        {
            Files = new ObservableCollection<TextFile>();
            NewCommand = new RelayCommand(NewFile);
            OpenCommand = new RelayCommand(OpenFile);
            SaveCommand = new RelayCommand(SaveFile, CanExecuteWhenModified);
            SaveAsCommand = new RelayCommand(SaveAsFile);
            ExitCommand = new RelayCommand(Exit);
            UndoCommand = new RelayCommand(Undo);
            RedoCommand = new RelayCommand(Redo);
            CutCommand = new RelayCommand(Cut);
            CopyCommand = new RelayCommand(Copy);
            PasteCommand = new RelayCommand(Paste);
            DeleteCommand = new RelayCommand(Delete);
            SelectAllCommand = new RelayCommand(SelectAll);
            IncreaseFontSizeCommand = new RelayCommand(IncreaseFontSize);
            DecreaseFontSizeCommand = new RelayCommand(DecreaseFontSize);
            ResetFontSizeCommand = new RelayCommand(ResetFontSize);
            CloseTabCommand = new RelayCommand(CloseTab);
            HelpCommand = new RelayCommand(ShowHelp); 
            AboutCommand = new RelayCommand(ShowAbout);

            _editorFontSize = 14;
            _resultFontSize = 14;

            //Files.Add(new TextFile("NewFile1.txt"));
            //SelectedFile = Files[0];
        }


        private void ShowHelp(object parameter)
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), "Help.html");
                using (Stream resourceStream = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/Help.html")).Stream)
                using (Stream fileStream = File.Create(tempPath))
                {
                    resourceStream.CopyTo(fileStream);
                }
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии страницы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ShowAbout(object parameter)
        {
            try
            {
                string tempPath = Path.Combine(Path.GetTempPath(), "About.html");
                using (Stream resourceStream = Application.GetResourceStream(new Uri("pack://application:,,,/Resources/About.html")).Stream)
                using (Stream fileStream = File.Create(tempPath))
                {
                    resourceStream.CopyTo(fileStream);
                }
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии страницы: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void CloseTab(object parameter)
        {
            if (parameter is TextFile fileToClose)
            {
                CheckSaveChanges();

                Files.Remove(fileToClose);
                if (Files.Count > 0 && SelectedFile == fileToClose)
                {
                    SelectedFile = Files[Files.Count - 1];
                }
            }
        }

        private void NewFile(object parameter)
        {
            var inputDialog = new Window
            {
                Title = "Новый файл",
                Width = 300,
                Height = 150,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                Content = new StackPanel
                {
                    Margin = new Thickness(10),
                    Children =
                    {
                        new TextBlock { Text = "Введите имя файла:" },
                        new TextBox { Name = "FileNameInput", Margin = new Thickness(0, 5, 0, 5) },
                        new Button { Content = "OK", IsDefault = true, Margin = new Thickness(0, 5, 0, 0) }
                    }
                }
            };

            var okButton = (inputDialog.Content as StackPanel).Children[2] as Button;
            var fileNameInput = (inputDialog.Content as StackPanel).Children[1] as TextBox;
            okButton.Click += (s, e) => inputDialog.DialogResult = true;
            fileNameInput.Text = $"NewFile{Files.Count + 1}.txt";

            if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(fileNameInput.Text))
            {
                var newFile = new TextFile(fileNameInput.Text);
                Files.Add(newFile);
                SelectedFile = newFile;
            }
        }

        private void OpenFile(object parameter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string text = File.ReadAllText(openFileDialog.FileName);
                var newFile = new TextFile(Path.GetFileName(openFileDialog.FileName), text, openFileDialog.FileName);
                Files.Add(newFile);
                SelectedFile = newFile;
            }
        }

        private void SaveFile(object parameter)
        {
            if (SelectedFile == null) return;

            if (string.IsNullOrEmpty(SelectedFile.FilePath))
                SaveAsFile(parameter);
            else
            {
                File.WriteAllText(SelectedFile.FilePath, SelectedFile.Text);
                SelectedFile.SaveCompleted();
            }
        }

        private void SaveAsFile(object parameter)
        {
            if (SelectedFile == null) return;
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                FileName = SelectedFile.FileName
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, SelectedFile.Text);
                SelectedFile.FilePath = saveFileDialog.FileName;
                SelectedFile.FileName = Path.GetFileName(saveFileDialog.FileName);
                SelectedFile.SaveCompleted();
            }
        }

        public bool CheckSaveChanges()
        {
            foreach (var file in Files.Where(f => f.IsModified))
            {
                var result = MessageBox.Show($"Сохранить изменения в файле '{file.FileName}'?",
                    "Сохранение",
                    MessageBoxButton.YesNoCancel,
                    MessageBoxImage.Question);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        SelectedFile = file;
                        SaveFile(null);
                        if (file.IsModified) return false; // Если сохранение не удалось
                        break;
                    case MessageBoxResult.No:
                        continue;
                    case MessageBoxResult.Cancel:
                        return false;
                }
            }
            return true;
        }

        private void Exit(object parameter)
        {
            if (CheckSaveChanges())
            {
                Application.Current.Shutdown();
            }
        }

        private bool CanExecuteWhenModified(object parameter) => SelectedFile?.IsModified ?? false;

        private void Undo(object parameter) => ApplicationCommands.Undo.Execute(null, null);
        private void Redo(object parameter) => ApplicationCommands.Redo.Execute(null, null);
        private void Cut(object parameter) => ApplicationCommands.Cut.Execute(null, null);
        private void Copy(object parameter) => ApplicationCommands.Copy.Execute(null, null);
        private void Paste(object parameter) => ApplicationCommands.Paste.Execute(null, null);
        private void Delete(object parameter) => ApplicationCommands.Delete.Execute(null, null);
        private void SelectAll(object parameter) => ApplicationCommands.SelectAll.Execute(null, null);

        private void IncreaseFontSize(object parameter)
        {
            if (EditorFontSize < 72) EditorFontSize += 2;
            if (ResultFontSize < 72) ResultFontSize += 2;
        }

        private void DecreaseFontSize(object parameter)
        {
            if (EditorFontSize > 6) EditorFontSize -= 2;
            if (ResultFontSize > 6) ResultFontSize -= 2;
        }

        private void ResetFontSize(object parameter)
        {
            EditorFontSize = 14;
            ResultFontSize = 14;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}