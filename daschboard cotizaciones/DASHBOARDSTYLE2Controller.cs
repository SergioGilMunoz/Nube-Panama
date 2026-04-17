using Microsoft.Ajax.Utilities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;
using Velzon.Modelos;
using Velzon.Modelos.DB;
using Velzon.Modelos.DBSIAV2024;

namespace Velzon.Controllers
{
    public class DASHBOARDSTYLE2Controller : Controller
    {
        // GET: Dashboard2
        public ActionResult Index()
        {

            if (Session["DatosUsuario"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }


        public ActionResult cotizaciones2()
        {

            if (Session["DatosUsuario"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }




        // esto es para la IA
        public ActionResult ObtenerClientesIA()
            {
                var db = new SIAV_prod_4Entities();
                int idEmpresa = SessionUtils.ObtenerIdEmpresaDeSesion(Session);

                string cantidad = "";
                // Obtener la fecha actual y calcular la fecha de hace 6 meses
                DateTime fechaFin = DateTime.Today; // Fecha de fin es hoy
                DateTime fechaInicio = fechaFin.AddMonths(-6); // Fecha de inicio es 6 meses atrás


                var clientesBusquedaVencidas = db.UP_COTIZACIONE_VENCIDAS_X_VENCER(fechaInicio, fechaFin, cantidad).ToList();
                // var clientesBusqueda = db.UP_COTIZACIONE_VENCIDAS_X_VENCER(fechaInicio, fechaFin, cantidad).ToList();
                // Mapea los resultados a un formato JSON
                var resultClientesVencidas = clientesBusquedaVencidas.Select(r => new
                {
                    id_Cliente = r.id_cliente,
                    Cliente = r.cliente,
                    Pais = r.pais,
                    C13xvencer = r.C1_3_x_vencer,
                    C46xvencer = r.C4_6_x_vencer,
                    C7xvencer = r.C7_x_vencer,
                    Vence_hoy = r.vence_hoy,
                    C13xvencida = r.C1_3_x_vencida,
                    C46xvencida = r.C4_6_x_vencida,
                    C7xvencida = r.C7_x_vencida,
                    fechadesde = r.fecha_desde,
                    fechahasta = r.fecha_hasta,
                    notificados = r.notificados

                }).ToList();

                return Content(JsonConvert.SerializeObject(resultClientesVencidas), "application/json");
            }


         // IA
        public ActionResult ObtenerDatosVencidasVencerporIdIA(ClienteBusqueda2 clienteBusqueda)
        {
            var db = new SIAV_prod_4Entities();
            var dback = new BackOffice_WebEntities();
            var (idUsuario, idEmpresa) = SessionUtils.ObtenerValoresDeSesion(Session);


            string cantidad = clienteBusqueda.cliente ?? "";
            //string filtro = ClienteBusqueda.filtro ?? "";
            // string cliente = ClienteBusqueda.Cliente ?? "";
            //  string estado = parametros.Estado ?? "";
            // Extraer la parte de la fecha (ignorar la parte de la hora)
            string fechaSolo1 = clienteBusqueda.periodo1.Split('T')[0];
            string[] fechaPartes1 = fechaSolo1.Split('-');
            int año1 = int.Parse(fechaPartes1[0]);
            int mes1 = int.Parse(fechaPartes1[1]);
            int dia1 = int.Parse(fechaPartes1[2]);

            DateTime fecha1 = new DateTime(año1, mes1, dia1);

            // Hacer lo mismo para la segunda fecha
            string fechaSolo2 = clienteBusqueda.periodo2.Split('T')[0];
            string[] fechaPartes2 = fechaSolo2.Split('-');
            int año2 = int.Parse(fechaPartes2[0]);
            int mes2 = int.Parse(fechaPartes2[1]);
            int dia2 = int.Parse(fechaPartes2[2]);

            DateTime fecha2 = new DateTime(año2, mes2, dia2);



            //  var clientesBusqueda = db.UP_COTIZACIONE_LOG_ENVIOS(clienteBusqueda.periodo1, clienteBusqueda.periodo2, cantidad).ToList();
            var clientesBusqueda = db.UP_COTIZACIONE_VENCIDAS_X_VENCER_DETALLE(fecha1, fecha2, clienteBusqueda.idCliente).ToList();


            // Mapea los resultados a un formato JSON
            var resultClientes = clientesBusqueda.Select(r => new
            {
                id_cotizacion = r.id_cotizacion,
                nombre_cotizacion = r.nombre_cotizacion,
                fecha_alta = r.fecha_alta,
                fecha_vencimiento = r.fecha_vencimiento,
                nombre_cliente = r.nombre_cliente,
                counter_cliente = r.counter_cliente,
                dias_vencimiento = r.dias_vencimiento,
                fecha_viaje = r.fecha_viaje,
                counter_rge = r.counter_rge,
                telefono = r.telefono,
                correo = r.correo_electronico,
                idCliente = r.id_cliente,
                estado = r.estado,
                fechaNotificacion = r.fecha_notificacion,
                fechaSeguimiento = r.fecha_seguimiento,
                fechaSeguimientoCorreo = r.fecha_mensaje_correo,
                urldescarga = r.url_descarga
                //  fechaSeguimiento = r.fecha_notificacion

            }).ToList();

            return Content(JsonConvert.SerializeObject(resultClientes), "application/json");
        }
      




        public JsonResult Datos(Parametros parametros)
        {
            try
            {
                var db = new SIAV_prod_4Entities();
                int idEmpresa = SessionUtils.ObtenerIdEmpresaDeSesion(Session);

                string region = parametros.Region ?? "";
                string pais = parametros.Pais ?? "";
                string cliente = parametros.Cliente ?? "";
                string estado = parametros.Estado ?? "";


                //var result = db.up_web_rge_lista_venta_x_cliente_x_meses(parametros.Region, parametros.Pais, parametros.Cliente,
                //   parametros.Estado, parametros.Periodo1,parametros.Periodo2,parametros.Modo).ToList();

                  var result = db.up_web_rge_lista_cotizaciones_x_cliente_x_meses(region, pais, cliente,
                  estado, parametros.Periodo1, parametros.Periodo2).ToList();

                var regions = result
               .GroupBy(r => new { r.id_region, r.region_nombre })
               .Select(g => new
               {
                   id = g.Key.id_region,
                   name = g.Key.region_nombre,
                   countries = g.GroupBy(c => new { c.id_pais, c.pais_nombre })
                                .Select(cg => new
                                {
                                    id = cg.Key.id_pais,
                                    name = cg.Key.pais_nombre,
                                    clients = cg.GroupBy(cl => new { cl.id_cliente, cl.cliente_nombre })
                                                .Select(clg => new
                                                {
                                                    id = clg.Key.id_cliente,
                                                    name = clg.Key.cliente_nombre,
                                                    values = clg.ToDictionary(
                                                        v => v.id_mes,
                                                        v => new
                                                        {
                                                            total_cantidad_solicitadas = v.cantidad_solicitadas,
                                                            total_catindad_a_revisar = v.catindad_a_revisar,
                                                            total_cantidad_vencidas = v.cantidad_vencidas,
                                                            
                                                        }
                                                    )
                                                }).ToList()
                                }).ToList()
               }).ToList();

                //// Serializar la lista de resultados como JSON
                string jsonRegiones = JsonConvert.SerializeObject(regions, Formatting.Indented);

                return Json(new { data = jsonRegiones, success = true }, JsonRequestBehavior.AllowGet);

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpGet]
        public JsonResult BuscarClientes(string term)
        {
            try
            {
                var db = new SIAV_prod_4Entities();
                // Por ejemplo, supongamos que tienes una lista de hoteles en una variable hoteles
                var (idUsuario, idEmpresa) = SessionUtils.ObtenerValoresDeSesion(Session);


                var clientes = db.up_web_lista_clientes(0);

                // Convertir el término de búsqueda a minúsculas
                term = term.ToLower();

                // Filtra los hoteles que coincidan con el término de búsqueda (ignorando mayúsculas y minúsculas)
                //  var resultados = clientes.Where(h => (h.Nombre ?? "").ToLower().Contains(term)).ToList();

                // Filtrar los clientes que coincidan con el término de búsqueda (ignorando mayúsculas y minúsculas)
                var resultados = clientes
                    .Where(h => (h.Nombre ?? "").ToLower().Contains(term))
                    .Select(h => new
                    {
                        id = h.id_cliente,
                        text = h.Nombre,

                    })
                    .ToList();


                // Devuelve los resultados como JSON
                return Json(resultados, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }


        [HttpPost]
        public JsonResult ClienteBusquedaInical()
        {
            try
            {
                var db = new SIAV_prod_4Entities();
                int idEmpresa = SessionUtils.ObtenerIdEmpresaDeSesion(Session);

                string cantidad = "";
                // Obtener la fecha actual y calcular la fecha de hace 6 meses
                DateTime fechaFin = DateTime.Today; // Fecha de fin es hoy
                DateTime fechaInicio = fechaFin.AddMonths(-6); // Fecha de inicio es 6 meses atrás


                var clientesBusquedaVencidas = db.UP_COTIZACIONE_VENCIDAS_X_VENCER(fechaInicio,fechaFin, cantidad).ToList();
                // var clientesBusqueda = db.UP_COTIZACIONE_VENCIDAS_X_VENCER(fechaInicio, fechaFin, cantidad).ToList();
                // Mapea los resultados a un formato JSON
                var resultClientesVencidas = clientesBusquedaVencidas.Select(r => new
                {
                    Cliente = r.cliente,
                    Pais = r.pais,
                    C13xvencer = r.C1_3_x_vencer,
                    C46xvencer = r.C4_6_x_vencer,
                    C7xvencer = r.C7_x_vencer,
                    Vence_hoy = r.vence_hoy,
                    C13xvencida = r.C1_3_x_vencida,
                    C46xvencida = r.C4_6_x_vencida,
                    C7xvencida = r.C7_x_vencida  ,
                    id_Cliente=r.id_cliente,
                    fechadesde=r.fecha_desde,
                    fechahasta=r.fecha_hasta,
                    notificados = r.notificados

                }).ToList();

                string jsonResultVencidas = JsonConvert.SerializeObject(resultClientesVencidas);

                ///////// por Semana
                // Obtener la fecha de inicio como el lunes de la semana actual
                DateTime today = DateTime.Today;
                // Ajusta para que el lunes sea siempre el primer día de la semana
                int daysSinceMonday = (today.DayOfWeek == DayOfWeek.Sunday) ? -6 : (int)today.DayOfWeek - (int)DayOfWeek.Monday;
                DateTime fechaInicioSemana = today.AddDays(-daysSinceMonday);

                // Calcular la fecha de fin de semana como el domingo de la misma semana
                DateTime fechaFinSemana = fechaInicioSemana.AddDays(6); // Esto establece la fecha al domingo de la misma semana

                var clientesBusquedaSemana = db.UP_COTIZACIONE_CREADAS_SEMANA(fechaInicioSemana, fechaFinSemana, cantidad).ToList();

                // Mapea los resultados a un formato JSON
                var resultClientesSemana = clientesBusquedaSemana.Select(r => new
                {
                    Cliente = r.cliente,
                    Pais = r.pais,
                    Domingo = r.domingo,
                    Lunes = r.lunes,
                    Martes = r.martes,
                    Miercoles = r.miercoles,
                    Jueves = r.jueves,
                    Viernes = r.viernes,
                    Sabado = r.sabado ,
                    id_cliente =r.id_cliente,
                    fecha_desde  =r.fecha_desde,
                    fecha_hasta =r.fecha_hasta


                }).ToList();

                string jsonResultSemana = JsonConvert.SerializeObject(resultClientesSemana);

                /////////// Log 
                ///  // Calcular el rango de fechas: desde hace 14 días hasta hoy
                DateTime fechaFinLog = DateTime.Today;  // Hoy
                DateTime fechaInicioLog = fechaFin.AddDays(-14); // 14 días atrás desde hoy

                var clientesBusquedaLog = db.UP_COTIZACIONE_LOG_ENVIOS(fechaFinLog, fechaInicioLog, cantidad).ToList();

                // Mapea los resultados a un formato JSON
                var resultClientesLog = clientesBusquedaLog.Select(r => new
                {
                    Cliente = r.cliente,
                    Pais = r.pais,
                    nombreCotizacion = r.nombre_cotizacion,
                    inicioViaje = r.inicio_viaje,
                    finViaje = r.fin_viaje,
                    fechaNotificacion1 = r.fecha_notificacion_1,
                    respuestaNotificacion1 = r.respuesta_notificacion_1,
                    fechaNotificacion2 = r.fecha_notificacion_2,
                    respuestaNotificacion2 = r.respuesta_notificacion_2

                }).ToList();

                string jsonResultLog = JsonConvert.SerializeObject(resultClientesLog);
                return Json(new { dataVencidas = jsonResultVencidas, dataSemana= jsonResultSemana, dataLog= jsonResultLog, success = true, selector = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }




        [HttpPost]
        public JsonResult ClientesBusquedaCotizacionesVencidas(ClienteBusqueda2 clienteBusqueda)
        {
            try
            {
                var db = new SIAV_prod_4Entities();
                int idEmpresa = SessionUtils.ObtenerIdEmpresaDeSesion(Session);

                string cantidad = clienteBusqueda.cliente ?? "";

           //     DateTime fehca1 = new DateTime(clienteBusqueda.periodo1.Split('-')[2]);

                string[] fechaPartes = clienteBusqueda.periodo1.Split('-');
                int año = int.Parse(fechaPartes[2]);
                int mes = int.Parse(fechaPartes[1]);
                int dia = int.Parse(fechaPartes[0]);

                DateTime fecha1 = new DateTime(año, mes, dia);


                string[] fechaPartes2 = clienteBusqueda.periodo2.Split('-');
                int año1 = int.Parse(fechaPartes2[2]);
                int mes1 = int.Parse(fechaPartes2[1]);
                int dia1 = int.Parse(fechaPartes2[0]);

                DateTime fecha2 = new DateTime(año1, mes1, dia1);


                // var clientesBusqueda = db.UP_COTIZACIONE_VENCIDAS_X_VENCER(clienteBusqueda.periodo1, clienteBusqueda.periodo2, cantidad).ToList();
                var clientesBusqueda = db.UP_COTIZACIONE_VENCIDAS_X_VENCER(fecha1, fecha2, cantidad).ToList();

                // var clientesBusqueda = db.UP_COTIZACIONE_VENCIDAS_X_VENCER(fechaInicio, fechaFin, cantidad).ToList();
                // Mapea los resultados a un formato JSON
                var resultClientes = clientesBusqueda.Select(r => new
                    {
                        Cliente = r.cliente,
                        Pais = r.pais,
                        C13xvencer = r.C1_3_x_vencer,
                        C46xvencer = r.C4_6_x_vencer,
                        C7xvencer = r.C7_x_vencer,
                        Vence_hoy = r.vence_hoy,
                        C13xvencida = r.C1_3_x_vencida,
                        C46xvencida = r.C4_6_x_vencida,
                        C7xvencida = r.C7_x_vencida ,
                        id_Cliente = r.id_cliente,
                        fechadesde = r.fecha_desde,
                        fechahasta = r.fecha_hasta ,
                        notificados=r.notificados

                }).ToList();

                string jsonResult = JsonConvert.SerializeObject(resultClientes);

                return Json(new { data= jsonResult, success = true, selector = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }


        [HttpPost]
        public JsonResult ClientesBusquedaCotizacionesSemana(ClienteBusqueda2 clienteBusqueda)
        {
            try
            {
                var db = new SIAV_prod_4Entities();
                int idEmpresa = SessionUtils.ObtenerIdEmpresaDeSesion(Session);

                string cantidad = clienteBusqueda.cliente ?? "";
                //string filtro = ClienteBusqueda.filtro ?? "";
                // string cliente = ClienteBusqueda.Cliente ?? "";
                //  string estado = parametros.Estado ?? "";

                string[] fechaPartes = clienteBusqueda.periodo1.Split('-');
                int año = int.Parse(fechaPartes[2]);
                int mes = int.Parse(fechaPartes[1]);
                int dia = int.Parse(fechaPartes[0]);

                DateTime fecha1 = new DateTime(año, mes, dia);


                string[] fechaPartes2 = clienteBusqueda.periodo2.Split('-');
                int año1 = int.Parse(fechaPartes2[2]);
                int mes1 = int.Parse(fechaPartes2[1]);
                int dia1 = int.Parse(fechaPartes2[0]);

                DateTime fecha2 = new DateTime(año1, mes1, dia1);


                //  var clientesBusqueda = db.UP_COTIZACIONE_CREADAS_SEMANA(clienteBusqueda.periodo1, clienteBusqueda.periodo2, cantidad).ToList();
                var clientesBusqueda = db.UP_COTIZACIONE_CREADAS_SEMANA(fecha1, fecha2, cantidad).ToList();

                // Mapea los resultados a un formato JSON
                var resultClientes = clientesBusqueda.Select(r => new
                {
                    Cliente = r.cliente,
                    Pais = r.pais,
                    Domingo = r.domingo,
                    Lunes = r.lunes,
                    Martes = r.martes,
                    Miercoles = r.miercoles,
                    Jueves = r.jueves,
                    Viernes = r.viernes,
                    Sabado = r.sabado,
                    id_cliente = r.id_cliente,
                    fecha_desde = r.fecha_desde,
                    fecha_hasta = r.fecha_hasta

                }).ToList();

                string jsonResult = JsonConvert.SerializeObject(resultClientes);


                return Json(new { data= jsonResult, success = true, selector = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }




        [HttpPost]
        public JsonResult ClientesBusquedaCotizacionesLog(ClienteBusqueda2 clienteBusqueda)
        {
            try
            {
                var db = new SIAV_prod_4Entities();
             
               

                string cantidad = clienteBusqueda.cliente ?? "";
                //string filtro = ClienteBusqueda.filtro ?? "";
                // string cliente = ClienteBusqueda.Cliente ?? "";
                //  string estado = parametros.Estado ?? "";
                string[] fechaPartes = clienteBusqueda.periodo1.Split('-');
                int año = int.Parse(fechaPartes[2]);
                int mes = int.Parse(fechaPartes[1]);
                int dia = int.Parse(fechaPartes[0]);

                DateTime fecha1 = new DateTime(año, mes, dia);


                string[] fechaPartes2 = clienteBusqueda.periodo2.Split('-');
                int año1 = int.Parse(fechaPartes2[2]);
                int mes1 = int.Parse(fechaPartes2[1]);
                int dia1 = int.Parse(fechaPartes2[0]);

                DateTime fecha2 = new DateTime(año1, mes1, dia1);



              //  var clientesBusqueda = db.UP_COTIZACIONE_LOG_ENVIOS(clienteBusqueda.periodo1, clienteBusqueda.periodo2, cantidad).ToList();
                var clientesBusqueda = db.UP_COTIZACIONE_LOG_ENVIOS(fecha1, fecha2, cantidad).ToList();


                // Mapea los resultados a un formato JSON
                var resultClientes = clientesBusqueda.Select(r => new
                {
                    Cliente = r.cliente,
                    Pais = r.pais,
                    nombreCotizacion = r.nombre_cotizacion,
                    inicioViaje = r.inicio_viaje,
                    finViaje = r.fin_viaje,
                    fechaNotificacion1 = r.fecha_notificacion_1,
                    respuestaNotificacion1 = r.respuesta_notificacion_1,
                    fechaNotificacion2 = r.fecha_notificacion_2,
                    respuestaNotificacion2 = r.respuesta_notificacion_2 ,
                    telefono=r.telefono,
                    correoParte1 = r.correo_electronico.Contains("@") ? r.correo_electronico.Split('@')[0] : r.correo_electronico,
                    correoParte2 = r.correo_electronico.Contains("@") ? "@" + r.correo_electronico.Split('@')[1] : "",
                    idCliente = r.id_cliente,
                    counterCliente=r.counter_cliente,
                    conterRGE=r.counter_rge

                }).ToList();

                string jsonResult = JsonConvert.SerializeObject(resultClientes);

                return Json(new { data=jsonResult, success = true, selector = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }



        [HttpPost]
        public JsonResult ObtenerDatosVencidasVencerporId(ClienteBusqueda2 clienteBusqueda)
        {
            try
            {
                var db = new SIAV_prod_4Entities();
                var dback = new BackOffice_WebEntities();
                var (idUsuario, idEmpresa) = SessionUtils.ObtenerValoresDeSesion(Session);
            
                var rutaImagenNombreId = dback.obtenerRutaImagePorIdUsuario(idUsuario).FirstOrDefault();

        

                string cantidad = clienteBusqueda.cliente ?? "";
                //string filtro = ClienteBusqueda.filtro ?? "";
                // string cliente = ClienteBusqueda.Cliente ?? "";
                //  string estado = parametros.Estado ?? "";
                // Extraer la parte de la fecha (ignorar la parte de la hora)
                string fechaSolo1 = clienteBusqueda.periodo1.Split('T')[0];
                string[] fechaPartes1 = fechaSolo1.Split('-');
                int año1 = int.Parse(fechaPartes1[0]);
                int mes1 = int.Parse(fechaPartes1[1]);
                int dia1 = int.Parse(fechaPartes1[2]);

                DateTime fecha1 = new DateTime(año1, mes1, dia1);

                // Hacer lo mismo para la segunda fecha
                string fechaSolo2 = clienteBusqueda.periodo2.Split('T')[0];
                string[] fechaPartes2 = fechaSolo2.Split('-');
                int año2 = int.Parse(fechaPartes2[0]);
                int mes2 = int.Parse(fechaPartes2[1]);
                int dia2 = int.Parse(fechaPartes2[2]);

                DateTime fecha2 = new DateTime(año2, mes2, dia2);



                //  var clientesBusqueda = db.UP_COTIZACIONE_LOG_ENVIOS(clienteBusqueda.periodo1, clienteBusqueda.periodo2, cantidad).ToList();
                var clientesBusqueda = db.UP_COTIZACIONE_VENCIDAS_X_VENCER_DETALLE(fecha1, fecha2, clienteBusqueda.idCliente).ToList();

        
                // Mapea los resultados a un formato JSON
                var resultClientes = clientesBusqueda.Select(r => new
                {
                    id_cotizacion = r.id_cotizacion,
                    nombre_cotizacion = r.nombre_cotizacion,
                    fecha_alta = r.fecha_alta,
                    fecha_vencimiento = r.fecha_vencimiento,
                    nombre_cliente = r.nombre_cliente,
                    counter_cliente = r.counter_cliente ,
                    dias_vencimiento=r.dias_vencimiento ,
                    fecha_viaje=r.fecha_viaje ,
                    counter_rge=r.counter_rge  ,
                    telefono=r.telefono,
                    correo=r.correo_electronico,
                    idCliente=r.id_cliente ,
                    estado=r.estado,
                    fechaNotificacion=r.fecha_notificacion,
                    fechaSeguimiento = r.fecha_seguimiento,
                    fechaSeguimientoCorreo=r.fecha_mensaje_correo,
                    urldescarga =r.url_descarga
                  //  fechaSeguimiento = r.fecha_notificacion

                }).ToList();

             //  string jsonrutaImagenNombreId = JsonConvert.SerializeObject(rutaImagenNombreId);

                return Json(new { data = resultClientes, rutaImagenNombreId = rutaImagenNombreId, success = true, selector = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                throw;
            }
        }

        [HttpPost]
        public JsonResult EnviarCorreo(ClienteBusqueda2 clienteBusqueda)
        {
            try
            {
                using (var db = new SIAV_prod_4Entities())
                {
                    var dback = new BackOffice_WebEntities();

                    var (idUsuario, idEmpresa) = SessionUtils.ObtenerValoresDeSesion(Session);

                    var rutaImagenNombreId = dback.obtenerRutaImagePorIdUsuario(idUsuario).FirstOrDefault();

                    // Obtener los correos previos para la cotización
                    var correo = db.ObtenerCorreosCotizaciones(idEmpresa, clienteBusqueda.idCotizacion).FirstOrDefault();

                    // Construir el nuevo objeto de correo
                    var objetoCorreo = new
                    {
                        nombreRemitente = rutaImagenNombreId.nombre, // Nombre del remitente
                        avatarRemitente = rutaImagenNombreId.ruta_relativa, // Ruta de la imagen
                        emailDestinatario = clienteBusqueda.correo, // Correo destinatario
                        fecha = DateTime.Now.ToString("dd MMM yyyy, hh:mm tt"), // Fecha y hora actual
                        mensaje = new[] { clienteBusqueda.mensaje } // El mensaje en un array
                    };

                    string conversaciones = string.Empty;

                    if (correo == "[]" || correo =="" || correo == null)
                    {
                      
                        // Si no hay correos previos, usar el nuevo objetoCorreo
                        conversaciones = JsonConvert.SerializeObject(new[] { objetoCorreo });
                        // Actualizar la base de datos con la nueva lista de correos

                        db.InsertarCorreoCotizacion(idEmpresa, clienteBusqueda.idCotizacion, conversaciones, DateTime.Now);
                    }
                    else
                    {       // Si hay correos previos, deserializar los existentes
                        var listaCorreosPrevios = JsonConvert.DeserializeObject<List<dynamic>>(correo);
                        listaCorreosPrevios.Add(objetoCorreo); // Agregar el nuevo correo a la lista

                        // Volver a serializar la lista completa
                        conversaciones = JsonConvert.SerializeObject(listaCorreosPrevios);

                        db.ActualizarCorreosCotizaciones(idEmpresa, clienteBusqueda.idCotizacion, conversaciones, DateTime.Now);

                    }

                    var Jsoncorreo = db.ObtenerCorreosCotizaciones(idEmpresa, clienteBusqueda.idCotizacion).FirstOrDefault();

                    // Notificar al cliente
                      db.up_web_notifica_cliente_email_cotizacion(clienteBusqueda.idCotizacion, clienteBusqueda.correo, clienteBusqueda.mensaje);

                    return Json(new { data= Jsoncorreo,success = true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Devolver el mensaje de error en caso de excepción
                return Json(new { success = false, errorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }




        // este metodo se realizo para verificalr los clicks de los clientes al darle "lo quiero" o "rechazar"


        [HttpGet]
        public ActionResult RegistrarClick(int idCotizacion, string respuesta, string email)
        {
            try
            {
                using (var siav = new SIAV_prod_4Entities())
                {
                    // Get current date/time
                    DateTime fechaClick = DateTime.Now;

                    // Aquí id empresa podría ser cero porque no es necesario la sesión
                    var (idUsuario, idEmpresa) = SessionUtils.ObtenerValoresDeSesion(Session);

                    // Check if this client has already clicked for this quote
                    var clickPrevio = siav.VerificarClickPrevio(idCotizacion, email).FirstOrDefault();

                    if (clickPrevio == null)
                    {
                        // First click from this client for this quote
                        siav.RegistrarClickCotizacion(idEmpresa, idCotizacion, respuesta, email, fechaClick, true);
                    }
                    else
                    {
                        // Client already clicked - update their response instead of creating a new record
                        siav.ActualizarClickCotizacion(clickPrevio.ID, respuesta, fechaClick);
                    }

                    // Si la respuesta es "Rechazado", no redirigir a la vista, sino mostrar una página de confirmación o simplemente terminar
                    if (respuesta.Equals("Rechazado", StringComparison.OrdinalIgnoreCase))
                    {
                        // Opción 1: Redirigir a una página de agradecimiento simple
                        return RedirectToAction("RespuestaRechazada", "VistaCorreoDashboardCotizaciones", new { idCotizacion = idCotizacion , email= email });

                        // Opción 2: O mostrar una página simple con un mensaje
                        // return Content("<html><body><h2>Gracias por su respuesta</h2><p>Hemos registrado su decisión.</p></body></html>", "text/html");
                    }

                    // Si la respuesta es "Aceptado", redirigir a la vista original
                    return RedirectToAction("Index", "VistaCorreoDashboardCotizaciones", new { id = idCotizacion });
                }
            }
            catch (Exception ex)
            {
                // Log the error
                System.Diagnostics.Debug.WriteLine($"Error al registrar click: {ex.Message}");

                // Still redirect to avoid error page for the user, pero dependiendo de la respuesta
                if (respuesta.Equals("Rechazado", StringComparison.OrdinalIgnoreCase))
                {
                    return Content("<html><body><h2>Gracias por su respuesta</h2><p>Hemos registrado su decisión.</p></body></html>", "text/html");
                }

                return RedirectToAction("Index", "VistaCorreoDashboardCotizaciones", new { id = idCotizacion });
            }
        }


      







        [HttpPost]
        public JsonResult EnviarCorreoDirecto(CorreoReservaViewModel request)
        {
            try
            {
                using (var dback = new BackOffice_WebEntities())
                {


                    var (idUsuario, idEmpresa) = SessionUtils.ObtenerValoresDeSesion(Session);

                    var siav = new SIAV_prod_4Entities();


                    var respuesta = siav.up_retorna_cabecera_cotizacion(request.idCotizacion).FirstOrDefault();

                    string fechaInicio = respuesta?.fecha_desde?.ToString("dd/MM/yyyy") ?? "N/A";
                    string fechaFin = respuesta?.fecha_hasta?.ToString("dd/MM/yyyy") ?? "N/A";


                    var emailService = new EmailService();
                    string mensajeHtml = "";

                    string baseUrl = Request.Url.Scheme + "://" + Request.Url.Authority;

                 

                    string urlSi = $"{baseUrl}/DASHBOARDSTYLE2/RegistrarClick?idCotizacion={request.idCotizacion}&respuesta=Aceptado&email={Uri.EscapeDataString(request.correo)}";
                    string urlNo = $"{baseUrl}/DASHBOARDSTYLE2/RegistrarClick?idCotizacion={request.idCotizacion}&respuesta=Rechazado&email={Uri.EscapeDataString(request.correo)}";

                    // Generar HTML
                    mensajeHtml = $@" 

<div class="""">
    <div class=""aHl""></div>
    <div id="":26"" tabindex=""-1""></div>
    <div id="":1w"" class=""ii gt"" jslog=""20277; u014N:xr6bB; 1:WyIjdGhyZWFkLWY6MTgyNDcwMTQzNjE2NjU3OTQzNSJd; 4:WyIjbXNnLWY6MTgyNDcwMTQzNjE2NjU3OTQzNSIsbnVsbCxudWxsLG51bGwsMSwwLFsxLDAsMF0sMTQ1LDEwMDksbnVsbCxudWxsLG51bGwsbnVsbCxudWxsLDEsbnVsbCxudWxsLFszLDBdLG51bGwsbnVsbCxudWxsLG51bGwsbnVsbCxudWxsLDBd"">
        <div id="":1v"" class=""a3s aiL msg8691105329732021871"">
            <u></u>
            <div marginheight=""0"" marginwidth=""0"" style=""background-color:#f7f7f9 ; padding-top:8px ; margin-bottom:8px"">
                <div></div>
               
               
                <table bgcolor=""#ffffff"" align=""center"" style=""max-width:600px;width:100%!important;table-layout:fixed;font-family:Arial,sans-serif"">
                    <tbody>
                        <tr>
                            <td colspan=""2"" style=""color:#353e4a;font-family:'Arial';font-size:22px;text-align:center;padding-top:40px"">
                                <a href="""" title="""" style=""color:#353e4a;text-decoration:none;font-family:'Arial';font-size:22px"" target=""_blank"" >
                                    <img height=""33"" alt="""" style=""width:100%; max-width:600px; height:auto;"" src=""http://ec2-18-119-38-210.us-east-2.compute.amazonaws.com/assets/imagenesStyle/playapan.png"" class=""CToWUd"" data-bit=""iit"">
                                </a>
                            </td>
                        </tr>
                        <tr>
                            <td colspan=""2"" style=""color:#353e4a;text-align:center;font-size:16px;line-height:23px;padding-right:10px;padding-left:10px;padding-bottom:10px;padding-top:20px"">
                             <strong>  Estimado Cliente </strong>
                            </td>
                        </tr>
                        <tr>
                            <td colspan=""2"" style=""color:#353e4a;text-align:center;font-size:16px;line-height:23px;padding-right:10px;padding-left:10px;padding-bottom:0px;padding-top:10px"">
                               Soy {respuesta.usuario_cotiza}  
                            </td>
                        </tr>
                         <tr>
                            <td colspan=""2"" style=""color:#353e4a;text-align:center;font-size:16px;line-height:23px;padding-right:10px;padding-left:10px;padding-bottom:20px;padding-top:10px"">
                               ¡y tenemos todo listo para hacer de tu viaje una experiencia inolvidable!
                            </td>
                        </tr>
                       <tr>
                        <td colspan=""2"" style=""color:#353e4a;text-align:center;font-size:16px;line-height:23px;padding-right:10px;padding-left:10px;padding-bottom:20px"">
                             Hemos preparado para ti <b>una cotización especial</b>  con los mejores destinos , alojamientos  y actividades  adaptadas a tus necesidades.  
                        </td>
                    </tr>

                        <tr></tr>
                      
<tr>
    <td colspan=""2"" style=""color:#353e4a;text-align:center;font-size:16px;line-height:23px;padding-right:10px;padding-left:10px;padding-bottom:20px"">
        <table align=""center"" style=""width: 100%; max-width: 500px; border-collapse: collapse; margin: 0 auto;"">
            <tbody>
                <!-- Cabecera NRO. RESERVA -->
                <tr>
                    <td style=""background-color: #0f40af; color: #ffffff; padding: 10px; text-align: center; font-size: 14px; font-weight: bold;"">
                        NRO. RESERVA
                    </td>
                </tr>
                <!-- Detalle NRO. RESERVA -->
                <tr>
                    <td style=""border: 1px solid #dcdfe8; padding: 10px; text-align: center; font-size: 14px; color: #353e4a;"">
                        {respuesta.id_cotizacion}
                    </td>
                </tr>
               
                <!-- Cabecera PAXS -->
                <tr>
                    <td style=""background-color: #0f40af; color: #ffffff; padding: 10px; text-align: center; font-size: 14px; font-weight: bold;"">
                        PAXS
                    </td>
                </tr>
                <!-- Detalle PAXS -->
                <tr>
                    <td style=""border: 1px solid #dcdfe8; padding: 10px; text-align: center; font-size: 14px; color: #353e4a;"">
                        {respuesta.nombre_cotizacion}
                    </td>
                </tr>
                <!-- Fechas -->
                <tr>
                    <td style=""border: 1px solid #dcdfe8; padding: 10px; text-align: center; font-size: 14px; color: #353e4a;background-color:antiquewhite;"">
                        <strong>FECHA ENTRADA:</strong> {fechaInicio} - <strong>FECHA SALIDA:</strong> {fechaFin}
                    </td>
                </tr>
                <!-- Fecha límite -->
                <tr>
                    <td style=""border: 1px solid #dcdfe8; padding: 10px; text-align: center; font-size: 14px; color: #353e4a;background-color:gold"">
                        <strong>FECHA LÍMITE PAGO/RECONFIRMACIÓN:</strong> {respuesta.fecha_vencimiento?.ToString("dd/MM/yyyy")}
                    </td>
                </tr>
            </tbody>
        </table>
    </td>
</tr>

                       
                        <tr>
                            <td colspan=""2"" style=""border-top:1px solid #dcdfe8""></td>
                        </tr>
                        
                        <tr>
                            <td colspan=""2"" style=""color:#a0a6b5;font-size:13px;padding-bottom:10px;padding-top:10px;text-align:center;line-height:18px;padding-left:10px;padding-right:10px"">
                               Acepte para poder ver el resumen de la Cotizacion.
                            </td>
                        </tr>
 <tr>
                            <td colspan=""2"" style=""color:#a0a6b5;font-size:12px;padding-bottom:10px;text-align:center;line-height:18px;padding-left:10px;padding-right:10px"">
                                ¡Nos encantará acompañarte en esta aventura!
                            </td>
                        </tr>
                        <tr>
                            <td colspan=""1"" align=""right"" style=""padding-bottom:20px;padding-top:10px;padding-right:8px"">
                                 <a href=""{urlSi}"" style=""text-align:center;font-style:normal;font-weight:normal;line-height:1.15;text-decoration:none;word-break:break-word;border-style:solid;word-wrap:break-word;display:block;background-color:#0f40af;border-color:#c30f0f;border-radius:10px;border-width:0px;color:#ffffff;font-family:arial,helvetica,sans-serif;font-size:16px;height:18px;padding-bottom:15px;padding-top:15px;width:159px"" target=""_blank"" >
                                    <span>
                                        Ver Cotizacion
                                    </span>
                                </a>
                            </td>
                            <td colspan=""1"" align=""left"" style=""padding-bottom:20px;padding-top:10px;padding-left:8px"">
                                 <a href=""{urlNo}"" style=""text-align:center;font-style:normal;font-weight:normal;line-height:1.15;text-decoration:none;word-break:break-word;border-style:solid;word-wrap:break-word;display:block;background-color:#c30f0f;border-color:#c30f0f;border-radius:10px;border-width:0px;color:#ffffff;font-family:arial,helvetica,sans-serif;font-size:16px;height:18px;padding-bottom:15px;padding-top:15px;width:159px"" target=""_blank"" >
                                <span>
                                   No lo quiero
                                </span>
                            </a>
                            </td>
                        </tr>
                       
                        <tr>
                            <td colspan=""2"" style=""color:#a0a6b5;font-size:12px;padding-bottom:10px;text-align:center;line-height:18px"">
    <strong>Style Travel</strong> es una empresa líder en el sector turístico, especializada en ofrecer experiencias de viaje inolvidables. 
    Con <strong>Style Travel</strong>, viajar es sinónimo de tranquilidad, calidad y excelencia. 🚀✨  
</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
        <div class=""yj6qo""></div>
    </div>
    <div class=""WhmR8e"" data-hash=""0""></div>
</div>
";
                    string subject = $"Cotizacion N° {respuesta.id_cotizacion}";
                    // Enviar correo

                    var usuario = dback.ObtenerElUsuarioPorId(idUsuario).FirstOrDefault();



                 //   emailService.EnviarCorreo(request.correo, subject, mensajeHtml);

                   // emailService.EnviarCorreoDesdeUsuario(usuario.Correo, usuario.passCorreoUsuario, usuario.Nombre, request.correo, subject, mensajeHtml);

                    //emailService.EnviarCorreo(request.correo, subject, mensajeHtml);

                    // se modifico el procedimiento de envio de correo de Raul para q envie conel correo del q creo la reserva , para enviar tambien el html en el controlador
                    siav.up_web_notifica_cliente_email_cotizacion_2(respuesta.id_cotizacion, request.correo, mensajeHtml);

                    var rutaImagenNombreId = dback.obtenerRutaImagePorIdUsuario(idUsuario).FirstOrDefault();

                    // Obtener los correos previos para la cotización
                    var correo = siav.ObtenerCorreosCotizaciones(idEmpresa, request.idCotizacion).FirstOrDefault();

                    // Construir el nuevo objeto de correo
                    var objetoCorreo = new
                    {
                        nombreRemitente = rutaImagenNombreId.nombre, // Nombre del remitente
                        avatarRemitente = rutaImagenNombreId.ruta_relativa, // Ruta de la imagen
                        emailDestinatario = request.correo, // Correo destinatario
                        fecha = DateTime.Now.ToString("dd MMM yyyy, hh:mm tt"), // Fecha y hora actual
                        mensaje = new[] { request.mensaje } // El mensaje en un array
                    };

                    string conversaciones = string.Empty;

                    if (correo == "[]" || correo == "" || correo == null)
                    {

                        // Si no hay correos previos, usar el nuevo objetoCorreo
                        conversaciones = JsonConvert.SerializeObject(new[] { objetoCorreo });
                        // Actualizar la base de datos con la nueva lista de correos

                        siav.InsertarCorreoCotizacion(idEmpresa, request.idCotizacion, conversaciones, DateTime.Now);
                    }
                    else
                    {       // Si hay correos previos, deserializar los existentes
                        var listaCorreosPrevios = JsonConvert.DeserializeObject<List<dynamic>>(correo);
                        listaCorreosPrevios.Add(objetoCorreo); // Agregar el nuevo correo a la lista

                        // Volver a serializar la lista completa
                        conversaciones = JsonConvert.SerializeObject(listaCorreosPrevios);

                        siav.ActualizarCorreosCotizaciones(idEmpresa, request.idCotizacion, conversaciones, DateTime.Now);

                    }

                    //if ( correo != null)
                    //{
                    //    // Si hay correos previos, deserializar los existentes
                    //    var listaCorreosPrevios = JsonConvert.DeserializeObject<List<dynamic>>(correo);
                    //    listaCorreosPrevios.Add(objetoCorreo); // Agregar el nuevo correo a la lista

                    //    // Volver a serializar la lista completa
                    //    conversaciones = JsonConvert.SerializeObject(listaCorreosPrevios);

                    //    siav.ActualizarCorreosCotizaciones(idEmpresa, request.idCotizacion, conversaciones, DateTime.Now);
                    //}
                    //else
                    //{
                    //    // Si no hay correos previos, usar el nuevo objetoCorreo
                    //    conversaciones = JsonConvert.SerializeObject(new[] { objetoCorreo });
                    //    // Actualizar la base de datos con la nueva lista de correos

                    //    siav.InsertarCorreoCotizacion(idEmpresa, request.idCotizacion, conversaciones, DateTime.Now);
                    //}
                    var Jsoncorreo = siav.ObtenerCorreosCotizaciones(idEmpresa, request.idCotizacion).FirstOrDefault();

                    return Json(new {data= Jsoncorreo, success = true, message = "El correo se envió correctamente." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }







        [HttpPost]
        public JsonResult ObtenerConversacion(ClienteBusqueda2 clienteBusqueda)
        {
            try
            {
                using (var db = new SIAV_prod_4Entities())
                {
                    int idEmpresa = SessionUtils.ObtenerIdEmpresaDeSesion(Session);
                    var jsonConversacionesResult = db.obtenerIdCotizacion(clienteBusqueda.idCotizacion, idEmpresa).FirstOrDefault();
                    var jsonConversaciones = jsonConversacionesResult?.jsonConversaciones ?? "[]";

                    var Jsoncorreo = db.ObtenerCorreosCotizaciones(idEmpresa, clienteBusqueda.idCotizacion).FirstOrDefault();

                    return Json(new { success = true , jsonConversaciones = jsonConversaciones ,data= Jsoncorreo }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Devolver el mensaje de error en caso de excepción
                return Json(new { success = false, errorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // OBTIENE LA CONVERSACION DE LOS MENSAJES DEL CHAT WHASSAP
        [HttpPost]
        public JsonResult ObetnerSerializedMaytapi(ClienteBusqueda2 clienteBusqueda)
        {
            try
            {
                using (var db = new SIAV_prod_4Entities())
                {
                    int idEmpresa = SessionUtils.ObtenerIdEmpresaDeSesion(Session);
                    var jsonConversaciones = db.obtenerIdCotizacion(clienteBusqueda.idCotizacion, idEmpresa).FirstOrDefault().jsonConversaciones;

                    if (string.IsNullOrEmpty(jsonConversaciones))
                    {
                        return Json(new { success = false, message = "No hay conversaciones disponibles." }, JsonRequestBehavior.AllowGet);
                    }

                    // Deserializar el JSON de las conversaciones
                    var conversaciones = JsonConvert.DeserializeObject<List<dynamic>>(jsonConversaciones);

                    if (conversaciones == null || conversaciones.Count == 0)
                    {
                        return Json(new { success = false, message = "No se encontraron mensajes en las conversaciones." }, JsonRequestBehavior.AllowGet);
                    }

                    // Obtener el último ID de las conversaciones
                    int ultimoId = conversaciones.Count > 0 ? (int)conversaciones.Last().id : 0;

                    // Obtener el valor de `serialized` del último objeto
                    var ultimoMensaje = conversaciones.Last();
                    string serialized = ultimoMensaje.serialized?.ToString();

                    if (string.IsNullOrEmpty(serialized))
                    {
                        return Json(new { success = false, message = "No se encontró un valor válido para 'serialized'." }, JsonRequestBehavior.AllowGet);
                    }

                    // Enviar el mensaje usando `serialized`
                    var resultado = EnvioMensaje.TestMessage(serialized);
                    var result = JsonConvert.DeserializeObject<ResponseTestMaytapi>(resultado.Message);

                    if (result == null || result.results == null || result.results.Count == 0)
                    {
                        return Json(new { success = false, message = "El resultado de TestMessage es inválido o está vacío." }, JsonRequestBehavior.AllowGet);
                    }

                    // Procesar cada mensaje en la lista (si es necesario)
                    foreach (var res in result.results)
                    {
                        // Crear un nuevo objeto JSON por defecto
                        var nuevoMensaje = new
                        {
                            id = ++ultimoId, // Incrementar ID
                            from_Id = "2",
                            to_Id = "1",
                            mensaje = res.mensaje, // Tomar el mensaje de cada objeto
                            has_DropDown = true,
                            has_Images = new List<object> { null },
                            has_Files = new List<object> { null },
                            datetime = FormatearFecha(res.fecha_llegada), // Formatear la fecha
                            idUsuario = 100,
                            nombreUsuario = "Cliente",
                            rutaImagen = "/assets/images/persona.jpg",
                            serialized = res.serialized // Tomar el valor `serialized`
                        };

                        // Agregar el nuevo mensaje a la lista de conversaciones
                        conversaciones.Add(nuevoMensaje);
                    }

                    // Serializar la lista actualizada a JSON
                    string jsonActualizado = JsonConvert.SerializeObject(conversaciones);

                    // Guardar la lista actualizada en la base de datos
                    db.ActualizarConversacionMaytapi(idEmpresa, clienteBusqueda.idCotizacion, jsonActualizado, DateTime.Now);

                    var jsonConversacionesResult = db.obtenerIdCotizacion(clienteBusqueda.idCotizacion, idEmpresa).FirstOrDefault().jsonConversaciones;

                    return Json(new { success = true, mensaje = "Mensajes agregados correctamente.", jsonConversaciones = jsonConversacionesResult }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                // Devolver el mensaje de error en caso de excepción
                return Json(new { success = false, errorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        public string FormatearFecha(string fecha)
        {
            try
            {
                DateTime fechaConvertida;

                // Verificar si el formato es ISO 8601
                if (fecha.Contains("T"))
                {
                    // Parsear fecha ISO 8601
                    fechaConvertida = DateTime.Parse(fecha, null, DateTimeStyles.RoundtripKind);
                }
                else if (fecha.Contains("/Date"))
                {
                    // Extraer milisegundos de /Date(1733799543313)/ y convertir a DateTime
                    var milisegundos = long.Parse(fecha.Replace("/Date(", "").Replace(")/", ""));
                    fechaConvertida = DateTimeOffset.FromUnixTimeMilliseconds(milisegundos).UtcDateTime;
                }
                else
                {
                    throw new FormatException("El formato de la fecha no es válido.");
                }

                // Ajustar a la zona horaria local
                var zonaHorariaLocal = TimeZoneInfo.Local;
                fechaConvertida = TimeZoneInfo.ConvertTimeFromUtc(fechaConvertida, zonaHorariaLocal);

                // Sumar 3 minutos adicionales
                fechaConvertida = fechaConvertida.AddMinutes(3);

                // Diccionario para traducir los meses al español
                var mesesEnEspañol = new Dictionary<string, string>
        {
            { "January", "Ene" },
            { "February", "Feb" },
            { "March", "Mar" },
            { "April", "Abr" },
            { "May", "May" },
            { "June", "Jun" },
            { "July", "Jul" },
            { "August", "Ago" },
            { "September", "Sep" },
            { "October", "Oct" },
            { "November", "Nov" },
            { "December", "Dic" }
        };

                // Obtener el mes en inglés y traducirlo al español
                string mesEnIngles = fechaConvertida.ToString("MMMM", CultureInfo.InvariantCulture);
                string mesEnEspañol = mesesEnEspañol.ContainsKey(mesEnIngles) ? mesesEnEspañol[mesEnIngles] : mesEnIngles;

                // Formatear la fecha con el mes traducido
                string fechaFormateada = $"{fechaConvertida:dd} {mesEnEspañol} {fechaConvertida:yy}, {fechaConvertida:h:mm} {fechaConvertida:tt}";

                // Convertir el indicador AM/PM a "am" o "pm" en minúsculas sin puntos
                fechaFormateada = fechaFormateada.Replace("a. m.", "am").Replace("p. m.", "pm");

                return fechaFormateada;
            }
            catch (Exception ex)
            {
                // Manejo de errores
                return "Fecha inválida: " + ex.Message;
            }
        }


   



        [HttpPost]
        public JsonResult EnviarMensaje(RequestDataModel model)
        {
            try
            {
                using (var db = new SIAV_prod_4Entities())
                {
                    int idEmpresa = SessionUtils.ObtenerIdEmpresaDeSesion(Session);
                    var idCotizacion = db.obtenerIdCotizacion(model.IdCotizacion, idEmpresa).FirstOrDefault();

                    // Convertir el nuevo `ChatMessageData` de la vista a JSON
                    string jsonNuevoMensaje = JsonConvert.SerializeObject(model.ChatMessageData);



                    if (idCotizacion != null)
                    {
                        var resultado = EnvioMensaje.SendMessage(123, model.Telefono, "text", model.ChatMessageData.mensaje, "text");
                      //  var resultado = EnvioMensaje.SendMessage(123,"+51987611070", "text", qq, "text");
                  
                        // Verificar si el resultado fue exitoso
                        if (!resultado.Success)
                        {
                            // Si el resultado es false, devolver un JSON con el estado de fallo
                            return Json(new { success = false, message = "No se pudo enviar el mensaje...." }, JsonRequestBehavior.AllowGet);
                        }

                        // Deserializar el JSON y agregar la clave "serialized"
                        var nuevoMensaje = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonNuevoMensaje);
                        nuevoMensaje["serialized"] = resultado.Message;
                        jsonNuevoMensaje = JsonConvert.SerializeObject(nuevoMensaje);

                        // Deserializar el JSON existente en una lista de mensajes
                        var jsonConversacionesExistente = JsonConvert.DeserializeObject<List<MessageViewModel>>(idCotizacion.jsonConversaciones);

                        // Deserializar el nuevo mensaje para añadirlo a la lista
                        var mensajeNuevo = JsonConvert.DeserializeObject<MessageViewModel>(jsonNuevoMensaje);

                        // Añadir el nuevo mensaje
                        jsonConversacionesExistente.Add(mensajeNuevo);

                        // Serializar de nuevo a JSON
                        string conversacionActualizadaJson = JsonConvert.SerializeObject(jsonConversacionesExistente);

                        // Guardar la lista actualizada en la base de datos
                       // db.ActualizarConversacionMaytapi(idEmpresa, model.IdCotizacion, conversacionActualizadaJson);
                        db.ActualizarConversacion(idEmpresa, model.IdCotizacion, conversacionActualizadaJson, model.ChatMessageData.idUsuario, model.ChatMessageData.nombreUsuario, model.ChatMessageData.rutaImagen, DateTime.Now,model.idCliente,model.nombreCotizacionMensaje);

                        return Json(new { success = resultado.Success, message = resultado.Message }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        var resultado = EnvioMensaje.SendMessage(123, model.Telefono, "text", model.ChatMessageData.mensaje, "text");

                        // Verificar si el resultado fue exitoso
                        if (!resultado.Success)
                        {
                            // Si el resultado es false, devolver un JSON con el estado de fallo
                            return Json(new { success = false, message = resultado.Message }, JsonRequestBehavior.AllowGet);
                        }

                        // Convertir el nuevo `ChatMessageData` a JSON y agregar "serialized"
                        var nuevoMensaje = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonNuevoMensaje);
                        nuevoMensaje["serialized"] = resultado.Message;
                        jsonNuevoMensaje = JsonConvert.SerializeObject(nuevoMensaje);

                        // Crear una nueva lista con el mensaje
                        var mensajes = new List<MessageViewModel> { JsonConvert.DeserializeObject<MessageViewModel>(jsonNuevoMensaje) };

                        string jsonConversaciones = JsonConvert.SerializeObject(mensajes);

                        db.InsertarConversacion(model.IdCotizacion, idEmpresa, model.Telefono, jsonConversaciones, model.ChatMessageData.idUsuario, model.ChatMessageData.nombreUsuario, model.ChatMessageData.rutaImagen, DateTime.Now, model.idCliente, model.nombreCotizacionMensaje);

                        return Json(new { success = resultado.Success, message = resultado.Message }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                // Devolver el mensaje de error en caso de excepción
                return Json(new { success = false, errorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }





        //-----------------------------------------------------------//

        [HttpPost]
        public JsonResult ClienteBusquedaInical2(ClienteBusqueda2 clienteBusqueda)
        {
            try
            {
                var db = new SIAV_prod_4Entities();
                var dback = new BackOffice_WebEntities();
              //  int idEmpresa = SessionUtils.ObtenerIdEmpresaDeSesion(Session);
                var (idUsuario, idEmpresa) = SessionUtils.ObtenerValoresDeSesion(Session);

                var rutaImagenNombreId = dback.obtenerRutaImagePorIdUsuario(idUsuario).FirstOrDefault();

                string cantidad = "";

                // Determinar las fechas según los valores de clienteBusqueda.periodo1 y clienteBusqueda.periodo2
                DateTime fechaFin, fechaInicio;

                // Obtener la fecha actual y calcular la fecha de hace 6 meses
                //DateTime fechaFin = DateTime.Today;
                //DateTime fechaInicio = fechaFin.AddMonths(-6);

                if (clienteBusqueda.periodo1 == null && clienteBusqueda.periodo2 == null)
                {
                    // Si ambos son 0, usar los últimos 6 meses
                    fechaFin = DateTime.Today;
                    fechaInicio = fechaFin.AddMonths(-6);
                }
                else
                {
                    // Si tienen valores, usarlos como fechas
                    string[] fechaPartes = clienteBusqueda.periodo1.Split('-');
                    int año = int.Parse(fechaPartes[2]);
                    int mes = int.Parse(fechaPartes[1]);
                    int dia = int.Parse(fechaPartes[0]);

                     fechaInicio = new DateTime(año, mes, dia);


                    string[] fechaPartes2 = clienteBusqueda.periodo2.Split('-');
                    int año1 = int.Parse(fechaPartes2[2]);
                    int mes1 = int.Parse(fechaPartes2[1]);
                    int dia1 = int.Parse(fechaPartes2[0]);

                    fechaFin = new DateTime(año1, mes1, dia1);

                    cantidad = clienteBusqueda.cliente ?? "";
                }

                // Ejecutar procedimiento almacenado A
                var clientesBusquedaVencidas = db.UP_COTIZACIONE_VENCIDAS_X_VENCER(fechaInicio, fechaFin, cantidad)
                                                // .Take(4) // Limitar a 4 resultados
                                                 .ToList();

                // Ejecutar procedimiento almacenado B
                var clientesBusquedaDetalle = db.UP_COTIZACIONE_VENCIDAS_X_VENCER_DETALLE2(fechaInicio, fechaFin)
                                                 .ToList();

                // Crear lista para el resultado combinado
                var resultadoCombinado = new List<object>();

                // Agrupar los detalles por id_cliente
                var detallesAgrupados = clientesBusquedaDetalle
                    .GroupBy(d => d.id_cliente)
                    .ToDictionary(g => g.Key, g => g.ToList());
                 
                // Combinar los resultados
                foreach (var cliente in clientesBusquedaVencidas)
                {
                    var detalles = new List<object>();
                    if (detallesAgrupados.ContainsKey(cliente.id_cliente))
                    {
                        var detallesCliente = detallesAgrupados[cliente.id_cliente];
                        int totalCotizaciones = detallesCliente.Count;
                        int cotizacionesVencidas = detallesCliente.Count(d => d.dias_vencimiento < 0);
                        int cotizacionesPorVencer = detallesCliente.Count(d => d.dias_vencimiento >= 0);
                        int totalSeguimientos = detallesCliente.Count(d => d.fecha_seguimiento != null);
                        int totalSeguimientoCorreo = detallesCliente.Count(d => d.fecha_mensaje_correo != null);

                        // Count status types
                        int statusCotizacion = detallesCliente.Count(d => d.estado == "Cotización");
                        int statusAnulado = detallesCliente.Count(d => d.estado == "Anulado");
                        int statusReservado = detallesCliente.Count(d => d.estado == "En Reservación");

                        detalles = detallesCliente.Select(d => new {
                            d.id_cotizacion,
                            d.nombre_cotizacion,
                            d.fecha_alta,
                            d.fecha_vencimiento,
                            d.counter_cliente,
                            d.nombre_cliente,
                            d.id_cliente,
                            d.dias_vencimiento,
                            d.correo_electronico,
                            d.telefono,
                            d.estado,
                            d.fecha_notificacion,
                            d.fecha_seguimiento,
                            d.fecha_mensaje_correo,
                            d.url_descarga,
                            d.fecha_viaje,
                            d.counter_rge,
                            d.counter_correo,
                            d.id_counter_rge
                        }).Cast<object>().ToList();

                        var clienteCombinado = new
                        {
                            cliente.id_cliente,
                            Cliente = cliente.cliente,
                            cliente.notificados,
                            TotalCotizaciones = totalCotizaciones,
                            CotizacionesVencidas = cotizacionesVencidas,
                            CotizacionesPorVencer = cotizacionesPorVencer,
                            TotalSeguimientos = totalSeguimientos,
                            TotalSeguimientoCorreo = totalSeguimientoCorreo,
                            // Add status counts to the response
                            StatusCotizacion = statusCotizacion,
                            StatusAnulado = statusAnulado,
                            StatusReservado = statusReservado,
                            Detalles = detalles
                        };
                        resultadoCombinado.Add(clienteCombinado);
                    }
                    else
                    {
                        var clienteCombinado = new
                        {
                            cliente.id_cliente,
                            Cliente = cliente.cliente,
                            cliente.notificados,
                            TotalCotizaciones = 0,
                            CotizacionesVencidas = 0,
                            CotizacionesPorVencer = 0,
                            TotalSeguimientos = 0,
                            TotalSeguimientoCorreo = 0,
                            // Initialize status counts as zero for clients with no details
                            StatusCotizacion = 0,
                            StatusAnulado = 0,
                            StatusReservado = 0,
                            Detalles = detalles
                        };
                        resultadoCombinado.Add(clienteCombinado);
                    }
                }

                resultadoCombinado = resultadoCombinado
             .OrderBy(c => ((dynamic)c).notificados)   
    .ThenBy(c => ((dynamic)c).TotalSeguimientos)        
    .ThenBy(c => ((dynamic)c).TotalSeguimientoCorreo)
    .ToList();

                ///////// por Semana
                // Obtener la fecha de inicio como el lunes de la semana actual
                DateTime today = DateTime.Today;
                // Ajusta para que el lunes sea siempre el primer día de la semana
                int daysSinceMonday = (today.DayOfWeek == DayOfWeek.Sunday) ? -6 : (int)today.DayOfWeek - (int)DayOfWeek.Monday;
                DateTime fechaInicioSemana = today.AddDays(-daysSinceMonday);

                // Calcular la fecha de fin de semana como el domingo de la misma semana
                DateTime fechaFinSemana = fechaInicioSemana.AddDays(6); // Esto establece la fecha al domingo de la misma semana

                var clientesBusquedaSemana = db.UP_COTIZACIONE_CREADAS_SEMANA(fechaInicioSemana, fechaFinSemana, cantidad).ToList();

                // Mapea los resultados a un formato JSON
                var resultClientesSemana = clientesBusquedaSemana.Select(r => new
                {
                    Cliente = r.cliente,
                    Pais = r.pais,
                    Domingo = r.domingo,
                    Lunes = r.lunes,
                    Martes = r.martes,
                    Miercoles = r.miercoles,
                    Jueves = r.jueves,
                    Viernes = r.viernes,
                    Sabado = r.sabado,
                    id_cliente = r.id_cliente,
                    fecha_desde = r.fecha_desde,
                    fecha_hasta = r.fecha_hasta


                }).ToList();

                string jsonResultSemana = JsonConvert.SerializeObject(resultClientesSemana);

                /////////// Log 
                ///  // Calcular el rango de fechas: desde hace 14 días hasta hoy
                DateTime fechaFinLog = DateTime.Today;  // Hoy
                DateTime fechaInicioLog = fechaFin.AddDays(-14); // 14 días atrás desde hoy

                var clientesBusquedaLog = db.UP_COTIZACIONE_LOG_ENVIOS(fechaFinLog, fechaInicioLog, cantidad).ToList();

                // Mapea los resultados a un formato JSON
                var resultClientesLog = clientesBusquedaLog.Select(r => new
                {
                    Cliente = r.cliente,
                    Pais = r.pais,
                    nombreCotizacion = r.nombre_cotizacion,
                    inicioViaje = r.inicio_viaje,
                    finViaje = r.fin_viaje,
                    fechaNotificacion1 = r.fecha_notificacion_1,
                    respuestaNotificacion1 = r.respuesta_notificacion_1,
                    fechaNotificacion2 = r.fecha_notificacion_2,
                    respuestaNotificacion2 = r.respuesta_notificacion_2

                }).ToList();

                string jsonResultLog = JsonConvert.SerializeObject(resultClientesLog);

                return Json(new { data = resultadoCombinado, rutaImagenNombreId = rutaImagenNombreId, dataSemana = jsonResultSemana, dataLog = jsonResultLog, success = true, selector = true }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult ultimosMensajesWhassap(ClienteBusqueda2 clienteBusqueda)
        {
            try
            {
                using (var db = new SIAV_prod_4Entities())
                {
                    int idEmpresa = SessionUtils.ObtenerIdEmpresaDeSesion(Session);

            

                    var cotizaciones = db.ultimosMensajesWhassap(idEmpresa)
                        // .Where(c => c.Id_Cotizaciones == 734)
                       // .Where(c => c.Id_Cotizaciones == 734 || c.Id_Cotizaciones == 473)
                        .ToList();

                    var resultado = new List<object>();

                    foreach (var cot in cotizaciones)
                    {
                        // Deserializar el campo jsonConversaciones (string a lista de objetos)
                        var conversaciones = JsonConvert.DeserializeObject<List<Conversacion>>(cot.jsonConversaciones);

                        var ultimoMensajeCounter = conversaciones
                        .Where(m => m.from_Id == "1" && m.to_Id == "2")
                        .LastOrDefault(); // último según el orden original


                        var serializedUltimoCounter = EnvioMensaje.TestMessage(ultimoMensajeCounter.serialized);
                      
                        var mensajeCliente = JsonConvert.DeserializeObject<ResponseTestMaytapi>(serializedUltimoCounter.Message);


                      //  var mensaje = mensajeCliente?.results?.FirstOrDefault()?.mensaje;

                        var mensaje = mensajeCliente?.results?.LastOrDefault()?.mensaje;

                        if (mensaje != null)
                        {
                            resultado.Add(new
                            {
                                Id_Cotizaciones = cot.Id_Cotizaciones,
                                nombre_cotizacion = cot.nombre_cotizacion,
                                Id_Usuario = cot.Id_Usuario,
                                Nombre_Usuario = cot.Nombre_Usuario,
                                ruta_Imagen = cot.ruta_Imagen,
                                idCliente = cot.idCliente,
                                telefono = cot.telefono,
                                UltimoMensajeCliente = mensaje
                            });
                        }


                    }
                var  jsonResultado = JsonConvert.SerializeObject(resultado);

                    return Json(new { success = true, data = jsonResultado }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public JsonResult anulaCotizacion(ClienteBusqueda2 clienteBusqueda)
        {
            try
            {
                using (var db = new SIAV_prod_4Entities())
                {
                    int idEmpresa = SessionUtils.ObtenerIdEmpresaDeSesion(Session);

                    db.up_anula_cotizacion(clienteBusqueda.idCotizacion, clienteBusqueda.mensaje);
                    return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        public JsonResult conteoDeCorreosEnviados(ClienteBusqueda2 clienteBusqueda)
        {
            try
            {
                using (var db = new SIAV_prod_4Entities())
                {
                    int idEmpresa = SessionUtils.ObtenerIdEmpresaDeSesion(Session);

                    // Convertir DateTime al formato específico del JSON: "30 jul. 2025"
                    string fechaFormateada = ConvertirFechaAFormatoJSON(clienteBusqueda.fechaBusquedaCorreo);

                    // Ejecutar el procedimiento y capturar los resultados
                    var resultados = db.sp_BuscarRemitentesPorFecha(fechaFormateada).ToList();

                    var resultadosJson = JsonConvert.SerializeObject(resultados, Formatting.Indented);

                    // Devolver los resultados
                    return Json(new
                    {
                        success = true,
                        data = resultados,
                        count = resultados.Count,
                        fechaOriginal = clienteBusqueda.fechaBusquedaCorreo.ToString("yyyy-MM-dd"),
                        fechaFormateada = fechaFormateada
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    errorMessage = ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        // MÉTODO AUXILIAR para convertir DateTime al formato del JSON
        private string ConvertirFechaAFormatoJSON(DateTime fecha)
        {
            // Mapeo de meses en español (abreviados con punto)
            string[] mesesAbreviados = {
        "", "ene.", "feb.", "mar.", "abr.", "may.", "jun.",
        "jul.", "ago.", "sep.", "oct.", "nov.", "dic."
    };

            // Formato: "30 jul. 2025"
            return $"{fecha.Day:D2} {mesesAbreviados[fecha.Month]} {fecha.Year}";
        }

        // ALTERNATIVA: Si el formato tiene variaciones, usar múltiples intentos
        private List<string> GenerarVariantesDeFecha(DateTime fecha)
        {
            string[] mesesAbreviados = {
        "", "ene.", "feb.", "mar.", "abr.", "may.", "jun.",
        "jul.", "ago.", "sep.", "oct.", "nov.", "dic."
    };

            string[] mesesCompletos = {
        "", "enero", "febrero", "marzo", "abril", "mayo", "junio",
        "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"
    };

            var variantes = new List<string>
    {
        // Formato principal: "30 jul. 2025"
        $"{fecha.Day:D2} {mesesAbreviados[fecha.Month]} {fecha.Year}",
        
        // Sin cero inicial: "3 jul. 2025"  
        $"{fecha.Day} {mesesAbreviados[fecha.Month]} {fecha.Year}",
        
        // Mes completo: "30 julio 2025"
        $"{fecha.Day:D2} {mesesCompletos[fecha.Month]} {fecha.Year}",
        
        // Con coma: "30 jul. 2025, 05:43"
        $"{fecha.Day:D2} {mesesAbreviados[fecha.Month]} {fecha.Year},",
        
        // Solo mes y año: "jul. 2025"
        $"{mesesAbreviados[fecha.Month]} {fecha.Year}"
    };

            return variantes;
        }




        // Clase auxiliar para deserializar el JSON
        public class Conversacion
        {
            public string id { get; set; }
            public string from_Id { get; set; }
            public string to_Id { get; set; }
            public string mensaje { get; set; }
            public bool has_DropDown { get; set; }
            public List<string> has_Images { get; set; }
            public List<string> has_Files { get; set; }
            public string datetime { get; set; }
            public int idUsuario { get; set; }
            public string nombreUsuario { get; set; }
            public string rutaImagen { get; set; }
            public string serialized { get; set; }
            public string telefono { get; set; }
        }








    }
}



