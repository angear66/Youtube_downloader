using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using BSD.FW.DBHelper;

namespace YoutubeCMS
{
    public partial class frmNewContent : Form
    {
        public frmNewContent()
        {
            InitializeComponent();
        }

        public DataTable VodApp
        {
            set
            {
                this.cmbApp.DataSource = value;
                this.cmbApp.ValueMember = "APP_CODE";
                this.cmbApp.DisplayMember = "APP_NAME";
            }
        }
        public object  SelectApp
        {
            set
            {
                if (this.cmbApp.DataSource != null)
                    this.cmbApp.SelectedValue = value;
            }
        }
        public string AppCode
        {
            get { return this.cmbApp.SelectedValue.ToString(); }
        }
        public string VideoID
        {
            get { return this.txtVID.Text; }
        }
        private void btnOK_Click(object sender, EventArgs e)
        {
            if (this.txtVID.Text == "")
                MessageBox.Show("Please Fill Video ID!!!");
            else
                this.DialogResult = DialogResult.OK;
        }
    }
}
