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
        // List to store department details
        private List<Department> departments;

        // Constructor for the form
        public mcdo()
        {
            InitializeComponent();
            // Load the departments when the form initializes
            LoadDepartments();
            // Attach event handler for combobox selection change
            comboBox1.SelectedIndexChanged += ComboBox1_SelectedIndexChanged;
        }

        // Event handler for POST button click
        private void postbtn_Click(object sender, EventArgs e)
        {
            // Retrieve user input from text fields
            string username = usernametxt.Text;
            string password = passtxt.Text;
            string email = emailtxt.Text;
            // Get department number from selected department in combobox
            int deptNo = departments[comboBox1.SelectedIndex].dnumber;
            // For demonstration, salary number is same as department number
            int salNo = deptNo;

            // Create an anonymous object with user data
            var data = new
            {
                username = username,
                pass = password,
                email = email,
                dept_no = deptNo,
                sal_no = salNo
            };

            // Serialize the data to JSON format
            var jsonData = JsonConvert.SerializeObject(data);

            // Use HttpClient to send POST request
            using (var client = new HttpClient())
            {
                var content = new StringContent(jsonData, Encoding.UTF8, "application/json");
                // Send the POST request and wait for response
                var response = client.PostAsync("http://localhost/myapi/api.php", content).Result;
                if (response.IsSuccessStatusCode)
                {
                    // Notify user of success and clear input fields
                    MessageBox.Show("User added successfully");
                    usernametxt.Text = "";
                    passtxt.Text = "";
                    emailtxt.Text = "";
                }
                else
                {
                    // Notify user of failure
                    MessageBox.Show("Failed to add user");
                }
            }
        }

        // Event handler for GET button click
        private void getbtn_Click(object sender, EventArgs e)
        {
            // Use HttpClient to send GET request
            using (var client = new HttpClient())
            {
                var response = client.GetAsync("http://localhost/myapi/api.php?action=getUsers").Result;
                if (response.IsSuccessStatusCode)
                {
                    // Read and deserialize the response JSON data
                    var jsonData = response.Content.ReadAsStringAsync().Result;
                    var users = JsonConvert.DeserializeObject<List<User>>(jsonData);

                    // Create a DataTable to display user data
                    DataTable dataTable = new DataTable();
                    dataTable.Columns.Add("Username");
                    dataTable.Columns.Add("Password");
                    dataTable.Columns.Add("Email");
                    dataTable.Columns.Add("Position");
                    dataTable.Columns.Add("Total Salary");

                    // Populate the DataTable with user data
                    foreach (var user in users)
                    {
                        dataTable.Rows.Add(user.username, user.pass, user.email, user.dname, user.totalsalary);
                    }

                    // Set the DataTable as the data source for the DataGridView
                    datagrid_output.DataSource = dataTable;
                }
                else
                {
                    // Notify user of failure to fetch data
                    MessageBox.Show("Failed to fetch data from the server.");
                }
            }
        }

        // Method to load department details asynchronously
        private async void LoadDepartments()
        {
            // Use HttpClient to send GET request
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync("http://localhost/myapi/api.php?action=getDepartments");
                if (response.IsSuccessStatusCode)
                {
                    // Read and deserialize the response JSON data
                    var jsonData = await response.Content.ReadAsStringAsync();
                    departments = JsonConvert.DeserializeObject<List<Department>>(jsonData);

                    // Clear existing items and populate combobox with department names
                    comboBox1.Items.Clear();
                    foreach (var department in departments)
                    {
                        comboBox1.Items.Add(department.dname);
                    }
                }
                else
                {
                    // Notify user of failure to fetch departments
                    MessageBox.Show("Failed to fetch departments from the server.");
                }
            }
        }

        // Event handler for combobox selection change
        private async void ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Ensure a department is selected
            if (comboBox1.SelectedIndex == -1)
                return;

            // Get the selected department's number
            var selectedDepartment = departments[comboBox1.SelectedIndex];
            int dnumber = selectedDepartment.dnumber;

            // Use HttpClient to send GET request for total salary
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync($"http://localhost/myapi/api.php?action=getTotalSalary&dnumber={dnumber}");
                if (response.IsSuccessStatusCode)
                {
                    // Read and deserialize the response JSON data
                    var jsonData = await response.Content.ReadAsStringAsync();
                    var totalsalary = JsonConvert.DeserializeObject<TotalSalary>(jsonData);
                    // Display the total salary in the textbox
                    salarytxt.Text = totalsalary.totalsalary.ToString();
                }
                else
                {
                    // Notify user of failure to fetch total salary
                    MessageBox.Show("Failed to fetch total salary from the server.");
                }
            }
        }

        // Class to represent User data
        class User
        {
            public string username { get; set; }
            public string pass { get; set; }
            public string email { get; set; }
            public string dname { get; set; }
            public int totalsalary { get; set; }
        }

        // Class to represent Department data
        class Department
        {
            public int dnumber { get; set; }
            public string dname { get; set; }
        }

        // Class to represent TotalSalary data
        class TotalSalary
        {
            public int totalsalary { get; set; }
        }

        // Event handler for combobox selection change (duplicate)
        private void comboBox1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            // This event handler is currently empty and appears to be a duplicate
        }
    }
}
