using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Reflection;

namespace Roim.Common
{
    /// <summary>
    /// Excel的Cell
    /// </summary>
    public class ExcelCellModel
    {
        public string Value { get; set; }

        public int Row { get; set; }

        public int Column { get; set; }

        public bool NeedBorder { get; set; }

        public ExcelCellModel(string value, int row, int column)
        {
            Value = value;
            Row = row;
            Column = column;
        }
    }

    /// <summary>
    /// 操作Excel
    /// </summary>
    public class ExcelHelper
    {
        public ExcelHelper()
        {

        }

        /// <summary>
        /// 写ExcelCell
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="sheetName">sheet名称</param>
        /// <param name="row">行</param>
        /// <param name="column">列</param>
        /// <param name="fromPath">Excel模板路径</param>
        /// <param name="toPath">Excel写入值路径</param>
        /// <param name="needBorder">是否要边框</param>
        public void WriteExcelCell(string value, string sheetName, int row, int column, string fromPath, string toPath, bool needBorder = false)
        {
            IWorkbook Workbook = null;

            using (FileStream SwFrom = new FileStream(fromPath, FileMode.Open))
            {
                try
                {
                    Workbook = new HSSFWorkbook(SwFrom);
                }
                catch
                {
                    using (FileStream SwFromX = new FileStream(fromPath, FileMode.Open))
                    {
                        Workbook = new XSSFWorkbook(SwFromX);
                    }
                }

            }

            ISheet Sheet1 = Workbook.GetSheet(sheetName);

            IRow Row = Sheet1.GetRow(row - 1);
            if (Row == null)
                Row = Sheet1.CreateRow(row - 1);

            ICell Cell = Row.GetCell(column - 1);
            if (Cell == null)
                Cell = Row.CreateCell(column - 1);

            if (needBorder)
            {
                ICellStyle Style = Workbook.CreateCellStyle();

                Style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                Style.BottomBorderColor = HSSFColor.Black.Index;
                Style.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                Style.LeftBorderColor = HSSFColor.Black.Index;
                Style.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                Style.RightBorderColor = HSSFColor.Black.Index;
                Style.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                Style.TopBorderColor = HSSFColor.Black.Index;

                Cell.CellStyle = Style;
            }
            Cell.SetCellValue(value);
            using (FileStream SwTo = new FileStream(toPath, FileMode.OpenOrCreate))
            {
                Workbook.Write(SwTo);
                SwTo.Close();
            }

        }

        /// <summary>
        /// 写ExcelCell集合
        /// </summary>
        /// <param name="list">ExcelCell集合</param>
        /// <param name="sheetName">sheet名称</param>
        /// <param name="fromPath">Excel模板路径</param>
        /// <param name="toPath">Excel写入值路径</param>
        /// <param name="needBorder">是否要边框</param>
        public void WriteExcelCells(List<ExcelCellModel> list, string sheetName, string fromPath, string toPath)
        {
            if (list == null)
                return;

            IWorkbook Workbook = null;

            double Numtemp = 0;

            using (FileStream SwFrom = new FileStream(fromPath, FileMode.Open))
            {
                try
                {
                    Workbook = new HSSFWorkbook(SwFrom);
                }
                catch
                {
                    using (FileStream SwFromX = new FileStream(fromPath, FileMode.Open))
                    {
                        Workbook = new XSSFWorkbook(SwFromX);
                    }
                }

            }

            ISheet Sheet1 = Workbook.GetSheet(sheetName);

            IRow Row = null;

            ICell Cell = null;

            ICellStyle Style = Workbook.CreateCellStyle();

            Style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            Style.BottomBorderColor = HSSFColor.Black.Index;
            Style.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            Style.LeftBorderColor = HSSFColor.Black.Index;
            Style.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            Style.RightBorderColor = HSSFColor.Black.Index;
            Style.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            Style.TopBorderColor = HSSFColor.Black.Index;

            foreach (ExcelCellModel item in list)
            {
                Row = Sheet1.GetRow(item.Row - 1);
                if (Row == null)
                    Row = Sheet1.CreateRow(item.Row - 1);

                Cell = Row.GetCell(item.Column - 1);
                if (Cell == null)
                    Cell = Row.CreateCell(item.Column - 1);

                if (item.NeedBorder)
                {
                    Cell.CellStyle = Style;
                }

                if (double.TryParse(item.Value, out Numtemp))
                    Cell.SetCellValue(Numtemp);
                else
                    Cell.SetCellValue(item.Value);

            }

            using (FileStream SwTo = new FileStream(toPath, FileMode.OpenOrCreate))
            {
                Workbook.Write(SwTo);
                SwTo.Close();
            }

        }

