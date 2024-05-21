using Microsoft.Win32;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Threading;

namespace WpfAudioPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MediaPlayer mediaPlayer = new MediaPlayer();
        DispatcherTimer timer = new DispatcherTimer();
        OpenFileDialog openFileDialog;
        string folderPath;
        List<string> folderPlaylist;
        Button pauseButton;
        int currentAudioIndex;
        public MainWindow()
        {
            InitializeComponent();
            mediaPlayer.MediaOpened += MediaPlayer_MediaOpened;
            mediaPlayer.MediaEnded += MediaPlayer_MediaEnded;
            timer.Interval = TimeSpan.FromMilliseconds(1);
            timer.Tick += Timer_Tick;

            //Pause button
            pauseButton = new Button
            {
                Name = "btnPause",
                Content = "⏸",
                FontSize = 16,
                Width = 40,
                Height = 40,
                IsEnabled = false,
                Visibility = Visibility.Collapsed
            };
            pauseButton.Click += btnPause_Click;
            spButtonPanel.Children.Insert(spButtonPanel.Children.IndexOf(btnPlay) + 1, pauseButton);
        }
        private void MediaPlayer_MediaOpened(object? sender, EventArgs e)
        {
            btnPlay.Visibility = Visibility.Collapsed;
            pauseButton.Visibility = Visibility.Visible;
            UpdateSongInfo();
            Play();
            btnPrevious.IsEnabled = true;
            btnNext.IsEnabled = true;
            btnPlay.IsEnabled = true;
            pauseButton.IsEnabled = true;
        }
        private void MediaPlayer_MediaEnded(object sender, EventArgs e)
        {
            PlayNext();
        }
        private void Timer_Tick(object sender, EventArgs e)
        {
            if (mediaPlayer.NaturalDuration.HasTimeSpan)
            {
                sliProgress.Minimum = 0;
                sliProgress.Maximum = mediaPlayer.NaturalDuration.TimeSpan.TotalMilliseconds;
                sliProgress.Value = mediaPlayer.Position.TotalMilliseconds;
                lblCurrentTime.Content = mediaPlayer.Position.ToString(@"mm\:ss");
                lblTotalTime.Content = mediaPlayer.NaturalDuration.TimeSpan.ToString(@"mm\:ss");
            }
        }
        private void btnPlay_Click(object sender, RoutedEventArgs e)
        {
            btnPlay.Visibility = Visibility.Collapsed;
            pauseButton.Visibility = Visibility.Visible;
            Play();
        }
        private void Play()
        {
            mediaPlayer.Play();
            timer.Start();
        }
        private void btnPause_Click(object sender, RoutedEventArgs e)
        {
            pauseButton.Visibility = Visibility.Collapsed;
            btnPlay.Visibility = Visibility.Visible;
            mediaPlayer.Pause();
            timer.Stop();
        }
        private void btnPrevious_Click(object sender, RoutedEventArgs e)
        {
            if (currentAudioIndex > 0)
            {
                currentAudioIndex--;
            }
            else if (currentAudioIndex == 0)
            {
                currentAudioIndex = folderPlaylist.Count - 1;
            }
            mediaPlayer.Open(new Uri(folderPlaylist[currentAudioIndex]));
            UpdateSongInfo();
            Play();
        }
        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            PlayNext();
        }
        private void PlayNext()
        {
            if (currentAudioIndex < folderPlaylist.Count - 1)
            {
                currentAudioIndex++;
            }
            else if (currentAudioIndex == folderPlaylist.Count - 1)
            {
                currentAudioIndex = 0;
            }
            mediaPlayer.Open(new Uri(folderPlaylist[currentAudioIndex]));
            UpdateSongInfo();
            Play();
        }
        private void UpdateSongInfo()
        {
             TagLib.File file = TagLib.File.Create(folderPlaylist[currentAudioIndex]);
             tbTitle.Text = file.Tag.Title;
             tbArtist.Text = file.Tag.FirstPerformer;
             lblTotalTime.Content = file.Properties.Duration.ToString(@"mm\:ss");

             if (file.Tag.Pictures.Length > 0)
             {
                 var bin = file.Tag.Pictures[0].Data.Data;
                 imgAlbum.Source = LoadImage(bin);
             }
             else
             {
                 imgAlbum.Source = null;
             }
        }
        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null || imageData.Length == 0)
                return null;

            var image = new BitmapImage();
            using (var mem = new System.IO.MemoryStream(imageData))
            {
                mem.Position = 0;
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = null;
                image.StreamSource = mem;
                image.EndInit();
            }
            image.Freeze();
            return image;
        }
        private void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Audio Files|*.mp3;*.wav;*.wma";
            if (openFileDialog.ShowDialog() == true)
            {
                folderPath = System.IO.Path.GetDirectoryName(openFileDialog.FileName);
                folderPlaylist = Directory.GetFiles(folderPath, "*.mp3")
                                       .Concat(Directory.GetFiles(folderPath, "*.wav"))
                                       .Concat(Directory.GetFiles(folderPath, "*.wma"))
                                       .ToList();
                currentAudioIndex = folderPlaylist.IndexOf(openFileDialog.FileName);
                mediaPlayer.Open(new Uri(folderPlaylist[currentAudioIndex]));
            }
        }
        private void btnVolume_Click(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer.Volume != 0)
            {
                mediaPlayer.Volume = 0;
                sliVolume.Value = 0;
            }
            else
            {
                mediaPlayer.Volume = 0.5;
                sliVolume.Value = sliVolume.Maximum / 2;
            }
        }
        private void sliProgress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sliProgress.IsMouseCaptureWithin)
            {
                mediaPlayer.Position = TimeSpan.FromMilliseconds(sliProgress.Value);
            }
        }
        private void sliVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sliVolume.Value == 0)
            {
                btnVolume.Content = "🔇";
            }
            else
            {
                btnVolume.Content = "🔊";
            }
            mediaPlayer.Volume = sliVolume.Value / sliVolume.Maximum;
        }
        private void sliVolume_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SliderClickToPosition(sender, e);
            if (sliVolume.Value == 0)
            {
                btnVolume.Content = "🔇";
            }
            else
            {
                btnVolume.Content = "🔊";
            }
            mediaPlayer.Volume = sliVolume.Value / sliVolume.Maximum;
        }
        private void sliProgress_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            SliderClickToPosition(sender, e);
            mediaPlayer.Position = TimeSpan.FromMilliseconds(sliProgress.Value);
        }
        private void SliderClickToPosition(object sender, MouseButtonEventArgs e)
        {
            Slider? slider = sender as Slider;
            if (slider != null)
            {
                Point mousePosition = e.GetPosition(slider);
                double ratio = mousePosition.X / slider.ActualWidth;
                double newPosition = ratio * slider.Maximum;
                slider.Value = newPosition;
            }
        }
    }
}