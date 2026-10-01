using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace QuanLyThietBi
{
    public partial class Form1 : Form
    {
        private BindingList<Product> _originalList = new BindingList<Product>();
        private BindingSource _bindingSource = new BindingSource();
        private string _selectedImagePath = string.Empty;

        public Form1()
        {
            InitializeComponent();
            SetupDataBinding();
            SetupEvents();
        }

        private void SetupDataBinding()
        {
            var categories = new BindingList<CategoryItem>
            {
                new CategoryItem(1, "Điện thoại"),
                new CategoryItem(2, "Laptop"),
                new CategoryItem(3, "Phụ kiện")
            };
            cboCategory.DataSource = categories;
            cboCategory.DisplayMember = "Name";
            cboCategory.ValueMember = "Id";

            _bindingSource.DataSource = _originalList;
            dgvProducts.DataSource = _bindingSource;

            _originalList.Add(new Product { ProductId = "SP001", ProductName = "iPhone 15 Pro", CategoryId = 1, CategoryName = "Điện thoại", UnitPrice = 28000000, Quantity = 10 });
            _originalList.Add(new Product { ProductId = "SP002", ProductName = "Laptop Dell XPS 13", CategoryId = 2, CategoryName = "Laptop", UnitPrice = 25000000, Quantity = 5 });

            UpdateTotalStatus();
            GenerateNewProductId();
        }

        private void SetupEvents()
        {
            btnChooseImage.Click += BtnChooseImage_Click;
            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnClear.Click += (s, e) => ClearForm();

            dgvProducts.SelectionChanged += DgvProducts_SelectionChanged;

            txtSearch.TextChanged += TxtSearch_TextChanged;

            menuExportCsv.Click += (s, e) => ExportToCsv();
            menuExit.Click += (s, e) => Application.Exit();
        }

        private void BtnChooseImage_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                _selectedImagePath = openFileDialog.FileName;
                picAvatar.ImageLocation = _selectedImagePath;
            }
        }

        private bool ValidateForm()
        {
            errorProvider.Clear();
            bool isValid = true;

            if (string.IsNullOrWhiteSpace(txtProductName.Text))
            {
                errorProvider.SetError(txtProductName, "Tên sản phẩm không được để trống!");
                isValid = false;
            }

            if (!decimal.TryParse(txtUnitPrice.Text, out decimal price) || price <= 0)
            {
                errorProvider.SetError(txtUnitPrice, "Đơn giá phải là số lớn hơn 0!");
                isValid = false;
            }

            if (!int.TryParse(txtQuantity.Text, out int qty) || qty < 0)
            {
                errorProvider.SetError(txtQuantity, "Số lượng phải là số nguyên ≥ 0!");
                isValid = false;
            }

            return isValid;
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateForm()) return;

            var selectedCat = (CategoryItem)cboCategory.SelectedItem;

            var p = new Product
            {
                ProductId = txtProductId.Text,
                ProductName = txtProductName.Text.Trim(),
                CategoryId = selectedCat.Id,
                CategoryName = selectedCat.Name,
                UnitPrice = decimal.Parse(txtUnitPrice.Text),
                Quantity = int.Parse(txtQuantity.Text),
                ImagePath = _selectedImagePath
            };

            _originalList.Add(p);
            UpdateTotalStatus();
            MessageBox.Show("Thêm sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            ClearForm();
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;
            if (!ValidateForm()) return;

            var selectedProduct = (Product)dgvProducts.CurrentRow.DataBoundItem;
            var selectedCat = (CategoryItem)cboCategory.SelectedItem;

            selectedProduct.ProductName = txtProductName.Text.Trim();
            selectedProduct.CategoryId = selectedCat.Id;
            selectedProduct.CategoryName = selectedCat.Name;
            selectedProduct.UnitPrice = decimal.Parse(txtUnitPrice.Text);
            selectedProduct.Quantity = int.Parse(txtQuantity.Text);
            if (!string.IsNullOrEmpty(_selectedImagePath))
            {
                selectedProduct.ImagePath = _selectedImagePath;
            }

            _bindingSource.ResetBindings(false);
            MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow == null) return;

            var selectedProduct = (Product)dgvProducts.CurrentRow.DataBoundItem;
            
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa sản phẩm [{selectedProduct.ProductName}]?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                _originalList.Remove(selectedProduct);
                UpdateTotalStatus();
                ClearForm();
                MessageBox.Show("Đã xóa sản phẩm thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void DgvProducts_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvProducts.CurrentRow != null && dgvProducts.CurrentRow.DataBoundItem is Product p)
            {
                txtProductId.Text = p.ProductId;
                txtProductName.Text = p.ProductName;
                cboCategory.SelectedValue = p.CategoryId;
                txtUnitPrice.Text = p.UnitPrice.ToString("0");
                txtQuantity.Text = p.Quantity.ToString();

                _selectedImagePath = p.ImagePath;
                if (!string.IsNullOrEmpty(p.ImagePath) && File.Exists(p.ImagePath))
                {
                    picAvatar.ImageLocation = p.ImagePath;
                }
                else
                {
                    picAvatar.Image = null;
                }
            }
        }

        private void TxtSearch_TextChanged(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim().ToLower();
            if (string.IsNullOrEmpty(keyword))
            {
                _bindingSource.DataSource = _originalList;
            }
            else
            {
                var filtered = _originalList.Where(p => p.ProductName.ToLower().Contains(keyword)).ToList();
                _bindingSource.DataSource = new BindingList<Product>(filtered);
            }
        }

        private void ExportToCsv()
        {
            using (SaveFileDialog sfd = new SaveFileDialog() { Filter = "CSV Files (*.csv)|*.csv", FileName = "ProductList.csv" })
            {
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        var sb = new StringBuilder();
                        sb.AppendLine("Mã SP,Tên SP,Danh Mục,Đơn Giá,Số Lượng");

                        foreach (var p in _originalList)
                        {
                            sb.AppendLine($"\"{p.ProductId}\",\"{p.ProductName}\",\"{p.CategoryName}\",{p.UnitPrice},{p.Quantity}");
                        }

                        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                        MessageBox.Show("Xuất file CSV thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void ClearForm()
        {
            errorProvider.Clear();
            GenerateNewProductId();
            txtProductName.Clear();
            txtUnitPrice.Clear();
            txtQuantity.Clear();
            cboCategory.SelectedIndex = 0;
            picAvatar.Image = null;
            _selectedImagePath = string.Empty;
        }

        private void GenerateNewProductId()
        {
            int maxId = 0;
            foreach (var item in _originalList)
            {
                if (item.ProductId.StartsWith("SP") && int.TryParse(item.ProductId.Substring(2), out int id))
                {
                    if (id > maxId) maxId = id;
                }
            }
            txtProductId.Text = $"SP{(maxId + 1):D3}";
        }

        private void UpdateTotalStatus()
        {
            lblStatusTotal.Text = $"Tổng số sản phẩm: {_originalList.Count}";
        }
    }
}