using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using System.Windows;
using System.Globalization;
using System.Timers;


namespace Client_SCM_2
{

    public partial class Form1 : Form
    {
        public SortableBindingList<Record> record_list = new SortableBindingList<Record> { };
        public List<string> user_list = new List<string> { };
        public Dictionary<string, string> user_dictionary = new Dictionary<string, string> { };
        public static DataGridViewCellStyle style = new DataGridViewCellStyle();
        public bool global_strobe = false;
        public TimeSpan yellowThreshold = TimeSpan.FromMinutes(1);
        public TimeSpan redThreshold = TimeSpan.FromMinutes(2);


        public Form1()
        {
            
            InitializeComponent();
            dataGridView.DataSource = record_list;
            dataGridView.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.cell_formatting);
            dataGridView.SelectionChanged += new EventHandler(selectionChanged);

            this.Controls.Add(dataGridView);
            this.dataGridView.Columns["EvtTime"].Visible = false;
            this.dataGridView.Columns["Station"].Visible = false;
            this.dataGridView.Columns["EvtActive"].Visible = false;
            this.dataGridView.Columns["LastLoop"].Visible = false;
            this.dataGridView.Columns["CompLvl"].Visible = false;
            this.dataGridView.Columns["RecordID"].Visible = false;
            this.dataGridView.Columns["ConnectTime"].Visible = false;
            this.dataGridView.Columns["Notes"].Visible = false;
            this.dataGridView.Columns["EvtTimeString"].Visible = false;
            this.dataGridView.Columns["Color"].Visible = false;
            this.dataGridView.Columns["Flashing"].Visible = false;
            this.dataGridView.Columns["Strobe"].Visible = false;
            this.dataGridView.Columns["Timer"].Visible = false;
            this.dataGridView.Columns["StartTime"].Visible = false;
            this.dataGridView.Columns["Font"].Visible = false;
            this.dataGridView.RowHeadersVisible = false;
            this.dataGridView.Columns["TimerString"].HeaderText = "Timer";
            this.dataGridView.Sort(this.dataGridView.Columns["Timer"], ListSortDirection.Descending);
            this.dataGridView.Columns["UserName"].HeaderCell.Style.Padding = new Padding(5, 5, 5, 5);
            this.dataGridView.Columns["LocCode"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView.Columns["UserName"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView.Columns["TimerString"].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView.Columns["UserName"].Width = 130;
            this.dataGridView.Columns["LocCode"].Width = 115;


            //dataGridView.RowsDefaultCellStyle.SelectionBackColor = System.Drawing.Color.Transparent;

            start_timers();   
        }

        void start_timers()
        {
            System.Windows.Forms.Timer clock0 = new System.Windows.Forms.Timer();
            clock0.Interval = 1000;
            clock0.Tick += new EventHandler(strobeFlash);
            clock0.Enabled = true;

            System.Windows.Forms.Timer clock1 = new System.Windows.Forms.Timer();
            clock1.Interval = 10000;
            clock1.Tick += new EventHandler(userDictHandler);
            clock1.Enabled = true;


        }

        void selectionChanged(object sender, EventArgs e)
        {
            dataGridView.CurrentCell.Selected = false;
        }

        void strobeFlash(object sender, EventArgs e)
        {
            
            global_strobe = !global_strobe;
            foreach (Record record in record_list)
            {
                if (record.Flashing)
                {
                    if (global_strobe)
                    {
                        record.Strobe = true;
                    }
                    else if (!global_strobe)
                    {
                        record.Strobe = false;
                    }
                }
                else
                {
                    record.Strobe = false;
                }
                record.Flashing = false;
                if (record.LocCode == "")
                {


                    record.EvtTimeString = "";
                }
                else
                {
                    
                    if (record.EvtTimeString != "")
                    {
                        record.Timer = DateTime.Now - record.StartTime;
                        record.TimerString = record.Timer.ToString(@"hh\:mm\:ss");

                        //(DateTime.TryParseExact(r.EvtTimeString, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                    }
                    
                    //Console.WriteLine("EvtTimeString= " + record.EvtTimeString);
                    //DateTime dt1 = Convert.ToDateTime(record.EvtTimeString);
                    //DateTime dt2 = DateTime.Now;
                    //record.EvtTimeString = (dt2 - dt1).ToString().Substring(0, 8);
                    //record.Timer = (dt2 - dt1);
                    //record.TimerString = record.Timer.ToString().Substring(0, 8);
                }
                //dataGridView.ClearSelection();
            }

            List<Record> duplicates = new HashSet<Record>(record_list.Where(c => record_list.Count(x => x.LocCode == c.LocCode) > 1)).ToList();
            foreach (Record record in duplicates)
            {
                record.Flashing = true;
            }

            record_list.ResetBindings();
            this.dataGridView.Sort(this.dataGridView.Columns["Timer"], ListSortDirection.Descending);
        }

        //void userListBuilder(object sender, EventArgs e)
        //{
            //user_list.Clear();
            //foreach (Record r in record_list)
            //{
                //user_list.Add(r.UserName.ToString());
            //}
            //SCMClient.sendList();

        //}

        void userDictHandler(object sender, EventArgs e)
        {
            if (user_dictionary.Count > 0)
            {
                user_dictionary.Clear();
            }
            foreach (Record r in record_list)
            {
                user_dictionary.Add(r.UserName, r.LocCode);
            }
            SCMClient.sendDict(user_dictionary);
        }

        public void cell_formatting(object sender, System.Windows.Forms.DataGridViewCellFormattingEventArgs e)
        {
            //dataGridView.ClearSelection();
            if (dataGridView.Columns[e.ColumnIndex].Name.Equals("LocCode"))
            {
                if (e.Value != null && e.Value.ToString() != "")
                {
                    Record r = dataGridView.Rows[e.RowIndex].DataBoundItem as Record;
                    if (r.Color != "")
                    {
                        e.CellStyle.BackColor = Color.FromName(r.Color);
                        e.CellStyle.ForeColor = Color.FromName(r.Font);
                    }
                    else
                    {
                        e.CellStyle.BackColor = Color.White;
                    }

                    if (r.Strobe)
                    {
                        e.CellStyle.BackColor = Color.Purple;
                        e.CellStyle.ForeColor = Color.White;

                    }
                }
            }
            if (dataGridView.Columns[e.ColumnIndex].Name.Equals("TimerString"))
            {
                if (e.Value != null && e.Value.ToString() != "")
                {
                    Record r = dataGridView.Rows[e.RowIndex].DataBoundItem as Record;
                    if (r.Timer > redThreshold)
                    {
                        e.CellStyle.BackColor = Color.Red;
                        e.CellStyle.ForeColor = Color.White;
                    }
                    //else if (r.Timer > new TimeSpan(0, 0, 0, 15, 0))
                    //{
                        //e.CellStyle.BackColor = Color.Orange;
                    //}
                    else if (r.Timer > yellowThreshold)
                    {
                        e.CellStyle.BackColor = Color.Yellow;
                        
                    }
                }
            }
            //dataGridView.ClearSelection();
        }

        public void AddRecord(Record r)
        {
            
            bool duplicate_found = false;
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => this.AddRecord(r)));
            }
            else
            {
                if (record_list.Count > 0)
                {
                    for (int i = 0; i < record_list.Count; i++)
                    {
                        duplicate_found = false;
                        if (record_list[i].UserName == r.UserName)
                        {
                            duplicate_found = true;
                            if (record_list[i].LocCode != r.LocCode)
                            {

                                record_list.RemoveAt(i);
                                this.record_list.Add(r);
                                r.StartTime = DateTime.Parse(r.EvtTime);
                                //Console.WriteLine(r.UserName + " added to list, same username but different loc");
                            }
                            //else
                            //{
                                //Console.WriteLine(r.UserName + " attempted to add to list, already have them and the same LOC");
                            //}
                            break;


                        }
                    }
                    if (!duplicate_found)
                    {
                        this.record_list.Add(r);
                        //Console.WriteLine(r.UserName + " added due to unique username loc combo");
                        //Console.WriteLine("SHADE");
                        //Console.WriteLine(r.EvtTime);
                        r.StartTime = DateTime.Parse(r.EvtTime);
                        //Console.WriteLine(r.StartTime);
                    }
                }
                else
                {
                    this.record_list.Add(r);
                    //Console.WriteLine(r.UserName + " added as the first record in the list");
                    //Console.WriteLine("PHANTOM");
                    //Console.WriteLine(r.EvtTime);
                    r.StartTime = DateTime.Parse(r.EvtTime);
                    //Console.WriteLine(r.StartTime);
                }
            }
        }

