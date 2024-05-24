using System;
using System.Collections.Generic;
using System.Data;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;

namespace mprtCorp
{
    public partial class mcdo : Form
    {
        private List<Department> departments;

        public mcdo()
        {
            InitializeComponent();
            LoadDepartments();
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
        }

        // POST Process
        private void postbtn_Click(object sender, EventArgs e)
        {
            string username = usernametxt.Text;
            string password = passtxt.Text;
            string email = emailtxt.Text;
            int deptNo = departments[comboBox1.SelectedIndex].dnumber;
            int salNo = deptNo; 

            var data = new
            {
                username = username,
                pass = password,
                email = email,
                dept_no = deptNo,
                sal_no = salNo
            };

            var jsonData = JsonConvert.SerializeObject(data);

            using (var client = new HttpClient())
            {
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = client.PostAsync("http://localhost/myapi/api.php", content).Result;
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("User added successfully");
                    usernametxt.Text = "";
                    passtxt.Text = "";
                    emailtxt.Text = "";
                }
                else
                {
                    MessageBox.Show("Failed to add user");
                }
            }
        }

        // GET Process
        private void getbtn_Click(object sender, EventArgs e)
        {
            using (var client = new HttpClient())
            {
                var response = client.GetAsync("http://localhost/myapi/api.php?action=getUsers").Result;
                if (response.IsSuccessStatusCode)
                {
                    var jsonData = response.Content.ReadAsStringAsync().Result;
                    var users = JsonConvert.DeserializeObject<List<User>>(jsonData);

                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("Username");
                    dataTable.Columns.Add("Password");
                    dataTable.Columns.Add("Email");
                    dataTable.Columns.Add("Position");
                    dataTable.Columns.Add("Total Salary");

                    foreach (var user in users)
                    {
                        dataTable.Rows.Add(user.username, user.pass, user.email, user.dname, user.totalsalary);
                    }

                    datagrid_output.DataSource = dataTable;
                }
                else
                {
                    MessageBox.Show("Failed to fetch data from the server.");
                }
            }
        }

        // Load Department Name Process
        private async void LoadDepartments()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("http://localhost/myapi/api.php?action=getDepartments");
                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    departments = JsonConvert.DeserializeObject<List<Department>>(jsonData);

                    comboBox1.Items.Clear();
                    foreach (var department in departments)
                    {
                        comboBox1.Items.Add(department.dname);
                    }
                }
                else
                {
                    MessageBox.Show("Failed to fetch departments from the server.");
                }
            }
        }

        // Selecting Process
        private async void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1)
                return;

            var selectedDepartment = departments[comboBox1.SelectedIndex];
            int dnumber = selectedDepartment.dnumber;

            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"http://localhost/myapi/api.php?action=getTotalSalary&dnumber={dnumber}");
                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    var totalsalary = JsonConvert.DeserializeObject<TotalSalary>(jsonData);
                    salarytxt.Text = totalsalary.totalsalary.ToString();
                }
                else
                {
                    MessageBox.Show("Failed to fetch total salary from the server.");
                }
            }
        }

        class User
        {
            public string username { get; set; }
            public string pass { get; set; }
            public string email { get; set; }
            public string dname { get; set; }
            public int totalsalary { get; set; }
        }

        class Department
        {
            public int dnumber { get; set; }
            public string dname { get; set; }
        }

        class TotalSalary
        {
            public int totalsalary { get; set; }
        }
    }
}
