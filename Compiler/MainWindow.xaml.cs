using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Compiler
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            Closing += MainWindowClosing;
        }

        private void MainWindowClosing(object sender, CancelEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                
                if (!viewModel.CheckSaveChanges())
                {
                    // Если CheckSaveChanges вернул false (отмена), отменяем закрытие окна
                    e.Cancel = true;
                }
            }
        }

        //Метод для синхронизации прокрутки scrollviewer в textbox и textblock
        private void Synhronize_Scrollers(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox && textBox.Template.FindName("PART_ContentHost", textBox) is ScrollViewer editorScrollViewer)
            {
                
                if (VisualTreeHelper.GetParent(textBox) is Grid grid)
                {
                    if (grid.Children[0] is ScrollViewer lineNumbersScrollViewer)
                    {
                        //Собственно сама синхронизация
                        editorScrollViewer.ScrollChanged += (s, args) =>
                            lineNumbersScrollViewer.ScrollToVerticalOffset(args.VerticalOffset);
                    }
                }
            }
        }

        //private void EditorScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        //{
        //    lineNumbersScrollViewer.ScrollToVerticalOffset(e.VerticalOffset);
        //}

        private void Window_Drop(object sender, DragEventArgs e)
        {
            // Проверяем, содержит ли перетаскиваемый объект данные в формате FileDrop
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Извлекаем массив путей к файлам (может быть несколько)
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (DataContext is MainViewModel viewModel)
                {
                    
                    foreach (string filePath in files)
                    {
                        try
                        {
                            string text = File.ReadAllText(filePath);
                            var newFile = new TextFile(System.IO.Path.GetFileName(filePath), text, filePath);
                            viewModel.Files.Add(newFile);
                            viewModel.SelectedFile = newFile;
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Ошибка при открытии файла {filePath}: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
        }

        //Показываем красивый курсор
        private void Window_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true; 
        }


        
    }
}