using Android.App;
using Android.Widget;
using Android.OS;
using Plugin.Settings;
using Plugin.Settings.Abstractions;
using System;
using System.Globalization;
using System.IO;
using System.Net;
namespace Anver
{
    [Activity(Label = "Anver", MainLauncher = true)]
    public class MainActivity : Activity
    {
        public TextView dateTV;
        public TextView classTV;
        private static ISettings AppSettings => CrossSettings.Current;

        DateTimeFormatInfo dfi = DateTimeFormatInfo.CurrentInfo;
        DateTime date1 = DateTime.Now;
        Calendar cal;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            cal = dfi.Calendar;

            if (AppSettings.Contains("hasAuth"))
            {
                InitMain();
                return;
            }

            // Set our view from the "login" layout resource
            SetContentView(Resource.Layout.Login);

            FindViewById<Button>(Anver.Resource.Id.LoginBtn).Click += onLoginClick;
           
        }

        void onLoginClick(object sender, EventArgs e)
        {
            string user = FindViewById<TextView>(Resource.Id.userText).Text;
            string pass = FindViewById<TextView>(Resource.Id.pwText).Text;

            byte[] bytes = System.Text.Encoding.UTF8.GetBytes((user + ":" + pass).ToCharArray());
            string auth = System.Convert.ToBase64String(bytes);

            try
            {            
                WebRequest request = WebRequest.Create("http://www.vertretung.andreanum.de/");
                request.Headers["Authorization"] = "Basic " + auth;
                WebResponse response = request.GetResponse();
                Stream data = response.GetResponseStream();
                string html = String.Empty;
                using (StreamReader sr = new StreamReader(data))
                {
                    html = sr.ReadToEnd();
                }
            } catch (Exception ex)
            {
                FindViewById<TextView>(Resource.Id.debug).Text = ex.Message;
                return;
            }
            FindViewById<TextView>(Resource.Id.debug).Text = "Success!";
            AppSettings.AddOrUpdateValue("hasAuth", auth);

            FindViewById<Button>(Anver.Resource.Id.LoginBtn).Click -= onLoginClick;

            InitMain();
        }
        
        void InitMain()
        {
            SetContentView(Resource.Layout.Main);
            //Init Actual Layout
            FindViewById<Button>(Anver.Resource.Id.button1).Click += onBtnClick;
            dateTV = FindViewById<TextView>(Resource.Id.editText1);
            classTV = FindViewById<TextView>(Resource.Id.editText2);

            if (AppSettings.Contains("defaultClass"))
            {
                classTV.SetText(AppSettings.GetValueOrDefault("defaultClass", ""), TextView.BufferType.Normal);
            }
            if (DateTime.Now.Hour > 16)
                dateTV.SetText(cal.GetDayOfMonth(DateTime.Now.AddDays(1)) + "." + cal.GetMonth(DateTime.Now.AddDays(1)) + ".", TextView.BufferType.Normal);
            else
                dateTV.SetText(cal.GetDayOfMonth(date1) + "." + cal.GetMonth(date1) + ".", TextView.BufferType.Normal);
        }


        void onBtnClick(object sender, EventArgs e)
        {
            if (!dateTV.Text.EndsWith("."))
                dateTV.SetText(dateTV.Text + ".", TextView.BufferType.Normal);
            try
            {
                DateTime.Parse(dateTV.Text + DateTime.Now.Year);
            }
            catch (Exception ee)
            {
                FindViewById<TextView>(Resource.Id.textView1).SetText("Das Datum ist inkorrekt.", TextView.BufferType.Normal);
                return;
            }

            string[] strings = loadSite().Split(new string[] { "<a name=" }, StringSplitOptions.None);

            string date = (dateTV.Text[2] == '0' && dateTV.Text[1] == '.') ? dateTV.Text.Remove(2, 1) : dateTV.Text;
            date = (dateTV.Text[3] == '0' && dateTV.Text[2] == '.') ? dateTV.Text.Remove(2, 1) : dateTV.Text;
            date = (date[0] == '0') ? date.Substring(1) : date;

            bool foundEntry = false;

            foreach (string s in strings)
            {
                if (s.Contains(date))
                {
                    char[] c = Parser.parse(s, classTV.Text).ToCharArray();
                    if (c.Length == 0)
                        FindViewById<TextView>(Resource.Id.textView1).SetText("Keine Einträge zu " + classTV.Text + " gefunden.", TextView.BufferType.Normal);
                    else
                        FindViewById<TextView>(Resource.Id.textView1).SetText(c, 0, c.Length);

                    foundEntry = true;
                }
            }
            //if (!foundEntry)
            //    FindViewById<TextView>(Resource.Id.textView1).SetText("Keinen Vertretungsplan zum eingegebenen Datum gefunden.", TextView.BufferType.Normal);

            AppSettings.AddOrUpdateValue("defaultClass", classTV.Text);
        }

        string loadSite()
        {
            //Get week of year of entered date
            DateTime parsedDate = DateTime.Parse(dateTV.Text + cal.GetYear(DateTime.Now));
            DateTime t = new DateTime(cal.GetYear(DateTime.Now), 1, 1);
            int week = cal.GetWeekOfYear(parsedDate, CalendarWeekRule.FirstDay, cal.GetDayOfWeek(t));
            //If week < 10 it must be 05 instead of 5
            string weekStr = (week < 10) ? "0" + week : week + "";
            FindViewById<TextView>(Resource.Id.textView1).SetText("http://www.vertretung.andreanum.de/" + weekStr + "/w/w00000.htm", TextView.BufferType.Normal);
            //Get Plan from website
            WebRequest request = WebRequest.Create("http://www.vertretung.andreanum.de/" + weekStr + "/w/w00000.htm");
            request.Headers["Authorization"] = "Basic " + AppSettings.GetValueOrDefault("hasAuth", "wrong");
            WebResponse response = request.GetResponse();
            Stream data = response.GetResponseStream();
            string html = String.Empty;
            using (StreamReader sr = new StreamReader(data, Encoding.GetEncoding("iso-8859-1")))
            {
                html = sr.ReadToEnd();
            }
            return html;
        }
    }
}

