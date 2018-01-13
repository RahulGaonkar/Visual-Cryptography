using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace VCPasswordnew
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if ((testBox1.Text == "hardik" && textBox2.Text == "nagda") || (testBox1.Text == "rahul" && textBox2.Text == "gaonkar") || (testBox1.Text == "alen" && textBox2.Text == "dsouza") || (testBox1.Text == "nishit" && textBox2.Text == "savla"))
            {
              
                VisualCryptography ss = new VisualCryptography();
                ss.Show();
                //this.Close();
                //  MessageBox.Show("Login Sucessful");
                //var path = Environment.ExpandEnvironmentVariables(@"C:\Users\hardik\Desktop\Visual Cryptography version1\Visual Cryptography\bin\Debug\Visual Cryptography.exe");
                //Process.Start(path);
                
            }
            else
            {
                MessageBox.Show("Unauthorized access");
            }
        }
    }
}
