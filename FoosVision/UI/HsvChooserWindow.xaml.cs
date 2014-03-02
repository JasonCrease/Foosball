using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Runtime.InteropServices;
using Emgu.CV;

namespace UI
{
    /// <summary>
    /// Interaction logic for MaiHsvChooserWindownWindow.xaml
    /// </summary>
    public partial class HsvChooserWindow : Window
    {
        int pieceX = 0;
        int pieceY = 0;

        public HsvChooserWindow()
        {
            this.Left = 0;
            this.Top = 0;

            InitializeComponent();

            SliderHmin.Value = 0;
            SliderHmax.Value = 23;
            SliderSmin.Value = 40;
            SliderSmax.Value = 143;
            SliderVmin.Value = 118;
            SliderVmax.Value = 256;

            SliderHmin.Value = 21;
            SliderHmax.Value = 89;
            SliderSmin.Value = 0;
            SliderSmax.Value = 30;
            SliderVmin.Value = 42;
            SliderVmax.Value = 201;

            SliderHmin.Value = 0;
            SliderHmax.Value = 256;
            SliderSmin.Value = 0;
            SliderSmax.Value = 256;
            SliderVmin.Value = 0;
            SliderVmax.Value = 160;

            RedrawTimer = new System.Threading.Timer(new System.Threading.TimerCallback(Redraw), null, 500, 2000);

            //ButtonGo_Click(this, null);
        }

        System.Threading.Timer RedrawTimer;
        Engine.Engine m_Engine;

        private void Redraw(object stateInfo)
        {
            Dispatcher.Invoke(new Action(() => TimerRender()), null);
        }

        private void TimerRender()
        {
            LabelHmax.Content = SliderHmax.Value.ToString();
            LabelHmin.Content = SliderHmin.Value.ToString();
            LabelSmax.Content = SliderSmax.Value.ToString();
            LabelSmin.Content = SliderSmin.Value.ToString();
            LabelVmax.Content = SliderVmax.Value.ToString();
            LabelVmin.Content = SliderVmin.Value.ToString();

            ButtonGo_Click(this, null);
        }

        private void ButtonGo_Click(object sender, RoutedEventArgs e)
        {
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();

            var tableImage = new Image<Emgu.CV.Structure.Bgr, byte>(System.IO.Path.GetFullPath(".\\..\\Images\\pic11.jpg"));

            //Engine.TableFinder finder = new Engine.TableFinder();
            //var points = finder.GetTableCorners(tableImage);
            //OrigImage.Source = UI.Utils.BitmapSourceConvert.ToBitmapSource(finder.ThresholdImage);

            //return;

            //m_Engine = new Engine.Engine();
            //m_Engine.ProcessNextFrame(tableImage);

            var resultImage = Engine.ImageProcess.ThresholdHsv(tableImage, 
                (int)SliderHmin.Value, (int)SliderHmax.Value, 
                (int)SliderSmin.Value, (int)SliderSmax.Value, 
                (int)SliderVmin.Value, (int)SliderVmax.Value);
            //resultImage = resultImage.Erode(0);


            OrigImage.Source = UI.Utils.BitmapSourceConvert.ToBitmapSource(resultImage);

            //sw.Stop();
            //TextBlockTimeTaken.Text = sw.ElapsedMilliseconds.ToString() + "ms";
        }
    }
}
