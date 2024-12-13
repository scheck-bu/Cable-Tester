using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace cableTester
{
    public partial class io_frm : Form
    {
        public dio_ctrl.DigitalIOCtrl DigitalIOCtrl1;
        public io_frm()
        {
            InitializeComponent();
        }

        private void io_frm_Load(object sender, EventArgs e)
        {

        }

        public void addDIOCtrl(string niPath, string device, string model)
        {
            // create digital IO control from DAQ instrument
            this.DigitalIOCtrl1 = new dio_ctrl.DigitalIOCtrl(niPath, model, device);

            this.DigitalIOCtrl1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.DigitalIOCtrl1.Location = new System.Drawing.Point(9, 9);
            this.DigitalIOCtrl1.Name = "DigitalIOCtrl1";
            this.DigitalIOCtrl1.Size = new System.Drawing.Size(255, 343);
            this.DigitalIOCtrl1.TabIndex = 5;
            this.DigitalIOCtrl1.Visible = true;

            this.Controls.Add(DigitalIOCtrl1);
            this.Refresh();

            int numCtrls = this.Controls.Count;
        }

        private void close_btn_Click(object sender, EventArgs e)
        {
            this.Visible = false;
        }

    }
}
