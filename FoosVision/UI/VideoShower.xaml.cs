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
            this.Left = 0;
            this.Top = 0;
            InitializeComponent();
        }

        Capture m_Capture;
        Engine.Engine m_Engine;
        object frameLock = new object();

        int m_ExpectedFrame = 0;
        int m_ActualFrame = 0;
        int m_FramesDropped = 0;

        private void ExpectedFrameUpdate(object dontCare)
        {
            m_ExpectedFrame++;
        }

        private void ShowFrames(object dontCare)
        {
            Engine.TableFinder finder = new Engine.TableFinder();

            for (; ; )
            {
                if (m_ActualFrame < m_ExpectedFrame)    // We're behind
                {
                    if (m_Capture.Grab())   // So grab the next frame, but don't process it
                    {
                        m_ActualFrame++;
                        m_FramesDropped++;
                    }
                    else       // No frames left. So exit loop
                    {
                        m_Playing = false;

                        if (m_Timer != null)
                        {
                            m_Timer.Change(-1, -1);
                            m_Timer = null;
                        }
                        return;
                    }
                }
                else if (m_ActualFrame > m_ExpectedFrame)
                {
                    Thread.Sleep(10);
                }
                else
                {
                    m_ActualFrame++;
                    Image<Bgr, byte> img = m_Capture.QueryFrame();
                    if (img != null)
                    {
                        Image<Bgr, byte> resized = img; // img.Resize(720, 1280, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC);

                        m_Engine.TableImage = resized;
                        m_Engine.Process();
                        var imageToShow = m_Engine.TableImage;

                        var points = finder.GetTableCorners(resized);

                        imageToShow.Draw(new CircleF(points[0], 12), new Bgr(0, 0, 200), 4);
                        imageToShow.Draw(new CircleF(points[1], 12), new Bgr(0, 100, 200), 4);
                        imageToShow.Draw(new CircleF(points[2], 12), new Bgr(100, 0, 200), 4);
                        imageToShow.Draw(new CircleF(points[3], 12), new Bgr(0, 200, 200), 4);

                        Dispatcher.Invoke(new Action(() =>
                            {
                                vidImage.Source = UI.Utils.BitmapSourceConvert.ToBitmapSource(imageToShow);
                                labelBallSpeed.Content = m_Engine.BallSpeed.ToString("#.##");
                            }));
                        resized = null;
                        img = null;

                        //GC.Collect();
                    }
                    else
                    {
                        m_Playing = false;

                        if (m_Timer != null)
                        {
                            m_Timer.Change(-1, -1);
                            m_Timer = null;
                        }
                        return;
                    }
                }
            }
        }

        bool m_Playing = false;

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            m_Playing = true;
            m_Engine = new Engine.Engine();
            m_Capture = new Capture(System.IO.Path.GetFullPath(".\\..\\Videos\\vid6.mp4"));
            m_Timer = new Timer(ExpectedFrameUpdate, null, 0, 1000 / 29);
            m_DisplayFrames = new Thread(ShowFrames);
            m_DisplayFrames.Start();
        }

        Timer m_Timer;
        Thread m_DisplayFrames;
    }
}
