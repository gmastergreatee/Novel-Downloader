namespace Novel_Downloader
{
    partial class LibraryUserControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tblNovelList = new System.Windows.Forms.TableLayoutPanel();
            this.pLoading = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.pEmptyLibrary = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.btnAddNovel = new System.Windows.Forms.Button();
            this.pLoading.SuspendLayout();
            this.pEmptyLibrary.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tblNovelList
            // 
            this.tblNovelList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tblNovelList.AutoSize = true;
            this.tblNovelList.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tblNovelList.ColumnCount = 2;
            this.tblNovelList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblNovelList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblNovelList.Location = new System.Drawing.Point(6, 3);
            this.tblNovelList.Name = "tblNovelList";
            this.tblNovelList.RowCount = 2;
            this.tblNovelList.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblNovelList.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tblNovelList.Size = new System.Drawing.Size(744, 0);
            this.tblNovelList.TabIndex = 0;
            // 
            // pLoading
            // 
            this.pLoading.Controls.Add(this.label1);
            this.pLoading.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pLoading.Location = new System.Drawing.Point(6, 3);
            this.pLoading.Name = "pLoading";
            this.pLoading.Size = new System.Drawing.Size(745, 408);
            this.pLoading.TabIndex = 0;
            this.pLoading.Visible = false;
            // 
            // label1
            // 
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(745, 408);
            this.label1.TabIndex = 0;
            this.label1.Text = "Loading novels, please wait...";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // pEmptyLibrary
            // 
            this.pEmptyLibrary.Controls.Add(this.label2);
            this.pEmptyLibrary.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pEmptyLibrary.Location = new System.Drawing.Point(6, 3);
            this.pEmptyLibrary.Name = "pEmptyLibrary";
            this.pEmptyLibrary.Size = new System.Drawing.Size(745, 408);
            this.pEmptyLibrary.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(0, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(745, 408);
            this.label2.TabIndex = 0;
            this.label2.Text = "No novels added in the Library";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(763, 453);
            this.tableLayoutPanel1.TabIndex = 1;
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.Controls.Add(this.tblNovelList);
            this.panel1.Controls.Add(this.pLoading);
            this.panel1.Controls.Add(this.pEmptyLibrary);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(6, 3, 6, 3);
            this.panel1.Size = new System.Drawing.Size(757, 414);
            this.panel1.TabIndex = 0;
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.btnAddNovel, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(0, 420);
            this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(763, 33);
            this.tableLayoutPanel2.TabIndex = 1;
            // 
            // btnAddNovel
            // 
            this.btnAddNovel.AutoSize = true;
            this.btnAddNovel.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnAddNovel.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAddNovel.Location = new System.Drawing.Point(625, 3);
            this.btnAddNovel.Name = "btnAddNovel";
            this.btnAddNovel.Size = new System.Drawing.Size(135, 27);
            this.btnAddNovel.TabIndex = 0;
            this.btnAddNovel.Text = "Add Existing Novel";
            this.btnAddNovel.UseVisualStyleBackColor = true;
            this.btnAddNovel.Click += new System.EventHandler(this.btnAddNovel_Click);
            // 
            // LibraryUserControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "LibraryUserControl";
            this.Size = new System.Drawing.Size(763, 453);
            this.Load += new System.EventHandler(this.LibraryUserControl_Load);
            this.pLoading.ResumeLayout(false);
            this.pEmptyLibrary.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tblNovelList;
        private System.Windows.Forms.Panel pLoading;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel pEmptyLibrary;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Button btnAddNovel;
    }
}
