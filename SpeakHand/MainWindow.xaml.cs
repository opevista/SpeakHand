using WpfAnimatedGif;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using NAudio.CoreAudioApi;

namespace SpeakHand
{
    public class AudioDevice
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IsFavorite { get; set; }
    }

    public partial class MainWindow : Window
    {
        private const string JsonFileName = "speakers.json";
        private List<AudioDevice> _allDevices = new List<AudioDevice>();

        public MainWindow()
        {
            InitializeComponent();
            // GIF画像をWpfAnimatedGifでセット
            try
            {
                var uri = new Uri("pack://application:,,,/thinking_face_animated.gif", UriKind.Absolute);
                var image = new System.Windows.Media.Imaging.BitmapImage();
                image.BeginInit();
                image.UriSource = uri;
                image.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                image.EndInit();
                ImageBehavior.SetAnimatedSource(FavoritesEmptyImage, image);
            }
            catch { }
            ScanAndLoadDevices();
            UpdateFavoritesPanel();
        }

        private void ScanAndLoadDevices()
        {
            var enumerator = new MMDeviceEnumerator();
            var devices = enumerator.EnumerateAudioEndPoints(DataFlow.Render, DeviceState.Active);
            var currentDevices = devices.Select(d => new AudioDevice { Id = d.ID, Name = d.FriendlyName, IsFavorite = false }).ToList();

            if (File.Exists(JsonFileName))
            {
                try
                {
                    var json = File.ReadAllText(JsonFileName);
                    var savedDevices = JsonSerializer.Deserialize<List<AudioDevice>>(json);
                    
                    foreach (var currentDevice in currentDevices)
                    {
                        var savedDevice = savedDevices.FirstOrDefault(d => d.Id == currentDevice.Id);
                        if (savedDevice != null)
                        {
                            currentDevice.IsFavorite = savedDevice.IsFavorite;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading favorites: {ex.Message}");
                }
            }
            
            _allDevices = currentDevices;
            SpeakerComboBox.ItemsSource = _allDevices.Select(d => d.Name);
            UpdateFavoritesPanel();
        }

        private void UpdateFavoritesPanel()
        {
            FavoritesPanel.Children.Clear();
            var favoriteDevices = _allDevices.Where(d => d.IsFavorite).ToList();
            if (FavoritesEmptyPanel != null)
            {
                FavoritesEmptyPanel.Visibility = favoriteDevices.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            foreach (var device in favoriteDevices)
            {
                var panel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(5) };

                var button = new Button
                {
                    Content = device.Name,
                    Tag = device.Id,
                    Margin = new Thickness(0, 0, 5, 0)
                };
                button.Click += FavoriteButton_Click;


                var removeButton = new Button
                {
                    Tag = device.Id,
                    Width = 24,
                    Height = 24,
                    ToolTip = "Remove from favorites",
                    Padding = new Thickness(0),
                    Margin = new Thickness(0)
                };
                // ContentをTextBlockで明示しフォントサイズも指定
                removeButton.Content = new TextBlock { Text = "✕", FontSize = 16, HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
                removeButton.Click += RemoveFavoriteButton_Click;

                panel.Children.Add(button);
                panel.Children.Add(removeButton);
                FavoritesPanel.Children.Add(panel);
            }
        }

        private void RemoveFavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string deviceId)
            {
                var device = _allDevices.FirstOrDefault(d => d.Id == deviceId);
                if (device != null)
                {
                    device.IsFavorite = false;
                    SaveDevicesToJson();
                    UpdateFavoritesPanel();
                }
            }
        }

        private void SaveDevicesToJson()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(_allDevices, options);
            File.WriteAllText(JsonFileName, json);
        }

        private void SetDefaultDevice(string deviceId)
        {
            try
            {
                var policyConfig = (IPolicyConfig)new PolicyConfigClient();
                policyConfig.SetDefaultEndpoint(deviceId, ERole.eMultimedia);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}\nFailed to switch default device.");
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(JsonFileName))
                {
                    File.Delete(JsonFileName);
                }
                ScanAndLoadDevices();
                MessageBox.Show("Favorites and device list have been reset.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Reset failed: {ex.Message}");
            }
        }

        private void ScanButton_Click(object sender, RoutedEventArgs e)
        {
            ScanAndLoadDevices();
            MessageBox.Show("Device list has been refreshed.");
        }

        private void SetDefaultButton_Click(object sender, RoutedEventArgs e)
        {
            if (SpeakerComboBox.SelectedItem == null) return;
            var selectedDeviceName = SpeakerComboBox.SelectedItem.ToString();
            var deviceToSet = _allDevices.FirstOrDefault(d => d.Name == selectedDeviceName);
            if (deviceToSet?.Id != null)
            {
                SetDefaultDevice(deviceToSet.Id);
            }
        }

        private void FavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.Tag is string deviceId)
            {
                SetDefaultDevice(deviceId);
            }
        }

        private void ToggleFavoriteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SpeakerComboBox.SelectedItem == null) return;
            var selectedDeviceName = SpeakerComboBox.SelectedItem.ToString();
            var deviceToToggle = _allDevices.FirstOrDefault(d => d.Name == selectedDeviceName);

            if (deviceToToggle != null)
            {
                deviceToToggle.IsFavorite = !deviceToToggle.IsFavorite;
                SaveDevicesToJson();
                UpdateFavoritesPanel();
            }
        }
    }

    // --- COM Interface Definitions ---
    public enum ERole
    {
        eConsole = 0,
        eMultimedia = 1,
        eCommunications = 2
    }

    [ComImport]
    [Guid("f8679f50-850a-41cf-9c72-430f290290c8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    interface IPolicyConfig
    {
        void GetMixFormat();
        void GetDeviceFormat();
        void ResetDeviceFormat();
        void SetDeviceFormat();
        void GetProcessingPeriod();
        void SetProcessingPeriod();
        void GetShareMode();
        void SetShareMode();
        void GetPropertyValue();
        void SetPropertyValue();
        void SetDefaultEndpoint([MarshalAs(UnmanagedType.LPWStr)] string wszDeviceId, ERole eRole);
        void SetEndpointVisibility();
    }

    [ComImport]
    [Guid("870af99c-171d-4f9e-af0d-e63df40c2bc9")]
    class PolicyConfigClient { }
}