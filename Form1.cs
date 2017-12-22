using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;

/*
 * ������ : ����HongSi
 * Email �ּ� : hong3738@naver.com
 * ��α� �ּ� : http://blog.naver.com/hong3738
 */


namespace DataGridViewPrintSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        // ������ ����
        private void btnCreateData_Click(object sender, EventArgs e)
        {
            int Index = 0;
            for (int i = 0; i < 100; i++)
            {
                List<object> Sampelitems = new List<object>();

                foreach (DataGridViewColumn DataCol in dataGridView1.Columns)
                    Sampelitems.Add(DataCol.Name + Index);

                dataGridView1.Rows.Add(Sampelitems.ToArray());

                Index++;
            }
        }
        
        // �μ� �̸�����
        private void btnPrintPreview_Click(object sender, EventArgs e)
        {
            printPreviewDialog1.Document = printDocument1;
            printPreviewDialog1.ShowDialog();
        }

        // �μ� ����
        private void btnPrintSetting_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.PageSetupDialog pageSetupDialog = new PageSetupDialog();

            if (pageSetupDialog.Document == null)
                pageSetupDialog.Document = printDocument1;


            pageSetupDialog.ShowDialog();
            printDocument1 = pageSetupDialog.Document;
        }

        // �μ�
        private void btnPrint_Click(object sender, EventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = printDocument1;
            printDialog.UseEXDialog = true;
            
            if (DialogResult.OK == printDialog.ShowDialog())
            {
                printDocument1.DocumentName = "SamplePrint";
                printDocument1.Print();
            }
        }


        // �μ⸦ �ϱ� ���� ����
        StringFormat TextFormat; // ���� �ؽ�Ʈ ����
        int TotalWidth = 0; // ��ü ����
        int CellHeight = 0; // ���� ����
        int CurRowPos = 0; // ���� ���� ��ġ
        bool HedaerPage = false; // �Ӹ��� üũ
        bool NewPage = false;// ���ο� ������ üũ
        int HeaderHeight = 0; // ���� ����

        List<int> ColumnLeftDatas = new List<int>(); // ���� ���� ������
        List<int> ColumnWidthDatas = new List<int>();// ���� ����� WIdth ����


        // �μ� ���� ��
        private void printDocument1_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            // ������ �ʱ�ȭ
            CellHeight = 0;
            CurRowPos = 0;
            HedaerPage = true;
            NewPage = true;

            // ����
            ColumnLeftDatas = new List<int>();
            ColumnWidthDatas = new List<int>();

            // �ؽ�Ʈ
            TextFormat = new StringFormat();
            TextFormat.Alignment = StringAlignment.Center;
            TextFormat.LineAlignment = StringAlignment.Center;


            // ���������� �׷��ֱ� ���� �� ���
            TotalWidth = 0;

            foreach (DataGridViewColumn dggr in dataGridView1.Columns)
                TotalWidth += dggr.Width;
        }

        // �μ� �� Paint
        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
                // ���� ����
                int LeftMargin = e.MarginBounds.Left;

                // ���� ����
                int TopMargin = e.MarginBounds.Top;

                // ������������ �ִ��� üũ
                bool isNexPage = false;

                // �ӽ� ���� ���̰�
                int TempWidth = 0;             
                
                // ���� �ʺ� �� ���� ���
                if (HedaerPage)
                {
                    foreach (DataGridViewColumn Grcol in dataGridView1.Columns)
                    {
                        TempWidth = (int)(Math.Floor((double)((double)Grcol.Width / (double)TotalWidth * (double)TotalWidth *
                                       ((double)e.MarginBounds.Width / (double)TotalWidth))));

                        HeaderHeight = (int)(e.Graphics.MeasureString(Grcol.HeaderText,
                                    Grcol.InheritedStyle.Font, TempWidth).Height) + 11;

                        // ���� �� ��� ��ġ ���
                        ColumnLeftDatas.Add(LeftMargin);
                        ColumnWidthDatas.Add(TempWidth);
                        LeftMargin += TempWidth;
                    }
                }
                
                // ���� ���� ������ŭ �׸���.
                while (CurRowPos <= dataGridView1.Rows.Count - 1)
                {
                    DataGridViewRow RowData = dataGridView1.Rows[CurRowPos];
                    
                    // ���� ���� ����
                    CellHeight = RowData.Height + 5;

                    int ColCount = 0;

                    if (TopMargin + CellHeight >= e.MarginBounds.Height + e.MarginBounds.Top)
                    {
                        // ������ �������� ������ ���� �ʰ��� ���� �������� �̵�
                        NewPage = true;
                        isNexPage = true;
                        HedaerPage = false;
                        break;
                    }
                    else
                    {
                        if (NewPage)
                        {
                            string CustomeSummary = "�Ӹ��� �׽�Ʈ";

                            // �Ӹ���
                            SizeF HeaderTextSize = e.Graphics.MeasureString(CustomeSummary,
                                new Font(dataGridView1.Font, FontStyle.Bold), e.MarginBounds.Width);

                            e.Graphics.DrawString(CustomeSummary, new Font(dataGridView1.Font, FontStyle.Bold),
                                    Brushes.Red, e.MarginBounds.Left, e.MarginBounds.Top -
                                    HeaderTextSize.Height - 10);

                            // ��¥
                            string HeaderDate = DateTime.Now.ToString();

                            SizeF HeaderDateSize = e.Graphics.MeasureString(HeaderDate, new Font(dataGridView1.Font, FontStyle.Bold), e.MarginBounds.Width);

                            e.Graphics.DrawString(HeaderDate, new Font(dataGridView1.Font, FontStyle.Bold),
                                    Brushes.Red, e.MarginBounds.Left + (e.MarginBounds.Width -
                                    HeaderDateSize.Width), e.MarginBounds.Top -
                                    HeaderTextSize.Height - 10);

                            // �� �׸���
                            TopMargin = e.MarginBounds.Top;
                            foreach (DataGridViewColumn GridCol in dataGridView1.Columns)
                            {
                                // ���� ����
                                Rectangle Drawrect = new Rectangle(ColumnLeftDatas[ColCount], TopMargin, ColumnWidthDatas[ColCount], HeaderHeight);

                                // ���
                                e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), Drawrect);

                                // �׵θ�
                                e.Graphics.DrawRectangle(Pens.Black, Drawrect);

                                // ������
                                e.Graphics.DrawString(GridCol.HeaderText, GridCol.InheritedStyle.Font, new SolidBrush(GridCol.InheritedStyle.ForeColor),  Drawrect, TextFormat);

                            ColCount++;
                            }
                            NewPage = false;
                            TopMargin += HeaderHeight;
                        }

                        ColCount = 0;
                        
                        // ���� �ش� �ϴ� ���� �׸���
                        foreach (DataGridViewCell CellData in RowData.Cells)
                        {
                            // ���� ����
                            Rectangle DrawRect = new Rectangle(ColumnLeftDatas[ColCount], TopMargin,
                                                                ColumnWidthDatas[ColCount], CellHeight);

                            // ���� �׵θ� �׸���
                            e.Graphics.DrawRectangle(Pens.Black, DrawRect);

                            // �� ��ġ
                            ColCount++;

                            if (CellData.Value == null)
                                    continue;

                                // ������ �׸���           
                                e.Graphics.DrawString(CellData.Value.ToString(), CellData.InheritedStyle.Font,
                                            new SolidBrush(CellData.InheritedStyle.ForeColor),
                                            DrawRect, TextFormat);

                        }
                    }

                    TopMargin += CellHeight;

                    CurRowPos++;
                }                

                // ���� ������ �ִ��� ���� �Ǵ�.
                if (isNexPage)
                    e.HasMorePages = true;
                else
                    e.HasMorePages = false;
        }

      
    }    
}