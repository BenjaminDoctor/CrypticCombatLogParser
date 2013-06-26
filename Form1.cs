using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CrypticCombatLogParser
{
    public partial class Form1 : Form
    {
        CombatLog combatLog = new CombatLog();

        public Form1()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void menu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void btnLoad_Click(object sender, EventArgs e)
        {

            DialogResult result = ofdOpenFile.ShowDialog();
            String fileName = "";

            if (result == DialogResult.OK)
            {
                fileName = ofdOpenFile.FileName;
                dgLog.DataSource = combatLog.loadData(fileName);
                cmbPlayers.DataSource = combatLog.ownerList();
            }            
            
        }
    }
}
