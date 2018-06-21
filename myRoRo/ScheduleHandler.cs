using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Data.Sqlite.Internal;

namespace myRoRo
{
    class ScheduleHandler
    {
        private DatabaseHelper helper;
        private string lineBreak = "\n";
        int index;

        public ScheduleHandler(int index)
        {
            this.index = index;
            helper = new DatabaseHelper();
        }

        public List<string> getClassList()
        {

            List<string> outputList = new List<string>();  //this is the List which will be put out. at the end it will contain all Classes that are appearing in the schedule

            using (SqliteConnection db = new SqliteConnection("Filename=" + DatabaseHelper.DATABASE_NAME))
            {
                db.Open();
                SqliteCommand selectCommand = new SqliteCommand("SELECT " + helper.COL_1 + " FROM " + DatabaseHelper.TABLE_NAME + index + " GROUP BY " + helper.COL_1, db);
                SqliteDataReader query;


                try
                {
                    query = selectCommand.ExecuteReader();


                }
                catch (SqliteException e)
                {
                    #if DEBUG
                        Debug.WriteLine("SQLite Exception: " + e.Message + e.StackTrace);
                    #endif
                    return null;
                }

                while (query.Read())
                {
                    outputList.Add(query.GetString(0));
                }

                db.Close();
            }
            return outputList;
        }

        public List<string> GetForSQL(string SQL)
        {
            List<string> output = new List<string>();
            using (SqliteConnection db = new SqliteConnection("Filename=" + DatabaseHelper.DATABASE_NAME))
            {
                SqliteCommand command = new SqliteCommand(SQL, db);
                SqliteDataReader query;

                try
                {
                    query = command.ExecuteReader();
                }
                catch (SqliteException e)
                {
                    #if DEBUG
                        Debug.WriteLine("SQLite Exception: " + e.Message + e.StackTrace);
                    #endif
                    return null;
                }

                int i = 0;
                while (query.Read())
                {
                    try
                    {
                        query.GetString(i);
                    }
                    catch (Exception e)
                    {
                        break;
                    }
                    i++;
                }

                db.Close();
            }

            return output;
        }

