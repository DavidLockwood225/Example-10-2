using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using System.Drawing.Printing;

namespace UsingSQLServerDatabases_Project
{
    public partial class frmInventory : Form
    {
        SqlConnection inventoryConnection;
        SqlCommand inventoryCommand;
        SqlDataAdapter inventoryAdapter;
        DataTable inventoryTable;
        CurrencyManager inventoryManager;
        string myState;
        int myBookmark;
        int pageNumber;
        public frmInventory()
        {
            InitializeComponent();
        }

        private void frmInventory_Load(object sender, EventArgs e)
        {
            inventoryConnection = new SqlConnection("Server=(localdb)\\MSSQLLocalDB;"
                                                  + "AttachDbFilename=" + Path.GetFullPath("SQLInventoryDB.mdf")
                                                  + ";Integrated Security=True;"
                                                  + "Connect Timeout=30;");
            inventoryConnection.Open();
            inventoryCommand = new SqlCommand("SELECT * FROM Inventory ORDER BY Item", inventoryConnection);
            inventoryAdapter = new SqlDataAdapter();
            inventoryAdapter.SelectCommand = inventoryCommand;
            inventoryTable = new DataTable();
            inventoryAdapter.Fill(inventoryTable);
            txtItem.DataBindings.Add("Text", inventoryTable, "Item");
            txtLocation.DataBindings.Add("Text", inventoryTable, "_Location");
            txtStore.DataBindings.Add("Text", inventoryTable, "Store");
            dtpDatePurchased.DataBindings.Add("Text", inventoryTable, "DatePurchased");
            txtPurchaseCost.DataBindings.Add("Text", inventoryTable, "PurchaseCost");
            txtSerialNumber.DataBindings.Add("Text", inventoryTable, "SerialNumber");
            chkEngraved.DataBindings.Add("Checked", inventoryTable, "Engraved");
            lblPhotoFile.DataBindings.Add("Text", inventoryTable, "PhotoFile");
            inventoryManager = (CurrencyManager)this.BindingContext[inventoryTable];
            ShowPhoto();
            SetState("View");
        }

        private void frmInventory_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                SqlCommandBuilder inventoryAdapterCommands = new SqlCommandBuilder(inventoryAdapter);
                inventoryAdapter.Update(inventoryTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Saving Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            inventoryConnection.Close();
            inventoryCommand.Dispose();
            inventoryAdapter.Dispose();
            inventoryTable.Dispose();
        }
        private void ShowPhoto()
        {
            if (!lblPhotoFile.Text.Equals(""))
            {
                try
                {
                    picItem.Image = Image.FromFile(Path.GetFullPath(lblPhotoFile.Text));
                }
                catch (Exception ex)                
                {
                    MessageBox.Show(ex.Message, "Error Loading Photo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                picItem.Image = null;
            }
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            inventoryManager.Position = 0;
            ShowPhoto();
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            inventoryManager.Position--;
            ShowPhoto();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            inventoryManager.Position++;
            ShowPhoto();
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            inventoryManager.Position = inventoryManager.Count - 1;
            ShowPhoto();
        }
        private void SetState (string appState)
        {
            myState = appState;
            switch (myState)
            {
                case "View":
                    btnFirst.Enabled = true;
                    btnPrevious.Enabled = true;
                    btnNext.Enabled = true;
                    btnLast.Enabled = true;
                    btnEdit.Enabled = true;
                    btnSave.Enabled = false;
                    btnCancel.Enabled = false;
                    btnAdd.Enabled = true;
                    btnDelete.Enabled = true;
                    btnPrint.Enabled = true;
                    txtItem.ReadOnly = true;
                    txtLocation.ReadOnly = true;
                    txtStore.ReadOnly = true;
                    dtpDatePurchased.Enabled = false;
                    txtPurchaseCost.ReadOnly = true;
                    txtSerialNumber.ReadOnly = true;
                    chkEngraved.Enabled = false;
                    btnLoadPhoto.Enabled = false;
                    break;
                default:
                    btnFirst.Enabled = false;
                    btnPrevious.Enabled = false;
                    btnNext.Enabled = false;
                    btnLast.Enabled = false;
                    btnEdit.Enabled = false;
                    btnSave.Enabled = true;
                    btnCancel.Enabled = true;
                    btnAdd.Enabled = false;
                    btnDelete.Enabled = false;
                    btnPrint.Enabled = false;
                    txtItem.ReadOnly = false;
                    txtLocation.ReadOnly = false;
                    txtStore.ReadOnly = false;
                    dtpDatePurchased.Enabled = true;
                    txtPurchaseCost.ReadOnly = false;
                    txtSerialNumber.ReadOnly = false;
                    chkEngraved.Enabled = true;
                    btnLoadPhoto.Enabled = true;
                    break;
            }
            txtItem.Focus();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            SetState("Edit");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if(txtItem.Text.Trim().Equals(""))
            {
                MessageBox.Show("You must enter an Item description.", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtItem.Focus();
                return;
            }
            string savedItem = txtItem.Text;
            int savedRow;
            inventoryManager.EndCurrentEdit();
            if (myState.Equals("Add"))
            {
                inventoryTable.Rows[inventoryManager.Count - 1]["Engraved"] = chkEngraved.Checked;
                chkEngraved.DataBindings.Add("Checked", inventoryTable, "Engraved");
            }
            inventoryTable.DefaultView.Sort = "Item";
            savedRow = inventoryTable.DefaultView.Find(savedItem);
            inventoryManager.Position = savedRow;
            SetState("View");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            inventoryManager.CancelCurrentEdit();
            if (myState.Equals("Add"))
            {
                inventoryManager.Position = myBookmark;
                chkEngraved.DataBindings.Add("Checked", inventoryTable, "Engraved");
            }
            ShowPhoto();
            SetState("View");
        }

        private void btnLoadPhoto_Click(object sender, EventArgs e)
        {
            try
            {
                if (dlgOpen.ShowDialog() == DialogResult.OK)
                {
                    lblPhotoFile.Text = dlgOpen.FileName;
                    ShowPhoto();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Opening Photo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            myBookmark = inventoryManager.Position;
            picItem.Image = null;
            chkEngraved.DataBindings.Clear();
            chkEngraved.Checked = false;
            SetState("Add");
            inventoryManager.AddNew();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this record?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                inventoryManager.RemoveAt(inventoryManager.Position);
                ShowPhoto();
            }
            SetState("View");
        }

        private void txtItem_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
                txtLocation.Focus();
        }

        private void txtLocation_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
                txtStore.Focus();
        }

        private void txtStore_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
            {
                if (dtpDatePurchased.Enabled)
                    dtpDatePurchased.Focus();
                else
                    txtPurchaseCost.Focus();
            }
        }

        private void dtpDatePurchased_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
                txtPurchaseCost.Focus();
        }