        public void RemoveRecord(Record r)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new MethodInvoker(() => this.RemoveRecord(r)));
            }
            else
            {
                record_list.Remove(r);
            }
        }
    }

    public class SCMClient
    {
        public static Form1 form;
        public static UdpClient listener;
        //public static UdpClient listener;
        public System.Timers.Timer SCMTimer = new System.Timers.Timer();
        public static bool connected = false;
        public static bool aliveReceived = false;
        //public static IPAddress server_address = IPAddress.Parse("10.168.146.255");
        public static IPEndPoint server_ip = new IPEndPoint(IPAddress.Parse("10.168.146.255"), 5685);

        public void connect()
        {
            Console.WriteLine("IN CONNECT()");
            logMessage("CONNECTING");

            //bool connected = false;
            while (!connected)
            {
                try
                {
                    // You need to "use" a udpClient in this function, here are the two relevant errors:
                    //
                    //System.ObjectDisposedException: Cannot access a disposed object.
                    //
                    //Once the socket has been disconnected, you can only reconnect again asynchronously, and only to a different EndPoint.
                    //BeginConnect must be called on a thread that won't exit until the operation has been completed.

                    Console.WriteLine("TRY");
                    listener = new UdpClient(0);                    
                    listener.Connect(server_ip);                   
                    byte[] send_data = Encoding.ASCII.GetBytes("INIT");
                    listener.Send(send_data, send_data.Length);
                    Thread.Sleep(1000);
                    if (listener.Available > 0)
                    {
                        byte[] received_bytes = listener.Receive(ref server_ip);
                        string received_data = Encoding.ASCII.GetString(received_bytes);
                        //Console.WriteLine(received_data);
                        string jsonArray = received_data.Substring(4);
                        int[] colorThresholds = JsonConvert.DeserializeObject<int[]>(jsonArray);
                        form.yellowThreshold = TimeSpan.FromMinutes(colorThresholds[0]);
                        form.redThreshold = TimeSpan.FromMinutes(colorThresholds[1]);
                        Console.WriteLine("JSON SUCCESSFUL");
                        connected = true;

                        Console.WriteLine("aliveReceived TRUE");
                        aliveReceived = true;
                        Console.WriteLine("connected TRUE");
                        Console.WriteLine("RECEIVED DATA");
                        listen();                    
                    }
                }

                catch (Exception e)
                {
                    Console.WriteLine("HUNTER");
                    Console.WriteLine(e);
                    Console.WriteLine(e.StackTrace);
                }
                Thread.Sleep(3000);
            }
        }

        //public static void sendList()
        //{

            //byte[] send_data = Encoding.ASCII.GetBytes("LIST" + JsonConvert.SerializeObject(form.user_dictionary));
            //listener.Send(send_data, send_data.Length);
        //}

        public static void sendDict(Dictionary<string, string> d)
        {
            try
            {
                byte[] send_data = Encoding.ASCII.GetBytes("LIST" + JsonConvert.SerializeObject(d));
                listener.Send(send_data, send_data.Length);
            }
            catch
            {
                Console.WriteLine("KHOVOSTOV");
            }

        }

        public void checkConnected(object sender, ElapsedEventArgs e)
        {
            byte[] send_data = Encoding.ASCII.GetBytes("SCMALIVE");

            if (!aliveReceived)
            {              
                //listener.Send(send_data, send_data.Length);
                listener.Close();
                connected = false;
                Console.WriteLine("connected FALSE");
                connect();
            }

            else
            {
                listener.Send(send_data, send_data.Length);
                aliveReceived = false;
                Console.WriteLine("aliveReceived FALSE");
            }
        }

        public static void listen()
        {

            //Console.WriteLine("In listen");
            while (connected)
            {
                if (listener.Available > 0)
                {
                    byte[] received_bytes = listener.Receive(ref server_ip);
                    connected = true;
                    string received_data = Encoding.ASCII.GetString(received_bytes);
                    //Console.WriteLine("Received data: " + received_data);
                    if (received_data.Substring(0, 5) == "remov")
                    {
                        //Console.WriteLine("Substring after remove: " + received_data.Substring(6));
                        try
                        {
                            //form.record_list.Remove(form.record_list.Where(r => r.UserName.ToString() == received_data.Substring(6)));
                            for (int x = 0; x < form.record_list.Count; x++)
                            {
                                if (form.record_list[x].UserName.ToString() == received_data.Substring(6))
                                {
                                    form.RemoveRecord(form.record_list[x]);
                                }
                            }

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("TITAN");
                            Console.WriteLine(e);
                            Console.WriteLine(e.StackTrace);
                        }

                    }

                    if (received_data == "ALIVE")
                    {
                        listener.Send(received_bytes, received_bytes.Length);
                        aliveReceived = true;
                        //Console.WriteLine("aliveReceived TRUE");
                    }

                    if (received_data == "SCMALIVE")
                    {
                        aliveReceived = true;
                       // Console.WriteLine("aliveReceived TRUE");
                    }

                    if (received_data.Substring(0, 4) == "INIT")
                    {
                        //Console.WriteLine("RECEIVED INIT, TRYING TO JSON");
                        int[] colorThresholds = JsonConvert.DeserializeObject<int[]>(received_data.Substring(4));
                        //foreach (int x in colorThresholds)
                        //{
                            //Console.WriteLine(x);
                        //}
                    }

                    if (received_data.Length > 50)
                    {

                        Record record = (Record) JsonConvert.DeserializeObject<Record>(received_data);
                        form.AddRecord(record);

                        //Console.WriteLine(record.UserName);
                    }
                }

                else
                {
                    Thread.Sleep(1000);
                }


                

                //catch (Exception e)
               // {
                    //Console.WriteLine("WARLOCK");
                   // Console.WriteLine(e);
                   // Console.WriteLine(e.StackTrace);
                //}
            }
        }

        public void setform(Form1 f)
        {
            SCMClient.form = f;
        }

        public string getTempPath()
        {
            string path = System.Environment.GetEnvironmentVariable("TEMP");
            if (!path.EndsWith("\\")) path += "\\";
            return path;
        }

        public void logMessage(string msg)
        {
            System.IO.StreamWriter sw = System.IO.File.AppendText(getTempPath() + "SCMLOG.txt");
            try
            {
                string logMsg = System.String.Format("{0:G}: {1}.", System.DateTime.Now, msg);
                sw.WriteLine(logMsg);
            }
            finally
            {
                sw.Close();
            }
        }
    }

    public class Record
    {

        public string Station { get; set; }
        public string UserName { get; set; }
        public int EvtActive { get; set; }
        public string EvtTime { get; set; }
        public string EvtTimeString { get; set; }
        public string LocCode { get; set; }
        public string LastLoop { get; set; }
        public int CompLvl { get; set; }
        public float RecordID { get; set; }
        public string ConnectTime { get; set; }
        public string Notes { get; set; }
        public string Color { get; set; }
        public string Font { get; set; }
        public bool Flashing { get; set; }
        public bool Strobe { get; set; }
        public TimeSpan Timer { get; set; }
        public string TimerString { get; set; }
        public DateTime StartTime { get; set; }

        public Record(string a, string b, int c, string d, string e, string f, string g, int h, float i, string j, string k, string l, string m)
        {
            this.Station = a;
            this.UserName = b;
            this.EvtActive = c;
            this.EvtTime = d;
            this.EvtTimeString = e;
            this.LocCode = f;
            this.LastLoop = g;
            this.CompLvl = h;
            this.RecordID = i;
            this.ConnectTime = j;
            this.Notes = k;
            this.Color = l;
            this.Font = m;
            this.Flashing = false;
            this.Strobe = false;
            this.Timer = TimeSpan.Zero;
            this.TimerString = "";
            this.StartTime = DateTime.Now;

        }
    }

    public class SortableBindingList<T> : BindingList<T> //BindingList<T> alone cannot be "sortable", this is a custom class taken from: http://stackoverflow.com/questions/23661195/datagridview-using-sortablebindinglist
    {
        private bool isSortedValue;
        ListSortDirection sortDirectionValue;
        PropertyDescriptor sortPropertyValue;

        public SortableBindingList()
        {
        }

        public SortableBindingList(IList<T> list)
        {
            foreach (object o in list)
            {
                this.Add((T)o);
            }
        }



        protected override void ApplySortCore(PropertyDescriptor prop,
            ListSortDirection direction)
        {
            Type interfaceType = prop.PropertyType.GetInterface("IComparable");

            if (interfaceType == null && prop.PropertyType.IsValueType)
            {
                Type underlyingType = Nullable.GetUnderlyingType(prop.PropertyType);

                if (underlyingType != null)
                {
                    interfaceType = underlyingType.GetInterface("IComparable");
                }
            }

            if (interfaceType != null)
            {
                sortPropertyValue = prop;
                sortDirectionValue = direction;

                IEnumerable<T> query = base.Items;

                if (direction == ListSortDirection.Ascending)
                {
                    query = query.OrderBy(i => prop.GetValue(i));
                }
                else
                {
                    query = query.OrderByDescending(i => prop.GetValue(i));
                }

                int newIndex = 0;
                foreach (object item in query)
                {
                    this.Items[newIndex] = (T)item;
                    newIndex++;
                }

                isSortedValue = true;
                this.OnListChanged(new ListChangedEventArgs(ListChangedType.Reset, -1));
            }
            else
            {
                throw new NotSupportedException("Cannot sort by " + prop.Name +
                    ". This" + prop.PropertyType.ToString() +
                    " does not implement IComparable");
            }
        }

        protected override PropertyDescriptor SortPropertyCore
        {
            get { return sortPropertyValue; }
        }

        protected override ListSortDirection SortDirectionCore
        {
            get { return sortDirectionValue; }
        }

        protected override bool SupportsSortingCore
        {
            get { return true; }
        }

        protected override bool IsSortedCore
        {
            get { return isSortedValue; }
        }
    }
}
