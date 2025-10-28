using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Wpf.ViewModels;
using Wpf.Models;

namespace Wpf
{
    // Główny widok okna
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainViewModel();
            this.DataContext = vm;
            vm.RequestFocusOnItem += Vm_RequestFocusOnItem;
        }

        // Ustaw fokus na nowo dodanym polu
        private void Vm_RequestFocusOnItem(TodoItem item)
        {
            this.Dispatcher.InvokeAsync(() =>
            {
                var container = ItemsList.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                if (container == null)
                {
                    // Spróbuj przewinąć i ponowić próbę
                    ItemsList.UpdateLayout();
                    ItemsList.ScrollIntoView(item);
                    container = ItemsList.ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                }

                if (container != null)
                {
                    var tb = FindVisualChild<TextBox>(container);
                    if (tb != null) tb.Focus();
                }
            });
        }

        private static T? FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;
            var count = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T t) return t;
                var result = FindVisualChild<T>(child);
                if (result != null) return result;
            }
            return null;
        }

        // Potwierdzenie i usunięcie zadania
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is TodoItem item)
            {
                var result = MessageBox.Show(this, "Czy na pewno chcesz usunąć zadanie?", "Potwierdź usunięcie", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes && this.DataContext is MainViewModel vm)
                {
                    if (vm.DeleteItemCommand.CanExecute(item)) vm.DeleteItemCommand.Execute(item);
                }
            }
        }
    }
}