        /// <summary>
        /// 把DataTable写入Excel
        /// </summary>
        /// <param name="table">数据源</param>
        /// <param name="sheetName">sheet名称</param>
        /// <param name="rowBegin">开始行数</param>
        /// <param name="columnBegin">开始列数</param>
        /// <param name="fromPath">Excel模板路径</param>
        /// <param name="toPath">Excel写入值路径</param>
        public void WriteExcel(DataTable table, string sheetName, int rowBegin, int columnBegin, string fromPath, string toPath)
        {
            if (table == null)
                return;
            IWorkbook Workbook = null;

            using (FileStream SwFrom = new FileStream(fromPath, FileMode.Open))
            {
                try
                {
                    Workbook = new HSSFWorkbook(SwFrom);
                }
                catch
                {
                    using (FileStream SwFromX = new FileStream(fromPath, FileMode.Open))
                    {
                        Workbook = new XSSFWorkbook(SwFromX);
                    }
                }

            }

            ISheet Sheet1 = Workbook.GetSheet(sheetName);

            IRow Row = null;

            ICellStyle Style = Workbook.CreateCellStyle();
            Style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            Style.BottomBorderColor = HSSFColor.Black.Index;
            Style.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            Style.LeftBorderColor = HSSFColor.Black.Index;
            Style.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            Style.RightBorderColor = HSSFColor.Black.Index;
            Style.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            Style.TopBorderColor = HSSFColor.Black.Index;

            double Numtemp = 0;

            for (int i = 0; i < table.Rows.Count; i++)
            {
                Row = Sheet1.CreateRow(i + rowBegin - 1);
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    ICell TableCell = null;
                    if (double.TryParse(table.Rows[i][j].ToString(), out Numtemp))
                    {
                        TableCell = Row.CreateCell(j + columnBegin - 1, CellType.Numeric);
                        TableCell.SetCellValue(Numtemp);
                    }
                    else
                    {
                        TableCell = Row.CreateCell(j + columnBegin - 1);
                        TableCell.SetCellValue(table.Rows[i][j].ToString());
                    }
                    TableCell.SetCellValue(table.Rows[i][j].ToString());
                    TableCell.CellStyle = Style;

                }
            }

            using (FileStream SwTo = new FileStream(toPath, FileMode.OpenOrCreate))
            {
                Workbook.Write(SwTo);
                SwTo.Close();
            }

        }

