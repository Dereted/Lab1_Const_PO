using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Lab__1_KonsPO
{
    public partial class Form1 : Form
    {
        private readonly string connectionString =
            ConfigurationManager.ConnectionStrings["KPBD"].ConnectionString;

        private SqlDataAdapter adapterMaterial;
        private SqlDataAdapter adapterPhone;
        private SqlCommandBuilder builderMaterial;
        private SqlCommandBuilder builderPhone;
        private DataSet dataSet = new DataSet();

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) { LoadData(); }

        private void LoadData()
        {
            adapterMaterial = new SqlDataAdapter("SELECT Id, Name FROM Material", connectionString);
            adapterPhone = new SqlDataAdapter("SELECT Id, Name, Cost, ManufacturerId FROM Phone", connectionString);

            builderMaterial = new SqlCommandBuilder(adapterMaterial);
            builderPhone = new SqlCommandBuilder(adapterPhone);

            dataSet.Clear();
            adapterMaterial.Fill(dataSet, "material");
            adapterPhone.Fill(dataSet, "phone");

            bindingSource1.DataSource = dataSet.Tables["material"];
            dataGridView1.DataSource = bindingSource1;
            bindingNavigator1.BindingSource = bindingSource1;

            bindingSource2.DataSource = dataSet.Tables["phone"];
            dataGridView2.DataSource = bindingSource2;

            FillManufacturerComboBox();
        }

        private void FillManufacturerComboBox()
        {
            if (dataGridView2.Columns["ManufacturerId"] is DataGridViewComboBoxColumn col)
            {
                col.DataSource = dataSet.Tables["material"];
                col.DisplayMember = "Name";
                col.ValueMember = "Id";
                col.DataPropertyName = "ManufacturerId";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            dataGridView3.DataSource = null;
            dataGridView3.Columns.Clear();

            string sql = @"SELECT p.Name AS Phone, p.Cost, m.Name AS Manufacturer
                           FROM Phone p
                           JOIN Material m ON p.ManufacturerId = m.Id
                           WHERE p.Cost > @minCost
                           ORDER BY p.Cost DESC";

            using (SqlConnection conn = new SqlConnection(connectionString))
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@minCost", numericUpDown1.Value);

                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                        dataGridView3.Columns.Add(reader.GetName(i), reader.GetName(i));

                    while (reader.Read())
                    {
                        object[] row = new object[reader.FieldCount];
                        reader.GetValues(row);
                        dataGridView3.Rows.Add(row);
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter adapter = new SqlDataAdapter("Orders_getByMonth", conn);
                adapter.SelectCommand.CommandType = CommandType.StoredProcedure;
                adapter.SelectCommand.Parameters.Add("@month", SqlDbType.Int).Value =
                    (int)numericUpDown2.Value;

                DataSet ds = new DataSet();
                adapter.Fill(ds, "report2");
                dataGridView4.DataSource = ds.Tables["report2"];
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            bindingSource1.EndEdit();
            bindingSource2.EndEdit();

            adapterMaterial.Update(dataSet, "material");   
            adapterPhone.Update(dataSet, "phone");
        }
    }
}