        /*public List<string> GetClassListCustom()
        {

            SQLiteDatabase db = databaseHelper.getWritableDatabase();
        Cursor res;
        SharedPreferences pref = context.getSharedPreferences("Tralala", MODE_PRIVATE);
        ArrayList<String> outputList = new ArrayList<>();

        res = db.rawQuery("SELECT kl FROM " + databaseHelper.TABLE_NAME + index  + " WHERE " + pref.getString(SettingsActivity.CUSTOMSQL_NAME, "") + " GROUP BY " + databaseHelper.COL_1, null);

        while (res.moveToNext()) {
            outputList.add(res.getString(0));
        }
        res.close();

        return outputList;
        }

    public ArrayList<android.text.Spanned> getClassInfoCustom(String thisClass) throws Exception {
        SharedPreferences pref = context.getSharedPreferences("Tralala", MODE_PRIVATE);

        return getClassInfoForSQL("SELECT * FROM " + databaseHelper.TABLE_NAME + index + " WHERE (" + pref.getString(SettingsActivity.CUSTOMSQL_NAME, "")+ ") and " + databaseHelper.COL_1 + " = '" + thisClass + "'");
    }

    public ArrayList<String> getClassListPersonalized(int id) {
        SQLiteDatabase db = databaseHelper.getWritableDatabase();
        Cursor res;
        SharedPreferences pref = context.getSharedPreferences("Tralala", MODE_PRIVATE);
        String coursesRaw = pref.getString(SettingsActivity.CLASSES_NAME, "");
        ArrayList<String> outputList = new ArrayList<>();
        if (coursesRaw == "")
            return outputList;

        ArrayList<String> extraArguments = new ArrayList<>();
        String buffer = "";

        for(int i = 0; i < coursesRaw.length(); i++) {
            if ((coursesRaw.charAt(i) == ';') || (i == (coursesRaw.length() - 1))) {
                if (i == (coursesRaw.length() - 1))
                    buffer = buffer + coursesRaw.charAt(i);
                extraArguments.add(databaseHelper.COL_1 + " = '" + buffer.trim() + "' COLLATE NOCASE");
                buffer = "";
            } else {
                buffer = buffer + coursesRaw.charAt(i);
            }
        }

        String extraArgumentsSQL = "";
        if (extraArguments.size() != 0)
            extraArgumentsSQL = " WHERE ";

        for(int i = 0; i < extraArguments.size(); i++) {
            extraArgumentsSQL = extraArgumentsSQL + extraArguments.get(i) + " COLLATE NOCASE";
            if(i < (extraArguments.size() - 1))
                extraArgumentsSQL = extraArgumentsSQL + " or ";
        }

        res = db.rawQuery("SELECT kl FROM " + databaseHelper.TABLE_NAME + index  + extraArgumentsSQL + " GROUP BY " + databaseHelper.COL_1, null);

        try {
            while (res.moveToNext()) {
                outputList.add(res.getString(0));
            }
        } finally {
            res.close();
        }

        return outputList;
    }

    /**
     * The basic idea of this method is that you have go a String in SharedPreferences. It will look like this: "gen 1;gku 1;Fr 2" The ; seperates them from each other. So this method returns
     * a ArrayList of Spannable Strings with the Class you want and in addition only the courses you want. When the user puts in all the Info, first he will be asked to put in all the classes
     * which potentially could fit the rules
     * @param thisClass
     * @return
     
        public ArrayList<android.text.Spanned> getClassInfoPersonalized(String thisClass)
        {
            SQLiteDatabase db = databaseHelper.getWritableDatabase();
            Cursor res;
            SharedPreferences pref = context.getSharedPreferences("Tralala", MODE_PRIVATE);
            String coursesRaw = pref.getString("Courses", "");

            /*if (coursesRaw == "")
                return new ArrayList<>();

            ArrayList<String> extraArguments = new ArrayList<>();
            String buffer = "";

            boolean moreThanZero = false;

            if (coursesRaw != "" && coursesRaw.length() > 0)
            {
                for (int i = 0; i < coursesRaw.length(); i++)
                {
                    if ((coursesRaw.charAt(i) == ';') || (i == (coursesRaw.length() - 1)))
                    {
                        if (i == (coursesRaw.length() - 1))
                            buffer = buffer + coursesRaw.charAt(i);
                        extraArguments.add(databaseHelper.COL_3 + " = '" + buffer.trim() + "' COLLATE NOCASE or " + databaseHelper.COL_6 + " = '" + buffer.trim() + "' COLLATE NOCASE");
                        buffer = "";
                    }
                    else
                    {
                        buffer = buffer + coursesRaw.charAt(i);
                    }
                }
                moreThanZero = true;
            }
            else
            {
                moreThanZero = false;
            }

            String extraArgumentsSQL = "";

            if (moreThanZero)
            {
                if (extraArguments.size() > 0)
                    if (extraArguments != null)
                    {
                        extraArgumentsSQL = " and (";
                        moreThanZero = true;
                    }
                    else
                        moreThanZero = false;
                else
                    moreThanZero = false;
            }

            for (int i = 0; i < extraArguments.size(); i++)
            {
                extraArgumentsSQL = extraArgumentsSQL + extraArguments.get(i);
                if (i < (extraArguments.size() - 1))
                    extraArgumentsSQL = extraArgumentsSQL + " or ";
            }

            if (moreThanZero)
            {
                return getClassInfoForSQL("SELECT * FROM " + databaseHelper.TABLE_NAME + index + " WHERE " + databaseHelper.COL_1 + " = '" + thisClass + "'" + extraArgumentsSQL + ")");
            }
            else
            {
                return getClassInfoForSQL("SELECT * FROM " + databaseHelper.TABLE_NAME + index + " WHERE " + databaseHelper.COL_1 + " = '" + thisClass + "'"/* + extraArgumentsSQL);
            }
        }

        public ArrayList<android.text.Spanned> getClassInfo(String thisClass)
        {
            return getClassInfoForSQL("SELECT * FROM " + databaseHelper.TABLE_NAME + index + " WHERE " + databaseHelper.COL_1 + " = '" + thisClass + "'");
        }*/

        public string GetClassInfo(string thisClass)
        {
            return GetClassInfoForSQL("SELECT * FROM " + DatabaseHelper.TABLE_NAME + index + " WHERE " + helper.COL_1 + " = '" + thisClass + "'");
        }


