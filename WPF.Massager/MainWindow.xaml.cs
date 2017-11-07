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
using System.Diagnostics;

namespace WPF.Massager
{
    public partial class MainWindow : Window
    {
        static readonly object _lock = new object();
        static readonly Dictionary<int, TcpClient> list_clients = new Dictionary<int, TcpClient>();
        static readonly List<string> ban_list_clients = new List<string>();
        static readonly Dictionary<int, string> list_clients_name = new Dictionary<int, string>();
        static readonly List<int> rootuser = new List<int>(); 
        static NetworkStream nscl = null;
        static Boolean conected = false;
        static Boolean topbottom = true;
        public static String username = "#FFFFFF\u0002User";

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, UInt32 dwNewLong);

        const string version = "Alpha 0.3.0";

        public static void alwaysonbottom(Window F, bool b)
        {
            var Handle = new WindowInteropHelper(F).Handle;
            if (b)
            {
                SetWindowPos(Handle, 1, (int)F.Left, (int)F.Top, (int)F.Width, (int)F.Height, 0x0010);
            }
            else
            {
                SetWindowPos(Handle, -1, (int)F.Left, (int)F.Top, (int)F.Width, (int)F.Height, 0x0001 | 0x0002);
            }
        }

        public static void removetaskbarico(Window F, bool b)
        {
            F.ShowInTaskbar = !b;
        }

