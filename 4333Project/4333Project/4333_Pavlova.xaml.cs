using Microsoft.Office.Interop.Word;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;


namespace _4333Project
{
    using Excel = Microsoft.Office.Interop.Excel;
    using Word = Microsoft.Office.Interop.Word;

    public partial class _4333_Pavlova : System.Windows.Window
    {
        public _4333_Pavlova()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog()
            {
                DefaultExt = "*.xls;*.xlsx",
                Filter = "файл Excel (Spisok.xlsx)|*.xlsx",
                Title = "Выберите файл базы данных"
            };
            if (!(ofd.ShowDialog() == true))
                return;
            string[,] list;

            Excel.Application ObjWorkExcel = new Excel.Application();

            Excel.Workbook ObjWorkBook = ObjWorkExcel.Workbooks.Open(ofd.FileName);

            Excel.Worksheet ObjWorkSheet = (Excel.Worksheet)ObjWorkBook.Sheets[1];

            var lastCell = ObjWorkSheet.Cells.SpecialCells(Excel.XlCellType.xlCellTypeLastCell);
            int _columns = (int)lastCell.Column;
            int _rows = (int)lastCell.Row;
            list = new string[_rows, _columns];
            for (int j = 0; j < _columns; j++)
            {
                for (int i = 0; i < _rows; i++)
                {
                    list[i, j] = ObjWorkSheet.Cells[i + 1, j + 1].Text;

                }
            }
            ObjWorkBook.Close(false, Type.Missing, Type.Missing);
            ObjWorkExcel.Quit();
            GC.Collect();

            using (isrpo2_3Entities iSRPOEntities = new isrpo2_3Entities())
            {
                for (int i = 1; i < _rows; i++)
                {
                    iSRPOEntities.Worker.Add(new Worker()
                    {
                        CodClienta = list[i, 0],
                        Doljnost = list[i, 1],
                        FIO = list[i, 2],
                        Loginn = list[i, 3],
                        Parol = list[i, 4],
                        PosledniVhod = list[i, 5],
                        TipVhoda = list[i, 6]
                    });

                }
                iSRPOEntities.SaveChanges();
            }

        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            List<Worker> Workers;
            using (isrpo2_3Entities usersEntities = new isrpo2_3Entities())
            {
                var Doljnoxt =
                usersEntities.Worker.Select(s => s.Doljnost).Distinct().ToList();

                Workers =
                usersEntities.Worker.ToList().OrderBy(s =>
                s.FIO).ToList();

                var app = new Excel.Application();
                app.SheetsInNewWorkbook = Doljnoxt.Count;
                Excel.Workbook wb = app.Workbooks.Add(Type.Missing);

                for (int i = 0; i < Doljnoxt.Count; i++)
                {
                    Excel.Worksheet worksheet;
                    if (i + 1 <= app.Worksheets.Count)
                    {
                        worksheet = app.Worksheets.Item[i + 1];
                    }
                    else
                    {
                        worksheet = app.Worksheets.Add();
                    }

                    worksheet.Name = Doljnoxt[i].ToString().Replace(":", "").Replace("\\", "").Replace("?", "").Replace("*", "").Replace("[", "").Replace("]", ""); // Убираем запрещенные символы

                    int rowindex = 1;
                    worksheet.Cells[rowindex, 1] = "Код сотрудника";
                    worksheet.Cells[rowindex, 2] = "ФИО";
                    worksheet.Cells[rowindex, 3] = "Логин";
                    worksheet.Cells[rowindex, 4] = "Пароль";
                    worksheet.Cells[rowindex, 5] = "Последний вход";
                    worksheet.Cells[rowindex, 6] = "Тип входа";

                    rowindex++;
                }

                var groupedWorkers = Workers.GroupBy(w => w.Doljnost).ToList();

                foreach (var group in groupedWorkers)
                {
                    string sheetName = group.Key;

                    Excel.Worksheet worksheet = null;
                    try
                    {
                        worksheet = app.Worksheets.Item[sheetName];
                    }
                    catch
                    {
                        worksheet = app.Worksheets.Add();
                        worksheet.Name = sheetName;

                        worksheet.Cells[1, 1] = "Код сотрудника";
                        worksheet.Cells[1, 2] = "ФИО";
                        worksheet.Cells[1, 3] = "Логин";
                        worksheet.Cells[1, 4] = "Пароль";
                        worksheet.Cells[1, 5] = "Последний вход";
                        worksheet.Cells[1, 6] = "Тип входа";
                    }

                    int rowIndex = 2;
                    foreach (Worker worker in Workers)
                    {
                        if (worker.Doljnost == group.Key)
                        {
                            worksheet.Cells[rowIndex, 1] = worker.CodClienta;
                            worksheet.Cells[rowIndex, 2] = worker.FIO;
                            worksheet.Cells[rowIndex, 3] = worker.Loginn;
                            worksheet.Cells[rowIndex, 4] = worker.Parol;
                            worksheet.Cells[rowIndex, 5] = worker.PosledniVhod;
                            worksheet.Cells[rowIndex, 6] = worker.TipVhoda;

                            rowIndex++;
                        }
                    }
                }
                app.Visible = true;
            }

        }

        private void ImportJson_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "JSON files (*.json)|*.json|All files (*.*)|*.*";

