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
 * 만든이 : 떫은HongSi
 * Email 주소 : hong3738@naver.com
 * 블로그 주소 : http://blog.naver.com/hong3738
 */


namespace DataGridViewPrintSample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        // 데이터 생성
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
        
        // 인쇄 미리보기
        private void btnPrintPreview_Click(object sender, EventArgs e)
        {
            printPreviewDialog1.Document = printDocument1;
            printPreviewDialog1.ShowDialog();
        }

        // 인쇄 설정
        private void btnPrintSetting_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.PageSetupDialog pageSetupDialog = new PageSetupDialog();

            if (pageSetupDialog.Document == null)
                pageSetupDialog.Document = printDocument1;


            pageSetupDialog.ShowDialog();
            printDocument1 = pageSetupDialog.Document;
        }

        // 인쇄
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


        // 인쇄를 하기 위한 정보
        StringFormat TextFormat; // 행의 텍스트 포맷
        int TotalWidth = 0; // 전체 넓이
        int CellHeight = 0; // 행의 높이
        int CurRowPos = 0; // 현재 행의 위치
        bool HedaerPage = false; // 머리글 체크
        bool NewPage = false;// 새로운 페이지 체크
        int HeaderHeight = 0; // 열의 높이

        List<int> ColumnLeftDatas = new List<int>(); // 열의 왼쪽 정보들
        List<int> ColumnWidthDatas = new List<int>();// 실제 사용할 WIdth 정보


        // 인쇄 시작 전
        private void printDocument1_BeginPrint(object sender, System.Drawing.Printing.PrintEventArgs e)
        {
            // 데이터 초기화
            CellHeight = 0;
            CurRowPos = 0;
            HedaerPage = true;
            NewPage = true;

            // 정보
            ColumnLeftDatas = new List<int>();
            ColumnWidthDatas = new List<int>();

            // 텍스트
            TextFormat = new StringFormat();
            TextFormat.Alignment = StringAlignment.Center;
            TextFormat.LineAlignment = StringAlignment.Center;


            // 비율적으로 그려주기 위한 값 계산
            TotalWidth = 0;

            foreach (DataGridViewColumn dggr in dataGridView1.Columns)
                TotalWidth += dggr.Width;
        }

        // 인쇄 할 Paint
        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
                // 왼쪽 마진
                int LeftMargin = e.MarginBounds.Left;

                // 위쪽 마진
                int TopMargin = e.MarginBounds.Top;

                // 다음페이지가 있는지 체크
                bool isNexPage = false;

                // 임시 저장 넓이값
                int TempWidth = 0;             
                
                // 열의 너비 및 높이 계산
                if (HedaerPage)
                {
                    foreach (DataGridViewColumn Grcol in dataGridView1.Columns)
                    {
                        TempWidth = (int)(Math.Floor((double)((double)Grcol.Width / (double)TotalWidth * (double)TotalWidth *
                                       ((double)e.MarginBounds.Width / (double)TotalWidth))));

                        HeaderHeight = (int)(e.Graphics.MeasureString(Grcol.HeaderText,
                                    Grcol.InheritedStyle.Font, TempWidth).Height) + 11;

                        // 높이 및 헤더 위치 계산
                        ColumnLeftDatas.Add(LeftMargin);
                        ColumnWidthDatas.Add(TempWidth);
                        LeftMargin += TempWidth;
                    }
                }
                
                // 현재 행의 개수만큼 그린다.
                while (CurRowPos <= dataGridView1.Rows.Count - 1)
                {
                    DataGridViewRow RowData = dataGridView1.Rows[CurRowPos];
                    
                    // 행의 높이 설정
                    CellHeight = RowData.Height + 5;

                    int ColCount = 0;

                    if (TopMargin + CellHeight >= e.MarginBounds.Height + e.MarginBounds.Top)
                    {
                        // 프린터 설정에서 설정한 값이 초과시 다음 페이지로 이동
                        NewPage = true;
                        isNexPage = true;
                        HedaerPage = false;
                        break;
                    }
                    else
                    {
                        if (NewPage)
                        {
                            string CustomeSummary = "머리말 테스트";

                            // 머리글
                            SizeF HeaderTextSize = e.Graphics.MeasureString(CustomeSummary,
                                new Font(dataGridView1.Font, FontStyle.Bold), e.MarginBounds.Width);

                            e.Graphics.DrawString(CustomeSummary, new Font(dataGridView1.Font, FontStyle.Bold),
                                    Brushes.Red, e.MarginBounds.Left, e.MarginBounds.Top -
                                    HeaderTextSize.Height - 10);

                            // 날짜
                            string HeaderDate = DateTime.Now.ToString();

                            SizeF HeaderDateSize = e.Graphics.MeasureString(HeaderDate, new Font(dataGridView1.Font, FontStyle.Bold), e.MarginBounds.Width);

                            e.Graphics.DrawString(HeaderDate, new Font(dataGridView1.Font, FontStyle.Bold),
                                    Brushes.Red, e.MarginBounds.Left + (e.MarginBounds.Width -
                                    HeaderDateSize.Width), e.MarginBounds.Top -
                                    HeaderTextSize.Height - 10);

                            // 열 그리기
                            TopMargin = e.MarginBounds.Top;
                            foreach (DataGridViewColumn GridCol in dataGridView1.Columns)
                            {
                                // 영역 정보
                                Rectangle Drawrect = new Rectangle(ColumnLeftDatas[ColCount], TopMargin, ColumnWidthDatas[ColCount], HeaderHeight);

                                // 배경
                                e.Graphics.FillRectangle(new SolidBrush(Color.LightGray), Drawrect);

                                // 테두리
                                e.Graphics.DrawRectangle(Pens.Black, Drawrect);

                                // 데이터
                                e.Graphics.DrawString(GridCol.HeaderText, GridCol.InheritedStyle.Font, new SolidBrush(GridCol.InheritedStyle.ForeColor),  Drawrect, TextFormat);

                            ColCount++;
                            }
                            NewPage = false;
                            TopMargin += HeaderHeight;
                        }

                        ColCount = 0;
                        
                        // 열에 해당 하는 정보 그리기
                        foreach (DataGridViewCell CellData in RowData.Cells)
                        {
                            // 영역 정보
                            Rectangle DrawRect = new Rectangle(ColumnLeftDatas[ColCount], TopMargin,
                                                                ColumnWidthDatas[ColCount], CellHeight);

                            // 행의 테두리 그리기
                            e.Graphics.DrawRectangle(Pens.Black, DrawRect);

                            // 행 위치
                            ColCount++;

                            if (CellData.Value == null)
                                    continue;

                                // 데이터 그리기           
                                e.Graphics.DrawString(CellData.Value.ToString(), CellData.InheritedStyle.Font,
                                            new SolidBrush(CellData.InheritedStyle.ForeColor),
                                            DrawRect, TextFormat);

                        }
                    }

                    TopMargin += CellHeight;

                    CurRowPos++;
                }                

                // 다음 페이지 있는지 여부 판단.
                if (isNexPage)
                    e.HasMorePages = true;
                else
                    e.HasMorePages = false;
        }

      
    }    
}