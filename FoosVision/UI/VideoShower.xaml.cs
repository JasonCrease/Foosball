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
using System.Windows.Shapes;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Threading;

namespace SeeBoard
{
    /// <summary>
    /// Interaction logic for VideoShower.xaml
    /// </summary>
    public partial class VideoShower : Window
    {
        public VideoShower()
        {
            InitializeComponent();
        }

        Capture m_Capture;
        Engine.Engine m_Engine;
        object frameLock = new object();

        private void NextFrame(object dontCare)
        {
            Dispatcher.Invoke(new Action(() => ShowFrame() ));
        }

        private void ShowFrame()
        {
            if (!m_Playing) return;

            lock (frameLock)
            {
                Image<Bgr, byte> img = m_Capture.QueryFrame();
                if (img != null)
                {
                    Image<Bgr, byte> resized = img; // img.Resize(720, 1280, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                    m_Engine.TableImage = resized;
                    //m_Engine.Process(17, 89, 33, 135, 42, 201);
                    m_Engine.Process(17, 32, 130, 256, 118, 256);

                    vidImage.Source = UI.Utils.BitmapSourceConvert.ToBitmapSource(m_Engine.TableImage);
                    resized = null;
                    img = null;
                    //GC.Collect();
                }
                else
                {
                    m_Playing = false;
                    m_Timer = null;
                }
            }
        }

        bool m_Playing = false;

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            m_Playing = true;
            m_Engine = new Engine.Engine();
            m_Capture = new Capture(System.IO.Path.GetFullPath(".\\..\\Videos\\vid2.mp4"));
            m_Timer = new Timer(NextFrame, null, 0, 1000 / 12);
        }

        Timer m_Timer;
    }
}
