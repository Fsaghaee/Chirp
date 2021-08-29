

using System;
using System.Threading;
using Chirp;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace ChirpConsoleExample
{
    class Program
    {
        static bool end = false;
        public static string ourData = "";
        public static int shouldPack = 0;
        public static int recievedPack = 0;
        public static string encPassword = "";
        public static string encSalt = "";
        public static string SendingData = "";


        static void Main(string[] args)
        {

            Console.WriteLine("- Data Over Ultrasound - Send -  ");
            Console.WriteLine("- Please Write something to send");
            string i = Console.ReadLine();
            Console.WriteLine("- Please Write your private Key");
            encPassword = Console.ReadLine();
            
            if (string.IsNullOrEmpty(encPassword)){
                encPassword = "ThisIsOurPublicKey";
            }

            if (!string.IsNullOrEmpty(encPassword))
            {
             
                encSalt = "ThisIsOurSalt";

            }


            //We store our input in a variable for future uses
            SendingData = i;
            sending(i);


        }


        public static void sending(string i)
        {

            string app_key = "4F091Cec2AcF3eb1bF5efedEb";
            string app_secret = "dfD154B4b9eFbCdCCbd54a664ea61F8DFaa1AA1902b63d1B3e";
            string app_config = "MmW23g0q8G7E4eUEhYL4f3XFj0ISqbdAM5/yM7PY/gBFRoavArvw31Uw5Hckg/Ck+RebmIOprFYH3J5utEQ5aN8+bSVTUugOtExBJF2PRw2UFRpEFbl/Rk7P0JqRTQK1GJhM11nTVYqn4C8KTxkHUuxCJy0XopJtMB35qGx5xq0+4fZwMt9DgUxubqh2BjDf5sF9+W6T54/Yfysnrx4Bvb1irBA5HjqSkCF4OaYL44RjzdV+YA7AzP2aPL/EmkMdoCCooLewDTViCl0vAV+LwinLvAC3sG9M6QoZZMr6MhzA8AOLcvjGrE0FewUOi/sxdmHGUA1s7j/ZKofhbs6G0SXPCPXp4l5TYKrSstCF+lR3FPxydjqinVXI6sEq9uMcsCvF68sefFLIq950/mb2wJUJPAgDqVZnFQ+UM5JQCcVSiXIswQSrD54fNM6vXXSv0f1zNzioVTU4B0MgY0YICZTwHHdMWyLfcoT6OCt+eTWobcVqkyMB57fNlr8Kf86Tm/lCtbpri1NUIeHIT1/vzASjCGRcPNRu1Fib0AYWBA1Nu8BgS9A088LpANYfmASkm38F8BWB7jpqO/kcPsUNJ6/04d8rw6InWFA4Gc/4myNvzKKijguXtCTWLikAvn5dZvi/v/hEePy8vZS4gLOyt6oFLGE/fMaNdfoxbXicKVP4sJyjoR4Q89v6p+rYI+QgCX4SLDB0zSId3+z0KqDn6TbTXU5QO1RwY6iyu86ZTEjyaT4RRDdKZ+b4QGEdKsl9oUp4J5IQ26PMZq8B7RdU0+uOSo+T6+7OHGReMtlpNJXxkY/WhnO2ZTIaaRcTqsuz7wvZbOPWZ8UgXxCss2mEEIRhqW7ysjSVXF44K5QtBaB8g+vcNkNMfA6xJMSvL5DbYnYMlqdVK+AmYhgaqaWvYknF2rLLdKYWAamwJiPVhOMjkcu8Ihr7gtCZltnn+z5jASMBsYWdAAizIgu/TC+XY7hOPyhm3S3jaBXSXFnZB+Uq80aMzSE4jKlymzAbk2SF/Ui2XjzIJDj+BXGmnyYeSIvjLpccL+ea07K+qKCg6/TesG65LhaibYmx91cMzGCNFEOjVBnLUM1tcaBbhCgs+c4NdU2O4+juPxBi3AXZVOw=";
            ChirpSDK chirp;
            try
            {

                chirp = new ChirpSDK(app_key, app_secret, app_config);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                return;
            }

            chirp.ListenToSelf = false;
            chirp.OnStateChanged += OnStateChanged;
            chirp.OnSending += OnSending;
            chirp.OnSent += OnSent;
            chirp.OnReceiving += OnReceiving;
            chirp.OnReceived += OnReceived;
            chirp.OnUnsupportedCaptureDevice += OnUnsupportedCaptureDevice;
            chirp.OnUnsupportedRendererDevice += OnUnsupportedRendererDevice;

            ChirpError err = chirp.Start(AudioMode.SendAndReceive);

            if (err != null)
            {
                Console.WriteLine($"Start error : {err}");
            }



            //change characters to binaries

            ourData = StringToBinary(i);
            //get all packets and store in an array (String) "ReadyPackets" 
            string[] ReadyPackets = getpackets(ourData);

            Console.WriteLine("Post process :");
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("Total Packet number that should be sending:" + ReadyPackets.Length);
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("Packets detail :");

            Console.WriteLine("---------------------------"); 
            for (int q = 0; q < ReadyPackets.Length; q++)
            {
                Console.WriteLine("Packet number :" + q + " / the lenght of the Packet :" + ReadyPackets[q].Length + " / the content of the Packet :\n" + ReadyPackets[q]);

            }
            Console.WriteLine("-------------------------------------------------------------");
            Console.WriteLine("Sending process :");
            Console.WriteLine("---------------------------");

            //convert from binary to char ( here we store the number of packets in "firstdata"
            string firstdata = ReadyPackets.Length.ToString();

            //Sending the packets number "firstdata"
            Console.WriteLine("Sending the Packet size: " + firstdata);
            byte[] preSend = Encoding.ASCII.GetBytes(firstdata);

            //Sending the packets number "firstdata" but encrypted
            if (!string.IsNullOrEmpty(encPassword) && !string.IsNullOrEmpty(encSalt))
            {
                RijndaelManaged myAlg = new RijndaelManaged();
                byte[] salt = Encoding.ASCII.GetBytes(encSalt);
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(encPassword, salt);
                myAlg.Key = key.GetBytes(myAlg.KeySize / 8);
                myAlg.IV = key.GetBytes(myAlg.BlockSize / 8);
                preSend = EncryptAES(firstdata, myAlg.Key, myAlg.IV);
            }



            if (preSend == null)
            {
                Console.WriteLine($"Random payload is null : {err}");
            }



            //calculating the time before and after the process and sending data.
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            err = chirp.Send(preSend);
            stopwatch.Stop();
            Console.WriteLine("Elapsed Time is {0} ms", stopwatch.ElapsedTicks);
            
            Thread.Sleep(12000);


                        Console.WriteLine("-------------------------------------------------------------");


            Console.WriteLine("Sending the Packets");
            Console.WriteLine("---------------------------");


            //Sending Data
            for (int w = 0; w < ReadyPackets.Length; w++)
            {

                //convert the getpacket result (stored as String array) to ASCII
                byte[] payload = Encoding.ASCII.GetBytes(ReadyPackets[w]);


                //encrypt the Data
                if (!string.IsNullOrEmpty(encPassword) && !string.IsNullOrEmpty(encSalt))
                {
                    RijndaelManaged myAlg = new RijndaelManaged();
                    byte[] salt = Encoding.ASCII.GetBytes(encSalt);
                    Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(encPassword, salt);
                    myAlg.Key = key.GetBytes(myAlg.KeySize / 8);
                    myAlg.IV = key.GetBytes(myAlg.BlockSize / 8);
                    payload = EncryptAES(ReadyPackets[w], myAlg.Key, myAlg.IV);
                }





                if (payload == null)
                {
                    Console.WriteLine($"Random payload is null : {err}");
                }

                stopwatch = new Stopwatch();
                stopwatch.Start();
                err = chirp.Send(payload);
                stopwatch.Stop();
                Console.WriteLine("Elapsed Time is {0} ms", stopwatch.ElapsedTicks);

                Thread.Sleep(12000);
            }






            //Sendng "Done" information

            Console.WriteLine("---------------------------");

            Console.WriteLine("Sending: Last Informations");
            byte[] endSend = Encoding.ASCII.GetBytes("0");

            if (!string.IsNullOrEmpty(encPassword) && !string.IsNullOrEmpty(encSalt))
            {
                RijndaelManaged myAlg = new RijndaelManaged();
                byte[] salt = Encoding.ASCII.GetBytes(encSalt);
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(encPassword, salt);
                myAlg.Key = key.GetBytes(myAlg.KeySize / 8);
                myAlg.IV = key.GetBytes(myAlg.BlockSize / 8);
                endSend = EncryptAES("0", myAlg.Key, myAlg.IV);
            }

            if (endSend == null)
            {
                Console.WriteLine($"Random payload is null : {err}");
            }

            stopwatch = new Stopwatch();
            stopwatch.Start();
            err = chirp.Send(endSend);
            stopwatch.Stop();
            Console.WriteLine("Elapsed Time is {0} ms", stopwatch.ElapsedTicks);


          


            if (err != null)
            {
                Console.WriteLine($"Error when sending : {err}");
            }

            Console.WriteLine("\nUse Ctrl + C to quit.\n");

            while (!end)
            {
                Thread.Sleep(500);
            }

            chirp.Stop();
        }






        private static void OnReceived(byte[] dataArray, uint Channel)
        {


            string x = "";
            x = Encoding.ASCII.GetString(dataArray);

            if (!string.IsNullOrEmpty(encPassword) && !string.IsNullOrEmpty(encSalt))
            {
                RijndaelManaged myAlg = new RijndaelManaged();
                byte[] salt = Encoding.ASCII.GetBytes(encSalt);
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(encPassword, salt);
                myAlg.Key = key.GetBytes(myAlg.KeySize / 8);
                myAlg.IV = key.GetBytes(myAlg.BlockSize / 8);
                x = DecryptAES(dataArray, myAlg.Key, myAlg.IV);
            }

            Console.WriteLine("Data : " + BinaryToString(x));

            bool isIntString = BinaryToString(x).All(char.IsDigit);




            if (isIntString)
            {

                Console.WriteLine("!!! The receiver didn't get this packet:" + BinaryToString(x));
                string[] ReadyPackets = getpackets(ourData);

                if (ReadyPackets.Length < int.Parse(x))
                {
                    Console.WriteLine("!!! The receiver didn't get this Data:" + ReadyPackets[int.Parse(BinaryToString(x))]);
                    Thread.Sleep(5000);
                    sendingWitoutPacketingWithDone(ReadyPackets[int.Parse(BinaryToString(x))].Replace("-", ""));
                    Thread.Sleep(15000);
                    Console.WriteLine("Sending last information:");

                }


            }

            else if (BinaryToString(x).Contains("Done"))
            {
                Console.WriteLine("All packets have been sent and received");

            }



        }








        public static void sendingWitoutPacketingWithDone(string i)
        {

            string app_key = "4F091Cec2AcF3eb1bF5efedEb";
            string app_secret = "dfD154B4b9eFbCdCCbd54a664ea61F8DFaa1AA1902b63d1B3e";
            string app_config = "MmW23g0q8G7E4eUEhYL4f3XFj0ISqbdAM5/yM7PY/gBFRoavArvw31Uw5Hckg/Ck+RebmIOprFYH3J5utEQ5aN8+bSVTUugOtExBJF2PRw2UFRpEFbl/Rk7P0JqRTQK1GJhM11nTVYqn4C8KTxkHUuxCJy0XopJtMB35qGx5xq0+4fZwMt9DgUxubqh2BjDf5sF9+W6T54/Yfysnrx4Bvb1irBA5HjqSkCF4OaYL44RjzdV+YA7AzP2aPL/EmkMdoCCooLewDTViCl0vAV+LwinLvAC3sG9M6QoZZMr6MhzA8AOLcvjGrE0FewUOi/sxdmHGUA1s7j/ZKofhbs6G0SXPCPXp4l5TYKrSstCF+lR3FPxydjqinVXI6sEq9uMcsCvF68sefFLIq950/mb2wJUJPAgDqVZnFQ+UM5JQCcVSiXIswQSrD54fNM6vXXSv0f1zNzioVTU4B0MgY0YICZTwHHdMWyLfcoT6OCt+eTWobcVqkyMB57fNlr8Kf86Tm/lCtbpri1NUIeHIT1/vzASjCGRcPNRu1Fib0AYWBA1Nu8BgS9A088LpANYfmASkm38F8BWB7jpqO/kcPsUNJ6/04d8rw6InWFA4Gc/4myNvzKKijguXtCTWLikAvn5dZvi/v/hEePy8vZS4gLOyt6oFLGE/fMaNdfoxbXicKVP4sJyjoR4Q89v6p+rYI+QgCX4SLDB0zSId3+z0KqDn6TbTXU5QO1RwY6iyu86ZTEjyaT4RRDdKZ+b4QGEdKsl9oUp4J5IQ26PMZq8B7RdU0+uOSo+T6+7OHGReMtlpNJXxkY/WhnO2ZTIaaRcTqsuz7wvZbOPWZ8UgXxCss2mEEIRhqW7ysjSVXF44K5QtBaB8g+vcNkNMfA6xJMSvL5DbYnYMlqdVK+AmYhgaqaWvYknF2rLLdKYWAamwJiPVhOMjkcu8Ihr7gtCZltnn+z5jASMBsYWdAAizIgu/TC+XY7hOPyhm3S3jaBXSXFnZB+Uq80aMzSE4jKlymzAbk2SF/Ui2XjzIJDj+BXGmnyYeSIvjLpccL+ea07K+qKCg6/TesG65LhaibYmx91cMzGCNFEOjVBnLUM1tcaBbhCgs+c4NdU2O4+juPxBi3AXZVOw=";
            ChirpSDK chirp;
            try
            {

                chirp = new ChirpSDK(app_key, app_secret, app_config);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                return;
            }

            chirp.ListenToSelf = false;
            chirp.OnStateChanged += OnStateChanged;
            chirp.OnSending += OnSending;
            chirp.OnSent += OnSent;
            chirp.OnReceiving += OnReceiving;
            chirp.OnReceived += OnReceived;
            chirp.OnUnsupportedCaptureDevice += OnUnsupportedCaptureDevice;
            chirp.OnUnsupportedRendererDevice += OnUnsupportedRendererDevice;

            ChirpError err = chirp.Start(AudioMode.SendAndReceive);

            if (err != null)
            {
                Console.WriteLine($"Start error : {err}");
            }




            Console.WriteLine("Sending: " + i);

            //convert the getpacket result (stored as String array) to ASCII
            byte[] payload = Encoding.ASCII.GetBytes(i);


            //encrypt the Data
            if (!string.IsNullOrEmpty(encPassword) && !string.IsNullOrEmpty(encSalt))
            {
                RijndaelManaged myAlg = new RijndaelManaged();
                byte[] salt = Encoding.ASCII.GetBytes(encSalt);
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(encPassword, salt);
                myAlg.Key = key.GetBytes(myAlg.KeySize / 8);
                myAlg.IV = key.GetBytes(myAlg.BlockSize / 8);
                payload = EncryptAES(i, myAlg.Key, myAlg.IV);
            }

            if (payload == null)
            {
                Console.WriteLine($"Random payload is null : {err}");
            }

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            err = chirp.Send(payload);
            stopwatch.Stop();
            Console.WriteLine("Elapsed Time is {0} ms", stopwatch.ElapsedTicks);
            Thread.Sleep(12000);



            Console.WriteLine("Sending: Last Informations");
            byte[] endSend = Encoding.ASCII.GetBytes("0");

            if (!string.IsNullOrEmpty(encPassword) && !string.IsNullOrEmpty(encSalt))
            {
                RijndaelManaged myAlg = new RijndaelManaged();
                byte[] salt = Encoding.ASCII.GetBytes(encSalt);
                Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(encPassword, salt);
                myAlg.Key = key.GetBytes(myAlg.KeySize / 8);
                myAlg.IV = key.GetBytes(myAlg.BlockSize / 8);
                endSend = EncryptAES("0", myAlg.Key, myAlg.IV);
            }

            if (endSend == null)
            {
                Console.WriteLine($"Random payload is null : {err}");
            }

            stopwatch = new Stopwatch();
            stopwatch.Start();
            err = chirp.Send(endSend);
            stopwatch.Stop();
            Console.WriteLine("Elapsed Time is {0} ms", stopwatch.ElapsedTicks);


            Console.WriteLine("- The End information has been sent");





            if (err != null)
            {
                Console.WriteLine($"Error when sending : {err}");
            }

            Console.WriteLine("\nUse Ctrl + C to quit.\n");

            while (!end)
            {
                Thread.Sleep(500);
            }

            chirp.Stop();
        }










        public static void recieveing()
        {

            string app_key = "4F091Cec2AcF3eb1bF5efedEb";
            string app_secret = "dfD154B4b9eFbCdCCbd54a664ea61F8DFaa1AA1902b63d1B3e";
            string app_config = "MmW23g0q8G7E4eUEhYL4f3XFj0ISqbdAM5/yM7PY/gBFRoavArvw31Uw5Hckg/Ck+RebmIOprFYH3J5utEQ5aN8+bSVTUugOtExBJF2PRw2UFRpEFbl/Rk7P0JqRTQK1GJhM11nTVYqn4C8KTxkHUuxCJy0XopJtMB35qGx5xq0+4fZwMt9DgUxubqh2BjDf5sF9+W6T54/Yfysnrx4Bvb1irBA5HjqSkCF4OaYL44RjzdV+YA7AzP2aPL/EmkMdoCCooLewDTViCl0vAV+LwinLvAC3sG9M6QoZZMr6MhzA8AOLcvjGrE0FewUOi/sxdmHGUA1s7j/ZKofhbs6G0SXPCPXp4l5TYKrSstCF+lR3FPxydjqinVXI6sEq9uMcsCvF68sefFLIq950/mb2wJUJPAgDqVZnFQ+UM5JQCcVSiXIswQSrD54fNM6vXXSv0f1zNzioVTU4B0MgY0YICZTwHHdMWyLfcoT6OCt+eTWobcVqkyMB57fNlr8Kf86Tm/lCtbpri1NUIeHIT1/vzASjCGRcPNRu1Fib0AYWBA1Nu8BgS9A088LpANYfmASkm38F8BWB7jpqO/kcPsUNJ6/04d8rw6InWFA4Gc/4myNvzKKijguXtCTWLikAvn5dZvi/v/hEePy8vZS4gLOyt6oFLGE/fMaNdfoxbXicKVP4sJyjoR4Q89v6p+rYI+QgCX4SLDB0zSId3+z0KqDn6TbTXU5QO1RwY6iyu86ZTEjyaT4RRDdKZ+b4QGEdKsl9oUp4J5IQ26PMZq8B7RdU0+uOSo+T6+7OHGReMtlpNJXxkY/WhnO2ZTIaaRcTqsuz7wvZbOPWZ8UgXxCss2mEEIRhqW7ysjSVXF44K5QtBaB8g+vcNkNMfA6xJMSvL5DbYnYMlqdVK+AmYhgaqaWvYknF2rLLdKYWAamwJiPVhOMjkcu8Ihr7gtCZltnn+z5jASMBsYWdAAizIgu/TC+XY7hOPyhm3S3jaBXSXFnZB+Uq80aMzSE4jKlymzAbk2SF/Ui2XjzIJDj+BXGmnyYeSIvjLpccL+ea07K+qKCg6/TesG65LhaibYmx91cMzGCNFEOjVBnLUM1tcaBbhCgs+c4NdU2O4+juPxBi3AXZVOw=";


            ChirpSDK chirp;
            try
            {

                chirp = new ChirpSDK(app_key, app_secret, app_config);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.ReadLine();
                return;
            }
            chirp.ListenToSelf = false;
            chirp.OnStateChanged += OnStateChanged;
            chirp.OnReceiving += OnReceiving;
            chirp.OnReceived += OnReceived;


            ChirpError err = chirp.Start(AudioMode.SendAndReceive);

            if (err != null)
            {
                Console.WriteLine($"Start error : {err}");
            }

            if (err != null)
            {
                Console.WriteLine($"Error when sending : {err}");
            }

            Console.WriteLine("\nUse Ctrl + C to quit.\n");

            while (!end)
            {
                Thread.Sleep(500);
            }

            chirp.Stop();
        }

        public static string[] getpackets(string data)
        {
            //first it calculates the number of packets 
            double Packets = Math.Ceiling(data.Length / 120.0);


            //creating an array in size of the packets number
            string[] result = new string[Convert.ToInt32(Packets)];




            for (int i = 0; i < result.Length; i++)
            {
                //adding the index position at the beggining of each index
                if (result.Length == 1)
                {

                    result[i] = StringToBinary(i.ToString()) + data.Substring(i * 120, data.Length % 120);
                }

                else if (i == result.Length - 1)
                {

                    int temp = i;

                    result[temp] = StringToBinary(i.ToString()) + data.Substring(temp * 120, data.Length % 120);
                }
                else
                {

                    result[i] = StringToBinary(i.ToString()) + data.Substring(i * 120, 120);
                }

            }

            return result;
        }


        public static string BinaryToString(string enctxt)
        {
            // use your encoding here
            Encoding enc = System.Text.Encoding.UTF8;

            string binaryString = enctxt.Replace(" ", "");

            var bytes = new byte[binaryString.Length / 8];

            var ilen = (int)(binaryString.Length / 8);

            for (var i = 0; i < ilen; i++)
            {
                bytes[i] = Convert.ToByte(binaryString.Substring(i * 8, 8), 2);
            }

            string str = enc.GetString(bytes);

            return str;

        }



        public static string StringToBinary(string data)
        {
            StringBuilder sb = new StringBuilder();

            foreach (char c in data.ToCharArray())
            {
                sb.Append(Convert.ToString(c, 2).PadLeft(8, '0'));
            }
            return sb.ToString();
        }

        private static void OnSending(byte[] data, uint Channel)
        {

        }
        private static void OnSent(byte[] data, uint Channel)
        {

            Console.WriteLine($"Encrypted Data has been sent :   {BitConverter.ToString(data).Replace("-", "")}");  

        }
        private static void OnReceiving(uint Channel)
        {
            Console.WriteLine("Receiving...");
        }

        private static void OnUnsupportedCaptureDevice(string deviceName, string errorMessage)
        {
            Console.WriteLine("Unsupported capture device connected");
            Console.WriteLine("{0} : {1}", deviceName, errorMessage);
        }
        private static void OnUnsupportedRendererDevice(string deviceName, string errorMessage)
        {
            Console.WriteLine("Unsupported renderer device connected");
            Console.WriteLine("{0} : {1}", deviceName, errorMessage);
        }
        private static void SignalHandler(object sender, ConsoleCancelEventArgs args)
        {
            if (args.SpecialKey == ConsoleSpecialKey.ControlC)
            {
                args.Cancel = true;
                end = true;
            }
        }
        private static void OnStateChanged(SDKState oldState, SDKState newState)
        {
            //  Console.WriteLine($"Old State : {oldState.ToString()}.\nNew State : {newState.ToString()}");
        }

        public static string Encrypt(string clearText, string x)
        {

            string EncryptionKey = x;

            byte[] clearBytes = Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }
        public static string Decrypt(string cipherText, string x)
        {

            string EncryptionKey = x;
            cipherText = cipherText.Replace(" ", "+");
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            try
            {
                using (Aes encryptor = Aes.Create())
                {
                    Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(EncryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                    encryptor.Key = pdb.GetBytes(32);
                    encryptor.IV = pdb.GetBytes(16);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                        {

                            cs.Write(cipherBytes, 0, cipherBytes.Length);
                            cs.Close();


                        }
                        cipherText = Encoding.Unicode.GetString(ms.ToArray());
                    }
                }
            }
            catch (Exception)
            {
                Console.WriteLine(" *****   Wronge encryption Key *****");
                return "Wrong key data droped...";
            }
            return cipherText;
        }
        static byte[] EncryptAesManaged(string raw)
        {
            var temp = new byte[7000];
            try
            {
                // Create Aes that generates a new key and initialization vector (IV).    
                // Same key must be used in encryption and decryption    
                using (AesManaged aes = new AesManaged())
                {


                    // Encrypt string    
                    temp = EncryptAES(raw, aes.Key, aes.IV);
                    // Print encrypted string    
                    Console.WriteLine("Encrypted data:" + System.Text.Encoding.UTF8.GetString(temp));
                    // Decrypt the bytes to a string.    
                    // string decrypted = DecryptAES(encrypted, aes.Key, aes.IV);
                    // Print decrypted string. It should be same as raw data    
                    //Console.WriteLine("Decrypted data: " + decrypted);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            Console.ReadKey();
            return temp;
        }
        static byte[] EncryptAES(string plainText, byte[] Key, byte[] IV)
        {
            byte[] encrypted;
            // Create a new AesManaged.    
            using (AesManaged aes = new AesManaged())
            {
                // Create encryptor    
                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
                // Create MemoryStream    
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create crypto stream using the CryptoStream class. This class is the key to encryption    
                    // and encrypts and decrypts data from any given stream. In this case, we will pass a memory stream    
                    // to encrypt    
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        // Create StreamWriter and write data to a stream    
                        using (StreamWriter sw = new StreamWriter(cs))
                        sw.Write(plainText);
                        encrypted = ms.ToArray();
                    }
                }
            }
            // Return encrypted data    
            return encrypted;
        }
        static string DecryptAES(byte[] cipherText, byte[] Key, byte[] IV)
        {
            string plaintext = null;
            // Create AesManaged    
            using (AesManaged aes = new AesManaged())
            {
                // Create a decryptor    
                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);
                // Create the streams used for decryption.    
                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    // Create crypto stream    
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        // Read crypto stream    
                        using (StreamReader reader = new StreamReader(cs))
                        plaintext = reader.ReadToEnd();
                    }
                }
            }
            return plaintext;
        }

    }
}
