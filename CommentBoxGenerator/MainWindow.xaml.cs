using System.Windows;

namespace CommentBoxGenerator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.DataContext = new CommentBoxGeneratorVm();
            InitializeComponent();
        }
    }
}
