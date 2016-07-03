using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GoogleSearch
{
    /// <summary>
    /// Interaction logic for SearchResultCard.xaml
    /// </summary>
    public partial class SearchResultCard : UserControl
    {
        public object Title
        {
            get
            {
                return TitleLabel.Content;
            }
            set
            {
                TitleLabel.Content = value;
            }
        }
        public object Link
        {
            get
            {
                return ResultLink.Content;
            }
            set
            {
                ResultLink.Content = value;
            }
        }
        public Brush MouseOverBrush { get; set; }
        public SolidColorBrush TitleForeground
        {
            get
            {
                return (SolidColorBrush)TitleLabel.Foreground;
            }
            set
            {
                TitleLabel.Foreground = value;
            }
        }
        public SolidColorBrush LinkForeground
        {
            get
            {
                return (SolidColorBrush)ResultLink.Foreground;
            }
            set
            {
                ResultLink.Foreground = value;
            }
        }

        public SearchResultCard()
        {
            InitializeComponent();
        }

        private void UserControl_MouseEnter(object sender, MouseEventArgs e)
        {
            if (MouseOverBrush == null)
                MouseOverBrush = new SolidColorBrush(Colors.White);
            Background = MouseOverBrush;
        }

        private void UserControl_MouseLeave(object sender, MouseEventArgs e)
        {
            Background = new SolidColorBrush(Colors.Transparent);
        }
    }
}
