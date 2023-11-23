using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BL
{
    public class CargaMasiva
    {
        public static void LeerArchivoCSV(string fileDirection)
        {
            try
            {
                var reader = new StreamReader(File.OpenRead(fileDirection));

                List<string> registros = new List<string>();

                DataTable tablaProducto = new DataTable();
                tablaProducto.Columns.Add("Nombre", typeof(string));
                tablaProducto.Columns.Add("PrecioUnitario", typeof(string));
                tablaProducto.Columns.Add("Stock", typeof(int));
                tablaProducto.Columns.Add("FechaRegistro", typeof(DateTime));
                tablaProducto.Columns.Add("IdProveedor", typeof(int));
                tablaProducto.Columns.Add("Proveedor", typeof(string));
                tablaProducto.Columns.Add("Numero", typeof(string));
                tablaProducto.Columns.Add("Direccion", typeof(string));

                var line = reader.ReadLine();

                while (!reader.EndOfStream)
                {
                    line = reader.ReadLine();
                    var values = line.Split(',');
                    // me quede aqui
                    
                }

            } catch (Exception ex)
            {

            }
        }
    }
}
