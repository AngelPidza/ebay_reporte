using System.Drawing;
using System.Net;
using ebay.Models;
using OfficeOpenXml.Drawing;
using OfficeOpenXml.Style;

namespace ebay.services;

public class ProductService
{
    private readonly HttpClient _httpClient;

    public ProductService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<Product>> GetProductsAsync()
    {
        var response = await _httpClient.GetAsync("api/products");
        response.EnsureSuccessStatusCode();

        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        return products ?? new List<Product>();
    }

    public async Task<Product> GetProductDetailsAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/products/{id}");
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Product>() ?? new Product();
        }

        throw new HttpRequestException($"Error retrieving product with id {id}");
    }
    
    public async Task<byte[]> ExportProductsToExcelAsync()
    {
        var products = await GetProductsAsync();
        var tempFiles = new List<string>();

        using var package = new OfficeOpenXml.ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("Products");

        // Encabezados
        var headers = worksheet.Cells["A1:D1"];
        headers.Style.Fill.PatternType = ExcelFillStyle.Solid;
        headers.Style.Fill.BackgroundColor.SetColor(Color.DarkBlue);
        headers.Style.Font.Color.SetColor(Color.White);
        headers.Style.Font.Bold = true;
        headers.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

        worksheet.Column(1).Width = 15;
        worksheet.Column(2).Width = 40;
        worksheet.Column(3).Width = 20;
        worksheet.Column(4).Width = 50;

        worksheet.Cells["A1"].Value = "ProductId";
        worksheet.Cells["B1"].Value = "Title";
        worksheet.Cells["C1"].Value = "Price";
        worksheet.Cells["D1"].Value = "Image";

        int row = 2;
        foreach (var product in products)
        {
            worksheet.Cells[row, 1].Value = product.ProductId;
            worksheet.Cells[row, 2].Value = product.Title;
            worksheet.Cells[row, 3].Value = product.Price;
            
            // Ajustar altura de fila para la imagen
            worksheet.Row(row).Height = 100;

            if (!string.IsNullOrEmpty(product.ImageUrl))
            {
                using (var webClient = new WebClient())
                {
                    byte[] imageBytes = webClient.DownloadData(product.ImageUrl);
                    string tempPath = Path.Combine(Path.GetTempPath(), $"temp_image_{Guid.NewGuid()}.jpg");
                    tempFiles.Add(tempPath);
                    File.WriteAllBytes(tempPath, imageBytes);
                    
                    var picture = worksheet.Drawings.AddPicture($"Picture{row}", tempPath);
                    
                    // Configurar la imagen para que se ajuste a la celda
                    picture.From.Column = 3; // Columna D
                    picture.From.Row = row - 1;
                    picture.SetSize(90, 90);
                    picture.EditAs = eEditAs.OneCell;

                    // Centrar la imagen en la celda
                    picture.From.ColumnOff = (int)((worksheet.Column(4).Width - 90) * 2834); // Píxeles EMU
                    picture.From.RowOff = (int)((worksheet.Row(row).Height - 90) * 2834); // Píxeles EMU
                }
            }

            row++;
        }

        // Formato de precio y bordes
        worksheet.Cells[$"C2:C{row-1}"].Style.Numberformat.Format = "$#,##0.00";
        worksheet.Cells[1, 1, row - 1, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);

        var excelBytes = await package.GetAsByteArrayAsync();
        
        foreach (var tempFile in tempFiles)
        {
            try { File.Delete(tempFile); } catch { }
        }

        return excelBytes;
    }
}