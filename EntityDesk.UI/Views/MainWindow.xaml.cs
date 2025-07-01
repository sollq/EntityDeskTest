using System.Windows;
using EntityDesk.UI.ViewModels;

namespace EntityDesk.UI;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (s, e) =>
        {
            if (DataContext is MainViewModel vm)
            {
                await vm.InitAsync();
                DataContext = vm;
            }
        };
    }
}