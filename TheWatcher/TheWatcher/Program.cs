using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using System.Net;
using System.Net.Mail;

namespace TheWatcher
{
    internal class Program
    {
        static void Main(string[] args)
        {
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("no-sandbox");
            options.AddArgument("--ignore-gpu-blocklist");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddExcludedArgument("enable-logging");

            ChromeDriver browser = new ChromeDriver(ChromeDriverService.CreateDefaultService(), options, TimeSpan.FromMinutes(3));
            browser.Manage().Timeouts().PageLoad.Add(System.TimeSpan.FromSeconds(30));

            browser.Url = "http://nikolanedeljkovic.com";
            MyExceptionObject myExceptionObject = new MyExceptionObject();
            bool whileHandler = true;
            UrlsToCheck urlsToCheck = new UrlsToCheck();


            using (WebClient wc = new WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                var json = wc.DownloadString("watcher.json");
                urlsToCheck = Newtonsoft.Json.JsonConvert.DeserializeObject<UrlsToCheck>(json);
            }

            while (whileHandler)
            {
                foreach (UrlToCheck urlToCheck in urlsToCheck.Urls)
                {
                    try
                    {
                        if (browser.Url != urlToCheck.Url)
                            browser.Navigate().GoToUrl(urlToCheck.Url);
                        browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
                        IWebElement ratingElement = browser.FindElement(By.XPath(urlToCheck.XPath));
                        if (ratingElement != null && ratingElement.Text == urlToCheck.Value)
                            Console.WriteLine("Success: " + urlToCheck.Title);
                        else
                        {
                            myExceptionObject.Messages.Add(urlToCheck.Url);
                            myExceptionObject.Names.Add(urlToCheck.Title);
                            myExceptionObject.EResults.Add(urlToCheck.Value);
                            myExceptionObject.AResults.Add(ratingElement.Text);
                            whileHandler = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        myExceptionObject.Occurence += 1;
                        myExceptionObject.Names.Add(urlToCheck.Title);
                        myExceptionObject.Messages.Add(ex.Message);
                        myExceptionObject.EResults.Add(urlToCheck.Value);
                        myExceptionObject.AResults.Add("");
                    }
                }
                if (myExceptionObject.Occurence >= 3)
                {
                    whileHandler = false;
                }
                if (whileHandler)
                    System.Threading.Thread.Sleep(urlsToCheck.CheckDuration);
            }

            var fromAddress = new MailAddress(urlsToCheck.Email, "The Watcher");
            var toAddress = new MailAddress(urlsToCheck.ToEmail, "Watched Person");
            string fromPassword = urlsToCheck.Password;
            string subject = "The Watcher Error";
            string body = @"
                <html lang=""en"">
                    <head>
                        <meta content = ""text/html; charset=utf-8"" http - equiv = ""Content-Type"">
   
                           <title>
                               The Watcher Errors
                           </title>
   
                           <style type = ""text/css"">
                                .error - table{ font - size: 12px; padding: 3px; border - collapse: collapse; border - spacing: 0; }
                            .error - table td{
                            border: 1px solid #D1D1D1; background-color: #F3F3F3; padding: 5px 10px;}
                            .error - table th{
                                border: 1px solid #424242; color: black;text-align: left; padding: 5px 10px;}
                        </style>
                    </head>
                    <body>
                        <table class=""error-table"">
                            <thead>
                                <tr>
                                    <th>Error Message</th>
                                    <th>Name</th>
					                <th>Expected Result</th>
					                <th>Accual Result</th>
                                </tr>
                            </thead>
                            <tbody>";
            for (int i = 0; i < myExceptionObject.Messages.Count; i++)
            {
                body += "<tr>";
                body += "<td>" + myExceptionObject.Messages[i] + "</td>";
                body += "<td>" + myExceptionObject.Names[i] + "</td>";
                body += "<td>" + myExceptionObject.EResults[i] + "</td>";
                body += "<td>" + myExceptionObject.AResults[i] + "</td>";
                body += "</tr>";
            }
            body += @"
                            </tbody>
                        </table>
                    </body>
                </html>
                ";
            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                message.IsBodyHtml = true;
                smtp.Send(message);
            }
        }
    }

    public class UrlsToCheck
    {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ToEmail { get; set; }
        public int CheckDuration { get; set; }
        public List<UrlToCheck> Urls { get; set; }
        public UrlsToCheck(string email, string password, string toEmail, int checkDuration, List<UrlToCheck> urlToCheck)
        {
            ToEmail = toEmail;
            CheckDuration = checkDuration;
            Urls = urlToCheck;
            Email = email;
            Password = password;
        }
        public UrlsToCheck()
        {
            Urls = new List<UrlToCheck>();
        }
    }

    public class UrlToCheck
    {
        public string Title { get; set; }
        public string Url { get; set; }
        public string Value { get; set; }
        public string XPath { get; set; }

        public UrlToCheck(string title, string url, string value, string xpath)
        {
            Title = title;
            Url = url;
            Value = value;
            XPath = xpath;
        }
        public UrlToCheck(UrlToCheck urlToCheck)
        {
            Title = urlToCheck.Title;
            Url = urlToCheck.Url;
            Value = urlToCheck.Value;
            XPath = urlToCheck.XPath;
        }
        public UrlToCheck()
        {
            Title = "";
            Url = "";
            Value = "";
            XPath = "";
        }
    }

    public class MyExceptionObject
    {
        public List<string> Messages { get; set; }
        public List<string> Names { get; set; }
        public List<string> EResults { get; set; }
        public List<string> AResults { get; set; }
        public int Occurence { get; set; }
        public MyExceptionObject()
        {
            Messages = new List<string>();
            Names = new List<string>();
            EResults = new List<string>();
            AResults = new List<string>();
            Occurence = 0;
        }
    }
}
