using System;
using System.IO;
using System.Windows.Forms;

namespace DienNuoc
{
    public partial class Form1 : Form
    {
        string[] customerNames = new string[100];
        string[] customerTypes = new string[100];
        int[] waterOld = new int[100];
        int[] waterNew = new int[100];
        int[] waterUsed = new int[100];
        int[] numPeople = new int[100];
        double[] totalCosts = new double[100];
        int count = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cbCustomerType.Items.Clear();
            cbCustomerType.Items.AddRange(new string[] { "Household", "Business", "Government", "Production" });

            dgvBills.ColumnCount = 6;
            dgvBills.Columns[0].Name = "No.";
            dgvBills.Columns[1].Name = "Name";
            dgvBills.Columns[2].Name = "Type";
            dgvBills.Columns[3].Name = "Water Used";
            dgvBills.Columns[4].Name = "People";
            dgvBills.Columns[5].Name = "Total (VND)";
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || cbCustomerType.SelectedIndex == -1)
            {
                MessageBox.Show("Please fill in all required fields.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtOldMeter.Text, out int oldMeter) || !int.TryParse(txtNewMeter.Text, out int newMeter))
            {
                MessageBox.Show("Invalid meter readings.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int used = newMeter - oldMeter;
            if (used < 0)
            {
                MessageBox.Show("New meter must be greater than or equal to old meter!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            customerNames[count] = txtName.Text.Trim();
            customerTypes[count] = cbCustomerType.SelectedItem.ToString();
            waterOld[count] = oldMeter;
            waterNew[count] = newMeter;
            waterUsed[count] = used;
            numPeople[count] = string.IsNullOrEmpty(txtNumPeople.Text) ? 0 : int.Parse(txtNumPeople.Text);
            totalCosts[count] = CalculateBill(customerTypes[count], waterUsed[count], numPeople[count]);
            count++;

            Display();
            ClearInputs();
            MessageBox.Show("Saved successfully!", "Notification", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvBills.CurrentRow == null) return;

            int index = dgvBills.CurrentCell.RowIndex;
            if (index >= 0 && index < count)
            {
                for (int i = index; i < count - 1; i++)
                {
                    customerNames[i] = customerNames[i + 1];
                    customerTypes[i] = customerTypes[i + 1];
                    waterOld[i] = waterOld[i + 1];
                    waterNew[i] = waterNew[i + 1];
                    waterUsed[i] = waterUsed[i + 1];
                    numPeople[i] = numPeople[i + 1];
                    totalCosts[i] = totalCosts[i + 1];
                }
                count--;
                Display();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dgvBills.CurrentRow == null) return;
            int index = dgvBills.CurrentCell.RowIndex;

            try
            {
                customerNames[index] = txtName.Text.Trim();
                customerTypes[index] = cbCustomerType.SelectedItem?.ToString() ?? "Unknown";
                waterOld[index] = int.Parse(txtOldMeter.Text);
                waterNew[index] = int.Parse(txtNewMeter.Text);
                waterUsed[index] = waterNew[index] - waterOld[index];
                numPeople[index] = string.IsNullOrEmpty(txtNumPeople.Text) ? 0 : int.Parse(txtNumPeople.Text);
                totalCosts[index] = CalculateBill(customerTypes[index], waterUsed[index], numPeople[index]);

                Display();
                ClearInputs();
                MessageBox.Show("Customer information updated.", "Notification");
            }
            catch
            {
                MessageBox.Show("Invalid input data.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSortASC_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < count - 1; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    if (totalCosts[i] > totalCosts[j]) Swap(i, j);
                }
            }
            Display();
        }

        private void btnSortDESC_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < count - 1; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    if (totalCosts[i] < totalCosts[j]) Swap(i, j);
                }
            }
            Display();
        }

        private void btnExportInvoice_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text) || cbCustomerType.SelectedIndex == -1)
            {
                MessageBox.Show("Please fill in all required fields before exporting.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtOldMeter.Text, out int oldMeter) || !int.TryParse(txtNewMeter.Text, out int newMeter))
            {
                MessageBox.Show("Invalid meter readings.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int consumption = newMeter - oldMeter;
            if (consumption < 0)
            {
                MessageBox.Show("New meter must be >= old meter.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string customerType = cbCustomerType.SelectedItem.ToString();
            int people = string.IsNullOrEmpty(txtNumPeople.Text) ? 0 : int.Parse(txtNumPeople.Text);
            double totalPrice = CalculateBill(customerType, consumption, people);

            string invoice = $"INVOICE\r\n" +
                             $"Customer Name: {txtName.Text.Trim()}\r\n" +
                             $"Customer Type: {customerType}\r\n" +
                             $"Water Meter Last Month: {oldMeter}\r\n" +
                             $"Water Meter This Month: {newMeter}\r\n" +
                             $"Water Used: {consumption} m³\r\n" +
                             $"Total Price: {totalPrice:N0} VND\r\n" +
                             $"Number of People: {people}\r\n" +
                             $"Date: {DateTime.Now:dd/MM/yyyy}";

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt",
                FileName = $"Invoice_{txtName.Text.Trim()}.txt"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                File.WriteAllText(sfd.FileName, invoice);
                MessageBox.Show("Invoice exported successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private double CalculateBill(string customerType, int consumption, int people)
        {
            double total = 0;
            if (customerType == "Household")
            {
                int p = Math.Max(1, people);
                int tierVolume = 10 * p;
                int remaining = consumption;

                int t1 = Math.Min(tierVolume, remaining);
                total += t1 * 5973; remaining -= t1;

                int t2 = Math.Min(tierVolume, remaining);
                total += t2 * 7052; remaining -= t2;

                int t3 = Math.Min(tierVolume, remaining);
                total += t3 * 8699; remaining -= t3;

                if (remaining > 0) total += remaining * 15929;
            }
            else
            {
                switch (customerType)
                {
                    case "Government": total = consumption * 9955; break;
                    case "Production": total = consumption * 11615; break;
                    case "Business": total = consumption * 22068; break;
                    default: total = consumption * 10000; break;
                }
            }
            return total + total * 0.10; // VAT 10%
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.ToLower();
            dgvBills.Rows.Clear();

            for (int i = 0; i < count; i++)
            {
                if (customerNames[i].ToLower().Contains(keyword))
                {
                    dgvBills.Rows.Add(i + 1, customerNames[i], customerTypes[i], waterUsed[i], numPeople[i], totalCosts[i]);
                }
            }
        }

        private void cbCustomerType_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtNumPeople.Enabled = cbCustomerType.SelectedItem?.ToString() == "Household";
            if (!txtNumPeople.Enabled) txtNumPeople.Clear();
        }

        private void dgvBills_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int index = e.RowIndex;
            if (index >= 0 && index < count)
            {
                txtName.Text = customerNames[index];
                cbCustomerType.SelectedItem = customerTypes[index];
                txtOldMeter.Text = waterOld[index].ToString();
                txtNewMeter.Text = waterNew[index].ToString();
                txtNumPeople.Text = numPeople[index].ToString();
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ClearInputs();
            Display();
        }

        private void Display()
        {
            dgvBills.Rows.Clear();
            for (int i = 0; i < count; i++)
            {
                dgvBills.Rows.Add(i + 1, customerNames[i], customerTypes[i], waterUsed[i], numPeople[i], totalCosts[i]);
            }
        }

        private void ClearInputs()
        {
            txtName.Clear();
            txtOldMeter.Clear();
            txtNewMeter.Clear();
            txtNumPeople.Clear();
            txtSearch.Clear();
            cbCustomerType.SelectedIndex = -1;
            dgvBills.ClearSelection();
        }

        private void Swap(int i, int j)
        {
            (customerNames[i], customerNames[j]) = (customerNames[j], customerNames[i]);
            (customerTypes[i], customerTypes[j]) = (customerTypes[j], customerTypes[i]);
            (waterOld[i], waterOld[j]) = (waterOld[j], waterOld[i]);
            (waterNew[i], waterNew[j]) = (waterNew[j], waterNew[i]);
            (waterUsed[i], waterUsed[j]) = (waterUsed[j], waterUsed[i]);
            (numPeople[i], numPeople[j]) = (numPeople[j], numPeople[i]);
            (totalCosts[i], totalCosts[j]) = (totalCosts[j], totalCosts[i]);
        }
    }
}