        /// <summary>
        /// 把DataTable写入Excel
        /// </summary>
        /// <param name="table">数据源</param>
        /// <param name="sheetName">sheet名称</param>
        /// <param name="rowBegin">开始行数</param>
        /// <param name="columnBegin">开始列数</param>
        /// <param name="fromPath">Excel模板路径</param>
        /// <param name="toPath">Excel写入值路径</param>
        public void WriteExcel_Dy(DataTable table, string sheetName, int rowBegin, int columnBegin, string fromPath, string toPath)
        {
            if (table == null)
                return;
            IWorkbook Workbook = null;

            using (FileStream SwFromX = new FileStream(fromPath, FileMode.Open))
            {
                Workbook = new XSSFWorkbook(SwFromX);
            }

            ISheet Sheet1 = Workbook.GetSheet(sheetName);
            Sheet1.CreateFreezePane(0, 1, 0, 1);

            IRow Row = null;

            #region 列头格式
            ICellStyle StyleHead = Workbook.CreateCellStyle();
            StyleHead.Alignment = HorizontalAlignment.Justify;//两端自动对齐（自动换行）
            StyleHead.VerticalAlignment = VerticalAlignment.Justify;
            IFont fontHead = Workbook.CreateFont();
            fontHead.IsBold = true;                                 //加粗 
            //fontHead.FontHeightInPoints = 12;                       //设置字体大小
            fontHead.FontName = "微软雅黑";                         //设置字体
            StyleHead.SetFont(fontHead);                            //向样式里添加字体设置
            #endregion

            Row = Sheet1.CreateRow(0);
            for (int j = 0; j < table.Columns.Count; j++)
            {
                ICell TableCell = null;

                TableCell = Row.CreateCell(j + columnBegin - 1);
                TableCell.SetCellValue(table.Columns[j].ToString());

                TableCell.CellStyle = StyleHead;
            }

            IFont font = Workbook.CreateFont();
            // font.IsBold = true;                                 //加粗 
            //font.FontHeightInPoints = 12;                       //设置字体大小
            font.FontName = "微软雅黑";                         //设置字体

            #region 偶数行格式
            ICellStyle Style_even = Workbook.CreateCellStyle();
            Style_even.Alignment = HorizontalAlignment.Justify;//两端自动对齐（自动换行）
            Style_even.VerticalAlignment = VerticalAlignment.Center;
            Style_even.SetFont(font);                            //向样式里添加字体设置
            Style_even.FillPattern = FillPattern.SolidForeground;
            Style_even.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.LightGreen.Index;
            #endregion
            
            #region 奇数行格式
            ICellStyle Style_odd = Workbook.CreateCellStyle();
            Style_odd.Alignment = HorizontalAlignment.Justify;//两端自动对齐（自动换行）
            Style_odd.VerticalAlignment = VerticalAlignment.Center;
            Style_odd.SetFont(font);                            //向样式里添加字体设置
            Style_odd.FillPattern = FillPattern.NoFill;
            Style_odd.FillForegroundColor = NPOI.HSSF.Util.HSSFColor.White.Index;
            #endregion

            for (int i = 0; i < table.Rows.Count; i++)
            {
                Row = Sheet1.CreateRow(i + rowBegin - 1);
                for (int j = 0; j < table.Columns.Count; j++)
                {
                    ICell TableCell = null;

                    TableCell = Row.CreateCell(j + columnBegin - 1);
                    TableCell.SetCellValue(table.Rows[i][j].ToString());

                    TableCell.CellStyle = i % 2 == 0 ? Style_even : Style_odd;
                }
            }

            using (FileStream SwTo = new FileStream(toPath, FileMode.OpenOrCreate))
            {
                Workbook.Write(SwTo);
                SwTo.Close();
            }
        }


