using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace myRoRo
{
    class DatabaseHelper
    {
        public static string DATABASE_NAME = "schedule.db";
        public static string TABLE_NAME = "day";
        public string COL_1 = "kl",
                      COL_2 = "std",
                      COL_3 = "fach",
                      COL_4 = "raum",
                      COL_5 = "vlehrer",
                      COL_6 = "vfach",
                      COL_7 = "vraum",
                      COL_8 = "info";

        public DatabaseHelper()
        {
            using (SqliteConnection db = new SqliteConnection("Filename=" + DATABASE_NAME))
            {
                db.Open();

                try
                {
                    EnsureTableExists(1, db).ExecuteReader();
                }
                catch (SqliteException e)
                {
                    #if DEBUG
                        Debug.WriteLine("SQLite Exception: " + e.Message + e.StackTrace);
                    #endif
                }

                db.Close();
            }
        }

        public void DeleteTable(int index)
        {
            using (SqliteConnection db = new SqliteConnection("Filename=" + DATABASE_NAME))
            {
                db.Open();

                SqliteCommand insertCommand = new SqliteCommand
                {
                    Connection = db,
                    CommandText = "DROP TABLE IF EXISTS " + TABLE_NAME + index
                };

                try
                {
                    insertCommand.ExecuteReader();
                }
                catch (SqliteException e)
                {
                    #if DEBUG
                        Debug.WriteLine("SQLite Exception: " + e.Message + e.StackTrace);
                    #endif
                }

                db.Close();
            }
        }

        private SqliteCommand EnsureTableExists(int index, SqliteConnection db)
        {
            return new SqliteCommand
            {
                Connection = db,

                CommandText = "create table if not exists " + TABLE_NAME + index + "(ID INTEGER PRIMARY KEY AUTOINCREMENT,"
                    + COL_1 + " TEXT,"
                    + COL_2 + " TEXT,"
                    + COL_3 + " TEXT,"
                    + COL_4 + " TEXT,"
                    + COL_5 + " TEXT,"
                    + COL_6 + " TEXT,"
                    + COL_7 + " TEXT,"
                    + COL_8 + " TEXT)"
            };
        }

        public bool InsertData(int tableIndex, 
            string kl, 
            string std, 
            string fach, 
            string raum,
            string vlehrer, 
            string vfach, 
            string vraum, 
            string info)
        {

            using (SqliteConnection db = new SqliteConnection("Filename=" + DATABASE_NAME))
            {
                db.Open();

                SqliteCommand insertCommand =  new SqliteCommand
                {
                    Connection = db,

                    CommandText = "INSERT INTO " + TABLE_NAME + tableIndex + " ("
                     + COL_1 + ", "
                     + COL_2 + ", "
                     + COL_3 + ", "
                     + COL_4 + ", "
                     + COL_5 + ", "
                     + COL_6 + ", "
                     + COL_7 + ", "
                     + COL_8

                     + ") VALUES (@0, @1 , @2, @3, @4, @5, @6, @7)"
                };

                insertCommand.Parameters.AddWithValue("@0", kl);
                insertCommand.Parameters.AddWithValue("@1", std);
                insertCommand.Parameters.AddWithValue("@2", fach);
                insertCommand.Parameters.AddWithValue("@3", raum);
                insertCommand.Parameters.AddWithValue("@4", vlehrer);
                insertCommand.Parameters.AddWithValue("@5", vfach);
                insertCommand.Parameters.AddWithValue("@6", vraum);
                insertCommand.Parameters.AddWithValue("@7", info);

                try
                {
                    EnsureTableExists(tableIndex, db).ExecuteReader();
                    insertCommand.ExecuteNonQuery();
                }
                catch (SqliteException e)
                {
                    #if DEBUG
                        Debug.WriteLine("SQLite Exception: " + e.Message + e.StackTrace);
                        /*ContentDialog noWifiDialog = new ContentDialog()
                        {
                            Title = "Verbindungsfehler",
                            Content = "Bitte Netzwerk überprüfen und erneut versuchen. Fehlermeldung:\n" + e.Message + e.StackTrace,
                            CloseButtonText = "Ok"
                        };

                        noWifiDialog.ShowAsync();*/
                    #endif
                    return false;
                }
                db.Close();
            }

            return true;
        }
    }
}
