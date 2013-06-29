using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace CrypticCombatLogParser
{
    class CombatLog
    {
        private DataTable dataTable = new DataTable();

        private void createTable()
        {
            dataTable.Columns.Add("Time");
            dataTable.Columns.Add("Owner");
            dataTable.Columns.Add("IOwner");
            dataTable.Columns.Add("Source");
            dataTable.Columns.Add("ISource");
            dataTable.Columns.Add("Target");
            dataTable.Columns.Add("ITarget");
            dataTable.Columns.Add("Event");
            dataTable.Columns.Add("IEvent");
            dataTable.Columns.Add("Type");
            dataTable.Columns.Add("Flags");
            dataTable.Columns.Add("Magnitude");
            dataTable.Columns.Add("BMagnitude");

        }

        public DataTable loadData(String fileName)
        {
            StreamReader sr = null;
            
            try
            {
                sr = new StreamReader(fileName);

                while (!sr.EndOfStream)
                {
                    String line = sr.ReadLine();
                    String[] row = line.Split(',');

                    dataTable.Rows.Add();

                    int tableCount = dataTable.Rows.Count - 1;

                    for(int i = 0; i < row.Count() + 1; i++)
                    {
                        if (i == 0)
                        {
                            //The first comma include the date and owner, so that needs to be split apart
                            dataTable.Rows[tableCount][0] = Regex.Split(row[i], "::")[0];
                            dataTable.Rows[tableCount][1] = Regex.Split(row[i], "::")[1];
                            i = 1;
                        }
                        else
                        {
                            dataTable.Rows[tableCount][i] = row[i - 1];

                            //If you are too far away the combat log will return nulls for some fields
                            //If owner is null then use source or target
                            if (dataTable.Rows[tableCount][2] == null && row[2] != null )
                            {
                                dataTable.Rows[tableCount][2] = row[2];
                            }
                            else if (dataTable.Rows[tableCount][2] == null && row[4] != null)
                            {
                                dataTable.Rows[tableCount][2] = row[4];
                            }
                        }
                    }

                }
            }
            catch(IOException e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                if (sr != null)
                {
                    sr.Close();
                    sr.Dispose();
                }
            }

            return dataTable;
        }

        public String[] ownerList()
        { 
            String[] c = {"Owner","IOwner"};

            DataView view = new DataView(dataTable);
            view.Sort = "Owner";

            DataTable owners = view.ToTable(true, c);

            String[] o = new String[owners.Rows.Count];

            int y = 0;
            String p = null;

            foreach (DataRow d in owners.Rows)
            {         
                if (d[1].ToString().Trim() != "")
                {
                    p = d[1].ToString().Substring(0, 1);
                   
                    if (p == "P")
                    {
                        o[y] = d[0].ToString();
                        y++;
                    }
                }               
            }

            return o;
        }

        public CombatLog()
        {
            createTable();
        }
    }
}