        /// <summary>
        /// 把ExcelCell集合和DataTable写入Excel
        /// </summary>
        /// <param name="list">ExcelCell数据源集合</param>
        /// <param name="table">DataTable数据源</param>
        /// <param name="sheetName">sheet名称</param>
        /// <param name="rowBegin">开始行数</param>
        /// <param name="columnBegin">开始列数</param>
        /// <param name="fromPath">Excel模板路径</param>
        /// <param name="toPath">Excel写入值路径</param>
        public void WriteExcel(List<ExcelCellModel> list, DataTable table, string sheetName, int rowBegin, int columnBegin, string fromPath, string toPath)
        {
            IWorkbook Workbook = null;

            using (FileStream SwFrom = new FileStream(fromPath, FileMode.Open))
            {
                try
                {
                    Workbook = new HSSFWorkbook(SwFrom);
                }
                catch
                {
                    using (FileStream SwFromX = new FileStream(fromPath, FileMode.Open))
                    {
                        Workbook = new XSSFWorkbook(SwFromX);
                    }
                }

            }

            ISheet Sheet1 = Workbook.GetSheet(sheetName);

            IRow Row = null;

            ICell Cell = null;

            if (list != null)
            {
                foreach (ExcelCellModel item in list)
                {
                    Row = Sheet1.GetRow(item.Row - 1);
                    if (Row == null)
                        Row = Sheet1.CreateRow(item.Row - 1);

                    Cell = Row.GetCell(item.Column - 1);
                    if (Cell == null)
                        Cell = Row.CreateCell(item.Column - 1);

                    Cell.SetCellValue(item.Value);
                }
            }

            if (table != null)
            {
                ICellStyle Style = Workbook.CreateCellStyle();
                Style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                Style.BottomBorderColor = HSSFColor.Black.Index;
                Style.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                Style.LeftBorderColor = HSSFColor.Black.Index;
                Style.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                Style.RightBorderColor = HSSFColor.Black.Index;
                Style.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                Style.TopBorderColor = HSSFColor.Black.Index;


                double Numtemp = 0;

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    Row = Sheet1.CreateRow(i + rowBegin - 1);
                    for (int j = 0; j < table.Columns.Count; j++)
                    {
                        ICell TableCell = null;
                        if (double.TryParse(table.Rows[i][j].ToString(), out Numtemp))
                        {
                            TableCell = Row.CreateCell(j + columnBegin - 1, CellType.Numeric);
                            TableCell.SetCellValue(Numtemp);
                        }
                        else
                        {
                            TableCell = Row.CreateCell(j + columnBegin - 1);
                            TableCell.SetCellValue(table.Rows[i][j].ToString());
                        }
                        TableCell.CellStyle = Style;
                    }
                }
            }

