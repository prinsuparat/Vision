using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.IO;
using System.Windows.Forms;


namespace DataLogger
{
    public class CLS_AsyncTcpServer:IDisposable
    {
        #region fields

        private TcpListener _listener;
        private ConcurrentDictionary<string, TcpClientState> _clients;
        private bool _disposed = false;

        #endregion


        #region properties

        public bool IsRunning { get; private set; }
        public IPAddress Address { get; private set; }
        public int Port { get; private set; }
        public Encoding Encoding { get; set; }

        #endregion


        #region constructor

        public CLS_AsyncTcpServer(int listenPort)
          : this(IPAddress.Any, listenPort)
        {
        }

        public CLS_AsyncTcpServer(IPEndPoint localEP)
          : this(localEP.Address, localEP.Port)
        {
        }

        public CLS_AsyncTcpServer(IPAddress localIPAddress, int listenPort)
        {
            Address = localIPAddress;
            Port = listenPort;
            this.Encoding = Encoding.Default;

            _clients = new ConcurrentDictionary<string, TcpClientState>();
            _listener = new TcpListener(Address, Port);
            _listener.AllowNatTraversal(true);
        }

        #endregion

        #region server

        public CLS_AsyncTcpServer Start()
        {
            return Start(10);
        }

        public CLS_AsyncTcpServer Stop()
        {
            if (!IsRunning) return this;

            try
            {
                _listener.Stop();

                foreach (var client in _clients.Values)
                {
                    client.TcpClient.Client.Disconnect(false);
                }
                _clients.Clear();
            }
            catch (ObjectDisposedException ex)
            {
                ExceptionHandler.Handle(ex);
            }
            catch (SocketException ex)
            {
                ExceptionHandler.Handle(ex);
            }

            IsRunning = false;

            return this;
        }

        private CLS_AsyncTcpServer Start(int count)
        {
            if (IsRunning) return this;

            IsRunning = true;
            _listener.Start(count);
            ContinueAcceptTcpClient(_listener);

            return this;
        }

        private void ContinueAcceptTcpClient(TcpListener _listener)
        {
            try
            {
                _listener.BeginAcceptTcpClient(new AsyncCallback(HandleTcpClientAccept), _listener);
            }
            catch (ObjectDisposedException ex)
            { }
            catch (SocketException ex)
            { }
        }

        private void HandleTcpClientAccept(IAsyncResult ar)
        {
            if (!IsRunning) return;
            TcpListener tcpListener = (TcpListener)ar.AsyncState;
            TcpClient tcpClient = tcpListener.EndAcceptTcpClient(ar);
            if (!tcpClient.Connected) return;

            byte[] buffer = new byte[tcpClient.ReceiveBufferSize];
            TcpClientState internalClient = new TcpClientState(tcpClient, buffer);

            //add client connnection to cach
            string tcpClientKey = internalClient.TcpClient.Client.RemoteEndPoint.ToString();
            _clients.AddOrUpdate(tcpClientKey, internalClient,(n,o) => {return internalClient;});
            RaiseClientConnected(tcpClient);

            //begin to read data
            NetworkStream networkStream = internalClient.NetworkStream;
            ContinueReadBuffer(internalClient, networkStream);

            //key listening new connection
            ContinueAcceptTcpClient(tcpListener);
        }

        private void HandleDatagramReceived(IAsyncResult ar)
        {
            if (!IsRunning) return;

            try
            {
                TcpClientState internalClient = (TcpClientState)ar.AsyncState;
                if (!internalClient.TcpClient.Connected) return;

                NetworkStream networkStream = internalClient.NetworkStream;

                int numberOfReadBytes = 0;
                try
                {
                    numberOfReadBytes = networkStream.EndRead(ar);
                }
                catch (Exception ex)
                {
                    ExceptionHandler.Handle(ex);
                }

                if (numberOfReadBytes == 0)
                {
                    TcpClientState internalClientToBeThrowAway;
                    string tcpClientKey = internalClient.TcpClient.Client.RemoteEndPoint.ToString();
                    _clients.TryRemove(tcpClientKey, out internalClientToBeThrowAway);
                    RaiseClientDisConnected(internalClient.TcpClient);

                    return;
                }

                //receive byte and trigger event notification
                var recievedBytes = new byte[numberOfReadBytes];
                Array.Copy(internalClient.Buffer, 0, recievedBytes, 0, numberOfReadBytes);

                RaiseDatagramReceived(internalClient.TcpClient, recievedBytes);
                RaisePlaintextReceived(internalClient.TcpClient, recievedBytes);

                ContinueReadBuffer(internalClient, networkStream);
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Handle(ex);
            }
        }

        private void ContinueReadBuffer(TcpClientState internalClient, NetworkStream networkStream)
        {
            try
            {
                networkStream.BeginRead(internalClient.Buffer, 0, internalClient.Buffer.Length, HandleDatagramReceived, internalClient);
            }
            catch (ObjectDisposedException ex)
            { }
        }

        #endregion

        #region event

        public event EventHandler<TcpDatagramReceivedEventArgs<byte[]>> DatagramReceived;
        public event EventHandler<TcpDatagramReceivedEventArgs<string>> PlaintextReceived;

        private void RaiseDatagramReceived(TcpClient sender, byte[] datagram)
        {
            if (DatagramReceived != null)
            {
                DatagramReceived(this, new TcpDatagramReceivedEventArgs<byte[]>(sender, datagram));
            }
        }

        private void RaisePlaintextReceived(TcpClient sender, byte[] datagram)
        {
            if (PlaintextReceived != null)
            {
                PlaintextReceived(this, new TcpDatagramReceivedEventArgs<string>(sender, this.Encoding.GetString(datagram, 0, datagram.Length)));
            }
        }


