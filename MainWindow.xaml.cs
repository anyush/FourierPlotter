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
// using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Collections.ObjectModel;
using System.IO;
using Microsoft.Win32;

namespace WPF2
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DateTime lastTime;
        long ticksExec = (new TimeSpan(0, 0, 10)).Ticks;
        long ticksLeft;
        ObservableCollection<Circle> instances;
        System.Windows.Threading.DispatcherTimer progTimer;
        List<System.Drawing.Point> shape;
        public MainWindow()
        {
            InitializeComponent();
            progTimer = new System.Windows.Threading.DispatcherTimer();
            progTimer.Tick += new EventHandler(UpdateProgressBar);
            progTimer.Interval = new TimeSpan(0, 0, 0, 0, 10);
            shape = new List<System.Drawing.Point>();

            instances = new ObservableCollection<Circle>();

            DGrid.ItemsSource = instances;
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Window dialog = new WPF2.ExitWindow();
            if (dialog.ShowDialog() == true)
                this.Close();
        }

        private void UpdateProgressBar(object sender, EventArgs e)
        {
            TimeSpan sinceLast = DateTime.Now - lastTime;
            ticksLeft -= sinceLast.Ticks;
            if (ticksLeft < 0)
                ticksLeft = 0;

            ProgBar.Value = ProgBar.Maximum * (ticksExec - ticksLeft) / ticksExec;
            lastTime = DateTime.Now;
            Render();
            if (ticksLeft == 0)
            {
                progTimer.Stop();
                Render();
                return;
            }
        }

        // https://swharden.com/CsharpDataVis/drawing/3-drawing-in-wpf.md.html
        private BitmapImage BmpImageFromBmp(Bitmap bmp)
        {
            using (var memory = new System.IO.MemoryStream())
            {
                bmp.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
        private void Render()
        {
            int width = (int)GBImg.ActualWidth;
            int height = (int)GBImg.ActualHeight;
            using (var bmp = new Bitmap(width, height))
            using (var gfx = Graphics.FromImage(bmp))
            using (var pen = new Pen(Color.Black))
            using (var b_pen = new Pen(Color.Blue))
            using (var r_brush = new SolidBrush(Color.Red))
            {
                gfx.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                gfx.Clear(Color.White);

                System.Drawing.Point center = new System.Drawing.Point(width / 2, height / 2);


                foreach (Circle c in instances)
                {
                    uint radius = c.Radius;

                    if (drCircChk.IsChecked)
                        gfx.DrawEllipse(pen, center.X - radius, center.Y - radius, 2 * radius, 2 * radius);


                    double moment = (double)(c.Frequency * (ticksExec - ticksLeft)) / ticksExec;
                    moment -= (int)moment;
                    double angle = moment * 2 * Math.PI;


                    System.Drawing.Point new_center =
                        new System.Drawing.Point((int)(center.X + radius * Math.Cos(angle)),
                        (int)(center.Y + radius * Math.Sin(angle)));

                    if (drLnChk.IsChecked)
                        gfx.DrawLine(pen, center, new_center);

                    center = new_center;
                }

                shape.Add(center);

                if (shape.Count > 1)
                    gfx.DrawCurve(b_pen, shape.ToArray());

                int point_radius = 3;
                gfx.FillEllipse(r_brush, center.X - point_radius, center.Y - point_radius, 2 * point_radius, 2 * point_radius);

                Img.Source = BmpImageFromBmp(bmp);
            }
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (progTimer.IsEnabled || ProgBar.Value == ProgBar.Maximum)
                return;
            lastTime = DateTime.Now;
            if (ticksLeft == 0)
                ticksLeft = ticksExec;
            progTimer.Start();
        }

        private void PauseButton_Click(object sender, RoutedEventArgs e)
        {
            progTimer.Stop();
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        // https://social.msdn.microsoft.com/Forums/vstudio/de-DE/d9afc793-ec66-428f-a087-bccbbcffda73/save-datagrid-row-changes-when-leaving-row?forum=wpf
        private void DGrid_SourceUpdated(object sender, DataTransferEventArgs e)
        {
            Render();
        }

        private void DGrid_KeyUp(object sender, KeyEventArgs e)
        {
            Render();
        }

        private void OptChngd_Click(object sender, RoutedEventArgs e)
        {
            Render();
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            instances = new ObservableCollection<Circle>();
            DGrid.ItemsSource = instances;
            Reset();
        }

        private void Reset()
        {
            progTimer.Stop();
            ticksLeft = ticksExec;
            ProgBar.Value = 0;
            shape = new List<System.Drawing.Point>();
            Render();
        }
        private void Open_Click(object sender, RoutedEventArgs e)
        {
            Stream file;
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "XML files (*.xml)|*.xml";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == true)
            {
                if ((file = dlg.OpenFile()) != null)
                {
                    ObservableCollection<Circle> data = SaverOpener.OpenFile(file);
                    if (data != null)
                    {
                        instances = data;
                        DGrid.ItemsSource = instances;
                        Render();
                    }
                    file.Close();
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            Stream file;
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "XML files (*.xml)|*.xml";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() == true)
            {
                if ((file = dlg.OpenFile()) != null)
                {
                    SaverOpener.SaveFile(file, ref instances);
                    file.Close();
                }
            }
        }
    }
}
