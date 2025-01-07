using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace CardSales
{
    public partial class Login : Form
    {
        public Login()
        {
            InitializeComponent();
            this.FormClosed += Login_FormClosed;
        }

        private void Login_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string solvertoken = "";
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = client.GetAsync("https://baeminsolver.onrender.com/v1/cardsales/solver").Result;

                // check if the response is successful
                if (response.IsSuccessStatusCode)
                {
                    // read the response content as a string
                    solvertoken = response.Content.ReadAsStringAsync().Result;

                }
                else
                {
                    // handle the error
                    MessageBox.Show("Failed to get response. Status code: " + response.StatusCode);
                }
            }
            GetToken(solvertoken);
            if(!string.IsNullOrEmpty(Properties.Settings.Default.Access_token))
            {
                Thread th = new Thread(new ThreadStart(async () =>
                {
                    try
                    {
                        var restclient = new RestClient("https://development.codef.io/v1/kr/card/a/cardsales/registration-status");
                        var request = new RestRequest();
                        request.AddHeader("Content-Type", "application/json");
                        request.AddHeader("Authorization", "Bearer " + Properties.Settings.Default.Access_token);
                        MessageBox.Show(Properties.Settings.Default.Access_token);
                        MessageBox.Show("password:" + Encrypt(password.Text, Properties.Settings.Default.Pub_key));
                        request.AddBody(new
                        {
                            organization = "0323",
                            id = username.Text,
                            password = Encrypt(password.Text, Properties.Settings.Default.Pub_key),
                            inquiryType = "0"
                        });
                        var res = restclient.Post(request);

                        if (res.ResponseStatus == ResponseStatus.Completed)
                        {
                            string decodedString = HttpUtility.UrlDecode(res.Content);
                            var r = JObject.Parse(decodedString);
                            if(r["result"]["code"].ToString() == "CF-00000")
                            {
                                Properties.Settings.Default.Username = username.Text;
                                Properties.Settings.Default.Password = Encrypt(password.Text, Properties.Settings.Default.Pub_key);
                                Properties.Settings.Default.Save();
                                MessageBox.Show("Login success!");
                                this.BeginInvoke(new Action(() =>
                                {
                                    this.Hide();
                                }));
                            }
                            else
                            {
                                MessageBox.Show("Username or Password is wrong");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Solver service error");
                    }

                }));

                th.Start();
            }
        }

        public void GetToken(string secretkey)
        {
            Thread th = new Thread(new ThreadStart(async () =>
            {
                try
                {
                    var restclient = new RestClient("https://oauth.codef.io/oauth/token?grant_type=client_credentials&scope=read");
                    var request = new RestRequest();
                    request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                    request.AddHeader("Authorization", "Basic " + secretkey);

                    var res = restclient.Post(request);
                    if (res.ResponseStatus == ResponseStatus.Completed)
                    {
                        var r = JObject.Parse(res.Content);
                        Properties.Settings.Default.Access_token = r["access_token"].ToString();
                        Properties.Settings.Default.Save();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Solver service error");
                }
            }));

            th.Start();
        }

        public static string Encrypt(string textToEncrypt, string publicKeyString)
        {
            byte[] textBytes = System.Text.Encoding.UTF8.GetBytes(textToEncrypt);
            byte[] publicKeyBytes = Convert.FromBase64String(publicKeyString);

            var keyLengthBits = 2048;  // need to know length of public key in advance!
            byte[] exponent = new byte[3];
            byte[] modulus = new byte[keyLengthBits / 8];
            Array.Copy(publicKeyBytes, publicKeyBytes.Length - exponent.Length, exponent, 0, exponent.Length);
            Array.Copy(publicKeyBytes, publicKeyBytes.Length - exponent.Length - 2 - modulus.Length, modulus, 0, modulus.Length);

            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
            RSAParameters rsaKeyInfo = rsa.ExportParameters(false);
            rsaKeyInfo.Modulus = modulus;
            rsaKeyInfo.Exponent = exponent;
            rsa.ImportParameters(rsaKeyInfo);
            byte[] encrypted = rsa.Encrypt(textBytes, RSAEncryptionPadding.Pkcs1);
            return Convert.ToBase64String(encrypted);
        }
    }
}
