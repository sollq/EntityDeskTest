using System.Windows.Controls;
using System.Diagnostics;
using System.Windows;

namespace EntityDesk.UI.Views
{
    public partial class OrderDetailView : UserControl
    {
        public OrderDetailView()
        {
            InitializeComponent();
            this.Loaded += (s, e) =>
            {
                Debug.WriteLine($"OrderDetailView Loaded. DataContext Type: {this.DataContext?.GetType().FullName ?? "null"}");
            };
            this.DataContextChanged += (s, e) =>
            {
                Debug.WriteLine($"OrderDetailView DataContextChanged. New DataContext Type: {e.NewValue?.GetType().FullName ?? "null"}");
            };
        }
    }
}
