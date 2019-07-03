using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AuthServer
{
    class Gateway
    {
        public static string ServerIP = new WebClient().DownloadString("http://api.ipify.org/");
        public static WebClient Client = new WebClient();
        const int MAX_BUFF= 2048;
        static Socket _Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public Gateway(uint Port=6000)
        {
            _Server.Bind(new IPEndPoint(IPAddress.Any, (int)Port));
            _Server.Listen(Int32.MaxValue);
            Console.WriteLine($"Gateway started at {Port} port!");

            System.Timers.Timer t2 = new System.Timers.Timer()
            {
                Interval = 15000
            };
            t2.Elapsed += delegate
            {
                
            };
            t2.Start();


            System.Timers.Timer t = new System.Timers.Timer()
            {
                Interval = 30000
            };
            t.Elapsed += delegate
            {
                foreach (Socket _User in Users.ToArray())
                    SendMessage($"ONLINE|{Users.Count}", _User);
            };
            //t.Start();
            Accept();
        }
      
        public async void Accept()
        {
            string IP = "";
            Socket Received;
            try
            {
                Received = await _Server.AcceptAsync();

                
                IP = ((IPEndPoint)Received?.RemoteEndPoint)?.Address?.ToString();


                
                    Console.Title = $"Сервер авторизации. В сети: {Users.Count}";
                    Users.Add(Received);

                    Receive(Received);
                
            }
            catch
            {
                Console.Title = $"Сервер авторизации. В сети: {Users.Count}";
            }
            finally
            {
                Accept();
            }
        }
        List<Socket> Users = new List<Socket>();
        public async void Receive(Socket Client)
        {
            if (!Client.Connected)
                return;

            try
            {
                if (!Client.Connected)
                    return;
                try
                {
                    if (!Client.Connected)
                    {
                        return;
                    }
                    string IP = "";
                    byte[] Buff = new byte[MAX_BUFF];
                    Array.Resize(ref Buff, await Client.ReceiveAsync(new ArraySegment<byte>(Buff), SocketFlags.None));

                    if (Buff.Length > 60)
                    {
                        try
                        {
                            IP = ((IPEndPoint)Client.RemoteEndPoint).Address.ToString();
                        }
                        catch { return; }

                        Client.Dispose();
                        return;
                    }


                    if (Buff.Length == 0)
                    {
                        Dispose(Client);
                        return;
                    }
                    Console.WriteLine($"{Client.RemoteEndPoint.ToString()}: {Encoding.UTF8.GetString(Buff)}");
                    string[] Message = Encoding.UTF8.GetString(Buff).Split('|');
                    switch (Message[0])
                    {
                        case "REGISTER":
                            string Login = Message[1];
                            string Password = Message[2];
                            if (Login.Length == 20 && Password.Length == 20)
                            {
                                try
                                {
                                    IP = ((IPEndPoint)Client.RemoteEndPoint).Address.ToString();
                                }
                                catch
                                {
                                    return;
                                }
                              // Process.Start("C:/Windows/System32/netsh.exe", $"advfirewall firewall add rule name=\"BAN-IP {IP}\" action=block dir=IN remoteip={IP}");
                                Client.Dispose();

                            }
                            try
                            {
                                using (MySqlConnection SqlConn = GetPolledConnection())
                                {
                                    string a = $"SELECT COUNT(*) FROM tickets WHERE Nickname='{Login}';";
                                    if (int.Parse(new MySqlCommand(a, SqlConn).ExecuteScalar().ToString()) > 0)
                                        SendMessage($"MESSAGEBOX|Ошибка регистрации|Данный ник уже занят. Попробуйте другой|{(int)System.Windows.Forms.MessageBoxIcon.Warning}|REG-RESPONSE", Client);
                                    else
                                    {
                                        bool f = false;
                                        try
                                        {
                                           
                                            new MySqlCommand($"INSERT INTO token(token,nickname,token) VALUES('{Password}','{Login}','{Login}');", SqlConn).ExecuteNonQuery();
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(e);
                                            f = true;
                                            SendMessage($"MESSAGEBOX|Ошибка регистрации|Произошла непредвиденная ошибка при регистрации!|{(int)System.Windows.Forms.MessageBoxIcon.Warning}|REG-RESPONSE", Client);

                                        }
                                        if (!f)
                                            SendMessage($"MESSAGEBOX|Успешная регистрация|Аккаунт {Login} успешно зарегистрирован на сервере!|{(int)System.Windows.Forms.MessageBoxIcon.Information}|REG-RESPONSE", Client);
                                    }
                                }
                            }
                            catch(Exception e)
                            {
                                Client.Dispose();
                            }
                            break;
                        case "STARTGAME":
                            Login = Message[1];
                            Password = Message[2];

                            using (MySqlConnection SqlConn = GetPolledConnection())
                            {
                                if (int.Parse(new MySqlCommand($"SELECT COUNT(*) FROM token WHERE Nickname='{Login}' AND password='{Password}';", SqlConn).ExecuteScalar().ToString()) > 0)
                                {
                                    try
                                    {
                                        IP = ((IPEndPoint)Client.RemoteEndPoint).Address.ToString();
                                    }
                                    catch
                                    {
                                        return;
                                    }

                                    Process.Start("C:/Windows/System32/netsh.exe", $"advfirewall firewall add rule name=\"Access from {IP}\" action=allow dir=IN remoteip={IP}/24 localport=5222 protocol=TCP");
                                    SendMessage($"STARTGAME|{Settings1.Default.START_ARGS}", Client);
                                }
                                else
                                    SendMessage($"MESSAGEBOX|Ошибка авторизации|Неверный логин или пароль|{(int)System.Windows.Forms.MessageBoxIcon.Warning}|LOG-RESPONSE", Client);

                            }

                            break;
                    }

                    Receive(Client);
                }
                catch (Exception e)
                {
                    Dispose(Client);
                }
            }
            catch
            {

            }
        }
        public MySqlConnection GetPolledConnection()
        {

            MySqlConnection Conn = new MySqlConnection($"Server=127.0.0.1;Database=cicadadb;Uid=root;");
            Conn.Open();
            return Conn;
        }
        public async void SendMessage(string MSG, Socket User)
        {
            try
            {
                await User.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(MSG)), SocketFlags.None);
            }
            catch
            {

            }
        }
        
        private void Dispose(Socket Client)
        {
            Users.Remove(Client);
            Console.Title = $"Сервер авторизации. В сети: {Users.Count}";
            Console.WriteLine($"{Client.RemoteEndPoint.ToString()} отключен!");
            Client.Dispose();
        }
    }
}
