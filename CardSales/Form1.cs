using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

namespace CardSales
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            CardCompany1.SelectedIndexChanged += CardCompany1_SelectedIndexChanged;
            CardCompany21.SelectedIndexChanged += CardCompany21_SelectedIndexChanged;
            CardCompany31.SelectedIndexChanged += CardCompany31_SelectedIndexChanged;

            string[] items1 = new string[] { "다래한정식족발" };
            memberStoreGroup.DataSource = items1;
            StartDateTimePicker1.Text = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd");
            EndDateTimePicker1.Text = DateTime.Now.ToString("yyyy-MM-dd");
            List<Card> items2 = new List<Card> { new Card() };
            items2.Add(new Card { Name = "전체", Code = "" });
            items2.Add(new Card { Name = "KB카드", Code = "01" });
            items2.Add(new Card { Name = "신한카드", Code = "03" });
            items2.Add(new Card { Name = "비씨카드", Code = "04" });
            items2.Add(new Card { Name = "롯데카드", Code = "11" });
            items2.Add(new Card { Name = "현대카드", Code = "12" });
            items2.Add(new Card { Name = "삼성카드", Code = "13" });
            items2.Add(new Card { Name = "씨티은행", Code = "18" });
            items2.Add(new Card { Name = "농협NH카드", Code = "19" });
            items2.Add(new Card { Name = "하나카드", Code = "21" });
            items2.Add(new Card { Name = "우리카드", Code = "23" });
            CardCompany1.DataSource = items2;
            CardCompany1.DisplayMember = "Name";
            CardCompany1.SelectedIndex = 1;
            CardCompany2.DisplayMember = "Name";

            memberStoreGroup1.DataSource = items1;
            StartDateTimePicker2.Text = DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd");
            EndDateTimePicker2.Text = DateTime.Now.ToString("yyyy-MM-dd");
            CardCompany21.DataSource = items2;
            CardCompany21.DisplayMember = "Name";
            CardCompany21.SelectedIndex = 1;
            CardCompany22.DisplayMember = "Name";

            memberStoreGroup2.DataSource = items1;
            StartDateTimePicker3.Text = DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd");
            EndDateTimePicker3.Text = DateTime.Now.ToString("yyyy-MM-dd");
            CardCompany31.DataSource = items2;
            CardCompany31.DisplayMember = "Name";
            CardCompany31.SelectedIndex = 1;
            CardCompany32.DisplayMember = "Name";

            memberStoreGroup3.DataSource = items1;

            for(int i = 2022; i < 2025; i++)
            {
                year.Items.Add(i.ToString());
            }
            for(int j = 1; j < 13; j++)
            {
                month.Items.Add(j.ToString());
            }

            year.SelectedIndex = 1;
            month.SelectedIndex = 1;

            Login login = new Login();
            login.ShowDialog();
        }

        private void CardCompany31_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckCard(CardCompany31.SelectedIndex, CardCompany32);
        }

        private void CardCompany21_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckCard(CardCompany21.SelectedIndex, CardCompany22);
        }

        private void CardCompany1_SelectedIndexChanged(object sender, EventArgs e)
        {
            CheckCard(CardCompany1.SelectedIndex, CardCompany2);
        }

        private void CheckApprovalDetails_Click(object sender, EventArgs e)
        {
            if((EndDateTimePicker1.Value - StartDateTimePicker1.Value).Days > 31)
            {
                MessageBox.Show("The inquiry period is up to 31 days.");
            }
            else
            {
                Thread th = new Thread(new ThreadStart(async () =>
                {
                    try
                    {
                        this.BeginInvoke(new Action(() =>
                        {
                            var restclient = new RestClient("https://development.codef.io/v1/kr/card/a/cardsales/approval-list");
                            var request = new RestRequest();
                            request.AddHeader("Content-Type", "application/json");
                            request.AddHeader("Authorization", "Bearer " + Properties.Settings.Default.Access_token);
                            Card card1 = (Card)CardCompany1.SelectedItem;
                            Card card2 = (Card)CardCompany2.SelectedItem;
                            request.AddBody(new
                            {
                                organization = "0323",
                                id = Properties.Settings.Default.Username,
                                password = Properties.Settings.Default.Password,
                                startDate = StartDateTimePicker1.Value.ToString("yyyyMMdd"),
                                endDate = EndDateTimePicker1.Value.ToString("yyyyMMdd"),
                                memberStoreGroup = memberStoreGroup.SelectedText,
                                cardCompany = card1.Code,
                                memberStoreNo = card2.Code,
                                orderBy = "",
                                inquiryType = "0"
                            });
                            var res = restclient.Post(request);

                            if (res.ResponseStatus == ResponseStatus.Completed)
                            {
                                string decodedString = HttpUtility.UrlDecode(res.Content);

                                DataTable dataTable = new DataTable();

                                // Define the columns for the DataTable
                                dataTable.Columns.Add("No", typeof(int));
                                dataTable.Columns.Add("구분", typeof(string));
                                dataTable.Columns.Add("거래일자", typeof(string));
                                dataTable.Columns.Add("거래시간", typeof(string));
                                dataTable.Columns.Add("카드사", typeof(string));
                                dataTable.Columns.Add("제휴카드사", typeof(string));
                                dataTable.Columns.Add("카드번호", typeof(string));
                                dataTable.Columns.Add("승인번호", typeof(string));
                                dataTable.Columns.Add("승인금액", typeof(string));
                                dataTable.Columns.Add("할부기간", typeof(string));
                                dynamic dynObj = JsonConvert.DeserializeObject(decodedString);
                                int count = 0;
                                foreach (var data in dynObj.data)
                                {
                                    var no = count + 1;
                                    var division = data.resTransTypeNm.ToString();
                                    var date = DateTime.ParseExact(data.resUsedDate.ToString(), "yyyyMMdd", null);
                                    var time = DateTime.ParseExact(data.resUsedTime.ToString(), "HHmmss", null);
                                    var cardCompany = data.resCardCompany.ToString();
                                    var cardName = data.resCardName.ToString();
                                    var cardNo = data.resCardNo.ToString();
                                    var approvalNo = data.resApprovalNo.ToString();
                                    var approvalAmount = data.resUsedAmount.ToString();
                                    string installmentMonth = data.resInstallmentMonth.ToString();
                                    if (installmentMonth == "00")
                                    {
                                        installmentMonth = "일시불";
                                    }
                                    else
                                    {
                                        installmentMonth.Replace("+", "개월");
                                    }
                                    dataTable.Rows.Add(
                                        no,
                                        division,
                                        date.ToString("yyyy-MM-dd"),
                                        time.ToString("HH:mm:ss"),
                                        cardCompany,
                                        cardName,
                                        cardNo,
                                        approvalNo,
                                        approvalAmount,
                                        installmentMonth
                                    );

                                    if (count == 0)
                                    {
                                        transactionTotal.Text = data.resTotalAmount.ToString();
                                        numberTransaction.Text = data.resTotalCount.ToString();
                                        approvalTotal.Text = data.resApprovalAmount.ToString();
                                        numberApproval.Text = data.resApprovalCount.ToString();
                                        cancellationTotal.Text = data.resCancelAmount.ToString();
                                        numberCancellation.Text = data.resCancelCount.ToString();
                                    }
                                    count++;
                                }
                                approvalResult.DataSource = dataTable;
                                approvalResult.Columns[0].Width = 30;
                                approvalResult.Columns[1].Width = 60;
                            }
                        }));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("There is some error, try again later");
                    }

                }));

                th.Start();
            }
        }

        public void CheckCard(int index, ComboBox cbx)
        {
            if(index == 1)
            {
                List<Card> items = new List<Card> { new Card() };
                items.Add(new Card { Name = "전체", Code = "" });
                cbx.DataSource = items;
                cbx.SelectedIndex = 1;
            }
            if (index == 2)
            {
                List<Card> items = new List<Card> { new Card() };
                items.Add(new Card { Name = "전체", Code = "" });
                items.Add(new Card { Name = "(000046392560)다래한정식족발", Code = "000046392560" });
                cbx.DataSource = items;
                cbx.SelectedIndex = 1;
            }
            if (index == 3)
            {
                List<Card> items = new List<Card> { new Card() };
                items.Add(new Card { Name = "전체", Code = "" });
                items.Add(new Card { Name = "(0049325418)다래한정식족발", Code = "0049325418" });
                cbx.DataSource = items;
                cbx.SelectedIndex = 1;
            }
            if (index == 4)
            {
                List<Card> items = new List<Card> { new Card() };
                items.Add(new Card { Name = "전체", Code = "" });
                items.Add(new Card { Name = "(760490991)다래한정식족발", Code = "760490991" });
                cbx.DataSource = items;
                cbx.SelectedIndex = 1;
            }
            if (index == 5)
            {
                List<Card> items = new List<Card> { new Card() };
                items.Add(new Card { Name = "전체", Code = "" });
                items.Add(new Card { Name = "(9216526012)다래한정식족발", Code = "9216526012" });
                cbx.DataSource = items;
                cbx.SelectedIndex = 1;
            }
            if (index == 6)
            {
                List<Card> items = new List<Card> { new Card() };
                items.Add(new Card { Name = "전체", Code = "" });
                items.Add(new Card { Name = "(680298217)다래한정식족발", Code = "680298217" });
                cbx.DataSource = items;
                cbx.SelectedIndex = 1;
            }
            if (index == 7)
            {
                List<Card> items = new List<Card> { new Card() };
                items.Add(new Card { Name = "전체", Code = "" });
                items.Add(new Card { Name = "(121190662)다래한정식족발", Code = "121190662" });
                cbx.DataSource = items;
                cbx.SelectedIndex = 1;
            }
            if (index == 8)
            {
                List<Card> items = new List<Card> { new Card() };
                items.Add(new Card { Name = "전체", Code = "" });
                cbx.DataSource = items;
                cbx.SelectedIndex = 1;
            }
            if (index == 9)
            {
                List<Card> items = new List<Card> { new Card() };
                items.Add(new Card { Name = "전체", Code = "" });
                items.Add(new Card { Name = "(000046392560)다래한정식족발", Code = "000046392560" });
                items.Add(new Card { Name = "(157373660)다래한정식족발", Code = "157373660" });
                cbx.DataSource = items;
                cbx.SelectedIndex = 1;
            }
            if (index == 10)
            {
                List<Card> items = new List<Card> { new Card() };
                items.Add(new Card { Name = "전체", Code = "" });
                items.Add(new Card { Name = "(00944578145)다래한정식족발", Code = "00944578145" });
                cbx.DataSource = items;
                cbx.SelectedIndex = 1;
            }
            if (index == 10)
            {
                List<Card> items = new List<Card> { new Card() };
                items.Add(new Card { Name = "전체", Code = "" });
                cbx.DataSource = items;
                cbx.SelectedIndex = 1;
            }
        }

        private void CheckDepositHistory_Click(object sender, EventArgs e)
        {
            Thread th = new Thread(new ThreadStart(async () =>
            {
                try
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        var restclient = new RestClient("https://development.codef.io/v1/kr/card/a/cardsales/deposit-list");
                        var request = new RestRequest();
                        request.AddHeader("Content-Type", "application/json");
                        request.AddHeader("Authorization", "Bearer " + Properties.Settings.Default.Access_token);
                        Card card1 = (Card)CardCompany21.SelectedItem;
                        Card card2 = (Card)CardCompany22.SelectedItem;
                        request.AddBody(new
                        {
                            organization = "0323",
                            id = Properties.Settings.Default.Username,
                            password = Properties.Settings.Default.Password,
                            startDate = StartDateTimePicker2.Value.ToString("yyyyMMdd"),
                            endDate = EndDateTimePicker2.Value.ToString("yyyyMMdd"),
                            memberStoreGroup = memberStoreGroup1.Text,
                            cardCompany = card1.Code,
                            memberStoreNo = card2.Code,
                            orderBy = ""
                        });
                        var res = restclient.Post(request);

                        if (res.ResponseStatus == ResponseStatus.Completed)
                        {
                            string decodedString = HttpUtility.UrlDecode(res.Content);

                            DataTable dataTable = new DataTable();

                            // Define the columns for the DataTable
                            dataTable.Columns.Add("No", typeof(int));
                            dataTable.Columns.Add("입금일자", typeof(string));
                            dataTable.Columns.Add("카드사", typeof(string));
                            dataTable.Columns.Add("가맹점번호", typeof(string));
                            dataTable.Columns.Add("결제은행", typeof(string));
                            dataTable.Columns.Add("결제계좌", typeof(string));
                            dataTable.Columns.Add("매출건수", typeof(string));
                            dataTable.Columns.Add("매출금액", typeof(string));
                            dataTable.Columns.Add("보류금액", typeof(string));
                            dataTable.Columns.Add("부가세대리납부금액", typeof(string));
                            dataTable.Columns.Add("기타입금", typeof(string));
                            dataTable.Columns.Add("실입금", typeof(string));
                            dynamic dynObj = JsonConvert.DeserializeObject(decodedString);
                            int count = 0;
                            var sumSales = 0;
                            var sumDeposit = 0;
                            foreach (var data in dynObj.data)
                            {
                                var no = count + 1;
                                var date = DateTime.ParseExact(data.resDepositDate.ToString(), "yyyyMMdd", null);
                                var cardCompany = data.resCardCompany.ToString();
                                var memberStoreNo = data.resMemberStoreNo.ToString();
                                var bankName = data.resBankName.ToString();
                                var paymentAccount = data.resPaymentAccount.ToString();
                                var salesCount = data.resSalesCount.ToString();
                                var salesAmount = data.resSalesAmount.ToString();
                                var suspendAmount = data.resSuspenseAmount.ToString();
                                var otherDeposit = data.resOtherDeposit.ToString();
                                var realAmount = data.resAccountIn.ToString();
                                dataTable.Rows.Add(
                                    no,
                                    date.ToString("yyyy-MM-dd"),
                                    cardCompany,
                                    memberStoreNo,
                                    bankName,
                                    paymentAccount,
                                    salesCount,
                                    salesAmount,
                                    suspendAmount,
                                    "0",
                                    otherDeposit,
                                    realAmount
                                );
                                count++;
                                sumSales += int.Parse(salesAmount);
                                sumDeposit += int.Parse(realAmount);
                            }
                            depositResult1.DataSource = dataTable;
                            depositResult1.Columns[0].Width = 30;
                            totalSales.Text = sumSales.ToString();
                            totalDeposit.Text = sumDeposit.ToString();
                        }
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There is some error, try again later");
                }

            }));

            th.Start();
        }

        private void CheckMonthlyDeposit_Click(object sender, EventArgs e)
        {
            Thread th = new Thread(new ThreadStart(async () =>
            {
                try
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        var restclient = new RestClient("https://development.codef.io/v1/kr/card/a/cardsales/monthly-deposit-list");
                        var request = new RestRequest();
                        request.AddHeader("Content-Type", "application/json");
                        request.AddHeader("Authorization", "Bearer " + Properties.Settings.Default.Access_token);
                        Card card1 = (Card)CardCompany31.SelectedItem;
                        Card card2 = (Card)CardCompany32.SelectedItem;
                        request.AddBody(new
                        {
                            organization = "0323",
                            id = Properties.Settings.Default.Username,
                            password = Properties.Settings.Default.Password,
                            startDate = StartDateTimePicker3.Value.ToString("yyyyMM"),
                            endDate = EndDateTimePicker3.Value.ToString("yyyyMM"),
                            memberStoreGroup = memberStoreGroup2.Text,
                            cardCompany = card1.Code,
                            memberStoreNo = card2.Code,
                            orderBy = "1",
                            inquiryType = "0"
                        });
                        var res = restclient.Post(request);

                        if (res.ResponseStatus == ResponseStatus.Completed)
                        {
                            string decodedString = HttpUtility.UrlDecode(res.Content);

                            DataTable dataTable = new DataTable();

                            // Define the columns for the DataTable
                            dataTable.Columns.Add("월", typeof(string));
                            dataTable.Columns.Add("매출건수", typeof(string));
                            dataTable.Columns.Add("매출합계", typeof(string));
                            dataTable.Columns.Add("입금합계", typeof(string));
                            dynamic dynObj = JsonConvert.DeserializeObject(decodedString);
                            foreach (var data in dynObj.data["resDepositHistoryList"])
                            {
                                var date = DateTime.ParseExact(data.resYearMonth.ToString(), "yyyyMM", null);
                                var salesCount = data.resSalesCount.ToString();
                                var salesAmount = data.resSalesAmount.ToString();
                                var realAmount = data.resTotalAmount.ToString();
                                dataTable.Rows.Add(
                                    date.ToString("yyyy-MM"),
                                    salesCount,
                                    salesAmount,
                                    realAmount
                                );
                            }
                            monthlydepositResult.DataSource = dataTable;
                        }
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There is some error, try again later");
                }

            }));

            th.Start();
        }

        private void CheckSalesBooks_Click(object sender, EventArgs e)
        {
            Thread th = new Thread(new ThreadStart(async () =>
            {
                try
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        var restclient = new RestClient("https://development.codef.io/v1/kr/card/a/cardsales/sales-ledger");
                        var request = new RestRequest();
                        request.AddHeader("Content-Type", "application/json");
                        request.AddHeader("Authorization", "Bearer " + Properties.Settings.Default.Access_token);
                        request.AddBody(new
                        {
                            organization = "0323",
                            id = Properties.Settings.Default.Username,
                            password = Properties.Settings.Default.Password,
                            year = year.Text,
                            month = month.Text,
                            memberStoreGroup = memberStoreGroup3.Text,
                        });
                        var res = restclient.Post(request);

                        if (res.ResponseStatus == ResponseStatus.Completed)
                        {
                            string decodedString = HttpUtility.UrlDecode(res.Content);

                            DataTable dataTable = new DataTable();

                            // Define the columns for the DataTable
                            dataTable.Columns.Add("날자", typeof(string));
                            dataTable.Columns.Add("매출(매입)내역 합계", typeof(string));
                            dataTable.Columns.Add("입금내역 합계", typeof(string));
                            dynamic dynObj = JsonConvert.DeserializeObject(decodedString);
                            foreach (var data in dynObj.data["resDetailList"])
                            {
                                var date = DateTime.ParseExact(year.Text + data.resDate.ToString(), "yyyyMMdd", null);
                                var salesAmount = "0";
                                if (data.resSalesAmount.ToString() != "")
                                {
                                    salesAmount = data.resSalesAmount.ToString();
                                }
                                var depositAmount = "0";
                                if (data.resDepositAmount.ToString() != "")
                                {
                                    depositAmount = data.resDepositAmount.ToString();
                                }
                                dataTable.Rows.Add(
                                    date.ToString("yyyy-MM-dd"),
                                    salesAmount,
                                    depositAmount
                                );
                            }
                            salesResult.DataSource = dataTable;
                        }
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("There is some error, try again later");
                }

            }));

            th.Start();

        }
    }
}
