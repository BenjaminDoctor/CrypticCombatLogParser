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
        private StreamReader sr;

        public CombatLog()
        {
            createTable();
        }

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

        public void loadData(String fileName)
        {
            try
            {
                openConnectionToFile(fileName);
                addRowsToDataTable();
            }
            catch(Exception e)
            {
                MessageBox.Show(e.ToString());
            }
            finally
            {
                sr.Close();
                sr.Dispose();
            }
        }

        private void openConnectionToFile(String fileName)
        {
            sr = new StreamReader(fileName);
        }

        private void addRowsToDataTable()
        {
            while (isNotEndOfFile())
            {
                string rowFromFile = getRowFromFile();
                rowFromFile = rowFromFile.Replace("::", ",");

                string[] rowArray = rowFromFile.Split(',');
                
                DataRow dataRow = convertStringToDataRow(rowArray); 
               
                dataTable.Rows.Add(dataRow);
            }
        }

        private Boolean isNotEndOfFile()
        {
            if (!sr.EndOfStream)
            {
                return true;
            }
            else
            {
                return false;
            }            
        }

        private string getRowFromFile()
        {
            String line = sr.ReadLine();
            
            return line;
        }

        private string replaceDoubleColons(string s)
        {
            s.Replace("::", ",");

            return s;
        }

        private DataRow convertStringToDataRow(string[] rowFromData)
        {
            DataRow dataRow = dataTable.NewRow();
            dataRow.ItemArray = rowFromData;

            return dataRow;
        }

        private void parseRow(string[] rowFromFile)
        {
            int tableCount = dataTable.Rows.Count - 1;

            for (int i = 0; i < rowFromFile.Count() + 1; i++)
            {
                if (isFirstColumn(i))
                {
                    //The first comma include the date and owner, so that needs to be split apart
                    dataTable.Rows[tableCount][0] = Regex.Split(rowFromFile[i], "::")[0];
                    dataTable.Rows[tableCount][1] = Regex.Split(rowFromFile[i], "::")[1];
                    i = 1;
                }
                else
                {
                    dataTable.Rows[tableCount][i] = rowFromFile[i - 1];

                    //If you are too far away the combat log will return nulls for some fields
                    //If owner is null then use source or target
                    if (dataTable.Rows[tableCount][2] == null && rowFromFile[2] != null)
                    {
                        dataTable.Rows[tableCount][2] = rowFromFile[2];
                    }
                    else if (dataTable.Rows[tableCount][2] == null && rowFromFile[4] != null)
                    {
                        dataTable.Rows[tableCount][2] = rowFromFile[4];
                    }
                }
            }
        }

        private Boolean isFirstColumn(int i)
        {
            if (i == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public DataTable getDataTable(String fileName)
        {
            loadData(fileName);

            return dataTable;
        }
        public String[] getOwnerList()
        { 
            //The columns to return from the dataTable
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
                   
                    //Determine if the row if for a Player
                    if (p == "P")
                    {
                        o[y] = d[0].ToString();
                        y++;
                    }
                }               
            }

            return o;
        }

        public string getStats(String player)
        {
            
            DataRow[] result = dataTable.Select("Owner = '" + player + "'");
            double damage = 0;
            int swings = 0;
            int crits = 0;
            TimeSpan combatTime = new TimeSpan();
            string dateTimeFormat =  "yy:MM:dd:H:m:s.f";
            DateTime previousTime = new DateTime();
            
            //Get the start point for time
            DataRow firstTime = result[0];
            previousTime = DateTime.ParseExact(firstTime[0].ToString(),dateTimeFormat,null);

            //Set the threshhold of time between events to signify end of combat
            TimeSpan threshhold = new TimeSpan(0,0,10);
            TimeSpan ts = new TimeSpan();

            foreach (DataRow r in result)
            {
                //Sum of damage.
                if (Double.Parse(r[11].ToString()) > 0)
                {
                    swings++;
                    damage += Double.Parse(r[11].ToString());

                    if (r[10].ToString() == "Critical")
                    {
                        crits++;
                    }
                }

                ts = DateTime.ParseExact(r[0].ToString(), dateTimeFormat, null) - previousTime;

                if (threshhold > ts)
                {
                    combatTime = combatTime.Add(ts);
                    previousTime = DateTime.ParseExact(r[0].ToString(), dateTimeFormat, null);
                }
                else
                {
                    previousTime = DateTime.ParseExact(r[0].ToString(), dateTimeFormat, null);
                }
                
            }

            double time = combatTime.TotalSeconds;

            String msg;
            msg = "Damage: " + damage.ToString() + "\r\n";
            msg += "Combat Time: " + combatTime.ToString() + "\r\n";
            msg += "Combat Time in Seconds: " + combatTime.TotalSeconds.ToString() + "\r\n";
            msg += "Combat Time converted: " + time.ToString() + "\r\n";

            double dps = damage / time;
            msg += "DPS: " + dps.ToString("F");
            msg += "\r\nSwings: " + swings.ToString();
            msg += "\r\nCrits: " + crits.ToString();

            double critPercent = ((double)crits / swings);
            msg += "\r\nCrit Percent: " + critPercent.ToString("P");

            return msg;

        }
    }
}
