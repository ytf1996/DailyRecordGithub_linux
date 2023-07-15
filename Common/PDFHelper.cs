using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Roim.Common
{
    public class PDFHelper
    {
        /// <summary>
        /// 合并PDF
        /// </summary>
        /// <param name="fileList">需要合并的文件路径</param>
        /// <param name="outMergeFile">合并后的文件路径</param>
        public static void MergePdfFiles(string[] fileList, string outMergeFile)
        {
            PdfReader reader;
            List<PdfReader> readerList = new List<PdfReader>();
            Document document = new Document();
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(outMergeFile, FileMode.Create));
            document.Open();
            PdfContentByte cb = writer.DirectContent;
            PdfImportedPage newPage;
            for (int i = 0; i < fileList.Length; i++)
            {
                reader = new PdfReader(fileList[i]);
                int iPageNum = reader.NumberOfPages;
                for (int j = 1; j <= iPageNum; j++)
                {
                    document.NewPage();
                    newPage = writer.GetImportedPage(reader, j);
                    cb.AddTemplate(newPage, 0, 0);
                }
                readerList.Add(reader);
            }
            document.Close();
            foreach (var rd in readerList)//清理占用  
            {
                if(rd != null)
                rd.Close();
            }
        }
    }
}
