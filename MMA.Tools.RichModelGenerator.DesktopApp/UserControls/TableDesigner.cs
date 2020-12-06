using System.Collections.Generic;
using System.Windows.Forms;

namespace MMA.Tools.RichModelGenerator.DesktopApp
{
    public class TableDesigner : DataGridView
    {
        public DataGridViewTextBoxColumn ColumnName { get; set; }
        public DataGridViewComboBoxColumn DataType { get; set; }
        public DataGridViewCheckBoxColumn IsNullable { get; set; }
        
        public TableDesigner(string name= "table1",int x= 31, int y= 35)
        {
            // 
            // ColumnName
            // 
            ColumnName = new DataGridViewTextBoxColumn
            {
                HeaderText = "Column Name",
                Name = "ColumnName",
                Width=145
            };
            // 
            // DataType
            // 
            DataType = new DataGridViewComboBoxColumn
            {
                HeaderText = "Data Type",
                Name = "DataType",
                Width=70
            };
            DataType.Items.AddRange(new string[] {
            "int",
            "long",
            "bool",
            "string",
            "DateTime"});

            // 
            // ColumnName
            // 
            IsNullable = new DataGridViewCheckBoxColumn
            {
                HeaderText = "Nullable",
                Name = "IsNullable",
                Width=48
            };

            

            AllowUserToOrderColumns = true;
            //AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            ColumnName,
            DataType,
            IsNullable});
            EditMode = DataGridViewEditMode.EditOnEnter;
            Location = new System.Drawing.Point(x, y);
            Name = name;
            RowHeadersWidth = 25;
            RowTemplate.Height = 25;
            SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            Size = new System.Drawing.Size(290, 200);
            TabIndex = 0;


        }
    }
}
