using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
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
using System.Windows.Shapes;
using WpfAudioPlayer;

namespace WpfAudioPlayer
{
    /// <summary>
    /// Interaction logic for Playlists.xaml
    /// </summary>
    public partial class Playlists : Window
    {
        public Playlists()
        {
            InitializeComponent();
        }

        private void btnSavePlaylist_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text File|*.txt";
            if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllLines(saveFileDialog.FileName, lbPlaylistAudios.Items.Cast<string>());
            }
        }

        private void btnLoadPlaylist_Click(object sender, RoutedEventArgs e)
        {
            lbPlaylistAudios.Items.Clear();
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Text File|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                StreamReader streamReader = new StreamReader(openFileDialog.FileName);
                while (!streamReader.EndOfStream)
                {
                    lbPlaylistAudios.Items.Add(streamReader.ReadLine());
                }
                streamReader.Close();
            }
        }

        private void btnAddSong_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Multiselect = true;
            openFileDialog.Filter = "Audio Files|*.mp3;*.wav;*.wma;*.m4a";
            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string? file in openFileDialog.FileNames)
                {
                    lbPlaylistAudios.Items.Add(file);
                }
            }
        }

        private void btnRemoveSong_Click(object sender, RoutedEventArgs e)
        {
            for (int i = lbPlaylistAudios.SelectedItems.Count - 1; i >= 0; i--)
            {
                string audio = Convert.ToString(lbPlaylistAudios.SelectedItems[i]);
                lbPlaylistAudios.Items.Remove(audio);
            }
        }

        private void btnPlayPlaylist_Click(object sender, RoutedEventArgs e)
        {
            if (lbPlaylistAudios.Items.Count != 0)
            {
                MainWindow.playlist.Clear();
                foreach (string audio in lbPlaylistAudios.Items.Cast<string>())
                {
                    MainWindow.playlist.Add(audio);
                }
                MainWindow.currentAudioIndex = 0;
                MainWindow.mediaPlayer.Open(new Uri(MainWindow.playlist[MainWindow.currentAudioIndex]));
                MainWindow.Play();
            }
        }
    }
}
