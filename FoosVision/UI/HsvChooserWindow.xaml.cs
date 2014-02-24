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

namespace FoosVision
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

            SliderHmin.Value = 10;
            SliderHmax.Value = 100;
            SliderSmin.Value = 10;
            SliderSmax.Value = 100;
            SliderVmin.Value = 10;
            SliderVmax.Value = 100;

            RedrawTimer = new System.Threading.Timer(new System.Threading.TimerCallback(Redraw), null, 500, 500);

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
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();

            m_Engine = new Engine.Engine();
            m_Engine.TableImagePath = System.IO.Path.GetFullPath(".\\..\\Images\\pic8.jpg");
            m_Engine.Process((int)SliderHmin.Value, (int)SliderHmax.Value, 
                (int)SliderSmin.Value, (int)SliderSmax.Value, 
                (int)SliderVmin.Value, (int)SliderVmax.Value);

            sw.Stop();
            TextBlockTimeTaken.Text = sw.ElapsedMilliseconds.ToString() + "ms";

            OrigImage.Source = UI.Utils.BitmapSourceConvert.ToBitmapSource(m_Engine.TableImageMasked);
        }
    }
}
