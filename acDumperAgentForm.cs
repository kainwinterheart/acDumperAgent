using acDumperAgent;

namespace acDumperAgentMain
{
    public partial class acDumperAgentForm : System.Windows.Forms.Form
    {
        private acDumperAgentClass agentClass;

        public acDumperAgentForm()
        {
            InitializeComponent();
        }
        private void menuItem2_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void menuItem1_Click(object sender, System.EventArgs e)
        {
            this.Visible = true;
            this.WindowState = System.Windows.Forms.FormWindowState.Normal;
            this.fixSize();

            refreshGrid();
        }

        private void refreshGrid()
        {
            taskInfo[] taskList = agentClass.getTasks();
            this.dataGridView1.Rows.Clear();

            foreach (taskInfo task in taskList)
            {
                this.dataGridView1.Rows.Add(task.name, task.active, task.lastRun, task.nextRun);
            }
        }

        private void Form1_FormClosed(object sender, System.Windows.Forms.FormClosedEventArgs e)
        {
            this.notifyIcon1.Visible = false;
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.Visible = false;

            this.Width = 330;
            this.Height = 240;

            warmUp();

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-GB");

            //agentClass.

            this.timer1.Enabled = true;
        }

        private void warmUp()
        {
            agentClass = new acDumperAgentClass();
            if (!agentClass.gotConfig)
            {
                agentClass.log("Can't load config files.");
                Close();
            }
        }

        private void acDumperAgentForm_Resize(object sender, System.EventArgs e)
        {
            if (this.WindowState == System.Windows.Forms.FormWindowState.Minimized)
            this.Visible = false; else this.fixSize();
        }

        private void fixSize()
        {
            this.dataGridView1.Left = 0;
            this.dataGridView1.Top = 0;
            this.dataGridView1.Width = this.Width;
            this.dataGridView1.Height = this.Height - this.CloseBtn.Height - 42;

            this.CloseBtn.Left = this.Width - this.CloseBtn.Width - 12;
            this.CloseBtn.Top = this.dataGridView1.Bottom + 4;
        }

        private void CloseBtn_Click(object sender, System.EventArgs e)
        {
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
        }

        private void notifyIcon1_Click(object sender, System.EventArgs e)
        {
            this.notifyIcon1.ShowBalloonTip(5000,
                    "acDumper Agent",
                    "Currently running " + agentClass.getNumRunningTasks() + " job(s).",
                    System.Windows.Forms.ToolTipIcon.Info);
        }

        private void timer1_Tick(object sender, System.EventArgs e)
        {
            if (this.Visible) refreshGrid();
        }
    }
}
