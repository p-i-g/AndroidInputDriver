using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
        public MainPage() {
            this.InitializeComponent();
            startServer();
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
        public async void StreamSocketListener_ConnectionReceived(StreamSocketListener sender, StreamSocketListenerConnectionReceivedEventArgs args) {
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add("connection received"));
            StreamReader streamReader = new StreamReader(args.Socket.InputStream.AsStreamForRead());
            for (; ; ) {
                string request;
                request = await streamReader.ReadLineAsync();
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => this.serverListBox.Items.Add(string.Format("server received the request: \"{0}\"", request)));
            }
        }
    }
}
