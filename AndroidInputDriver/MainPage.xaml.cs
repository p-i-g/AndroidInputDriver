using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Networking.Sockets;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace AndroidInputDriver {
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page {
        private static string SERVER_PORT = "3939";
        private VirtualDevice virtualDevice;
        private uint width;
        private uint height;
        public MainPage() {
            this.InitializeComponent();
            connectButton.AddHandler(PointerPressedEvent, new PointerEventHandler(Connect), true);
            width = DisplayInformation.GetForCurrentView().ScreenWidthInRawPixels;
            height = DisplayInformation.GetForCurrentView().ScreenHeightInRawPixels;
        }

        private async void startServer() {
            try {
                StreamSocketListener streamSocketListener = new StreamSocketListener();

                streamSocketListener.ConnectionReceived += this.StreamSocketListener_ConnectionReceived;

                await streamSocketListener.BindServiceNameAsync(SERVER_PORT);

                this.serverListBox.Items.Add("server is listening...");
            } catch (Exception e) {
                SocketErrorStatus socketErrorStatus = SocketError.GetStatus(e.GetBaseException().HResult);
                this.serverListBox.Items.Add(socketErrorStatus.ToString() != "Unknown" ? socketErrorStatus.ToString() : e.Message);
            }
        }
        private async void StreamSocketListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args) {
            this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add("connection received"));
            StreamReader streamReader = new StreamReader(args.Socket.InputStream.AsStreamForRead());
            string handshake;
            handshake = await streamReader.ReadLineAsync();
            if (handshake.Equals("Android Input Device")) {
                this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add("handshake message received"));
                using (Stream outputStream = args.Socket.OutputStream.AsStreamForWrite()) {
                    using (var streamWriter = new StreamWriter(outputStream)) {
                        await streamWriter.WriteLineAsync("Android Input Driver");
                        await streamWriter.FlushAsync();
                    }
                }
            } else {
                sender.Dispose();
                return;
            }
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add("connection received"));
            for (; ; ) {
                string request;
                request = await streamReader.ReadLineAsync();
                if(request == null) {
                    sender.Dispose();
                    return;
                }
                this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add(string.Format("server received the request: \"{0}\"", request)));
                InjectInput(request);
            }
        }
        private void Connect(object sender, PointerRoutedEventArgs e) {
            startServer();
            virtualDevice = new VirtualDevice(e.GetCurrentPoint((Button) sender));
            virtualDevice.InjectInputForPen(1000, 1000, 0, false);
        }
        private async void InjectInput(String request) {
            string[] split = request.Split(':');
            bool contact;
            if (split[0].Equals("DOWN")) {
                contact = true;
                int x, y;
                double pressure;
                string[] split2 = split[1].Split(',');
                x = (int)(Double.Parse(split2[0]) * width);
                y = (int)(Double.Parse(split2[1]) * height);
                pressure = Double.Parse(split2[2]);
                virtualDevice.InjectInputForPen(x, y, pressure, contact);
            } else if(split[0].Equals("HOVER")) {
                contact = false;
                int x, y;
                double pressure;
                string[] split2 = split[1].Split(',');
                x = (int)(Double.Parse(split2[0]) * width);
                y = (int)(Double.Parse(split2[1]) * height);
                pressure = Double.Parse(split2[2]);
                virtualDevice.InjectInputForPen(x, y, pressure, contact);
            }else if (split[0].Equals("MACRO")) {

                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add(split[1]));
                if (!String.IsNullOrWhiteSpace(split[1])) {
                    virtualDevice.InjectInputForKeyboard(split[1].Split(","));
                }
            }
        }
    }
}
