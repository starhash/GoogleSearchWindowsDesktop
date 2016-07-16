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
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Reflection;
using GoogleMiner;
using HtmlAgilityPack;

namespace GoogleSearch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string left = "";
        string right = "";
        GoogleSearchResultMiner gsrm;
        int CurrentPage;
        int CurrentLink;
        string Theme = "Light";
        Storyboard storyboard; 
        private Rect PreviousLocation;
        private bool IsMaximised;
        private int _sidepanewidth;
        public int SidePaneWidth
        {
            get { return _sidepanewidth; }
            set { _sidepanewidth = value; SideGrid.Width = value; }
        }

        public MainWindow()
        {
            InitializeComponent();
            SidePaneWidth = 480;
            Browser.NavigateToString("<html><head><style>img{position: absolute; top: 50%; left: 50%; width: 400px; height: 140px; margin-top: -65.5px; margin-left: -200px;}</style></head><body><img src=\"https://www.google.co.in/images/branding/googlelogo/2x/googlelogo_color_272x92dp.png\"></body></html>");
            LeftArrow.Text = "";
            RightArrow.Text = "";
            Browser.Visibility = System.Windows.Visibility.Visible;
            ResizeMode = System.Windows.ResizeMode.NoResize;
            dynamic activeX = Browser.GetType().InvokeMember("ActiveXInstance", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic, null, Browser, new object[] { });
            activeX.Silent = true;
            SearchResultCard defaultcard = new SearchResultCard() { 
                Title = "Nothing Here", 
                Link = "Start your new search now.\nType in you query in the\ntext box at the top",
                MouseOverBrush = new SolidColorBrush(Color.FromArgb(10,0,0,0)), 
                TitleForeground = new SolidColorBrush(Colors.Gray), 
                LinkForeground = new SolidColorBrush(Color.FromArgb(255, 74, 49, 211)), 
                Width = SidePaneWidth - 28, 
                Margin = new Thickness(2,0,2,0),
                Name = "DefaultCard"
            };
            defaultcard.MouseDoubleClick += defaultcard_MouseDoubleClick;
            ResultPaneStack.Children.Add(defaultcard);
            Settings.Opacity = 0.0;
        }

        public void InitializeGSRM()
        {
            if (SearchBox.Text.Length != 0 && !SearchBox.Text.Contains(' '))
            {
                gsrm = new GoogleSearchResultMiner(SearchBox.Text);
                gsrm.SafeSearch = SafeSearchCheck.IsChecked.Value;
                gsrm.SearchOptions = FindSearchOption();
                gsrm.InitializeAndLoad();
                Console.WriteLine(gsrm.CurrentURL);
                if (gsrm.ResultCount > 0)
                {
                    CurrentPage = 0;
                    CurrentLink = 0;
                    LoadFullPage();
                }
                else
                {
                    CurrentPage = 0;
                    CurrentLink = 0;
                    LeftArrow.Text = "";
                    RightArrow.Text = "";
                    Status.Content = "No results found. Please try again, with a new search key.";
                    SearchBox.Text = "";
                    SearchBox.Focus();
                }
            }
            else
            {
                Status.Content = "Start your search by entering keywords at the top.";
            }
        }
        public void LoadFullPage()
        {
            ResultPaneStack.Children.Clear();
            for (int i = 0; i < gsrm.ResultCount; i++)
            {
                SearchResultCard card = new SearchResultCard()
                {
                    Title = HtmlRemoval.StripTagsRegexCompiled(gsrm.SearchResults[i]),
                    Link = gsrm.SearchResultsLinks[i],
                    MouseOverBrush = new SolidColorBrush(Color.FromArgb(10, 0, 0, 0)),
                    TitleForeground = new SolidColorBrush(Colors.Gray),
                    LinkForeground = new SolidColorBrush(Color.FromArgb(255, 74, 49, 211)),
                    Width = SidePaneWidth - 28,
                    Margin = new Thickness(2, 0, 2, 0)
                };
                Tag = i;
                card.MouseDoubleClick += card_MouseDoubleClick;
                ResultPaneStack.Children.Add(card);
            }
            LeftArrow.Text = left;
            RightArrow.Text = right;
            SearchBox.Text = HtmlRemoval.StripTagsRegexCompiled(gsrm.SearchResults[0]);
            UpdateStatus(gsrm.SearchResultsLinks[0]);
            Browser.Navigate(gsrm.SearchResultsLinks[0]);
            Progress.IsIndeterminate = true;
        }
        void defaultcard_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SearchBox.Text = "";
            HideGrid();
            SearchBox.Focus();
            Keyboard.Focus(SearchBox);
        }
        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                InitializeGSRM();
            }
        }
        void card_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SearchResultCard card = (SearchResultCard)sender;
            CurrentLink = (int)Tag;
            Browser.Navigate((string)card.Link);
            Progress.IsIndeterminate = true;
            SearchBox.Text = ((string)card.Title);
            UpdateStatus(gsrm.SearchResultsLinks[CurrentLink]);
        }
        private void Browser_Navigated(object sender, NavigationEventArgs e)
        {
            Progress.IsIndeterminate = false;
            Title = "Google Search - " + ((dynamic)Browser.Document).Title;
            if (gsrm != null || Browser.Source != null)
                UpdateStatus(Browser.Source.OriginalString);
        }
        private void Title_GotFocus(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("IO");
        }
        private void SearchBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (gsrm == null)
                SearchBox.Text = "";
            else
            {
                SearchBox.Text = gsrm.SearchKey;
                SearchBox.CaretIndex = SearchBox.Text.Length - 1;
            }
            DoubleAnimation options = new DoubleAnimation()
            {
                From = 0,
                To = 16,
                Duration = new Duration(TimeSpan.FromMilliseconds(512)),
                EasingFunction = new SineEase()
            };
            Storyboard.SetTarget(options, SearchOptions);
            Storyboard.SetTargetProperty(options, new PropertyPath(StackPanel.HeightProperty));
            storyboard = new Storyboard();
            storyboard.Children.Add(options);
            storyboard.Begin();
        }
        private void SearchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (SearchBox.Text.Length == 0)
                SearchBox.Text = "Start you new search here";
            DoubleAnimation options = new DoubleAnimation()
            {
                From = 16,
                To = 0,
                Duration = new Duration(TimeSpan.FromMilliseconds(512)),
                EasingFunction = new SineEase()
            };
            Storyboard.SetTarget(options, SearchOptions);
            Storyboard.SetTargetProperty(options, new PropertyPath(StackPanel.HeightProperty));
            storyboard = new Storyboard();
            storyboard.Children.Add(options);
            storyboard.Begin();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Status.Content = "Google Search powered by Google";
        }
        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(Settings.Opacity == 0.0)
            {
                Thickness bt = Browser.Margin;
                Thickness bt2 = Browser.Margin;
                bt2.Top = ActualHeight;
                bt2.Bottom = bt.Top - ActualHeight;
                ThicknessAnimation da2 = new ThicknessAnimation()
                {
                    Duration = new Duration(TimeSpan.FromMilliseconds(512)),
                    From = bt,
                    To = bt2,
                    EasingFunction = new SineEase()
                };
                Storyboard.SetTarget(da2, Browser);
                Storyboard.SetTargetProperty(da2, new PropertyPath(WebBrowser.MarginProperty));
                DoubleAnimation settingsshow = new DoubleAnimation()
                {
                    From = 0.0,
                    To = 1.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(512)),
                    EasingFunction = new SineEase()
                };
                Storyboard.SetTarget(settingsshow, Settings);
                Storyboard.SetTargetProperty(settingsshow, new PropertyPath(Grid.OpacityProperty)); 
                storyboard = new Storyboard();
                storyboard.Children.Add(da2);
                storyboard.Children.Add(settingsshow);
                storyboard.Begin();
                SideGrid.Visibility = System.Windows.Visibility.Collapsed;
            }
            else
            {
                Thickness bt = Browser.Margin;
                Thickness bt2 = Browser.Margin;
                bt2.Top = 0;
                bt2.Bottom = 0;
                ThicknessAnimation da2 = new ThicknessAnimation()
                {
                    Duration = new Duration(TimeSpan.FromMilliseconds(512)),
                    From = bt,
                    To = bt2,
                    EasingFunction = new SineEase()
                };
                Storyboard.SetTarget(da2, Browser);
                Storyboard.SetTargetProperty(da2, new PropertyPath(WebBrowser.MarginProperty));
                DoubleAnimation settingsshow = new DoubleAnimation()
                {
                    From = 1.0,
                    To = 0.0,
                    Duration = new Duration(TimeSpan.FromMilliseconds(512)),
                    EasingFunction = new SineEase()
                };
                Storyboard.SetTarget(settingsshow, Settings);
                Storyboard.SetTargetProperty(settingsshow, new PropertyPath(Grid.OpacityProperty));
                storyboard = new Storyboard();
                storyboard.Children.Add(da2);
                storyboard.Children.Add(settingsshow);
                storyboard.Begin();
                SideGrid.Visibility = System.Windows.Visibility.Visible;
            }
        }
        private void Rectangle_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if(Mouse.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Maximized)
            {
                WindowState = System.Windows.WindowState.Normal;
                PreviousLocation = new Rect(new Point((int)Left, (int)Top), new Size(Width, Height));
                Left = 0;
                Top = 0;
                Width = SystemParameters.WorkArea.Width;
                Height = SystemParameters.WorkArea.Height;
                IsMaximised = true;
            }
        }
        private void Right_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CurrentLink = CurrentLink + 1;
            if (CurrentLink >= gsrm.SearchResultsLinks.Count)
            {
                if (CurrentLink == gsrm.SearchResultsLinks.Count)
                {
                    Status.Content = "Last Link On Page, press " + right + " again to go to next page of results.";
                }
                else if (CurrentLink == gsrm.SearchResultsLinks.Count + 1)
                {
                    Status.Content = " Loading next page : ";
                    CurrentLink = -1;
                    gsrm.NextPage();
                    LoadFullPage();
                    if (gsrm.ResultCount > 0)
                    {
                        CurrentPage++;
                        CurrentLink = 0;
                        LeftArrow.Text = left;
                        RightArrow.Text = right;
                        SearchBox.Text = HtmlRemoval.StripTagsRegexCompiled(gsrm.SearchResults[0]);
                        UpdateStatus(gsrm.SearchResultsLinks[0]);
                        Browser.Navigate(gsrm.SearchResultsLinks[0]);
                        Progress.IsIndeterminate = true;
                    }
                    else
                    {
                        CurrentPage++;
                        CurrentLink = 0;
                        LeftArrow.Text = "";
                        RightArrow.Text = "";
                        Status.Content = "No results found. Please try again, with a new search key.";
                        SearchBox.Text = "";
                        SearchBox.Focus();
                    }
                }
            }
            else
            {
                SearchBox.Text = HtmlRemoval.StripTagsRegexCompiled(gsrm.SearchResults[CurrentLink]);
                UpdateStatus(gsrm.SearchResultsLinks[CurrentLink]);
                Browser.Navigate(gsrm.SearchResultsLinks[CurrentLink]);
                Progress.IsIndeterminate = true;
            }
        }
        private void Left_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CurrentLink = CurrentLink - 1;
            if (CurrentLink < 0)
            {
                Status.Content = " Loading next page : ";
                CurrentLink = -1;
                gsrm.PrevPage();
                if (gsrm.ResultCount > 0)
                {
                    CurrentPage--;
                    CurrentLink = 0;
                    LeftArrow.Text = left;
                    RightArrow.Text = right;
                    SearchBox.Text = HtmlRemoval.StripTagsRegexCompiled(gsrm.SearchResults[0]);
                    UpdateStatus(gsrm.SearchResultsLinks[0]);
                    Browser.Navigate(gsrm.SearchResultsLinks[0]);
                    Progress.IsIndeterminate = true;
                }
                else
                {
                    CurrentPage--;
                    CurrentLink = 0;
                    LeftArrow.Text = "";
                    RightArrow.Text = "";
                    Status.Content = "No results found. Please try again, with a new search key.";
                    SearchBox.Text = "";
                    SearchBox.Focus();
                }
            }
            else
            {
                SearchBox.Text = HtmlRemoval.StripTagsRegexCompiled(gsrm.SearchResults[CurrentLink]);
                Status.Content = "Page " + (CurrentPage + 1) + ", Result " + (CurrentLink + 1) + " of " + gsrm.ResultCount + ", URL : " + gsrm.SearchResultsLinks[CurrentLink];
                Browser.Navigate(gsrm.SearchResultsLinks[CurrentLink]);
                Progress.IsIndeterminate = true;
            }
        }
        private void Maximize_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("YO");
            if (!IsMaximised)
            {
                PreviousLocation = new Rect(new Point((int)Left, (int)Top), new Size(Width, Height));
                BorderThickness = new Thickness(0);
                Left = 0;
                Top = 0;
                Width = SystemParameters.WorkArea.Width;
                Height = SystemParameters.WorkArea.Height;
                IsMaximised = true;
            }
            else
            {
                BorderThickness = new Thickness(1);
                Left = PreviousLocation.Left;
                Top = PreviousLocation.Top;
                Width = PreviousLocation.Width;
                Height = PreviousLocation.Height;
                IsMaximised = false;
            }
        }
        private void Help_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {

        }
        private void CloseButton_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Close();
        }
        private void Grid_MouseEnter(object sender, MouseEventArgs e)
        {
            ShowGrid();
        }
        private void SideGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            HideGrid();
        }
        public void ShowGrid()
        {
            DoubleAnimation da = new DoubleAnimation()
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(512)),
                From = 2,
                To = SidePaneWidth,
                EasingFunction = new SineEase()
            };
            Storyboard.SetTarget(da, SideGrid);
            Storyboard.SetTargetProperty(da, new PropertyPath(Grid.WidthProperty));
            Thickness bt = Browser.Margin;
            Thickness bt2 = Browser.Margin;
            bt2.Left = SidePaneWidth;
            ThicknessAnimation da2 = new ThicknessAnimation()
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(512)),
                From = bt,
                To = bt2,
                EasingFunction = new SineEase()
            };
            Storyboard.SetTarget(da2, Browser);
            Storyboard.SetTargetProperty(da2, new PropertyPath(WebBrowser.MarginProperty));

            storyboard = new Storyboard();
            storyboard.Children.Add(da);
            storyboard.Children.Add(da2);
            storyboard.Begin();
        }
        public void HideGrid()
        {
            DoubleAnimation da = new DoubleAnimation()
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                From = SidePaneWidth,
                To = 2,
                EasingFunction = new SineEase()
            };
            Storyboard.SetTarget(da, SideGrid);
            Storyboard.SetTargetProperty(da, new PropertyPath(Grid.WidthProperty));
            Thickness bt = Browser.Margin;
            Thickness bt2 = Browser.Margin;
            bt2.Left = 2;
            ThicknessAnimation da2 = new ThicknessAnimation()
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(500)),
                From = bt,
                To = bt2,
                EasingFunction = new SineEase()
            };
            Storyboard.SetTarget(da2, Browser);
            Storyboard.SetTargetProperty(da2, new PropertyPath(WebBrowser.MarginProperty));
            storyboard = new Storyboard();
            storyboard.Children.Add(da);
            storyboard.Children.Add(da2);
            storyboard.Begin();
        }
        public void UpdateStatus(string url)
        {
            Status.Content = url;
            PageNumber.Content = "Page " + (gsrm.CurrentPage + 1) + ". Link " + (CurrentLink + 1) + " of " + gsrm.SearchResults.Count;
        }
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if(gsrm != null)
                gsrm.SafeSearch = true;
            ((CheckBox)sender).Content = "Safe Search On";
        }
        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (gsrm != null)
                gsrm.SafeSearch = false;
            ((CheckBox)sender).Content = "Safe Search Off";
        }
        private SolidColorBrush SearchOptionSelected = new SolidColorBrush(Color.FromArgb(255, 221, 75, 57));
        private SolidColorBrush SearchOptionUnselected = new SolidColorBrush(Color.FromArgb(255, 120, 120, 120));
        private void SearchOptionWeb_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchOptionWeb.Foreground = SearchOptionSelected;
            SearchOptionNews.Foreground = SearchOptionUnselected;
            SearchOptionBooks.Foreground = SearchOptionUnselected;
            SearchOptionVideos.Foreground = SearchOptionUnselected;
            if (gsrm != null)
                gsrm.SearchOptions = 0;
            InitializeGSRM();
        }
        private void SearchOptionNews_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchOptionWeb.Foreground = SearchOptionUnselected;
            SearchOptionNews.Foreground = SearchOptionSelected;
            SearchOptionBooks.Foreground = SearchOptionUnselected;
            SearchOptionVideos.Foreground = SearchOptionUnselected;
            if (gsrm != null)
                gsrm.SearchOptions = 1;
            InitializeGSRM();
        }
        private void SearchOptionVideos_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchOptionWeb.Foreground = SearchOptionUnselected;
            SearchOptionNews.Foreground = SearchOptionUnselected;
            SearchOptionBooks.Foreground = SearchOptionUnselected;
            SearchOptionVideos.Foreground = SearchOptionSelected;
            if (gsrm != null)
                gsrm.SearchOptions = 2;
            InitializeGSRM();
        }
        private void SearchOptionImages_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SearchOptionWeb.Foreground = SearchOptionUnselected;
            SearchOptionNews.Foreground = SearchOptionUnselected;
            SearchOptionBooks.Foreground = SearchOptionSelected;
            SearchOptionVideos.Foreground = SearchOptionUnselected;
            if (gsrm != null)
                gsrm.SearchOptions = 3;
            InitializeGSRM();
        }
        private int FindSearchOption()
        {
            if (SearchOptionWeb.Foreground.Equals(SearchOptionSelected))
                return 0;
            else if (SearchOptionNews.Foreground.Equals(SearchOptionSelected))
                return 1;
            else if (SearchOptionVideos.Foreground.Equals(SearchOptionSelected))
                return 2;
            else if (SearchOptionBooks.Foreground.Equals(SearchOptionSelected))
                return 3;
            return 0;
        }

        private void PreviousPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (gsrm != null)
            {
                gsrm.PrevPage();
                LoadFullPage();
            }
        }

        private void NextPage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (gsrm != null)
            {
                gsrm.NextPage();
                LoadFullPage();
            }
        }

        private void PageWidthSetting_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (PageWidthSetting.Text.Length != 0)
            {
                int width;
                bool parse = int.TryParse(PageWidthSetting.Text, out width);
                if (parse)
                    SidePaneWidth = width;
                else if (PageWidthSetting.Text.Contains("*"))
                    SidePaneWidth = (int)Width;
            }
        }
    }
}
