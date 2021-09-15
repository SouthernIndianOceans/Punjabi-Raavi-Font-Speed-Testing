using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Threading;

namespace Punjabi_Speed_Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool Maxi;
        private List<string> lessonspath;
        private DispatcherTimer Tester;
        private int uptime, downtime, totaltime;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void Quit(object s, RoutedEventArgs e) => Exit();
        private void MaximizeBtn(object s, RoutedEventArgs e) => Maximize();
        private void MinimizeBtn(object s, RoutedEventArgs e) => Minimize();
        private void Window_Loaded(object sender, RoutedEventArgs e) => LoadItems();
        private void Stop(object s, RoutedEventArgs e) => Stoping();
        private void Exit() => Application.Current.Shutdown();
        private void Minimize() => WindowState = WindowState.Minimized;
        private void Maximize()
        {
            if (Maxi)
            {
                Width = 1200;
                Height = 600;
                Left = (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width - 1200) / 2;
                Top = (System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height - 600) / 2;
                Maxi = false;
                MM.Style = TryFindResource("Maximize") as Style;
                Tutor.Height = 268;
                Writer.Height = 268;
            }
            else
            {
                Width = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width;
                Height = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height;
                Left = 0;
                Top = 0;
                Maxi = true;
                MM.Style = TryFindResource("Maximized") as Style;
                Tutor.Height = (Area.ActualHeight / 2) - 3;
                Writer.Height = (Area.ActualHeight / 2) - 3;
            }
        }
        private void LoadItems()
        {
            try
            {
                lessonspath = new List<string>();
                string[] lesson = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\data\\");
                foreach (string variable in lesson)
                {
                    if (Path.GetExtension(variable) == ".txt")
                    {
                        lessonspath.Add(variable);
                        lessonbox.Items.Add(Path.GetFileName(variable));
                    }
                }
                Tester = new DispatcherTimer
                {
                    Interval = TimeSpan.FromSeconds(1)
                };
                Tester.Tick += Testing;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private void Start(object s, RoutedEventArgs e)
        {
            try
            {
                string filetoload = lessonspath[lessonbox.SelectedIndex];
                if (File.Exists(filetoload))
                {
                    TextRange tr = new TextRange(Tutor.Document.ContentStart, Tutor.Document.ContentEnd);
                    FileStream fs = new FileStream(filetoload, FileMode.Open);
                    tr.Text = "";
                    tr.Load(fs, DataFormats.Text);
                    fs.Close();
                }
                Writer.SelectAll();
                Writer.Selection.Text = "";
                uptime = 0;
                Uptimevalue.Content = "00:00";
                totaltime = int.Parse(Timebox.SelectionBoxItem.ToString().Substring(0, 2));
                TimeSpan ts = TimeSpan.FromMinutes(totaltime);
                Downtimevalue.Content = ts.Seconds.ToString();
                totaltime *= 60;
                downtime = totaltime;
                Progress.Value = 0;

                Tester.IsEnabled = true;
                Controlers.Content = "Test : " + Path.GetFileName(filetoload);

                StartPanel.Visibility = Visibility.Hidden;
                WorkPanel.Visibility = Visibility.Visible;

                Tester.Start();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private void Stoping()
        {
            try
            {
                if (Tester.IsEnabled)
                {
                    Tester.Stop();
                    Tester.IsEnabled = false;
                }
                StartPanel.Visibility = Visibility.Visible;
                WorkPanel.Visibility = Visibility.Hidden;
                TextRange tr1 = new TextRange(Writer.Document.ContentStart, Writer.Document.ContentEnd);
                TextRange tr2 = new TextRange(Tutor.Document.ContentStart, Tutor.Document.ContentEnd);
                string target = tr2.Text;
                string destin = tr1.Text;
                int correct = 0, wrong = 0;
                for (int i = 0; i < destin.Length; i++)
                {
                    if (destin[i] == target[i])
                    {
                        correct++;
                    }
                    else
                    {
                        wrong++;
                    }
                }
                int accuracy = correct * 100 / destin.Length;
                int gwpm = destin.Length / 5 / (uptime / 60);
                int nwpm = correct / 5 / (uptime / 60);
                string[] rss = new string[6];
                rss[0] = "Typed : " + destin.Length;
                rss[1] = "Correct : " + correct;
                rss[2] = "Wrong : " + wrong;
                rss[3] = "GWPM : " + gwpm;
                rss[4] = "NWPM : " + nwpm;
                rss[5] = "Accuracy : " + accuracy + " % ";
                Designs.ResultBox rb = new Designs.ResultBox();
                rb.a.Content = "Typed : " + destin.Length;
                rb.b.Content = "Correct : " + correct;
                rb.c.Content = "Wrong : " + wrong;
                rb.d.Content = "GWPM : " + gwpm;
                rb.e.Content = "NWPM : " + nwpm;
                rb.f.Content = "Accuracy : " + accuracy + " % ";
                Area.Children.Add(rb);
                Controlers.Content += " Completed...";
                string cd = Directory.GetCurrentDirectory();
                if (!Directory.Exists(cd + "\\Results"))
                {
                    Directory.CreateDirectory(cd + "\\Results");
                }
                string timenow = DateTime.Now.ToLongTimeString();
                File.WriteAllLines(cd + "\\Results\\R" + timenow + ".txt", rss);
                Resultlist.Items.Add(timenow);
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        private void Testing(object s, EventArgs e)
        {
            if (downtime <= 0)
            {
                Stoping();
            }
            uptime++;
            downtime--;
            Uptimevalue.Content = TimeSpan.FromSeconds(uptime);
            Downtimevalue.Content = TimeSpan.FromSeconds(downtime);
            Progress.Value = uptime * 100 / totaltime;
        }
        private void Writer_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                TextRange tr1 = new TextRange(Writer.Document.ContentStart, Writer.Document.ContentEnd);
                TextRange tr2 = new TextRange(Tutor.Document.ContentStart, Tutor.Document.ContentEnd);
                if (tr1.Text.Length == tr2.Text.Length)
                {
                    Stoping();
                }
                if (Writer.Document.Blocks.Count > 3) {
                    int xx = (Writer.Document.Blocks.Count / 3 * 100)+25;
                    Tutor.ScrollToVerticalOffset(xx);
                }
                int percentcomp = tr1.Text.Length * 100 / tr2.Text.Length;
                Tasks.Content = "Words : " + tr1.Text.Length + " / " + tr2.Text.Length + "   Lines : " + Writer.Document.Blocks.Count + " / " + Tutor.Document.Blocks.Count + "   Complete : " + percentcomp + " %";
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
