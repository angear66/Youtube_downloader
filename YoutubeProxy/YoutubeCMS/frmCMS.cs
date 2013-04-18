using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BSD.FW.DBHelper;
using BSD.FW;
using System.Text.RegularExpressions;

namespace YoutubeCMS
{
    public partial class frmCMS : Form
    {
        private DBHelper DBHelper;
        public string GetConfig(string configName)
        {
            System.Configuration.AppSettingsReader rd = new System.Configuration.AppSettingsReader();
            return rd.GetValue(configName, typeof(string)).ToString();
        }
        public string FactoryName
        {
            get
            {
                AESSecurity aes = new AESSecurity();
                return aes.Decrypt(this.GetConfig("DEFAULT_FACTORY"));
            }
        }
        public string ConnectionString
        {
            get
            {
                AESSecurity aes = new AESSecurity();
                return aes.Decrypt(this.GetConfig("DEFAULT_CONNECTION"));
            }
        }

        public frmCMS()
        {
            InitializeComponent();
            this.DBHelper = new DBHelper(new DBHelperConnection(this.ConnectionString ,this.FactoryName ));
            this.cmbApp.DataSource = this.DBHelper.GetDataSet("SELECT * FROM VOD_APP ORDER BY APP_CODE", "VOD_APP").Tables[0];
            this.cmbApp.DisplayMember = "APP_NAME";
            this.cmbApp.ValueMember = "APP_CODE";
            this.LoadList();
        }

        private void LoadList()
        {
            DataTable dt=this.DBHelper.GetDataSet("SELECT V_ID,TITLE FROM VOD_YOUTUBE WHERE APP_CODE='" + this.cmbApp.SelectedValue + "' ORDER BY TITLE", "VOD_YOUTUBE").Tables[0];
            dt.DefaultView.AllowDelete = false;
            dt.DefaultView.AllowEdit = false;
            dt.DefaultView.AllowNew = false;
            this.dgList.DataSource = dt.DefaultView;
            this.dgList.Columns[1].Width = 400;
        }
        private void mnuNew_Click(object sender, EventArgs e)
        {
            frmNewContent frm = new frmNewContent();
            frm.VodApp =(DataTable) this.cmbApp.DataSource;
            frm.SelectApp = this.cmbApp.SelectedValue;
            if (frm.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string url = string.Format("http://ottbms.truevisionstv.com/ottbms/default.aspx?v={0}&appCode={1}", frm.VideoID, frm.AppCode);
                    System.Net.WebClient client = new System.Net.WebClient();
                    string result = client.DownloadString(url);
                    if (result.Contains("Success"))
                        MessageBox.Show("Success");
                    else
                        MessageBox.Show("Fail");
                    this.LoadList();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        private void cmbApp_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.LoadList();
        }

    }
}
