using DL;
using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI.WebControls;

namespace PL.Controllers
{
    public class CargaMasivaController : Controller
    {
        // GET: Form
        public ActionResult Form()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CargaMasiva()
        {
            HttpPostedFileBase archivo = Request.Files["archivo"];


            string extencionArchivo = Path.GetExtension(archivo.FileName).ToLower();
            string extencionValida = ConfigurationManager.AppSettings["TipoExcel"];

            if (extencionArchivo == extencionValida)
            {
                //string rutaproyecto = Server.MapPath(@"~\CargaMasiva\");
                //string filePath = rutaproyecto + Path.GetFileNameWithoutExtension(archivo.FileName) + '-' + DateTime.Now.ToString("yyyyMMddHHmmss") + ".csv";

                //archivo.SaveAs(filePath);

                ViewBag.Informacion = LeerArchivoCSV(archivo);
            } else
            {
                ViewBag.Informacion = "El archivo no es valido.";

            }

            return View("Form");
        }



        public static string LeerArchivoCSV(HttpPostedFileBase archivo)
        {

            string informacion;

            try
            {
                var reader = new StreamReader(archivo.InputStream);

                string line = reader.ReadLine();

                List<string[]> listaColumnas = new List<string[]>();

                while (!reader.EndOfStream)
                {
                    string[] columna = new string[10];

                    line = reader.ReadLine().Trim().Replace(", ", "-");
                    line = line.Replace("\"", "");
                    line = line.Replace("$", "");
                    string[] values = line.Split(',');

                    int numeroColumnas = values.Length;

                    if (numeroColumnas == 11)
                    {
                        columna[0] = values[0].Trim();
                        columna[1] = values[1].Trim() + values[2].Trim();
                        columna[2] = values[3].Trim();
                        columna[3] = values[4].Trim();
                        columna[4] = values[5].Trim();
                        columna[5] = values[6].Trim();
                        columna[6] = values[7].Trim();
                        columna[7] = values[8].Trim();
                        columna[8] = values[9].Trim();
                        columna[9] = values[10].Trim();
                    }
                    else
                    {
                        columna[0] = values[0].Trim();
                        columna[1] = values[1].Trim();
                        columna[2] = values[2].Trim();
                        columna[3] = values[3].Trim();
                        columna[4] = values[4].Trim();
                        columna[5] = values[5].Trim();
                        columna[6] = values[6].Trim();
                        columna[7] = values[7].Trim();
                        columna[8] = values[8].Trim();
                        columna[9] = values[9].Trim();
                    }

                    // validacion de datos, no dejar campos vacios
                    for (int i = 0; i <= 9; i++)
                    {
                        if (i == 1 || i == 2 || i == 6 || i == 8)
                        {
                            if (columna[i] == null || columna[i] == "")
                            {
                                columna[i] = "0";
                            }
                        }
                        else
                        {
                            if (columna[i] == null || columna[i] == "")
                            {
                                columna[i] = "sn";
                            }
                        }
                    }


                    //validacion de fecha
                    int cantidadCaracteresFecha = columna[4].Length - 1;
                    string fechaColumna = columna[4];
                    string fechaString = "";

                    for (int i = 1; i <= 8; i++)
                    {
                        if (i == 0)
                        {
                            fechaString = fechaColumna[cantidadCaracteresFecha - 8 + i].ToString();
                        }
                        else
                        {
                            fechaString += fechaColumna[cantidadCaracteresFecha - 8 + i].ToString();
                        }
                    }

                    columna[4] = fechaString;

                    listaColumnas.Add(columna);
                }

                informacion = CargarDataTable(listaColumnas);


            }
            catch (Exception ex)
            {
                informacion = ex.Message;
            }

            return informacion;
        }

        public static string CargarDataTable(List<string[]> productos)
        {
            string informacion;

            DataTable tablaProducto = new DataTable();
            tablaProducto.Columns.Add("Id", typeof(int));
            tablaProducto.Columns.Add("Nombre", typeof(string));
            tablaProducto.Columns.Add("PrecioUnitario", typeof(float));
            tablaProducto.Columns.Add("Stock", typeof(int));
            tablaProducto.Columns.Add("FechaRegistro", typeof(DateTime));
            //tablaProducto.Columns.Add("Separador", typeof(string));
            tablaProducto.Columns.Add("IdProveedor", typeof(int));
            tablaProducto.Columns.Add("Proveedor", typeof(string));
            tablaProducto.Columns.Add("Numero", typeof(string));
            tablaProducto.Columns.Add("Direccion", typeof(string));
            foreach (string[] producto in productos)
            {

                DateTime fecha = DateTime.ParseExact(producto[4], "dd-MM-yy", CultureInfo.InvariantCulture);

                tablaProducto.Rows.Add(new object[]
                {
                    1,
                    producto[0].ToString(),
                    float.Parse(producto[1]),
                    int.Parse(producto[2]),
                    fecha,
                    //producto[5].ToString(),
                    int.Parse(producto[6]),
                    producto[7].ToString(),
                    Convert.ToDouble(producto[8].ToString()),
                    producto[9].ToString()
                });


            }

            if (tablaProducto.Rows.Count > 0)
            {
                informacion = CargaMasivaBulkCopy(tablaProducto);
            } else
            {
                informacion = "El DataTable esta vacio.";
            }

            return informacion;

        }


        public static string CargaMasivaBulkCopy(DataTable tablaProducto)
        {
            string informacion;

            try{
                using (SqlConnection conexion = new SqlConnection(DL.Conexion.GetConnectionString()))
                {
                    conexion.Open();

                    using (SqlTransaction transaction = conexion.BeginTransaction())
                    {
                        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(conexion, SqlBulkCopyOptions.Default, transaction))
                        {
                            try
                            {
                                bulkCopy.DestinationTableName = "Producto";
                                bulkCopy.WriteToServer(tablaProducto);
                                transaction.Commit();
                                informacion = "La carga masiva se realizo con exito.";
                            }
                            catch (Exception ex)
                            {
                                transaction.Rollback();
                                conexion.Close();
                                informacion = ex.Message;
                                throw;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                informacion = ex.Message;
            }

            return informacion;
        }



    }
}