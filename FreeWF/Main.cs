using FreeWF.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace FreeWF
{
    public partial class Main : Form
    {
        public static IPEndPoint S =null;
        private static WebClient cl = new WebClient();
        public static SynchronizationContext MainContext;
        public static Socket _Server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        public Main()
        {
            
            InitializeComponent();
            string[] c = cl.DownloadString("http://freewarface.ru/launcher.txt").Split(new[] { "|" }, StringSplitOptions.None);
            S = new IPEndPoint(IPAddress.Parse(c[0]), ushort.Parse(c[1]));
            this.FormClosing += delegate {SendMessage("EXIT"); Environment.Exit(0); };
            MainContext = SynchronizationContext.Current;
            BackgroundWorker SocketWorker = new BackgroundWorker();
            SocketWorker.DoWork += delegate
            {
                try
                {
                    _Server.Connect(S);
                }
                catch
                {
                    MessageBox.Show("Не удается подключится к серверу!", "Ошибка связи",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
                new Thread(Receive).Start();
                MainContext.Post((object o)=> 
                {
                    textBox1.Enabled = true;
                    textBox2.Enabled = true;
                    button1.Enabled = true;
                    button2.Enabled = true;
                    label4.Text = "Подключено!";
                }, null);
            };
            SocketWorker.RunWorkerAsync();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox1.Text == "" || textBox2.Text == "" ||
                   new Regex("^[A-Za-z0-9]+$").Matches(textBox1.Text).Count == 0 ||
                       textBox1.Text.Length > 10)
                MessageBox.Show("Не удается выполнить вход. Проверьте: \n\n*Поле 'логин' было заполнено\n*Поле 'пароль' было заполнено\n*Поле 'логин' должно содержать только английские буквы и цифры*\n*Логин не может быть больше 10 символов", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);

            else
            {
                button1.Enabled = false;
                MainContext.Post((object o) =>
                {
                    label4.Text = $"Вход...";
                }, null);
                SendMessage($"STARTGAME|{textBox1.Text}|{textBox2.Text}");
            }
        }

        public void Receive()
        {
            try
            {
                while (_Server.Connected)
                {
                    byte[] Buff = new byte[1024];
                    Array.Resize(ref Buff, _Server.Receive(Buff));
                    string[] Message = Encoding.UTF8.GetString(Buff).Split('|');
                    switch (Message[0])
                    {
                        case "ONLINE":
                            MainContext.Post((object o) =>
                            {
                                label4.Text = $"Онлайн: {Message[1]}";
                            }, null);
                            break;
                        case "MESSAGEBOX":
                            MessageBox.Show(Message[2], Message[1], MessageBoxButtons.OK, (MessageBoxIcon)int.Parse(Message[3]));
                            switch (Message[4])
                            {
                                case "REG-RESPONSE":
                                    MainContext.Post((object o) =>
                                    {
                                        button2.Enabled = true;
                                    }, null);
                                    break;
                                case "LOG-RESPONSE":
                                    MainContext.Post((object o) =>
                                    {
                                        button1.Enabled = true;
                                    }, null);
                                    break;
                            }
                            break;
                        case "STARTGAME":
                            string StartInfo = Message[1];

                            try
                            {
                                Process.Start("bin32\\Game.exe", StartInfo);
                                Environment.Exit(0);
                            }
                            catch(Exception e)
                            {
                                MessageBox.Show(e.Message, "Ошибка запуска", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                
                            }
                            
                            break;
                    }
                }
            }
            catch
            {
                MessageBox.Show("Потеряно соединение с сервером!", "Ошибка связи", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            if (!_Server.Connected)
            {
                MessageBox.Show("Не удается подключится к серверу!", "Ошибка связи", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
        }
        private void Main_Load(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
        {
            if(textBox1.Text == ""|| textBox2.Text == ""||
                new Regex("^[A-Za-z0-9]+$").Matches(textBox1.Text).Count == 0 ||
                    textBox1.Text.Length > 10)
                MessageBox.Show("Не удается произвести регистрацию. Проверьте: \n\n*Поле 'логин' было заполнено\n*Поле 'пароль' было заполнено\n*Поле 'логин' должно содержать только английские буквы и цифры*\n*Логин не может быть больше 10 символов", "", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
            {
                button2.Enabled = false;
                MainContext.Post((object o) =>
                {
                    label4.Text = $"Регистрация...";
                }, null);
                SendMessage($"REGISTER|{textBox1.Text}|{textBox2.Text}");
            }
        }
        public void SendMessage(string MSG)
        {
            try
            {
                _Server.Send(Encoding.UTF8.GetBytes(MSG));
            }
            catch
            {

            }
        }
    }
}