            using (FileStream fs = new FileStream("C:/Users/Aleksei/Downloads/4.json", FileMode.OpenOrCreate))
            {
                List<Worker> worker = JsonSerializer.Deserialize<List<Worker>>(fs);
                MessageBox.Show(worker[1].Doljnost);

                using (isrpo2_3Entities iSRPOEntities = new isrpo2_3Entities())
                {
                    for (int i = 1; i < worker.Count; i++)
                    {
                        iSRPOEntities.Worker.Add(new Worker()
                        {
                            CodClienta = worker[i].CodClienta,
                            Doljnost = worker[i].Doljnost,
                            FIO = worker[i].FIO,
                            Loginn = worker[i].Loginn,
                            Parol = worker[i].Parol,
                            PosledniVhod = worker[i].PosledniVhod,
                            TipVhoda = worker[i].TipVhoda
                        });

                    }
                    iSRPOEntities.SaveChanges();
                }
            }
        }

        private void ExportWord_Click(object sender, RoutedEventArgs e)
        {
            List<Worker> Workers;
            using (isrpo2_3Entities usersEntities = new isrpo2_3Entities())
            {
                var Doljnoxt =
                usersEntities.Worker.Select(s => s.Doljnost).Distinct().ToList();

                Workers =
                usersEntities.Worker.ToList().OrderBy(s =>
                s.FIO).ToList();

                var groupedWorkers = Workers.GroupBy(w => w.Doljnost).ToList();

                var app = new Word.Application();
                Word.Document document = app.Documents.Add();

                foreach (var worker in groupedWorkers)
                {
                    Word.Paragraph paragraph = document.Paragraphs.Add();
                    Word.Range range = paragraph.Range;

                    range.Text = Convert.ToString(Workers.Where(g => g.Doljnost == worker.Key).FirstOrDefault().Doljnost);
                    paragraph.set_Style("Заголовок 1");
                    range.InsertParagraphAfter();

                    Word.Paragraph tableParagraph = document.Paragraphs.Add();
                    Word.Range tableRange = tableParagraph.Range;
                    Word.Table workertable =
                    document.Tables.Add(tableRange, worker.Count() + 1, 7);
                    workertable.Borders.InsideLineStyle =
                    workertable.Borders.OutsideLineStyle =
                    Word.WdLineStyle.wdLineStyleSingle;
                    workertable.Range.Cells.VerticalAlignment =
                    Word.WdCellVerticalAlignment.wdCellAlignVerticalCenter;

                    Word.Range cellRange;
                    cellRange = workertable.Cell(1, 1).Range;
                    cellRange.Text = "Код";
                    cellRange = workertable.Cell(1, 2).Range;
                    cellRange.Text = "Должность";
                    cellRange = workertable.Cell(1, 3).Range;
                    cellRange.Text = "ФИО";
                    cellRange = workertable.Cell(1, 4).Range;
                    cellRange.Text = "Логин";
                    cellRange = workertable.Cell(1, 5).Range;
                    cellRange.Text = "Пароль";
                    cellRange = workertable.Cell(1, 6).Range;
                    cellRange.Text = "Последний вход";
                    cellRange = workertable.Cell(1, 7).Range;
                    cellRange.Text = "Тип входа";
                    workertable.Rows[1].Range.Bold = 1;
                    workertable.Rows[1].Range.ParagraphFormat.Alignment = Word.WdParagraphAlignment.wdAlignParagraphCenter;

                    int i = 1;
                    foreach (var person in worker)
                    {
                        cellRange = workertable.Cell(i + 1, 1).Range;
                        cellRange.Text = person.CodClienta;
                        cellRange.ParagraphFormat.Alignment =
                        Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        cellRange = workertable.Cell(i + 1, 2).Range;
                        cellRange.Text = person.Doljnost;
                        cellRange.ParagraphFormat.Alignment =
                        Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        cellRange = workertable.Cell(i + 1, 3).Range;
                        cellRange.Text = person.FIO;
                        cellRange.ParagraphFormat.Alignment =
                        Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        cellRange = workertable.Cell(i + 1, 4).Range;
                        cellRange.Text = person.Loginn;
                        cellRange.ParagraphFormat.Alignment =
                        Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        cellRange = workertable.Cell(i + 1, 5).Range;
                        cellRange.Text = person.Parol;
                        cellRange.ParagraphFormat.Alignment =
                        Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        cellRange = workertable.Cell(i + 1, 6).Range;
                        cellRange.Text = person.PosledniVhod;
                        cellRange.ParagraphFormat.Alignment =
                        Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        
                        cellRange = workertable.Cell(i + 1, 7).Range;
                        cellRange.Text = person.TipVhoda;
                        cellRange.ParagraphFormat.Alignment =
                        Word.WdParagraphAlignment.wdAlignParagraphCenter;
                        i++;
                    }
                    Word.Paragraph countStudentsParagraph = document.Paragraphs.Add();
                    Word.Range countStudentsRange =
                    countStudentsParagraph.Range;
                    countStudentsRange.Text = $"Количество работников данной должности - { worker.Count()} ";
                    countStudentsRange.Font.Color =
                    Word.WdColor.wdColorDarkRed;
                    countStudentsRange.InsertParagraphAfter();

                }
                document.Words.Last.InsertBreak(Word.WdBreakType.wdPageBreak);
                app.Visible = true;
            }
        }
    }
}