            using (FileStream SwTo = new FileStream(toPath, FileMode.OpenOrCreate))
            {
                Workbook.Write(SwTo);
                SwTo.Close();
            }
        }

        /// <summary>
        /// 把ExcelCell集合和DataTable集合写入Excel
        /// </summary>
        /// <param name="list">ExcelCell数据源集合</param>
        /// <param name="tables">DataTable数据源集合</param>
        /// <param name="sheetName">sheet名称</param>
        /// <param name="rowBegin">开始行数</param>
        /// <param name="columnBegin">开始列数</param>
        /// <param name="fromPath">Excel模板路径</param>
        /// <param name="toPath">Excel写入值路径</param>
        /// <param name="nextTableAddRow">每个DataTable相隔几行</param>
        public void WriteExcel(List<ExcelCellModel> list, List<DataTable> tables, string sheetName, int rowBegin, int columnBegin, string fromPath, string toPath
            , int nextTableAddRow = 1)
        {
            IWorkbook Workbook = null;

            using (FileStream SwFrom = new FileStream(fromPath, FileMode.Open))
            {
                try
                {
                    Workbook = new HSSFWorkbook(SwFrom);
                }
                catch
                {
                    using (FileStream SwFromX = new FileStream(fromPath, FileMode.Open))
                    {
                        Workbook = new XSSFWorkbook(SwFromX);
                    }
                }

            }

            ISheet Sheet1 = Workbook.GetSheet(sheetName);

            IRow Row = null;

            ICell Cell = null;

            if (list != null)
            {
                foreach (ExcelCellModel item in list)
                {
                    Row = Sheet1.GetRow(item.Row - 1);
                    if (Row == null)
                        Row = Sheet1.CreateRow(item.Row - 1);

                    Cell = Row.GetCell(item.Column - 1);
                    if (Cell == null)
                        Cell = Row.CreateCell(item.Column - 1);

                    Cell.SetCellValue(item.Value);
                }
            }

            if (tables != null)
            {
                ICellStyle Style = Workbook.CreateCellStyle();
                Style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
                Style.BottomBorderColor = HSSFColor.Black.Index;
                Style.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
                Style.LeftBorderColor = HSSFColor.Black.Index;
                Style.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
                Style.RightBorderColor = HSSFColor.Black.Index;
                Style.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
                Style.TopBorderColor = HSSFColor.Black.Index;

                int rowed = 0;

                double Numtemp = 0;

                foreach (DataTable dt in tables)
                {
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        Row = Sheet1.CreateRow(i + rowBegin - 1 + rowed);
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            ICell TableCell = null;
                            if (double.TryParse(dt.Rows[i][j].ToString(), out Numtemp))
                            {
                                TableCell = Row.CreateCell(j + columnBegin - 1, CellType.Numeric);
                                TableCell.SetCellValue(Numtemp);
                            }
                            else
                            {
                                TableCell = Row.CreateCell(j + columnBegin - 1);
                                TableCell.SetCellValue(dt.Rows[i][j].ToString());
                            }

                            TableCell.CellStyle = Style;
                        }
                    }

                    rowed = dt.Rows.Count + nextTableAddRow;
                }

            }

            using (FileStream SwTo = new FileStream(toPath, FileMode.OpenOrCreate))
            {
                Workbook.Write(SwTo);
                SwTo.Close();
            }

        }

        /// <summary>
        /// 将excel中的数据导入到DataTable中
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="sheetName">excel工作薄sheet的名称</param>
        /// <param name="beginRow">开始行数</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <returns>返回的DataTable</returns>
        public DataTable ExcelToDataTable(string path, string sheetName, int beginRow = 1, bool isFirstRowColumn = true)
        {
            IWorkbook Workbook = null;
            FileStream Fs = null;

            ISheet Sheet = null;
            ICell Cell = null;
            DataTable Dt = new DataTable();
            int StartRow = 0;
            try
            {
                InitExcel(path, sheetName, ref Workbook, ref Fs, ref Sheet);

                if (Sheet != null)
                {
                    IRow firstRow = Sheet.GetRow(beginRow - 1);
                    int cellCount = firstRow.LastCellNum; //一行最后一个cell的编号 即总的列数
                    ICell cell = null;

                    if (isFirstRowColumn)
                    {
                        for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                        {
                            cell = firstRow.GetCell(i);
                            DataColumn column = null;

                            if (cell == null)
                                column = new DataColumn();
                            else
                                column = new DataColumn(cell.ToString());
                            Dt.Columns.Add(column);
                        }
                        StartRow = beginRow;
                    }
                    else
                    {
                        StartRow = beginRow - 1;
                    }

                    //最后一列的标号
                    int rowCount = Sheet.LastRowNum;
                    for (int i = StartRow; i <= rowCount; ++i)
                    {
                        IRow row = Sheet.GetRow(i);
                        if (row == null) continue; //没有数据的行默认是null　　　　　　　

                        DataRow dataRow = Dt.NewRow();
                        for (int j = row.FirstCellNum; j < cellCount; ++j)
                        {
                            Cell = row.GetCell(j);
                            if (Cell != null) //同理，没有数据的单元格都默认是null
                            {
                                dataRow[j - row.FirstCellNum] = GetValueOfCell(Cell);
                            }

                        }
                        Dt.Rows.Add(dataRow);
                    }
                }

                return Dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Fs.Close();
            }
        }

        /// <summary>
        /// 从第N个sheet开始,将excel中的所有sheet数据导入到DataTable中(每个sheet中的数据格式需一致)
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="beginSheet">从第几个sheet开始</param>
        /// <param name="beginRow">从第几行开始</param>
        /// <param name="isFirstRowColumn">第一行是否是DataTable的列名</param>
        /// <returns></returns>
        public DataTable ExcelToDataTable(string path, int beginSheet = 1, int beginRow = 1, bool isFirstRowColumn = true)
        {
            IWorkbook Workbook = null;
            FileStream Fs = null;

            ISheet Sheet = null;
            DataTable Dt = new DataTable();

            int StartRow = 0;
            try
            {
                Fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                try
                {
                    Workbook = new HSSFWorkbook(Fs);
                }
                catch (Exception)
                {
                    Workbook = new XSSFWorkbook(Fs);
                }

                ICell Cell = null;

                for (int s = 0; s < Workbook.NumberOfSheets; s++)
                {
                    if (s < beginSheet - 1)
                        continue;

                    Sheet = Workbook.GetSheetAt(s);

                    if (Sheet != null)
                    {
                        IRow firstRow = Sheet.GetRow(beginRow - 1);
                        int cellCount = firstRow.LastCellNum; //一行最后一个cell的编号 即总的列数

                        if (isFirstRowColumn && s == beginSheet - 1)
                        {
                            for (int i = firstRow.FirstCellNum; i < cellCount; ++i)
                            {
                                Cell = firstRow.GetCell(i);
                                DataColumn column = null;

                                if (Cell != null)
                                    column = new DataColumn(Cell.ToString());
                                else
                                    column = new DataColumn();
                                Dt.Columns.Add(column);
                            }
                        }
                        StartRow = beginRow;

                        //最后一列的标号
                        int rowCount = Sheet.LastRowNum;


                        for (int i = StartRow; i <= rowCount; ++i)
                        {
                            IRow row = Sheet.GetRow(i);
                            if (row == null) continue; //没有数据的行默认是null　　　　　　　

                            DataRow dataRow = Dt.NewRow();
                            for (int j = row.FirstCellNum; j < cellCount; ++j)
                            {
                                Cell = row.GetCell(j);
                                if (Cell != null) //同理，没有数据的单元格都默认是null
                                {
                                    dataRow[j - row.FirstCellNum] = GetValueOfCell(Cell);
                                }
                            }
                            Dt.Rows.Add(dataRow);
                        }
                    }
                }
                return Dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Fs.Close();
            }
        }

        /// <summary>
        /// 获得某个单元格的值
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="sheetName">sheet名</param>
        /// <param name="row">行</param>
        /// <param name="column">列</param>
        /// <returns>结果</returns>
        public string GetCellValue(string path, string sheetName, int row, int column)
        {
            string Value = string.Empty;

            IWorkbook Workbook = null;
            FileStream Fs = null;

            ISheet Sheet = null;
            IRow FirstRow = null;
            ICell Cell = null;

            try
            {
                InitExcel(path, sheetName, ref Workbook, ref Fs, ref Sheet);
                if (Sheet != null)
                {
                    FirstRow = Sheet.GetRow(row - 1);
                    if (FirstRow != null)
                    {
                        Cell = FirstRow.GetCell(column - 1);
                        Value = GetValueOfCell(Cell);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Fs.Close();
            }

            return Value;
        }

        /// <summary>
        /// 根据一个值获得包含此值的第一个Cell所在的行数和列数
        /// </summary>
        /// <param name="path">路径</param>
        /// <param name="sheetName">sheet名称</param>
        /// <param name="value">查找的值</param>
        /// <param name="rowResult">所在行</param>
        /// <param name="columnResult">所在列</param>
        /// <param name="valueResult">该位置的值</param>
        public void GetRowAndColumnNumByLikeValue(string path, string sheetName, string value, out int rowResult, out int columnResult, out string valueResult)
        {
            rowResult = 0;
            columnResult = 0;
            valueResult = string.Empty;

            IWorkbook Workbook = null;
            FileStream Fs = null;

            ISheet Sheet = null;
            IRow Row = null;
            ICell Cell = null;
            int RowTotal = 0;
            int ColumnTotal = 0;
            bool HasFound = false;

            try
            {
                InitExcel(path, sheetName, ref Workbook, ref Fs, ref Sheet);
                if (Sheet != null)
                {

                    RowTotal = Sheet.PhysicalNumberOfRows;


                    for (int i = 0; i < RowTotal; i++)
                    {
                        Row = Sheet.GetRow(i);
                        if (Row == null)
                            continue;

                        ColumnTotal = Row.PhysicalNumberOfCells;
                        for (int j = Row.FirstCellNum - 1; j < Row.FirstCellNum - 1 + ColumnTotal; j++)
                        {
                            Cell = Row.GetCell(j);
                            if (Cell == null)
                                continue;

                            if (Cell.ToString().Contains(value.Trim()))
                            {
                                HasFound = true;
                                rowResult = i + 1;
                                columnResult = j + 1;

                                valueResult = GetValueOfCell(Cell);

                                break;
                            }
                        }
                        if (HasFound)
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (Fs != null)
                    Fs.Close();
            }

        }

        /// <summary>
        /// 获得某个单元格的值
        /// </summary>
        /// <param name="valueResult"></param>
        /// <param name="Cell"></param>
        /// <returns></returns>
        private static string GetValueOfCell(ICell Cell)
        {
            string valueResult = string.Empty;
            switch (Cell.CellType)
            {
                case CellType.Blank:
                    break;
                case CellType.Boolean:
                    valueResult = Cell.BooleanCellValue.ToString();
                    break;
                case CellType.Formula:
                    switch (Cell.CachedFormulaResultType)
                    {
                        case CellType.Numeric:
                            if (HSSFDateUtil.IsCellDateFormatted(Cell))
                            {
                                valueResult = Cell.DateCellValue.ToString("yyyy-MM-dd");
                            }
                            else
                            {
                                valueResult = Cell.NumericCellValue.ToString();
                            }

                            break;
                        case CellType.Blank:
                            break;
                        case CellType.Boolean:
                            valueResult = Cell.BooleanCellValue.ToString();
                            break;
                        case CellType.String:
                            valueResult = Cell.StringCellValue;
                            break;
                        case CellType.Unknown:
                            valueResult = Cell.ToString();
                            break;
                        default:
                            break;
                    }
                    break;
                case CellType.Numeric:
                    if (HSSFDateUtil.IsCellDateFormatted(Cell))
                    {
                        valueResult = Cell.DateCellValue.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        valueResult = Cell.NumericCellValue.ToString();
                    }
                    break;
                case CellType.String:
                    valueResult = Cell.StringCellValue;
                    break;
                case CellType.Unknown:
                    valueResult = Cell.ToString();
                    break;
                default:
                    break;

            }
            return valueResult;
        }

        private void InitExcel(string path, string sheetName, ref IWorkbook Workbook, ref FileStream Fs, ref ISheet Sheet)
        {
            Fs = new FileStream(path, FileMode.Open, FileAccess.Read);

            try
            {
                Workbook = new HSSFWorkbook(Fs);
            }
            catch (Exception)
            {
                Workbook = new XSSFWorkbook(Fs);
            }

            if (!string.IsNullOrEmpty(sheetName))
            {
                Sheet = Workbook.GetSheet(sheetName);
            }
            else
            {
                Sheet = Workbook.GetSheetAt(0);
            }
        }

    }

    public class ExcelHelperForModel<T>
    {
        public ExcelHelperForModel()
        {

        }

        /// <summary>
        /// 把Model写入Excel
        /// </summary>
        /// <param name="ts">数据源</param>
        /// <param name="sheetName">sheet名称</param>
        /// <param name="rowBegin">开始行数</param>
        /// <param name="columnBegin">开始列数</param>
        /// <param name="fromPath">Excel模板路径</param>
        /// <param name="toPath">Excel写入值路径</param>
        public void WriteExcel(List<ExcelCellModel> list, List<T> ts, string sheetName, int rowBegin, int columnBegin, string fromPath, string toPath)
        {
            if (ts == null)
                return;

            IWorkbook Workbook = null;

            using (FileStream SwFrom = new FileStream(fromPath, FileMode.Open))
            {
                try
                {
                    Workbook = new HSSFWorkbook(SwFrom);
                }
                catch
                {
                    using (FileStream SwFromX = new FileStream(fromPath, FileMode.Open))
                    {
                        Workbook = new XSSFWorkbook(SwFromX);
                    }
                }

            }

            ISheet Sheet1 = Workbook.GetSheet(sheetName);

            IRow Row = null;

            ICellStyle Style = Workbook.CreateCellStyle();
            Style.BorderBottom = NPOI.SS.UserModel.BorderStyle.Thin;
            Style.BottomBorderColor = HSSFColor.Black.Index;
            Style.BorderLeft = NPOI.SS.UserModel.BorderStyle.Thin;
            Style.LeftBorderColor = HSSFColor.Black.Index;
            Style.BorderRight = NPOI.SS.UserModel.BorderStyle.Thin;
            Style.RightBorderColor = HSSFColor.Black.Index;
            Style.BorderTop = NPOI.SS.UserModel.BorderStyle.Thin;
            Style.TopBorderColor = HSSFColor.Black.Index;

            double Numtemp = 0;

            if (list != null)
            {
                ICell TheCell = null;
                foreach (ExcelCellModel item in list)
                {
                    Row = Sheet1.GetRow(item.Row - 1);
                    if (Row == null)
                        Row = Sheet1.CreateRow(item.Row - 1);

                    TheCell = Row.GetCell(item.Column - 1);
                    if (TheCell == null)
                        TheCell = Row.CreateCell(item.Column - 1);

                    TheCell.SetCellValue(item.Value);
                }
            }

            if (ts.Count > 0)
            {
                PropertyInfo[] ProInfo = ts[0].GetType().GetProperties();
                object Value = null;

                for (int i = 0; i < ts.Count; i++)
                {
                    Row = Sheet1.CreateRow(i + rowBegin - 1);
                    for (int j = 0; j < ProInfo.Length; j++)
                    {
                        ICell TableCell = null;
                        Value = ProInfo[j].GetValue(ts[i], null);

                        if (Value == null)
                            Value = "";

                        if (double.TryParse(Value.ToString(), out Numtemp))
                        {
                            TableCell = Row.CreateCell(j + columnBegin - 1, CellType.Numeric);
                            TableCell.SetCellValue(Numtemp);
                        }
                        else
                        {
                            TableCell = Row.CreateCell(j + columnBegin - 1);
                            TableCell.SetCellValue(Value.ToString());
                        }
                        TableCell.SetCellValue(Value.ToString());
                        TableCell.CellStyle = Style;

                    }
                }
            }

            using (FileStream SwTo = new FileStream(toPath, FileMode.OpenOrCreate))
            {
                Workbook.Write(SwTo);
            }

        }

        /// <summary>
        /// 读取Excel中数据并转换为model
        /// </summary>
        /// <param name="path"></param>
        /// <param name="sheetName"></param>
        /// <returns></returns>
        public List<T> ReadExcel(string path, string sheetName)
        {
            ExcelHelper helper = new ExcelHelper();
            DataTable table = helper.ExcelToDataTable(path, sheetName);

            if (table == null || table.Rows.Count == 0)
            {
                return null;
            }

            T model;

            List<T> list = new List<T>();

            PropertyInfo property = null;

            foreach (DataRow itemRow in table.Rows)
            {
                model = System.Activator.CreateInstance<T>();
                foreach (DataColumn itemColumn in table.Columns)
                {
                    property = typeof(T).GetProperty(itemColumn.ColumnName);
                    if (property == null)
                    {
                        continue;
                    }
                    property.SetValue(model, itemRow[itemColumn]);
                }
                list.Add(model);
            }

            return list;
        }
    }
}
