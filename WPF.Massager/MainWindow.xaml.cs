using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Runtime.InteropServices;
using System.Windows.Documents;
using System.Windows.Media;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace WPF.Massager
{
    public partial class MainWindow : Window
    {
        static readonly object _lock = new object();
        static readonly Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();
        static NetworkStream nscl = null;
        public static String username = "NewUser";
        bool[] startinfo = new bool[3];
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, UInt32 dwNewLong);

        public static void alwaysonbottom(Window F)
        {
            var Handle = new WindowInteropHelper(F).Handle;
            SetWindowPos(Handle, 1, (int)F.Left, (int)F.Top, (int)F.Width, (int)F.Height, 0x0010);
        }

        public static void removetaskbarico(Window F)
        {
            F.ShowInTaskbar = false;
        }

        public static void hideinatbtab(Window F)
        {
            var Handle = new WindowInteropHelper(F).Handle;
            SetWindowLong(Handle, -20, GetWindowLong(Handle, -20) | 0x00000080);
        }

        private void ElementSetUp()
        {
            Width = 300;
            Height = SystemParameters.WorkArea.Height;
            Left = SystemParameters.FullPrimaryScreenWidth - Width;
            Top = 0;
        }

        public MainWindow()
        {
            InitializeComponent();
            ElementSetUp();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            removetaskbarico(this);
            hideinatbtab(this);
            alwaysonbottom(this);
        }

        private string MassageToEndMassage(string msg, int userid)
        {
            string endmsg = null;
            int j = 41 - userid -1;
            string pr = null;
            for (int o = 0; o <= userid; o++)
            {
                pr += " ";
            }
            int i = 0;
            foreach (char C in msg)
            {
                bool auto = true;
                if (C == '\r')
                {
                    continue;
                }
                if (C == '\n')
                {
                    i = -1;
                    auto = false;
                }
                if (i == -1)
                {
                    endmsg += "\n"+pr;
                    if (auto == false)
                    {
                        i=1;
                        continue;
                    }
                }
                i++;
                endmsg += C;
                if (i == j)
                {
                    i = -1;
                }
            }
            return endmsg + "\n";
        }

        private void AppendColorText(RichTextBox box, string text, Brush color)
        {
            try
            {
                TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
                tr.Text = text;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, color);
            }
            catch
            {

            }
        }

        private void SystemMassageColor(string s)
        {
            Dispatcher.Invoke(() => AppendColorText(Massages,s, Brushes.Orange));
            Dispatcher.Invoke(() => Massages.ScrollToEnd());
        }

        private void setnick(string s)
        {
            string old = username;
            if (s != "") username = s;
            SystemMassageColor("----------------------------------------\n");
            SystemMassageColor("YOU NAME UPDATE\n");
            SystemMassageColor("NEW NAME:"+username+"\n");
            SystemMassageColor("OLD NAME:" + old + "\n");
            SystemMassageColor("----------------------------------------\n");
        }

        private bool informmsg(string s)
        {
            if (s.Trim('\n', '\r', ' ', '\0', '.').Length > 0) return true; else return false;
        }

        private void positionwin(string p)
        {
            if (p == "right")
            {
                Left = SystemParameters.FullPrimaryScreenWidth - Width;
            }
            if(p == "left")
            {
                Left = 0;
            }
        }

        private void borderstyleset(string s)
        {
            if (s == "fixed")
            {
                startinfo[2] = false;
                ElementSetUp();
            }
            if (s == "unfixed")
            {
                startinfo[2] = true;
            }
        }

        private void setallfont(string s)
        {
            switch (s)
            {
                case "arial": Massage.FontFamily = Massages.FontFamily = new FontFamily("Arial"); return;
                case "courier new": Massage.FontFamily = Massages.FontFamily = new FontFamily("Courier New"); return;
                case "calibri": Massages.FontFamily = new FontFamily("Calibri"); return;
                case "impact": Massage.FontFamily = Massages.FontFamily = new FontFamily("Impact"); return;
                case "comic sans mS": Massage.FontFamily = Massages.FontFamily = new FontFamily("Comic Sans MS"); return;
                case "list": SystemMassageColor("----------------------------------------\n"); SystemMassageColor("FONTS LIST\nArial\nCourier New\nCalibri\nImpact\nComic Sans MS\n"); SystemMassageColor("----------------------------------------\n"); return;

            }
        }

        private void setbgcolor(string cl)
        {
            string[] C = cl.Split(' ');
            Double op = this.Background.Opacity;
            byte R, G, B;
            if (C.Length == 3 && byte.TryParse(C[0],out R)&& byte.TryParse(C[1], out G) && byte.TryParse(C[2], out B)) {
                Massage.SelectionBrush = this.Background = new SolidColorBrush(Color.FromRgb(R, G, B));
                this.Background.Opacity = op;
                Massages.Foreground = Massage.Foreground = new SolidColorBrush(Color.FromRgb((byte)(255-R), (byte)(255 - G), (byte)(255 - B)));
            }
        }

        private void setbgopas(string s)
        {
            Double p;
            if (Double.TryParse(s, out p) && p < 101 && p > 0)
            {
                this.Background.Opacity = p/100;
            }
        }

        void help()
        {
            SystemMassageColor("----------------------------------------\n");
            SystemMassageColor("COMANDS LIST\n");
            SystemMassageColor("/help (//hl)\n");
            SystemMassageColor("/setnick (//sn)\n");
            SystemMassageColor("/startserver (//ss)\n");
            SystemMassageColor("/startclient (//sc)\n");
            SystemMassageColor("/exit (//ex)\n");
            SystemMassageColor("/position (//po)\n");
            SystemMassageColor("/border (//bo)\n");
            SystemMassageColor("/font (//fo)\n");
            SystemMassageColor("/background (//bg)\n");
            SystemMassageColor("/pacity (//pa)\n");
            SystemMassageColor("----------------------------------------\n");
        }

        private void updateexe()
        {
            File.Move(System.Reflection.Assembly.GetExecutingAssembly().Location, System.Reflection.Assembly.GetExecutingAssembly().Location+".old");

        }

        private void Button_Click()
        {
            if (!informmsg(Massage.Text)) return;
            if (Massage.Text[0] == '/')
            {
                Massage.Text = Massage.Text.ToLower();
                string[] com = Massage.Text.Split();
                if (com.Length > 2)
                {
                    for (int comnom = 2; comnom < com.Length; comnom++)
                    {
                        com[1] += " "+com[comnom]; 
                    }
                }
                switch (com[0])
                {
                    case "//up":
                    case "/update": updateexe(); Massage.Text = ""; break;
                    case "//sn":
                    case "/setnick": setnick(com[1]); Massage.Text = ""; break;
                    case "//ss":
                    case "/startserver": startserver(); Massage.Text = ""; break;
                    case "//sc":
                    case "/startclient": startclient(com.Length > 1 ? com[1] : "127.0.0.1"); Massage.Text = ""; break;
                    case "//ex":
                    case "/exit": Massage.Text = ""; Environment.Exit(0); break;
                    case "//po":
                    case "/position": positionwin(com[1]); Massage.Text = ""; break;
                    case "//bo": 
                    case "/border": borderstyleset(com[1]); Massage.Text = ""; break;
                    case "//fo":
                    case "/font": setallfont(com[1]); Massage.Text = ""; break;
                    case "//bg":
                    case "/background": setbgcolor(com[1]); Massage.Text = ""; break;
                    case "//pa":
                    case "/pacity": setbgopas(com[1]); Massage.Text = ""; break;
                    case "//hl":
                    case "/help": help(); Massage.Text = ""; break;
                    default: Massage.Text = "/help"; Massage.SelectAll(); break;
                }
            }
            else
            {
                if (startinfo[0] == true)
                {
                    clientsend(username + "\0" + Massage.Text);
                    Massage.Text = "";
                }
                else
                {
                    Massage.Text = "/startclient";
                    Massage.SelectAll();
                }
            }
        }

        private void newmsg(string masage)
        {
            string[] msg = masage.Split('\0');
            string userid = msg[0];
            masage = msg[1];
            Brush B;
            if (userid == username) B = Brushes.DeepPink; else B = Brushes.ForestGreen;
            Dispatcher.Invoke(() => AppendColorText(Massages, userid, B));
            Dispatcher.Invoke(() => AppendColorText(Massages, MassageToEndMassage(":" + masage, userid.Length), Brushes.White));
            this.Dispatcher.Invoke(() => Massages.ScrollToEnd());
        }

        private void startserver()
        {
                Thread Server = new Thread(server);
                Server.Start();
        }

        private void server()
        {
            try
            {
                int count = 1;
                TcpListener ServerSocket = new TcpListener(IPAddress.Any, 11221);
                ServerSocket.Start();
                startinfo[1] = true;
                SystemMassageColor("----------------------------------------\n");
                SystemMassageColor("SERVER STARTED\n");
                string HostName = Dns.GetHostName();
                SystemMassageColor("NAME:" + HostName + "\n");
                IPAddress[] ipAddres = Dns.GetHostEntry(HostName).AddressList;
                foreach (IPAddress ip in ipAddres)
                {
                    SystemMassageColor("IP:" + ip.ToString() + "\n");
                }
                SystemMassageColor("PORT:11221\n");
                SystemMassageColor("----------------------------------------\n");
                while (true)
                {
                    TcpClient client = ServerSocket.AcceptTcpClient();
                    lock (_lock) list_clients.Add(count, client);
                    Thread.Sleep(500);
                    SystemMassageColor("----------------------------------------\n");
                    SystemMassageColor("NEW CONNECT\n");
                    SystemMassageColor("IP-WAN:" + client.Client.RemoteEndPoint + "\n");
                    SystemMassageColor("IP-LAN:" + client.Client.LocalEndPoint + "\n");
                    SystemMassageColor("TTL:" + client.Client.Ttl + "\n");
                    SystemMassageColor("----------------------------------------\n");
                    Thread t = new Thread(handle_clients);
                    t.Start(count);
                    count++;
                }
            }
            catch
            {
                SystemMassageColor("----------------------------------------\n");
                SystemMassageColor("SERVER NOT STARTED\n");
                SystemMassageColor("FATAL ERROR\n");
                SystemMassageColor("TRY RESTART PROGRAMM\n");
                SystemMassageColor("----------------------------------------\n");
            }
        }

        private static void handle_clients(object o)
        {
            int id = (int)o;
            TcpClient client;
            lock (_lock) client = list_clients[id];
            while (true)
            {
                NetworkStream stream = client.GetStream();
                byte[] buffer = new byte[1024 * 4];
                int byte_count = stream.Read(buffer, 0, buffer.Length);
                if (byte_count == 0)
                {
                    break;
                }
                string data = Encoding.UTF8.GetString(buffer, 0, byte_count);
                broadcast(data);
            }
            lock (_lock) list_clients.Remove(id);
            client.Client.Shutdown(SocketShutdown.Both);
            client.Close();
        }

        private static void broadcast(string data)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(data);
            lock (_lock)
            {
                foreach (TcpClient c in list_clients.Values)
                {
                    NetworkStream stream = c.GetStream();
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        private void clientsend(string s)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(s);
            nscl.Write(buffer, 0, buffer.Length);
        }

        private void startclient(string strip)
        {
            int port = 11221;
            try
            {
                IPAddress ip = IPAddress.Parse(strip);
                TcpClient client = new TcpClient();
                client.Connect(ip, port);
                SystemMassageColor("----------------------------------------\n");
                SystemMassageColor("YOU CONNECT\n");
                SystemMassageColor("IP:" + ip + "\n");
                SystemMassageColor("PORT:" + port + "\n");
                SystemMassageColor("----------------------------------------\n");
                nscl = client.GetStream();
                Thread thread = new Thread(o => ReceiveData((TcpClient)o));
                thread.Start(client);
                startinfo[0] = true;
            }
            catch
            {
                SystemMassageColor("----------------------------------------\n");
                SystemMassageColor("YOU NOT CONNECT\n");
                SystemMassageColor("SERVER NOT RESPONDING\n");
                SystemMassageColor("IP:" + strip + "\n");
                SystemMassageColor("PORT:" + port + "\n");
                SystemMassageColor("YOU CAN CREATE A SERVER /startserver\n");
                SystemMassageColor("----------------------------------------\n");
            }
        }

        private void ReceiveData(TcpClient client)
        {
            NetworkStream ns = client.GetStream();
            byte[] receivedBytes = new byte[1024 * 4];
            int byte_count;
            while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
            {
                string s = Encoding.UTF8.GetString(receivedBytes, 0, byte_count);
                newmsg(s);
            }
        }

        static bool sendkey = true;

        private void Massage_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
            {
                sendkey = true;
                Massage.AcceptsReturn = false;
            }
        }

        private void Massage_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.LeftShift)
            {
                sendkey = false;
                Massage.AcceptsReturn = true;
            }
            if (e.Key == System.Windows.Input.Key.Enter && sendkey)
            {
                Button_Click();
            }
            else
            {

            }
        }

        private void Massages_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (startinfo[2] && e.MiddleButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                int X = (int)e.GetPosition(this).X;
                if (X > 150) Left += Math.Abs(150-X)/10; else if (X < 150) Left -= Math.Abs(150 - X)/10;
            }
        }
    }
}
