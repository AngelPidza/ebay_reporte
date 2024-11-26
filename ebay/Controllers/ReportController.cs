using Microsoft.AspNetCore.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace ebay.Controllers
{
    public class ProductReporte
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int Stock { get; set; }
        public DateOnly? ReleaseDate { get; set; }
        public string Category { get; set; }
        public int TotalOrders { get; set; }
        public int WishlistCount { get; set; }
    }

    public class ReporteController : Controller
    {
        private static readonly BaseColor COLOR_PRIMARY = new BaseColor(32, 129, 226);
        private static readonly BaseColor COLOR_TEXT = new BaseColor(102, 102, 102);

        public IActionResult Index()
        {
            return View();
        }

        private List<ProductReporte> ObtenerDatosEjemplo()
        {
            return new List<ProductReporte>
            {
                new ProductReporte {
                    Title = "iPhone 14 Pro",
                    Description = "El último iPhone con cámara de 48MP",
                    Price = 999.99m,
                    Stock = 50,
                    ReleaseDate = new DateOnly(2023, 9, 15),
                    Category = "Electrónicos",
                    TotalOrders = 125,
                    WishlistCount = 450
                },
                new ProductReporte {
                    Title = "PlayStation 5",
                    Description = "Consola de videojuegos next-gen",
                    Price = 499.99m,
                    Stock = 30,
                    ReleaseDate = new DateOnly(2023, 1, 1),
                    Category = "Gaming",
                    TotalOrders = 200,
                    WishlistCount = 850
                },
                new ProductReporte {
                    Title = "AirPods Pro",
                    Description = "Auriculares inalámbricos con cancelación de ruido",
                    Price = 249.99m,
                    Stock = 100,
                    ReleaseDate = new DateOnly(2023, 10, 1),
                    Category = "Electrónicos",
                    TotalOrders = 300,
                    WishlistCount = 275
                },
                new ProductReporte {
                    Title = "Nike Air Max",
                    Description = "Zapatillas deportivas premium",
                    Price = 129.99m,
                    Stock = 75,
                    ReleaseDate = new DateOnly(2023, 6, 15),
                    Category = "Calzado",
                    TotalOrders = 180,
                    WishlistCount = 320
                },
                new ProductReporte {
                    Title = "Samsung QLED TV",
                    Description = "Smart TV 4K de 65 pulgadas",
                    Price = 1299.99m,
                    Stock = 25,
                    ReleaseDate = new DateOnly(2023, 3, 10),
                    Category = "Electrónicos",
                    TotalOrders = 45,
                    WishlistCount = 150
                }
            };
        }

        private void DibujarMarcaAgua(PdfContentByte cb, Document document)
        {
            PdfGState gstate = new PdfGState();
            gstate.FillOpacity = 0.1f;
            cb.SetGState(gstate);

            cb.BeginText();
            BaseFont bf = BaseFont.CreateFont(BaseFont.HELVETICA_BOLD, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
            cb.SetFontAndSize(bf, 100);
            cb.SetRGBColorFill(COLOR_PRIMARY.R, COLOR_PRIMARY.G, COLOR_PRIMARY.B);
            cb.ShowTextAligned(Element.ALIGN_CENTER, "MEGAMARKET",
                document.PageSize.Width / 2,
                document.PageSize.Height / 2, -45);
            cb.EndText();
        }

        private void AgregarEncabezado(Document document)
        {
            PdfPTable header = new PdfPTable(1);
            header.WidthPercentage = 100;
            header.DefaultCell.Border = Rectangle.NO_BORDER;
            header.DefaultCell.BackgroundColor = COLOR_PRIMARY;
            header.DefaultCell.Padding = 20;

            Paragraph title = new Paragraph("Reporte de Productos", 
                FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24, BaseColor.WHITE));
            title.Alignment = Element.ALIGN_CENTER;

            Paragraph subtitle = new Paragraph("MegaMarket - La mejor plataforma de compra y venta", 
                FontFactory.GetFont(FontFactory.HELVETICA, 14, BaseColor.WHITE));
            subtitle.Alignment = Element.ALIGN_CENTER;

            PdfPCell cell = new PdfPCell();
            cell.AddElement(title);
            cell.AddElement(subtitle);
            cell.Border = Rectangle.NO_BORDER;
            cell.BackgroundColor = COLOR_PRIMARY;
            cell.PaddingTop = 20;
            cell.PaddingBottom = 20;

            header.AddCell(cell);
            document.Add(header);
            document.Add(new Paragraph(" "));
        }

        private void AgregarTarjetasResumen(Document document, List<ProductReporte> productos)
        {
            PdfPTable cards = new PdfPTable(3);
            cards.WidthPercentage = 100;
            cards.SetWidths(new float[] { 1f, 1f, 1f });
            cards.DefaultCell.Padding = 10;
            cards.SpacingAfter = 20;

            decimal totalInventario = productos.Sum(p => p.Price * p.Stock);
            int totalProductos = productos.Count;
            int totalWishlists = productos.Sum(p => p.WishlistCount);

            void AgregarTarjeta(string titulo, string valor)
            {
                PdfPCell cell = new PdfPCell();
                cell.Padding = 15;
                cell.BackgroundColor = BaseColor.WHITE;
                
                Paragraph titleP = new Paragraph(titulo, 
                    FontFactory.GetFont(FontFactory.HELVETICA, 14, COLOR_TEXT));
                titleP.Alignment = Element.ALIGN_CENTER;
                
                Paragraph valueP = new Paragraph(valor, 
                    FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 24, COLOR_PRIMARY));
                valueP.Alignment = Element.ALIGN_CENTER;
                valueP.PaddingTop = 10;

                cell.AddElement(titleP);
                cell.AddElement(valueP);
                cards.AddCell(cell);
            }

            AgregarTarjeta("Total de Productos", totalProductos.ToString());
            AgregarTarjeta("Valor del Inventario", totalInventario.ToString("C"));
            AgregarTarjeta("Total en Wishlists", totalWishlists.ToString());

            document.Add(cards);
        }

        public IActionResult DescargarReporteProductos()
        {
            try
            {
                var productos = ObtenerDatosEjemplo();

                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                MemoryStream ms = new MemoryStream();
                PdfWriter writer = PdfWriter.GetInstance(document, ms);

                try
                {
                    document.Open();
                    
                    DibujarMarcaAgua(writer.DirectContentUnder, document);
                    AgregarEncabezado(document);
                    
                    Paragraph date = new Paragraph($"Fecha del reporte: {DateTime.Now:dd/MM/yyyy HH:mm}",
                        FontFactory.GetFont(FontFactory.HELVETICA, 12, COLOR_TEXT));
                    date.Alignment = Element.ALIGN_RIGHT;
                    date.SpacingAfter = 20f;
                    document.Add(date);

                    AgregarTarjetasResumen(document, productos);

                    PdfPTable table = new PdfPTable(7);
                    table.WidthPercentage = 100;
                    table.SetWidths(new float[] { 3f, 2f, 1.5f, 1.5f, 2f, 1.5f, 1.5f });

                    Font headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, BaseColor.WHITE);
                    Font contentFont = FontFactory.GetFont(FontFactory.HELVETICA, 11, BaseColor.BLACK);

                    string[] headers = { "Producto", "Categoría", "Precio", "Stock", "Lanzamiento", "Pedidos", "Wishlist" };
                    foreach (var header in headers)
                    {
                        PdfPCell cell = new PdfPCell(new Phrase(header, headerFont));
                        cell.BackgroundColor = COLOR_PRIMARY;
                        cell.HorizontalAlignment = Element.ALIGN_CENTER;
                        cell.VerticalAlignment = Element.ALIGN_MIDDLE;
                        cell.Padding = 8;
                        table.AddCell(cell);
                    }

                    foreach (var producto in productos)
                    {
                        table.AddCell(new PdfPCell(new Phrase(producto.Title, contentFont)));
                        table.AddCell(new PdfPCell(new Phrase(producto.Category, contentFont)));
                        table.AddCell(new PdfPCell(new Phrase(producto.Price.ToString("C"), contentFont)) 
                            { HorizontalAlignment = Element.ALIGN_RIGHT });
                        table.AddCell(new PdfPCell(new Phrase(producto.Stock.ToString(), contentFont)) 
                            { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(producto.ReleaseDate?.ToString("dd/MM/yyyy") ?? "", contentFont)) 
                            { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(producto.TotalOrders.ToString(), contentFont)) 
                            { HorizontalAlignment = Element.ALIGN_CENTER });
                        table.AddCell(new PdfPCell(new Phrase(producto.WishlistCount.ToString(), contentFont)) 
                            { HorizontalAlignment = Element.ALIGN_CENTER });
                    }

                    document.Add(table);

                    Paragraph footer = new Paragraph($"© {DateTime.Now.Year} MegaMarket - Documento Confidencial",
                        FontFactory.GetFont(FontFactory.HELVETICA, 10, COLOR_TEXT));
                    footer.Alignment = Element.ALIGN_CENTER;
                    footer.SpacingBefore = 20f;
                    document.Add(footer);

                    document.Close();

                    byte[] bytes = ms.ToArray();
                    return File(bytes, "application/pdf", $"ReporteProductos_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
                }
                catch (Exception ex)
                {
                    throw new Exception("Error generando el PDF: " + ex.Message);
                }
                finally
                {
                    if (document.IsOpen())
                        document.Close();
                    ms.Close();
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Error al generar el reporte: " + ex.Message);
            }
        }
    }
}