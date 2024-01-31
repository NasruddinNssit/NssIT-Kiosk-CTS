using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace NssIT.Kiosk.Sqlite.DB
{
    public class DBManager
    {
        private const string _dbFolderName = @"Database";
        private const string _dbFileName = @"NssITKioskDB.db";
        private const string _dbMasterFileName = @"KioskDatabaseMaster.db";

        private static System.Threading.SemaphoreSlim _manLock = new System.Threading.SemaphoreSlim(1);
        private static DBManager _dbMan = null;

        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
        private string _executionFilePath = null;
        // public string _executionFolderPath;
        private string _infoCenterFolder = "InfoCenter";
        private string _dbMasterFileFullPath = null;
        private string _dbFolderPath = null;
        private string _dbFileFullPath = null;
        //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

        // Note 
        //============
        // Kiosk's database physical file is duplicated from KioskDatabaseMaster.db and name accordingly. This file will be 
        // renewed each year when Kiosk is restart. If kiosk is restart on 1st January, system will duplicate a new copy of
        // KioskDatabaseMaster.db and name to the Year. Like NssITKioskDB2023.db.

        public static DBManager DBMan
        {
            get
            {
                if (_dbMan != null)
                    return _dbMan;
                else
                {
                    try
                    {
                        _manLock.WaitAsync().Wait();
                        if (_dbMan == null)
                        {
                            _dbMan = new DBManager();
                        }
                        return _dbMan;
                    }
                    finally
                    {
                        if (_manLock.CurrentCount == 0)
                            _manLock.Release();
                    }
                }
            }
        }

        private DBManager()
        {
            _executionFilePath = Assembly.GetExecutingAssembly().Location;

            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx
            _executionFilePath = Assembly.GetExecutingAssembly().Location;
            FileInfo fInf = new FileInfo(_executionFilePath);

            DirectoryInfo currFolder = fInf.Directory;
            DirectoryInfo parentFolder = fInf.Directory.Parent;

            // Get Application Base Folder
            _dbMasterFileFullPath = $@"{currFolder.FullName}\DatabaseMaster\KioskDatabaseMaster.db";
            _dbFolderPath = $@"{parentFolder.FullName}\{_infoCenterFolder}\{currFolder.Name}\Database";
            _dbFileFullPath = $@"{_dbFolderPath}\NssITKioskDB{DateTime.Now.Year.ToString().Trim()}.db";
            //xxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx

            DBFolderPath = _dbFolderPath;
        }

        public string DBFolderPath { get; private set; }

        public string CurrentDBFilePath
        {
            get
            {
                FileInfo fInf = new FileInfo(_dbFileFullPath);
                if (fInf.Exists == false)
                {
                    // Create New Log DB File .
                    try
                    {
                        FileInfo masInf = new FileInfo(_dbMasterFileFullPath);

                        if (masInf.Exists == false)
                        {
                            throw new Exception("Unable to allocate Kiosk Database Master File");
                        }
                        else
                        {
                            if (Directory.Exists(DBFolderPath) == false)
                            {
                                Directory.CreateDirectory(DBFolderPath);
                            }

                            FileInfo appDb = new FileInfo(_dbFileFullPath);
                            if (appDb.Exists == false)
                            {
                                File.Copy(_dbMasterFileFullPath, _dbFileFullPath);
                            }

                            FileInfo fInf2 = new FileInfo(_dbFileFullPath);
                            if (fInf2.Exists == false)
                            {
                                throw new Exception($@"Unable to copy Database master file to destination '{_dbFileFullPath}'");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _dbFileFullPath = null;
                        throw new Exception($@"Error when reading database location path; {ex.Message}");
                    }
                }

                return _dbFileFullPath;
            }
        }

        public string ConnectionString
        {
            get
            {
                string dbConnStr = $@"Data Source={CurrentDBFilePath};Version=3";
                return dbConnStr;
            }
        }
    }
}