        public event EventHandler<TcpClientConnectedEventArgs> ClientConnected;
        public event EventHandler<TcpClientDisconnectedEventArgs> ClientDisconnected;

        private void RaiseClientDisConnected(TcpClient tcpClient)
        {
            if (ClientDisconnected != null)
            {
                ClientDisconnected(this, new TcpClientDisconnectedEventArgs(tcpClient));
            } 
        }

        private void RaiseClientConnected(TcpClient tcpClient)
        {
            if (ClientConnected != null)
            {
                ClientConnected(this, new TcpClientConnectedEventArgs(tcpClient));
            }
        }


        #endregion


        #region send

        private void GuardRunning()
        {
            if (!IsRunning)
                throw new InvalidProgramException("This TCP server has not been started yet.");
        }

        //发送报文byte到指定客户端
        public void Send(TcpClient tcpClient, byte[] datagram)
        {
            GuardRunning();

            if (tcpClient == null)
                throw new ArgumentNullException("tcpClient");

            if (datagram == null)
                throw new ArgumentNullException("datagram");

            try
            {
                NetworkStream stream = tcpClient.GetStream();
                if (stream.CanWrite)
                {
                    stream.BeginWrite(datagram, 0, datagram.Length, HandleDatagramWritten, tcpClient);
                }
            }
            catch (ObjectDisposedException ex)
            {
                ExceptionHandler.Handle(ex);
            }
        }

        //发送报文string到指定客户端
        public void Send(TcpClient tcpClient, string datagram)
        {
            try
            {
                Send(tcpClient, this.Encoding.GetBytes(datagram));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
           
        }

        //发送报文byte至所有客户端
        public void SendToAll(byte[] datagram)
        {
            GuardRunning();

            foreach (var client in _clients.Values)
            {
                Send(client.TcpClient, datagram);
            }
        }

        //发送报文string到所有客户端
        public void SendToAll(string datagram)
        {
            GuardRunning();

            SendToAll(this.Encoding.GetBytes(datagram));
        }

        /// 发送报文至指定的客户端(同步）
        public void SyncSend(TcpClient tcpClient, byte[] datagram)
        {
            GuardRunning();

            if (tcpClient == null)
                throw new ArgumentNullException("tcpClient");

            if (datagram == null)
                throw new ArgumentNullException("datagram");

            try
            {
                NetworkStream stream = tcpClient.GetStream();
                if (stream.CanWrite)
                {
                    stream.Write(datagram, 0, datagram.Length);
                }
            }
            catch (ObjectDisposedException ex)
            {
                ExceptionHandler.Handle(ex);
            }
        }

        /// 发送报文至指定的客户端(同步）
        public void SyncSend(TcpClient tcpClient, string datagram)
        {
            SyncSend(tcpClient, this.Encoding.GetBytes(datagram));
        }

        /// 发送报文至所有客户端(同步）
        public void SyncSendToAll(byte[] datagram)
        {
            GuardRunning();

            foreach (var client in _clients.Values)
            {
                SyncSend(client.TcpClient, datagram);
            }
        }

        /// 发送报文至所有客户端(同步）
        public void SyncSendToAll(string datagram)
        {
            GuardRunning();

            SyncSendToAll(this.Encoding.GetBytes(datagram));
        }

        private void HandleDatagramWritten(IAsyncResult ar)
        {
            try
            {
                ((TcpClient)ar.AsyncState).GetStream().EndWrite(ar);
            }
            catch (ObjectDisposedException ex)
            {
                ExceptionHandler.Handle(ex);
            }
            catch (InvalidOperationException ex)
            {
                ExceptionHandler.Handle(ex);
            }
            catch (IOException ex)
            {
                ExceptionHandler.Handle(ex);
            }
        }
        #endregion

        #region IDisposable Members
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// Releases unmanaged and - optionally - managed resources
        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                if (disposing)
                {
                    try
                    {
                        Stop();

                        if (_listener != null)
                        {
                            _listener = null;
                        }
                    }
                    catch (SocketException ex)
                    {
                        ExceptionHandler.Handle(ex);
                    }
                }

                _disposed = true;
            }
        }

        #endregion

    }

    public class TcpClientState
    {
        public byte[] Buffer { get; set; }
        public TcpClient TcpClient { get; set; }
        public NetworkStream NetworkStream { get; set; }
        public TcpClientState(TcpClient tcpClient, byte[] buffer)
        {
            this.TcpClient = tcpClient;
            this.Buffer = buffer;
            this.NetworkStream = tcpClient.GetStream();
        }
    }

    public class TcpDatagramReceivedEventArgs<T> : EventArgs
    {
        public TcpClient TcpClient { set; get; }
        public T Datagram { set; get; }
        public TcpDatagramReceivedEventArgs(TcpClient tcpClient, T datagram)
        {
            this.TcpClient = tcpClient;
            this.Datagram = datagram;
        }
    }
    public class TcpClientConnectedEventArgs : EventArgs
    {
        public TcpClient TcpClient { get; set; }
        public TcpClientConnectedEventArgs(TcpClient tcp)
        {
            TcpClient = tcp;
        }
    }

    public class TcpClientDisconnectedEventArgs : EventArgs
    {
        public TcpClient TcpClient { get; set; }
        public TcpClientDisconnectedEventArgs(TcpClient tcp)
        {
            TcpClient = tcp;
        }
    }

    public class ExceptionHandler
    {
        public static void Handle(Exception ex)
        { }
    }
}
