using System.Diagnostics;
using System.Windows.Controls;

namespace EntityDesk.UI.Views;

public partial class OrderDetailView : UserControl
{
    public OrderDetailView()
    {
        InitializeComponent();
        Loaded += (s, e) =>
        {
            Debug.WriteLine(
                $"OrderDetailView Loaded. DataContext Type: {DataContext?.GetType().FullName ?? "null"}");
        };
        DataContextChanged += (s, e) =>
        {
            Debug.WriteLine(
                $"OrderDetailView DataContextChanged. New DataContext Type: {e.NewValue?.GetType().FullName ?? "null"}");
        };
    }
}