        public string GetClassInfoForSQL(string sql)
        {
            bool forInfo = true;
            List<string> outList = new List<string>();
            SqliteDataReader reader;

            using (SqliteConnection db = new SqliteConnection("Filename=" + DatabaseHelper.DATABASE_NAME))
            {
                SqliteCommand command = new SqliteCommand(sql, db);
                db.Open();

                try
                {
                    reader = command.ExecuteReader();
                }
                catch (SqliteException e)
                {
                    #if DEBUG
                        Debug.WriteLine("SQLite Exception: " + e.Message + e.StackTrace);
                    #endif
                    return null;
                }
                db.Close();
            }

            String currentLesson = "x";

            #if DEBUG
                int t = 0;
            #endif

            while (reader.Read())
            {
                #if DEBUG
                    t++;
                #endif
                forInfo = true;
                StringBuilder line = new StringBuilder();

                if (reader.GetString(2).Contains(currentLesson))
                    line.Append(Spaces(5));
                else if (currentLesson.Contains("10"))
                    line.Append(RemLB(reader.GetString(2)) + "." + Spaces(1));
                else
                    line.Append(RemLB(reader.GetString(2)) + "." + Spaces(2));

                if (reader.GetString(6).Contains("\u00A0"))
                {
                    if (reader.GetString(3).Contains("\u00A0"))
                        line.Append("[Fach]");
                    else
                        line.Append(NameShortcuts.GetRealClass(RemLB(reader.GetString(3))));
                }
                else
                {
                    line.Append(NameShortcuts.GetRealClass(RemLB(reader.GetString(6))));
                }

                if (reader.GetString(5).Contains("*Frei"))
                {
                    line.Append(" entfällt");
                    forInfo = false;
                }
                else if (reader.GetString(5).Contains("Raumänderung"))
                {
                    line.Append(": Raumänderung in Raum " + RemLB(reader.GetString(7)));
                    forInfo = false;
                }
                else if (reader.GetString(5).Contains("*Stillarbeit"))
                {
                    //if (myList.get(3) == "null")  //TODO: Stillarbeit Teacher
                    if (reader.GetString(4).Contains("\u00A0"))
                        line.Append(": " + "Stillarbeit");
                    else
                        line.Append(": " + "Stillarbeit in Raum " + RemLB(reader.GetString(4)));
                    forInfo = false;
                }


                if (forInfo)
                {
                    line.Append(" bei ");

                    if (reader.GetString(5).Contains("\u00A0"))
                    {
                        line.Append("[Lehrer]");
                    }
                    else
                    {
                        line.Append(NameShortcuts.GetRealName(RemLB(reader.GetString(5))));
                    }


                    if (reader.GetString(7).Contains("\u00A0"))
                    {
                        if (reader.GetString(4).Contains("\u00A0"))
                            line.Append(" in " + "[Raum]");
                        else
                        {
                            line.Append(" in Raum " + RemLB(reader.GetString(4)));
                        }
                    }
                    else
                    {
                        line.Append(" in Raum ");
                        line.Append(RemLB(reader.GetString(7)));
                    }
                }

                if (reader.GetString(8).Contains("verschoben"))
                {
                    if (reader.GetString(2).Contains(currentLesson))
                        line = new StringBuilder(Spaces(5) + NameShortcuts.GetRealClass(RemLB(reader.GetString(3))) + " wird " + RemLB(reader.GetString(8)));
                    else if (currentLesson.Contains("10"))
                        line = new StringBuilder(RemLB(reader.GetString(2)) + "." + Spaces(1) + NameShortcuts.GetRealClass(RemLB(reader.GetString(3))) + " wird " + RemLB(reader.GetString(8)));
                    else
                        line = new StringBuilder(RemLB(reader.GetString(2)) + "." + Spaces(2) + NameShortcuts.GetRealClass(RemLB(reader.GetString(3))) + " wird " + RemLB(reader.GetString(8)));  //[Fach] wird [verschoben auf Datum]
                }
                else if (reader.GetString(8).Contains("anstatt"))
                {
                    line.Append(" " + RemLB(reader.GetString(8)));
                }
                else if (reader.GetString(8).Contains("Aufg. erteilt"))
                {
                    line.Append(lineBreak + Spaces(5) + "Aufgaben erteilt");
                }
                else if (reader.GetString(8).Contains("Aufg. für zu Hause erteilt"))
                {
                    line.Append(lineBreak + Spaces(5) + "Aufgaben für Zuhause erteilt");
                }
                else if (reader.GetString(8).Contains("Aufg. für Stillarbeit erteilt"))
                {
                    line.Append(lineBreak + Spaces(5) + "Aufgaben für Stillarbeit erteilt");
                    //} else if (myList.get(six).contains("ganze Klasse")) {
                }
                else if (!reader.GetString(8).Contains("\u00A0"))
                {
                    line.Append(lineBreak + Spaces(5) + RemLB(reader.GetString(8)));
                }

                if (!reader.GetString(2).Contains("&nbsp;"))
                    currentLesson = RemLB(reader.GetString(2));

                outList.Add(line.ToString());
            }

            string outStr = "";

            foreach (string str in outList)
            {
                outStr = outStr + "\n" + str;
            }

            #if DEBUG
                Debug.WriteLine("Iterations | READ " + t);
            #endif
            return outStr.Replace(System.Environment.NewLine, "<HERE>");
        }

        public static string RemLB(string removeLineBreaks)
        {
            return removeLineBreaks.Replace(System.Environment.NewLine, "");
        }

        private string Spaces(int count)
        {
            string outStr = "";

            for (int i = 0; i < count; i++)
            {
                outStr = outStr + ' ';
            }

            return outStr;
        }
    }
}