using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using BtceApi;
using BtcE;
using System.Net.Mail;
using System.Net;
using System.Configuration;

namespace Cryptocoin
{
    class Program
    {
        static void Main(string[] args)
        {
            int currency;
            string sound;
            string notice;
            string curName;
            BtcePair curPair;
            decimal limitMin = -1;
            decimal limitMax = decimal.MaxValue;
            decimal curPrice = 0;
            decimal absPrice = 0;
            decimal curPricePrev = 0;
            string diff = "+";
            decimal curPriceChange = 0;
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            System.DateTime lastSent = DateTime.UtcNow;
            bool hasSent = false;
            string username, recipient;

            username = ConfigurationManager.AppSettings["Username"];
            recipient = ConfigurationManager.AppSettings["Recipient"];

            Console.WriteLine("Welcome to Cryptocoin.se's BTC-e watcher :\n");
            Console.WriteLine("Set currency");

            Console.WriteLine("1. BTC / USD");
            Console.WriteLine("2. LTC / BTC");
            Console.WriteLine("3. LTC / USD");
            //Console.WriteLine("4. PPC / BTC");

            currency = Convert.ToInt32(Console.ReadLine());
            switch (currency)
            {
                case 1:
                    curName = "BTC / USD";
                    curPair = BtcePair.BtcUsd;
                    break;
                case 2:
                    curName = "LTC / BTC";
                    curPair = BtcePair.LtcBtc;
                    break;
                case 3:
                    curName = "LTC / USD";
                    curPair = BtcePair.LtcUsd;
                    break;
                /*case 4:
                    curName = "PPC / BTC";
                    curPair = BtcePair.PpcBtc;
                    break;*/
                default:
                    curName = "BTC / USD";
                    curPair = BtcePair.BtcUsd;
                    break;
            }

            Console.WriteLine(curName);
            Console.WriteLine("Sound? Y/N");
            sound = Console.ReadLine();

            Console.WriteLine("Notification limit? Y/N");
            notice = Console.ReadLine();

            if (notice.ToUpper() == "Y")
            {
                Console.WriteLine("Set min value");
                limitMin = Convert.ToDecimal(Console.ReadLine());

                Console.WriteLine("Set max value");
                limitMax = Convert.ToDecimal(Console.ReadLine());
            }

            Console.WriteLine("Timestamp\tExchange\t Change rate (%)\tDiff");

            while(true) {
                Ticker curTick = BtceApi.GetTicker(curPair);

                curPrice = curTick.Last;

                if (curPricePrev == 0)
                    curPricePrev = curPrice;

                dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                dtDateTime = dtDateTime.AddSeconds(curTick.ServerTime).ToLocalTime();

                absPrice = Math.Abs(curPricePrev - curPrice);

                if (curPrice > curPricePrev){
                    Console.ForegroundColor = ConsoleColor.Green;
                    diff = "+";
                    curPriceChange = curPriceChange + absPrice;
                    if (sound.ToUpper() == "Y")
                        Console.Beep(1000, 250);
                }
                else if (curPrice == curPricePrev)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    diff = " ";
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    
                    diff = "-";
                    curPriceChange = curPriceChange - absPrice;
                    if (sound.ToUpper() == "Y")
                        Console.Beep(500, 250);
                }

                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write(dtDateTime.ToString("HH:mm:ss tt") + "\t");
                Console.Write(String.Format("{0:0.00000000}", curPrice) + "\t");

                if (diff == "+")
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (diff == "-")
                    Console.ForegroundColor = ConsoleColor.Red;

                if (curPrice == 0)
                    curPrice = 0.00000001m;

                Console.Write(diff + String.Format("{0:0.000000}", absPrice) + " (" + String.Format("{0:0.00}", absPrice / curPrice * 100) + "%)" + "\t");

                if (curPriceChange > 0)
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (curPriceChange < 0)
                    Console.ForegroundColor = ConsoleColor.Red;

                Console.Write(curPriceChange + "\n");
                curPricePrev = curPrice;

                if (limitMin > -1)
                {
                    if (curPrice < limitMin)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Lower limit!");

                        if (!hasSent)
                        {
                            createMail(recipient, username, "Cryptocoin.se's BTC-e watcher: Your lower limit has been reached!",
                                "You have set a custom limit that has been reached: " + limitMin + "\n\nCryptocoin.se");
                            hasSent = true;
                            lastSent = DateTime.UtcNow;
                        }
                        else
                        {
                            // time span between emails to prevent spam
                            if ((DateTime.UtcNow - lastSent).TotalSeconds > Convert.ToInt32(ConfigurationManager.AppSettings["Idletime"]))
                            {
                                hasSent = false;
                            }
                        }
                    }
                }
                if (limitMax < decimal.MaxValue)
                {
                    if (curPrice > limitMax)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Upper limit!");

                        if (!hasSent)
                        {
                            createMail(recipient, username, "Cryptocoin.se's BTC-e watcher: Your upper limit has been reached!", 
                                "You have set a custom limit that has been reached: " + limitMax + "\n\nCryptocoin.se");
                            hasSent = true;
                            lastSent = DateTime.UtcNow;
                        }
                        else
                        {
                            // time span between emails to prevent spam
                            if ((DateTime.UtcNow - lastSent).TotalSeconds > Convert.ToInt32(ConfigurationManager.AppSettings["Idletime"]))
                            {
                                hasSent = false;
                            }
                        }
                    }
                }

                System.Threading.Thread.Sleep(5000);
            }
            //Console.ReadKey();
        }

        static void createMail(string recipient, string from, string subject, string msg)
        {
            string username = ConfigurationManager.AppSettings["Username"];
            string password = ConfigurationManager.AppSettings["Password"];

            try
            {
                var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = true
                };
                client.Send(from, recipient, subject, msg);
                Console.WriteLine("Mail was sent successfully!");
            }
            catch (Exception)
            {
                Console.WriteLine("Failed to send mail.");
            }
        }
    }
}