        public static void hideinatbtab(Window F, bool b)
        {
            var Handle = new WindowInteropHelper(F).Handle;
            if (b)
            {            
                SetWindowLong(Handle, -20, 0x00000080);
            }
            else
            {
                SetWindowLong(Handle, -20, 0x00040000);
            }
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
            delloldversion();
            loadsetting();
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            alwaysonbottom(this, topbottom);
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

        private void AppendColorText(RichTextBox box, string text, Color color)
        {
            try
            {
                TextRange tr = new TextRange(box.Document.ContentEnd, box.Document.ContentEnd);
                tr.Text = text;
                tr.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
            }
            catch
            {
                int i = 0;
            }
        }

        private void SystemMassageColor(string s)
        {
            Dispatcher.Invoke(() => AppendColorText(Massages,s, Brushes.Orange.Color));
            Dispatcher.Invoke(() => Massages.ScrollToEnd());
        }

        private void savesetting()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)+"\\MSG\\";
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);
            if (File.Exists(path + "msg.conf")) File.Delete(path + "msg.conf");
            string text = "Config [" + DateTime.UtcNow.ToString() + "]\u0001";
            text += this.Background.ToString() + "\u0001";
            text += Massage.Foreground.ToString() + "\u0001";
            text += Massage.FontFamily.ToString() + "\u0001";
            text += this.Background.Opacity.ToString() + "\u0001";
            text += username + "\u0001";
            File.WriteAllText(path + "msg.conf", text);
        }

        private void loadsetting()
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\MSG\\";
            if (File.Exists(path + "msg.conf"))
            {
                string text = File.ReadAllText(path + "msg.conf");
                string[] args = text.Split('\u0001');
                try
                {
                    this.Background = new BrushConverter().ConvertFromString(args[1]) as Brush;
                    Massage.Foreground = new BrushConverter().ConvertFromString(args[2]) as Brush;
                    Massage.FontFamily = new FontFamilyConverter().ConvertFromString(args[3]) as FontFamily;
                    Background.Opacity = double.Parse(args[4]);
                    username = args[5];
                }
                catch
                {

                }
            }
        }

        private bool informmsg(string s)
        {
            if (s.Trim('\n', '\r', ' ', '\u0001', '.').Length > 0) return true; else return false;
        }

        void help()
        {
            SystemMassageColor("----------------------------------------\n");
            SystemMassageColor("COMANDS LIST\n");
            SystemMassageColor("/help (//hl)\n");
            SystemMassageColor("/startserver (//ss)\n");
            SystemMassageColor("/startclient (//sc)\n");
            SystemMassageColor("/update (//up)\n");
            SystemMassageColor("/sms (//sm)\n");
            SystemMassageColor("/root (//ro)\n");
            SystemMassageColor("/ban (//bn)\n");
            SystemMassageColor("----------------------------------------\n");
        }

        private void delloldversion()
        {
            string app = System.Reflection.Assembly.GetExecutingAssembly().Location;
            if (File.Exists(app+".old"))
            {
                File.Delete(app+".old");
                SystemMassageColor("----------------------------------------\n");
                SystemMassageColor("COMPLETE UPDATE\n");
                SystemMassageColor("YOU VERSION:" + version + "\n");
                SystemMassageColor("----------------------------------------\n");
            }
        }

        private void updateexe()
        {
            Thread.Sleep(1000);
            string app = System.Reflection.Assembly.GetExecutingAssembly().Location;
            File.Move(app, app + ".old");
            WebClient GIT = new WebClient();
            GIT.DownloadFile("https://github.com/AxHamis/WPF.Massager/blob/master/WPF.Massager/bin/Debug/WPF.Massager.exe?raw=true", app);
            Process.Start(app);
            Dispatcher.Invoke(() => this.Close());
        }

        private void restartexe()
        {
            Thread.Sleep(1000);
            string app = System.Reflection.Assembly.GetExecutingAssembly().Location;
            Process.Start(app);
            this.Close();
        }

        private void windowControlSet(bool b)
        {
            if (b == true)
            {
                WindowControl.Fill = Brushes.White;
                winappbutton.Width = 22;
                UpdateElementPosition(66,46);
                this.Height = this.Height - 300;
                hideinatbtab(this, false);
            }
            else
            {
                winappbutton.Width = Width - 18;
                WindowControl.Fill = Brushes.Transparent;
                UpdateElementPosition(66, 46);
                hideinatbtab(this, true);
            }
        }

        private void winappmode(string s)
        {
            switch (s)
            {
                case "on": windowControlSet(true); break;
                case "off": windowControlSet(false); ElementSetUp(); break;
            }
        }

        private void sendprivate(string s)
        {
            int idindex = s.IndexOf(' ');
            if (username.Trim(' ', '\n', '\r') != "" && !username.Contains("\u0001"))
            {
                clientsend("infomsg\u0001privatemsg\u0001" + s.Remove(idindex,s.Length-idindex) + "\u0001" + username + "\u0001" + s.Substring(idindex+1));
                Massage.Text = "";
            }
        }

        private void banusercomand(string s)
        {
            clientsend("infomsg\u0001root\u0001ban\u0001" + s);
        }

        private void addtoroot(string s)
        {
            int id;
            if(int.TryParse(s, out id))
            {
                rootuser.Add(id);
            }
        }

        private void Button_Click()
        {
            if (!informmsg(Massage.Text)) return;
            if (Massage.Text[0] == '/')
            {
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
                    case "/update": Thread UP = new Thread(updateexe); SystemMassageColor("----------------------------------------\n"+"START UPDATE\n"+"YOU VERSION:"+version+"\n"+"----------------------------------------\n"); UP.Start(); Massage.Text = ""; break;
                    case "//ss":
                    case "/startserver": startserver(); Massage.Text = ""; break;
                    case "//sc":
                    case "/startclient": startclient(com.Length > 1 ? com[1] : "127.0.0.1"); Massage.Text = ""; break;
                    case "//sm":
                    case "/sms": sendprivate(com[1]); Massage.Text = ""; break;
                    case "//bn":
                    case "/ban": banusercomand(com[1]); Massage.Text = ""; break;
                    case "//ro":
                    case "/root": addtoroot(com[1]); Massage.Text = ""; break;
                    case "//hl":
                    case "/help": help(); Massage.Text = ""; break;
                    case "/?testuserlist": string[] s = { "infomsg", "userlist", "5", "#DAA520\u0002Alex\u0002(127.0.0.1:11221)", "#D53032\u0002Make\u0002(127.0.0.2:11221)", "#9932CC\u0002Lisa\u0002(127.0.0.3:11221)", "#2A52BE\u0002Bob\u0002(127.0.0.4:11221)", "#32CD32\u0002Same\u0002(127.0.0.5:11221)" }; updateuserlist(s); break;
                    default: Massage.Text = "/help"; Massage.SelectAll(); break;
                }
            }
            else
            {
                if (conected)
                {
                    if (username.Trim(' ','\n','\r') != "" && !username.Contains("\u0001"))
                    {
                        clientsend(username + "\u0001" + Massage.Text);
                        Massage.Text = "";
                    }
                    else
                    {
                        Massage.Text = "/setnick";
                        Massage.SelectAll();
                    }
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
            string[] msg = masage.Split('\u0001');
            string useridargs = msg[0];
            string[] userid = useridargs.Split('\u0002');
            masage = msg[1];
            Color cl = (Color)ColorConverter.ConvertFromString(userid[0]);
            Dispatcher.Invoke(() => AppendColorText(Massages, userid[1], cl));
            Dispatcher.Invoke(() => AppendColorText(Massages, MassageToEndMassage(":" + masage, userid.Length), (Color)ColorConverter.ConvertFromString(Massage.Foreground.ToString())));
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
            try
            {
                while (true)
                {
                    if (ban_list_clients.Contains(client.Client.RemoteEndPoint.ToString().Split(':')[0])) break;
                    NetworkStream stream = client.GetStream();
                    byte[] buffer = new byte[1024 * 4];
                    int byte_count = stream.Read(buffer, 0, buffer.Length);
                    if (byte_count == 0)
                    {
                        break;
                    }
                    string data = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                    data = data.Split('\0')[0];
                    if (data.StartsWith("infomsg\u0001"))
                    {
                        string[] args = data.Split('\u0001');
                        switch (args[1])
                        {
                            case "username": if (list_clients_name.ContainsKey(id)) { list_clients_name.Remove(id); } list_clients_name.Add(id, args[2]); broadcast("infomsg\u0001userlist\u0001" + clientliststring()); break;
                            case "privatemsg": privatemassage(data.Remove(0,20+args[2].Length), args[2], id); break;
                            case "root": rootcom(args[2],args[3],id); break;
                        }
                    }
                    else
                    {
                        broadcast(data);
                    }
                }
                client.Client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
            catch
            {
                
            }
            lock (_lock) list_clients.Remove(id);
            list_clients_name.Remove(id);
            broadcast("infomsg\u0001userlist\u0001" + clientliststring());
        }

        private static void rootcom(string com, string arg, int id)
        {
            if (rootuser.Contains(id))
            {
                switch (com)
                {
                    case "ban": banuserserver(arg); break;
                }
            }
        }

        private static void banuserserver(string id)
        {
            int banid;
            TcpClient ip;
            if (int.TryParse(id,out banid) && list_clients.TryGetValue(banid,out ip))
            {
                ban_list_clients.Add(ip.Client.RemoteEndPoint.ToString().Split(':')[0]);
            }
        }

        private static void privatemassage(string s, string id, int idi)
        {
            int ido;
            if (int.TryParse(id, out ido) && list_clients.ContainsKey(ido))
            {
                byte[] buffer = Encoding.UTF8.GetBytes(s);
                TcpClient C;
                list_clients.TryGetValue(ido, out C);
                NetworkStream streamo = C.GetStream();
                streamo.Write(buffer, 0, buffer.Length);
                list_clients.TryGetValue(idi, out C);
                NetworkStream streami = C.GetStream();
                streami.Write(buffer, 0, buffer.Length);
            }
            
        }

        private static string clientliststring()
        {
            string cl = null;
            cl += list_clients_name.Count.ToString() + '\u0001';
            foreach (KeyValuePair<int, string> s in list_clients_name)
            {
                cl += s.Value + "\u0002["+s.Key+"]"+'\u0001';
            }
            return cl;
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
                clientsend("infomsg\u0001username\u0001" + username);
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
            conected = true;
            try
            {
                while ((byte_count = ns.Read(receivedBytes, 0, receivedBytes.Length)) > 0)
                {
                    string s = Encoding.UTF8.GetString(receivedBytes, 0, byte_count);
                    if (s.StartsWith("infomsg\u0001"))
                    {
                        string[] args = s.Split('\u0001');
                        switch (args[1])
                        {
                            case "userlist": updateuserlist(args); break;
                        }
                    }
                    else
                    {
                        newmsg(s);
                    }
                }
            }
            catch
            {

            }
            conected = false;
            Dispatcher.Invoke(() => usersbox.Document = new FlowDocument());
        }

        private void updateuserlist(string[] list)
        {
            Dispatcher.Invoke(() => usersbox.Document = new FlowDocument());
            Dispatcher.Invoke(() => usersbox.AppendText("\r\n"));
            for (int i = 3; i < 3 + int.Parse(list[2]); i++)
            {
                string[] s = list[i].Split('\u0002');
                Color cl = (Color)ColorConverter.ConvertFromString(s[0]);
                Dispatcher.Invoke(() => AppendColorText(usersbox, s[1]+' ', cl));
                Dispatcher.Invoke(() => AppendColorText(usersbox, s[2]+'\n', Color.FromRgb((byte)(cl.R/2), (byte)(cl.G/2), (byte)(cl.B/2))));
            }
        }

        private void UpdateElementPosition(double UP, double DOWN)
        {
            double SUP = UP + 10;
            double SDOWN = DOWN + 10;
            SplitterUP.Margin = new Thickness(SplitterUP.Margin.Left, UP, SplitterUP.Margin.Right, SplitterUP.Margin.Bottom);
            SplitterDOWN.Margin = new Thickness(SplitterDOWN.Margin.Left, SplitterDOWN.Margin.Top, SplitterDOWN.Margin.Right, DOWN);
            Massages.Margin = new Thickness(Massages.Margin.Left, SUP, Massages.Margin.Right, SDOWN);
            Massage.Height = DOWN;
            usersbox.Margin = new Thickness(usersbox.Margin.Left, WindowControl.Margin.Top+WindowControl.Height >= 0 ? WindowControl.Margin.Top + WindowControl.Height : 0, usersbox.Margin.Right, usersbox.Margin.Bottom);
            usersbox.Height = UP - 19;
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
        }

        private void closebutton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        Point ClickPoint;

        private void WindowControl_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                System.Windows.Input.Mouse.Capture(WindowControl);
                var screenPosition = this.PointToScreen(e.GetPosition(this));
                Left = screenPosition.X - ClickPoint.X;
                Top = screenPosition.Y - ClickPoint.Y;
            }
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            savesetting();
        }

        private void SplitterDOWN_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                System.Windows.Input.Mouse.Capture(SplitterDOWN);
                double Y = e.GetPosition(this).Y;
                if (Y > Height / 3 * 2 && Y < Height - 5)
                {
                    UpdateElementPosition(SplitterUP.Margin.Top,this.Height-(Y+6));
                }
            }
        }

        private void SplitterUP_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if(e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                System.Windows.Input.Mouse.Capture(SplitterUP);
                double Y = e.GetPosition(this).Y;
                if (Y < Height / 3 && Y > (WindowControl.Margin.Top + WindowControl.Height >= 0 ? 19 + 4 : 4))
                {
                    UpdateElementPosition(Y - 4, SplitterDOWN.Margin.Bottom);
                }
            }
        }

        private void CaptureOFF(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.Windows.Input.Mouse.Capture(null);
        }

        private void WindowControl_PreviewMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ClickPoint = e.GetPosition(this);
        }

        private void winappbutton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowControl.Fill == Brushes.White)
            {
                winappmode("off");
            }
            else
            {
                winappmode("on");
            }
        }

        private void window_Loaded(object sender, RoutedEventArgs e)
        {
            hideinatbtab(this, true);
            removetaskbarico(this, true);
        }

        private void pinbutton_Click(object sender, RoutedEventArgs e)
        {
            topbottom = !topbottom;
            alwaysonbottom(this, topbottom);
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            if (ControlCenter.Margin.Bottom == 0)
            {
                UpdateElementPosition(SplitterUP.Margin.Top, 46);
                ControlCenter.Margin = new Thickness(-1, 0, -1, -105);
            }
            else
            {
                UpdateElementPosition(SplitterUP.Margin.Top, 95);
                ControlCenter.Margin = new Thickness(-1, 0, -1, 0);
            }
        }

        private void TabItem_MouseEnter_UN(object sender, System.Windows.Input.MouseEventArgs e)
        {
            UNcolorRC_N.Text = username.Split('\u0002')[1];
            UNcolorRC.Fill = new BrushConverter().ConvertFromString(username.Split('\u0002')[0]) as Brush;
            Color cl = (Color)ColorConverter.ConvertFromString(UNcolorRC.Fill.ToString());
            UNcolorRC_R.Value = cl.R;
            UNcolorRC_G.Value = cl.G;
            UNcolorRC_B.Value = cl.B;
        }

        private void TabItem_MouseEnter_FG(object sender, System.Windows.Input.MouseEventArgs e)
        {
            FGcolorRC.Fill = Massage.Foreground;
            Color cl = (Color)ColorConverter.ConvertFromString(FGcolorRC.Fill.ToString());
            FGcolorRC_R.Value = cl.R;
            FGcolorRC_G.Value = cl.G;
            FGcolorRC_B.Value = cl.B;
            FGcolorRC_F.ItemsSource = System.Windows.Media.Fonts.SystemFontFamilies;
            FGcolorRC_F.SelectedItem = Massage.FontFamily;
        }

        private void TabItem_MouseEnter_BG(object sender, System.Windows.Input.MouseEventArgs e)
        {
            BGcolorRC.Fill = this.Background;
            Color cl = (Color)ColorConverter.ConvertFromString(BGcolorRC.Fill.ToString());
            BGcolorRC_O.Value = this.Background.Opacity;
            BGcolorRC_R.Value = cl.R;
            BGcolorRC_G.Value = cl.G;
            BGcolorRC_B.Value = cl.B;
        }

        private void BGcolorRC_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Color cl = Color.FromRgb(0,0,0);
            cl.R = (byte)BGcolorRC_R.Value;
            cl.G = (byte)BGcolorRC_G.Value;
            cl.B = (byte)BGcolorRC_B.Value;
            BGcolorRC.Fill = new BrushConverter().ConvertFromString(cl.ToString()) as Brush;
            BGcolorRC.Fill.Opacity = BGcolorRC_O.Value;
        }

        private void TabItem_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            this.Background = BGcolorRC.Fill;
        }

        private void TabItem_MouseLeave_1(object sender, System.Windows.Input.MouseEventArgs e)
        {
            Massage.Foreground = FGcolorRC.Fill;
            Massage.FontFamily = (FontFamily)FGcolorRC_F.SelectedItem;
        }

        private void TabItem_MouseLeave_2(object sender, System.Windows.Input.MouseEventArgs e)
        {
            string oldname = username;
            username = UNcolorRC.Fill.ToString() + '\u0002' + UNcolorRC_N.Text;
            if (conected && oldname != username)
            {
                clientsend("infomsg\u0001username\u0001" + username);
            }
        }

        private void FGcolorRC_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Color cl = Color.FromRgb(0, 0, 0);
            cl.R = (byte)FGcolorRC_R.Value;
            cl.G = (byte)FGcolorRC_G.Value;
            cl.B = (byte)FGcolorRC_B.Value;
            FGcolorRC.Fill = new BrushConverter().ConvertFromString(cl.ToString()) as Brush;
        }

        private void UNcolorRC_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Color cl = Color.FromRgb(0, 0, 0);
            cl.R = (byte)UNcolorRC_R.Value;
            cl.G = (byte)UNcolorRC_G.Value;
            cl.B = (byte)UNcolorRC_B.Value;
            UNcolorRC.Fill = new BrushConverter().ConvertFromString(cl.ToString()) as Brush;
        }
    }
}
