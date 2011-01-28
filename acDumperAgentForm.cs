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

            if (!this.timer2.Enabled) checkButtons();
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

            this.timer1.Interval = agentClass.getRefreshRate() * 1000;
            this.timer1.Enabled = true;
            this.timer2.Enabled = false;

            checkButtons();
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

            this.KillBtn.Left = 4;
            this.KillBtn.Top = this.CloseBtn.Top;

            this.StartBtn.Left = 4 + this.KillBtn.Width + 4;
            this.StartBtn.Top = this.CloseBtn.Top;
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

        private void timer2_Tick(object sender, System.EventArgs e)
        {
            this.timer2.Enabled = false;
            checkButtons();
        }

        private void checkButtons()
        {
            this.KillBtn.Enabled = agentClass.isDumperRunning();
            this.StartBtn.Enabled = !this.KillBtn.Enabled;
        }

        private void KillBtn_Click(object sender, System.EventArgs e)
        {
            agentClass.killDumper();
            this.KillBtn.Enabled = false;
            this.timer2.Enabled = true;
        }

        private void StartBtn_Click(object sender, System.EventArgs e)
        {
            agentClass.startDumper();
            this.StartBtn.Enabled = false;
            this.timer2.Enabled = true;
        }
    }
}
