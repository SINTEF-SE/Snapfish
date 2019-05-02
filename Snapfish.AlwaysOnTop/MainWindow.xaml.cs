namespace Snapfish.AlwaysOnTop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public MainWindow()
        {
            InitializeComponent();
            Left = System.Windows.SystemParameters.PrimaryScreenWidth - Width - 205;
        }
    }
}