        private void txtPurchaseCost_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar >= '0' && e.KeyChar <= '9') || (int)e.KeyChar == 8)
                e.Handled = false;
            else if ((int)e.KeyChar == 13)
            {
                txtSerialNumber.Focus();
                e.Handled = false;
            }
            else if (e.KeyChar == '.')
            {
                if (txtPurchaseCost.Text.IndexOf(".") == -1)
                    e.Handled = false;
                else
                    e.Handled = true;
            }
            else
            {
                e.Handled = true;
            }
        }

        private void txtSerialNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((int)e.KeyChar == 13)
            {
                if (btnLoadPhoto.Enabled)
                    btnLoadPhoto.Focus();
                else
                    txtItem.Focus();
            }
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            PrintDocument inventoryDocument;
            inventoryDocument = new PrintDocument();
            inventoryDocument.DocumentName = "Home Inventory";
            inventoryDocument.PrintPage += new PrintPageEventHandler(this.PrintInventory);
            pageNumber = 1;
            int savedPosition = inventoryManager.Position;
            dlgPreview.Document = inventoryDocument;
            dlgPreview.ShowDialog();
            inventoryDocument.Dispose();
            inventoryManager.Position = savedPosition;
            ShowPhoto();
        }
        private void PrintInventory(object sender, PrintPageEventArgs e)
        {
            inventoryManager.Position = pageNumber - 1;
            ShowPhoto();
            Font myFont = new Font("Arial", 14, FontStyle.Bold);
            int y = e.MarginBounds.Top + 50;
            e.Graphics.DrawString("Home Inventory (" + DateTime.Now.ToShortDateString() + ") - Page " + pageNumber.ToString(), myFont, Brushes.Black, e.MarginBounds.Left, y);
            y += 2 * Convert.ToInt32(myFont.GetHeight(e.Graphics));
            myFont = new Font("Arial", 12, FontStyle.Regular);
            e.Graphics.DrawString("Item:", myFont, Brushes.Black, e.MarginBounds.Left, y);
            e.Graphics.DrawString(txtItem.Text, myFont, Brushes.Black, e.MarginBounds.X + 150, y);
            y += Convert.ToInt32(myFont.GetHeight(e.Graphics));
            e.Graphics.DrawString("Location:", myFont, Brushes.Black, e.MarginBounds.X, y);
            e.Graphics.DrawString(txtLocation.Text, myFont, Brushes.Black, e.MarginBounds.X + 150, y);
            y += Convert.ToInt32(myFont.GetHeight(e.Graphics));
            e.Graphics.DrawString("Store:", myFont, Brushes.Black, e.MarginBounds.X, y);
            e.Graphics.DrawString(txtStore.Text, myFont, Brushes.Black, e.MarginBounds.X + 150, y);
            y += Convert.ToInt32(myFont)
        }
    }
}
