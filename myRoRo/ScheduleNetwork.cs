using HtmlAgilityPack;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
#if DEBUG
using System.Diagnostics;
#endif
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace myRoRo
{
    class ScheduleNetwork
    {
        public static string PAGES_COUNT = "pgs_count";

        private static async Task<List<string>> GetURLs(string url, string usrName, string pw)
        {
            try
            {
                #if DEBUG
                    Debug.WriteLine("Attempting to load initial page.");
                #endif
                HttpClient http = new HttpClient();


                string authInfo = usrName + ":" + pw;
                authInfo = Convert.ToBase64String(Encoding.ASCII.GetBytes(authInfo));
                http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authInfo);

                var response = await http.GetByteArrayAsync(url);
                String source = Encoding.GetEncoding("utf-8").GetString(response, 0, response.Length - 1);
                source = WebUtility.HtmlDecode(source);
                HtmlDocument result = new HtmlDocument();
                result.LoadHtml(source);

                #if DEBUG
                    Debug.WriteLine("Complete.");
                #endif

                List<string> outList = new List<string>();

                foreach (HtmlNode link in result.DocumentNode.SelectNodes("//a[@href]"))
                {
                    string hrefValue = link.GetAttributeValue("href", string.Empty);
                    outList.Add("http://www.romain-rolland-gymnasium.eu/schuelerbereich/svplaneinseitig/" + hrefValue);
                    #if DEBUG
                        Debug.WriteLine(hrefValue);
                    #endif
                }

                outList.RemoveAt(outList.Count - 1);
                
                string date = ScheduleHandler.RemLB(result.DocumentNode.SelectSingleNode("//h1").InnerText);
                Windows.Storage.ApplicationDataContainer localSettings =
                                    Windows.Storage.ApplicationData.Current.LocalSettings;
                localSettings.Values["UpdateDate"] = date;

                return outList;
            }
            catch (Exception e)
            {
                #if DEBUG
                    Debug.WriteLine("PAGE NOT LOADED: " + e.Message + e.StackTrace);
                #endif


                ContentDialog noWifiDialog = new ContentDialog()
                {
                    Title = "Verbindungsfehler",
                    Content = "Bitte Netzwerk überprüfen und erneut versuchen.\n\n Fehlermeldung:\n" + e.Message + e.StackTrace,
                    CloseButtonText = "Ok"
                };

                await noWifiDialog.ShowAsync();
                return null;
            }
        }

        public static async Task Refresh()
        {
            Windows.Storage.ApplicationDataContainer localSettings =
                Windows.Storage.ApplicationData.Current.LocalSettings;

            #if DEBUG
                Debug.WriteLine("Refresh() called.");
            #endif
            DatabaseHelper helper = new DatabaseHelper();

            string username = "wieland.schoebl";
            string password = "bawo2";
            string URL = "http://www.romain-rolland-gymnasium.eu/schuelerbereich/svplaneinseitig/Index.html";

            List<string> URLs = await GetURLs(URL, username, password);

            int pagesCount = 0;

            #if DEBUG
                Debug.WriteLine("URL Count: " + URLs.Count);
            #endif

            for (int i = 1; i <= URLs.Count; i++)
            {
                String url = URLs.ElementAt(i - 1);

                try
                {
                    HttpClient http = new HttpClient();

                    string authInfo = username + ":" + password;
                    authInfo = Convert.ToBase64String(Encoding.ASCII.GetBytes(authInfo));
                    http.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authInfo);

                    var response = await http.GetByteArrayAsync(url);
                    String source = Encoding.GetEncoding("ISO8859-1").GetString(response, 0, response.Length - 1);
                    source = WebUtility.HtmlDecode(source);


                    HtmlDocument result = new HtmlDocument();
                    result.LoadHtml(source);

                    helper.DeleteTable(i);

                    //START SQL
                    String currentClass = "Aufsicht";
                    String currentLesson = "";

                    #if DEBUG
                        Debug.WriteLine("One URL loaded.");
                        int t = 0;
                    #endif


                    foreach (HtmlNode table in result.DocumentNode.SelectNodes("//table"))
                    {
                        foreach (HtmlNode row in table.SelectNodes("tr"))
                        {
                            #if DEBUG
                                t++;
                            #endif
                            List<string> rowText = new List<string>();
                            List<string> rowHtml = new List<string>();
                            foreach (HtmlNode cell in row.SelectNodes("td"))
                            {
                                rowText.Add(cell.InnerText);
                                rowHtml.Add(cell.OuterHtml);
                            }

                            #if DEBUG
                                //Debug.WriteLine(rowHtml.ElementAt(0));
                            #endif

                            if (!(rowText.ElementAt(0).Contains(" ")))
                            { //All rows will need a class name, otherwise it will be much more difficult to parse the data
                                if (!rowText.ElementAt(0).Contains("Kl"))
                                {   //checking for irrelevant data (such as KL., which appears at the top.)
                                    currentClass = rowText.ElementAt(0);
                                    if (rowText.ElementAt(1).Contains(" "))
                                        helper.InsertData(i, rowText.ElementAt(0), currentLesson, rowText.ElementAt(2), rowText.ElementAt(3), rowText.ElementAt(4), rowText.ElementAt(5), rowText.ElementAt(6), rowText.ElementAt(7)); //inserting all the data into the SQL database
                                    else
                                    {
                                        helper.InsertData(i, rowText.ElementAt(0), rowText.ElementAt(1), rowText.ElementAt(2), rowText.ElementAt(3), rowText.ElementAt(4), rowText.ElementAt(5), rowText.ElementAt(6), rowText.ElementAt(7));
                                        currentLesson = rowText.ElementAt(1);
                                    }
                                }
                            }
                            else
                            {
                                if (rowText.ElementAt(1).Contains(" "))
                                    helper.InsertData(i, currentClass, currentLesson, rowText.ElementAt(2), rowText.ElementAt(3), rowText.ElementAt(4), rowText.ElementAt(5), rowText.ElementAt(6), rowText.ElementAt(7)); //inserting all the data into the SQL database
                                else
                                {
                                    helper.InsertData(i, currentClass, rowText.ElementAt(1), rowText.ElementAt(2), rowText.ElementAt(3), rowText.ElementAt(4), rowText.ElementAt(5), rowText.ElementAt(6), rowText.ElementAt(7));
                                    currentLesson = rowText.ElementAt(1);
                                }
                            }
                        }
                    }

                    #if DEBUG
                        Debug.WriteLine("Iterations: " + t);
                    #endif
                    //END SQL
                    string outStr = ScheduleHandler.RemLB(result.DocumentNode.SelectSingleNode("//h2").InnerText);

                    if (!outStr.Contains("erscheint")) {
                        localSettings.Values["Day" + i + "_Date"] = outStr;

                        outStr = ScheduleHandler.RemLB(result.DocumentNode.SelectSingleNode("//h1").InnerText);
                        localSettings.Values["Day" + i + "_UpdateDate"] = outStr;
                        //END SAVING DATES

                        pagesCount = i;
                    }
                }
                catch (Exception e)
                {
                    #if DEBUG
                        Debug.WriteLine("Error loading individual page: " + e.Message + e.StackTrace);
                    #endif

                    //ContentDialog noWifiDialog = new ContentDialog()
                    //{
                    //    Title = "Merkwürdiger Fehler",
                    //    Content = "Fehler beim laden einer einzelnen Seite.\n\n Fehlermeldung:\n" + e.Message + e.StackTrace,
                    //    CloseButtonText = "Ok"
                    //};

                    //await noWifiDialog.ShowAsync();
                }
            }

            localSettings.Values[PAGES_COUNT] = pagesCount;
        }

        public static string GetDate(int index)
        {
            Windows.Storage.ApplicationDataContainer localSettings =
                Windows.Storage.ApplicationData.Current.LocalSettings;
            return (string) localSettings.Values["Day" + index + "_Date"];
        }

        public static string GetUpdateDate()
        {
            Windows.Storage.ApplicationDataContainer localSettings =
                Windows.Storage.ApplicationData.Current.LocalSettings;
            return (string) localSettings.Values["UpdateDate"];
        }
    }
}
