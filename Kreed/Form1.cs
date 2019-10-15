using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ConfigurationSettings;
using DevExpress.XtraReports.UI;


namespace Kreed
{
    public partial class Form1 : DevExpress.XtraEditors.XtraForm
    {
        public Form1()
        {
            InitializeComponent();
        }
        public string userName = Environment.UserName;                 //currnt loged in user

        private void Form1_Load(object sender, EventArgs e)
        {

            //load backcolor
            this.BackColor = Color.FromArgb(255, 232, 232);

            try
            {

                string userName = Environment.UserName;                 //currnt loged in user
                DataTable dt = new DataTable();
                MyApp.Evo.ExecSQL("SELECT LoginName, UserType FROM zz_ctech_users WHERE LoginName = '" + userName + "'", ref dt);
                // MyApp.Evo.ExecSQL("SELECT LoginName FROM zz_ctech_users", ref dt);

                foreach (DataRow dr in dt.Rows)
                {
                    if (dr.Field<string>("LoginName") == userName && dr.Field<string>("UserType") == "Super User")
                    {
                        //Super user Code goes here 

                        label1.Text = "Super User View";
                        label2.Text = userName;

                        DataTable dt2 = new DataTable();
                        MyApp.Evo.ExecSQL("SELECT * FROM zz_ctech_salesorders", ref dt2);

                        dataGridView1.DataSource = dt2;


                        int columnIndex1 = 20;
                        int columnIndex2 = 21;
                        int columnIndex3 = 22; // quantity column , zero based index
                        int columnIndex23 = 23;
                        foreach (DataGridViewRow dgv2 in dataGridView1.Rows)
                        {

                            if (dgv2.Cells[columnIndex23].Value.ToString() != "")   //Exception null ref
                            {
                                dgv2.DefaultCellStyle.BackColor = Color.Beige;
                            }


                            if (dgv2.Cells[columnIndex1].Value.ToString() == "Y")   //Exception null ref
                            {
                                dgv2.Cells[columnIndex1].Style.BackColor = System.Drawing.Color.Red;

                            }
                            if (dgv2.Cells[columnIndex2].Value.ToString() == "Y" )   //Exception null ref
                            {
                                dgv2.Cells[columnIndex2].Style.BackColor = System.Drawing.Color.Red;

                            }
                            if (dgv2.Cells[columnIndex3].Value.ToString() == "Y")   //Exception null ref
                            {
                                dgv2.Cells[columnIndex3].Style.BackColor = System.Drawing.Color.Red;
                            }

                        }

                    }
                    else
                    {
                        label1.Text = "Administration View";
                        label2.Text = userName;
                        DataTable dt2 = new DataTable();
                        MyApp.Evo.ExecSQL("SELECT AccountID, OrderDateTime, cAccountName, Account, OrderNum, OrderValue, ExtOrderNum, Message1, OrderTaker, AuthorisedBy, OverCrLimit, ReasonCode, LastEscalation, LastEscalationLevel, PrintDateTime, Notification, DelDate    FROM zz_ctech_salesorders", ref dt2);

                        dataGridView1.DataSource = dt2;


                        int columnIndex = 10; // quantity column , zero based index
                        foreach (DataGridViewRow dgv in dataGridView1.Rows)
                        {                                              
                            if (dgv.Cells[columnIndex].Value.ToString() == "Y")   //Exception null ref
                            {                                
                                    dgv.Cells[columnIndex].Style.BackColor = System.Drawing.Color.Red;                           
                            }
                        }
                    }
                }

                    
            }
            catch(Exception ex)
            {
               // MessageBox.Show(ex.Message);
            }         
        } 

        private void gridControl1_Click(object sender, EventArgs e)
        {

        }

        public void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex > 0)
            {
                DataGridViewRow dgv = this.dataGridView1.Rows[e.RowIndex];
                OrderNum_label.Text = dgv.Cells["OrderNum"].Value.ToString();               
            }

     

            }
       
        private void simpleButton1_Click(object sender, EventArgs e)
        {
            String value = OrderNum_label.Text; 
            report1 rpt = new report1();
            rpt.sqlDataSource1.Queries[0].Parameters[0].Value = value;
            ReportPrintTool print = new ReportPrintTool(rpt);
            print.ShowPreviewDialog();
        }

        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void tableLayoutPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }
    }
}
