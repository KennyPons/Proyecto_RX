using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinFormAnimation;

namespace RayPro.Persistencia.db
{
    internal class dbExcell
    {

        private string filePath;
        public dbExcell(string path) {
            filePath = path;
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial; // Establecer el contexto de licencia
        }



        public void SaveDataSerialExcell(string com, int baudRate)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets["Sheet1"];
                if (worksheet == null)
                {
                    worksheet = package.Workbook.Worksheets.Add("Sheet1");
                    worksheet.Cells[1, 1].Value = "id";
                    worksheet.Cells[1, 2].Value = "COM";
                    worksheet.Cells[1, 3].Value = "BAUDIOS";
                }

                int rowCount = worksheet.Dimension?.Rows ?? 1;
                int newId = rowCount + 1; // Calcula el nuevo ID como el número de filas + 1

                worksheet.Cells[newId, 1].Value = newId;
                worksheet.Cells[newId, 2].Value = com;
                worksheet.Cells[newId, 3].Value = baudRate;

                package.Save();
            }
        }



        public (int id, string com, int baudRate) GetDataSerialExcell(int id)
        {
            FileInfo fileInfo = new FileInfo(filePath);
            using (ExcelPackage package = new ExcelPackage(fileInfo))
            {
                ExcelWorksheet worksheet = package.Workbook.Worksheets["Sheet1"];
                if (worksheet == null)
                {
                    throw new Exception("Worksheet not found");
                }

                for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                {
                    if (worksheet.Cells[row, 1].GetValue<int>() == id)
                    {
                        string com = worksheet.Cells[row, 2].GetValue<string>();
                        int baudRate = worksheet.Cells[row, 3].GetValue<int>();
                        return (id, com, baudRate);
                    }
                }

                throw new Exception("ID not found");
            }
        }

        //////////////////////////////////////////////////////////////
    }
}
