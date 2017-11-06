﻿using System;
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
        public static String username = "#FFFFFF\u0002User";

        bool[] startinfo = new bool[3];
        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        private static extern int SetWindowLong(IntPtr hWnd, int nIndex, UInt32 dwNewLong);

        const string version = "Alpha 0.1.3";

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
            delloldversion();
            loadsetting();
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

        private void setnick(string s)
        {
            string[] old = username.Split('\u0002');
            string nick = s[0].ToString().ToUpper() + s.Substring(1);
            username = old[0]+'\u0002'+nick;
            SystemMassageColor("----------------------------------------\n");
            SystemMassageColor("YOU NAME UPDATE\n");
            SystemMassageColor("NEW NAME:" + nick + "\n");
            SystemMassageColor("OLD NAME:" + old[1] + "\n");
            SystemMassageColor("----------------------------------------\n");
            if (startinfo[0] == true)
            {
                clientsend("infomsg\u0001username\u0001" + username);
            }
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
            switch (s)
            {
                case "fixed": startinfo[2] = false; startinfo[2] = false; ElementSetUp(); break;
                case "unfixed": startinfo[2] = true; Massages.Cursor = System.Windows.Input.Cursors.SizeWE; break;
            }
        }

        private void setallfont(string s)
        {
            switch (s)
            {
                case "arial": Massage.FontFamily = new FontFamily("Arial"); return;
                case "courier new": Massage.FontFamily = new FontFamily("Courier New"); return;
                case "calibri": Massages.FontFamily = new FontFamily("Calibri"); return;
                case "impact": Massage.FontFamily = new FontFamily("Impact"); return;
                case "comic sans ms": Massage.FontFamily = new FontFamily("Comic Sans MS"); return;
                case "list": SystemMassageColor("----------------------------------------\n"); SystemMassageColor("FONTS LIST\nArial\nCourier New\nCalibri\nImpact\nComic Sans MS\n"); SystemMassageColor("----------------------------------------\n"); return;
            }
            Massage.UpdateLayout();
        }

        private void setbgcolor(string cl)
        {
            string[] C = cl.Split(' ');
            double op = this.Background.Opacity;
            byte R, G, B;
            if (C.Length == 3 && byte.TryParse(C[0],out R)&& byte.TryParse(C[1], out G) && byte.TryParse(C[2], out B)) {
                Massage.SelectionBrush = this.Background = new SolidColorBrush(Color.FromRgb(R, G, B));
                this.Background.Opacity = op;
            }
        }

        private void setfgcolor(string cl)
        {
            string[] C = cl.Split(' ');
            byte R, G, B;
            if (C.Length == 3 && byte.TryParse(C[0], out R) && byte.TryParse(C[1], out G) && byte.TryParse(C[2], out B))
            {
                Massage.Foreground = new SolidColorBrush(Color.FromRgb(R,G,B));
            }
        }

        private void colornick(string cl)
        {
            string[] C = cl.Split(' ');
            byte R, G, B;
            if (C.Length == 3 && byte.TryParse(C[0], out R) && byte.TryParse(C[1], out G) && byte.TryParse(C[2], out B))
            {
                SolidColorBrush Br = new SolidColorBrush(Color.FromRgb(R, G, B));
                string[] userid = username.Split('\u0002');
                username = Br.ToString() + '\u0002' + userid[1];
                SystemMassageColor("----------------------------------------\n");
                SystemMassageColor("YOU NAME COLOR UPDATE\n");
                SystemMassageColor("NEW NAME COLOR:" + Br.ToString() + "\n");
                SystemMassageColor("OLD NAME COLOR:" + userid[0] + "\n");
                SystemMassageColor("----------------------------------------\n");
                if (startinfo[0] == true)
                {
                    clientsend("infomsg\u0001username\u0001" + username);
                }

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
            SystemMassageColor("/opacity (//op)\n");
            SystemMassageColor("/update (//up)\n");
            SystemMassageColor("/winappmode (//wa)\n");
            SystemMassageColor("/foreground (//fg)\n");
            SystemMassageColor("/colornick (//cn)\n");
            SystemMassageColor("/restart (//rs)\n");
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
                WindowControl.Margin = new Thickness(-1,-1,-1,0);
                usersbox.Margin = new Thickness(0, 19, 0, 0);
                userboxbutton.Margin = new Thickness(-1, 19, -1, 0);
                usersbox.Height = 0;
                Massages.Margin = new Thickness(0, userboxbutton.Margin.Top + 10, 0, Massages.Margin.Bottom);
                this.Height = this.Height - 300;
            }
            else
            {
                WindowControl.Margin = new Thickness(-1, -25, -1, 0);
                usersbox.Margin = new Thickness(0, 0, 0, 0);
                userboxbutton.Margin = new Thickness(-1, -1, -1, 0);
                usersbox.Height = 0;
                Massages.Margin = new Thickness(0, userboxbutton.Margin.Top + 10, 0, Massages.Margin.Bottom);
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
                    case "//rs":
                    case "/restart": Thread RS = new Thread(restartexe); SystemMassageColor("----------------------------------------\n" + "RESTARTING\n" + "----------------------------------------\n"); RS.Start(); Massage.Text = ""; break;
                    case "//sn":
                    case "/setnick": setnick(com[1]); Massage.Text = ""; break;
                    case "//cn":
                    case "/colornick": colornick(com[1]); Massage.Text = ""; break;
                    case "//ss":
                    case "/startserver": startserver(); Massage.Text = ""; break;
                    case "//sc":
                    case "/startclient": startclient(com.Length > 1 ? com[1] : "127.0.0.1"); Massage.Text = ""; break;
                    case "//ex":
                    case "/exit": Massage.Text = ""; this.Close(); break;
                    case "//po":
                    case "/position": positionwin(com[1]); Massage.Text = ""; break;
                    case "//bo": 
                    case "/border": borderstyleset(com[1]); Massage.Text = ""; break;
                    case "//fo":
                    case "/font": setallfont(com[1]); Massage.Text = ""; break;
                    case "//bg":
                    case "/background": setbgcolor(com[1]); Massage.Text = ""; break;
                    case "//fg":
                    case "/foreground": setfgcolor(com[1]); Massage.Text = ""; break;
                    case "//op":
                    case "/opacity": setbgopas(com[1]); Massage.Text = ""; break;
                    case "//wa":
                    case "/winappmode": winappmode(com[1]); Massage.Text = ""; break;
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
                if (startinfo[0] && conected)
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
                startinfo[0] = true;
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

        private void Massages_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (startinfo[2] && e.MiddleButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var screenPosition = this.PointToScreen(e.GetPosition(this));
                Left = screenPosition.X - 150;
            }
        }

        private void userboxbutton_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                if (e.GetPosition(this).Y < this.Height - Height/3*2 && e.GetPosition(this).Y > WindowControl.Margin.Top + 18)
                {
                    userboxbutton.Margin = new Thickness(-1, e.GetPosition(this).Y -1 , -1, 0);
                    usersbox.Height = userboxbutton.Margin.Top + 2 - usersbox.Margin.Top;
                    Massages.Margin = new Thickness(0, userboxbutton.Margin.Top + 10, 0, Massages.Margin.Bottom);
                }
            }
        }

        private void userboxbutton_Copy_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                if ( e.GetPosition(this).Y > Height/3*2 && e.GetPosition(this).Y < this.Height - 46)
                {
                    userboxbutton_Copy.Margin = new Thickness(-1, 0, -1, this.Height - e.GetPosition(this).Y);
                    Massage.Height = userboxbutton_Copy.Margin.Bottom;
                    Massages.Margin = new Thickness(0, Massages.Margin.Top, 0, userboxbutton_Copy.Margin.Bottom + 10);
                }
            }
        }

        private void closebutton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void WindowControl_PreviewMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                var screenPosition = this.PointToScreen(e.GetPosition(this));
                Left = screenPosition.X - 150;
                Top = screenPosition.Y - 10;
            }
        }

        private void window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            savesetting();
        }
    }
}
