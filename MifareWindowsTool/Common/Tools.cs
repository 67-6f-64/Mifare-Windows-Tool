﻿using CliWrap;
using MCT_Windows.Windows;
using MifareWindowsTool.Common;
using MifareWindowsTool.Properties;
using ORMi;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;

namespace MCT_Windows
{
    public enum TagAction
    {
        None,
        ReadSource,
        ReadTarget,
        Clone,
        Format_PassA,
        Format_PassB,
    }
    public enum TagType
    {
        Not0Writable,
        UnlockedGen1,
        DirectCUIDgen2
    }
    public enum DumpExists
    {
        None,
        Source,
        Target,
        Both
    }
    public class Win32_PnPEntity
    {
        public string Caption { get; set; }
        public string Status { get; set; }
        public string HardwareID { get; set; }
    }
    public class Tools
    {
        public string DefaultWorkingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        MediaPlayer Player = null;
        public bool lprocess = false;
        public bool running = false;
        public string CurrentUID = "";
        MainWindow Main { get; set; }
        public Tools(MainWindow main)
        {
            Main = main;

        }
        public string mySourceUID { get; set; } = "";
        public string myTargetUID { get; set; } = "";
        public string TMPFILESOURCE_MFD { get; set; } = "";
        public string TMPFILE_TARGETMFD { get; set; } = "";
        public string TMPFILE_UNK { get; set; } = "";
        public string TMPFILE_FND { get; set; } = "";
        public List<string> GetDrivers()
        {
            var col = new List<string>();
            //Declare, Search, and Get the Properties in Win32_SystemDriver
            System.Management.SelectQuery query = new System.Management.SelectQuery("Win32_SystemDriver");
            System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(query);
            foreach (System.Management.ManagementObject ManageObject in searcher.Get())
            {
                //Declare the Main Item
                var name = ManageObject["Name"].ToString() + " - " + ManageObject["State"].ToString(); // + " - " + ManageObject["Description"].ToString();
                col.Add(name);
               
            }
            return col;
        }
        //public void test()
        //{
        //    WMIHelper helper = new WMIHelper("root\\CimV2");
        //    var acr = helper.Query("SELECT hardwareID FROM Win32_PnPEntity where name like '%ACR122%'");

        //}
        public string DriverState(string driverName = "ACR122U")
        {
            System.Management.SelectQuery query = new System.Management.SelectQuery("Win32_SystemDriver");
            query.Condition = $"Name like '%{driverName}%'";
            if (running)
                query.Condition += " AND State = 'running'";
            System.Management.ManagementObjectSearcher searcher = new System.Management.ManagementObjectSearcher(query);
            var drivers = searcher.Get();

            if (drivers.Count > 0)
            {
                var dr = drivers.OfType<ManagementObject>().First();
                return dr["State"].ToString().ToLower();
            }
              

            return "";
        }

        internal bool TestWritePermission(string dirPath, bool throwIfFails = false)
        {
            try
            {
                using (FileStream fs = System.IO.File.Create(
                    Path.Combine(
                        dirPath,
                        Path.GetRandomFileName()
                    ),
                    1,
                    FileOptions.DeleteOnClose)
                )
                { }
                return true;
            }
            catch
            {
                if (throwIfFails)
                    throw;
                else
                    return false;
            }
        }

        public void PlayBeep(Uri baseUri)
        {
            try
            {
                Player = new MediaPlayer();
                Player.Open(new Uri("Beep_ACR122U.m4a", UriKind.RelativeOrAbsolute));
                Player.Play();

            }
            catch (Exception)
            {
            }
        }

        public bool CheckAndUseDumpIfExists(string MFDFile)
        {
            if (System.IO.File.Exists("dumps\\" + MFDFile))
            {
                long fileLength = new System.IO.FileInfo("dumps\\" + MFDFile).Length;
                if (fileLength == 0) return false;
                var dr = MessageBox.Show($"{Translate.Key(nameof(MifareWindowsTool.Properties.Resources.ADumpFile))} ({Path.GetFileName("dumps\\" + MFDFile)}) {Translate.Key(nameof(MifareWindowsTool.Properties.Resources.AlreadyExists))}, {Translate.Key(nameof(MifareWindowsTool.Properties.Resources.DoYouWantToReUseThisDump))}",
                    Translate.Key(nameof(MifareWindowsTool.Properties.Resources.DumpExisting)), MessageBoxButton.YesNo, MessageBoxImage.Question);

                return (dr == MessageBoxResult.Yes);
            }
            return false;
        }


    }
}
