using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using System.Net;
using System.Data;
using HtmlAgilityPack;
using System.Timers;
//using System.Threading;

// Dodac autorefresha co minute

namespace ticketAdmin
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LoadAll();
                       
        }

        private void LoadAll()
        {
            connectDB conn = new connectDB("ticketSys", "root", "localhost", "");
            ticketSystemGrid.ItemsSource = conn.Select().DefaultView;

            Timer t = new System.Timers.Timer();
            t.Interval = 60000; //60s
            t.Elapsed += refr;
            t.AutoReset = true;
            t.Enabled = true;
        }

        private void ticketSystemGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataRowView drv = ticketSystemGrid.SelectedItem as DataRowView;

            if(drv != null)
            {
                userNameBox.Text = drv.Row["UserName"].ToString();
                computerNameBox.Text = drv.Row["ComputerName"].ToString();
                addressNameBox.Text = drv.Row["Address"].ToString();
                telBox.Text = drv.Row["Tel"].ToString();
                roomNumberBox.Text = drv.Row["RoomNumber"].ToString();
                statusBox.Text = drv.Row["Status"].ToString();
                eventViewBox.Text = drv.Row["Text"].ToString();
            }

        }

       

        private void refreshView_Click(object sender, RoutedEventArgs e)
        {
            connectDB conn = new connectDB("ticketSys", "root", "localhost", "");
            ticketSystemGrid.ItemsSource = conn.Select().DefaultView;
        }

        private void closeEvent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                connectDB conn = new connectDB("ticketSys", "root", "localhost", "");
                DataRowView drv = ticketSystemGrid.SelectedItem as DataRowView;

                conn.Update(Convert.ToInt32(drv["ID"].ToString()), false);
                ticketSystemGrid.ItemsSource = conn.Select().DefaultView;
            }
            catch (Exception xe)
            {
                MessageBox.Show(xe.Message);
            }
        }

        private void openEvent_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                connectDB conn = new connectDB("ticketSys", "root", "localhost", "");
                DataRowView drv = ticketSystemGrid.SelectedItem as DataRowView;

                conn.Update(Convert.ToInt32(drv["ID"].ToString()), true);
                ticketSystemGrid.ItemsSource = conn.Select().DefaultView;
            }
            catch (Exception xe)
            {
                MessageBox.Show(xe.Message);
            }
        }

        private DateTime getTime()
        {
            var url = @"https://www.unixtimestamp.com/";
            var web = new HtmlWeb();
            var webData = web.Load(url);

            int timeInSec = 0;

            string trash = null;
            string unixSec = null;

            var htmlNode = webData.DocumentNode.SelectSingleNode("//body/div[1]/div[1]/div[2]/div[1]/div[1]/div[1]/p/h3");
            if (null != htmlNode)
            {
                var htmlRemove = webData.DocumentNode.SelectSingleNode("//body/div[1]/div[1]/div[2]/div[1]/div[1]/div[1]/p/h3/small");

                trash = htmlRemove.OuterHtml;
                unixSec = htmlNode.InnerHtml;
                //MessageBox.Show(htmlNode.InnerHtml);
            }

            StringBuilder sb = new StringBuilder(unixSec);

            timeInSec = Convert.ToInt32(sb.Replace(trash, "").ToString());

            DateTime dt = new DateTime(1970, 1, 1, 2, 0, 0).AddSeconds(timeInSec);
            

            return dt;
        }

        private void setRefr()
        {
            connectDB conn = new connectDB("ticketSys", "root", "localhost", "");
            ticketSystemGrid.ItemsSource = conn.Select().DefaultView;
            //timeBlock.Text = getTime().ToString("yyyy-dd-MM - H:mm:ss"); //Hiden already
        }

        private void refr(Object source, System.Timers.ElapsedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(this.setRefr));
        }

    }

    class connectDB
    {
        static string setConnection;
        static string nameDB = null;
        static string userNameDB = null;
        static string serverAddrDB = null;
        static string serverPassDB = null;
        MySqlConnection ticketDB;

        public void Connection(string saDB, string spDB)
        {
            setConnection = "SERVER=" + serverAddrDB + ";DATABASE=" + nameDB + "; User ID="+ userNameDB + ";Password=" + serverPassDB + ";SSLmode=none";
            ticketDB = new MySqlConnection(setConnection);
            ticketDB.Open();

            ticketDB.Close();
        }

        public connectDB(string nDB, string unDB, string saDB, string spDB)
        {
            nameDB = nDB;
            userNameDB = unDB;
            serverAddrDB = saDB;
            serverPassDB = spDB;
        }

        public DataTable Select()
        {
            Connection(serverAddrDB, serverPassDB);
            string query = "SELECT * FROM events";
            MySqlCommand commandSelect = new MySqlCommand(query, ticketDB);
            MySqlDataAdapter adp = new MySqlDataAdapter(commandSelect);
            DataTable dt = new DataTable();
            adp.Fill(dt);
            ticketDB.Open();

            ticketDB.Close();

            return dt;
        }

        public void Update(int id, bool status)
        {
            Connection(serverAddrDB, serverPassDB);
            string query = "UPDATE events SET status=" + status + " WHERE (id=" + id + ");";
            MySqlCommand commandInsert = new MySqlCommand(query, ticketDB);
            MySqlDataReader reader;
            ticketDB.Open();

            reader = commandInsert.ExecuteReader();

            reader.Close();
            ticketDB.Close();
        }
    }
}
