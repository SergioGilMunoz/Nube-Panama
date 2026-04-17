using Microsoft.AspNetCore.Mvc;
using MimeKit;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Velzon.Modelos;
using Velzon.Modelos.DB;
using Velzon.Modelos.DBSIAV2024;
using static Velzon.Controllers.DASHBOARDSTYLE2Controller;

namespace Velzon.Controllers
{
    public class DashboardRerservasController : Controller
    {
        // GET: DashboardRerservas
        public ActionResult Index()
        {
            if (Session["DatosUsuario"] == null)
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }




        public JsonResult cargarDataReservas(ClienteBusqueda2 clienteBusqueda)
        {
            try
            {
                using (var db = new SIAV_prod_4Entities())
                {
                    db.Database.CommandTimeout = 120;
                    int anioActual = DateTime.Now.Year;
                    var resultado = db.up_rpt_reservas_por_anio(anioActual).ToList();
                   //  var resultado = db.up_rpt_reservas_por_anio(anioActual).Take(200).ToList();

                    // Estructurar los datos según la jerarquía solicitada
                    var datosEstructurados = resultado
                        .GroupBy(r => new { r.id_region, r.nombre_region })
                        .Select(region => new
                        {
                            id_region = region.Key.id_region,
                            nombre_region = region.Key.nombre_region,
                            paises = region.GroupBy(p => new { p.id_pais, p.nombre_pais })
                                .Select(pais => new
                                {
                                    id_pais = pais.Key.id_pais,
                                    nombre_pais = pais.Key.nombre_pais,
                                    clientes = pais.GroupBy(c => new {
                                        c.id_cliente,
                                        c.nombre_cliente,
                                        c.correo_electronico,
                                        c.telefono,
                                        c.telefono_ws,
                              
                                    })
                                    .Select(cliente => new
                                    {
                                        id_cliente = cliente.Key.id_cliente,
                                        nombre_cliente = cliente.Key.nombre_cliente,
                                        correo_electronico = cliente.Key.correo_electronico,
                                        telefono = cliente.Key.telefono,
                                        telefono_ws = cliente.Key.telefono_ws,
                                        personal = cliente.GroupBy(x => new {
                                            // Agrupar por personal de reservas
                                            id_personal_reserva = x.id_personal_reserva,
                                            nombre_personal_reserva = x.nombre_personal_reserva,
                                            apellido_personal_reserva = x.apellido_personal_reserva,
                                            correo_personal_reserva = x.correo_personal_reserva,
                                            telefono_personal_reserva = x.telefono_personal_reserva,
                                            telefono_personal_reserva2 = x.telefono_personal_reserva2,
                                            cofirmar_datos_personal_reserva = x.cofirmar_datos_personal_reserva,
                                            cargo_personal_reserva = x.cargo_personal_reserva,
                                          
                                            // Agrupar por personal de cotizaciones
                                            id_personal_cotizacion = x.id_personal_cotizacion,
                                            nombre_personal_cotizacion = x.nombre_personal_cotizacion,
                                            apellido_personal_cotizacion = x.apellido_personal_cotizacion,
                                            correo_personal_cotizacion = x.correo_personal_cotizacion,
                                            telefono_personal_cotizacion = x.telefono_personal_cotizacion,
                                            telefono_personal_cotizacionWS = x.telefono_personal_cotizacionWS,
                                            cofirmar_datos_personal_cotizacion = x.cofirmar_datos_personal_cotizacion,
                                            cargo_personal_cotizacion = x.cargo_personal_cotizacion
                                        })
                                        .Select(personal => new
                                        {
                                            // Datos del personal de reservas
                                            personal_reservas = personal.Key.id_personal_reserva != null ? new
                                            {
                                                id_personal_reserva = personal.Key.id_personal_reserva,
                                                nombre_completo = $"{personal.Key.nombre_personal_reserva} {personal.Key.apellido_personal_reserva}".Trim(),
                                                correo_personal_reserva = personal.Key.correo_personal_reserva,
                                                telefono_personal_reserva = personal.Key.telefono_personal_reserva,
                                                telefono_personal_reserva2 = personal.Key.telefono_personal_reserva2,
                                                estado_actualizacion = personal.Key.cofirmar_datos_personal_reserva == true ? "Actualizado" :
                                                                     personal.Key.cofirmar_datos_personal_reserva == false ? "No Actualizado" : "No Actualizado",
                                                cargo_personal_reserva = personal.Key.cargo_personal_reserva,
                                            
                                                reservas = new
                                                {
                                                    reconfirmado = personal.Where(r => r.id_reserva != null && r.nombre_estado == "RECONFIRMADO")
                                                        .Select(r => new
                                                        {
                                                            id_reserva = r.id_reserva,
                                                            nombre_reserva = r.nombre_reserva,
                                                            nombre_paquete = r.nombre_paquete,
                                                            usuario_reserva = r.usuario_reserva,
                                                            fecha_reserva = r.fecha_reserva,
                                                            id_cotizacion_origen = r.id_cotizacion_origen,
                                                            costo = r.costo,
                                                            ganancia = r.ganancia,
                                                            total = r.costo + r.ganancia,
                                                            venta_mes_actual = r.venta_mes_actual,
                                                            venta_mes_anterior = r.venta_mes_anterior ,
                                                            venta_mes_anterior_anterior = r.venta_mes_anterior_anterior

                                                        }).ToList(),
                                                    confirmado = personal.Where(r => r.id_reserva != null && r.nombre_estado == "CONFIRMADO")   
                                                        .Select(r => new
                                                        {
                                                            id_reserva = r.id_reserva,
                                                            nombre_reserva = r.nombre_reserva,
                                                            nombre_paquete = r.nombre_paquete,
                                                            usuario_reserva = r.usuario_reserva,
                                                            fecha_reserva = r.fecha_reserva,
                                                            id_cotizacion_origen = r.id_cotizacion_origen,
                                                            costo = r.costo,
                                                            ganancia = r.ganancia,
                                                            total = r.costo + r.ganancia,
                                                            venta_mes_actual = r.venta_mes_actual,
                                                   
                                                        }).ToList(),
                                                    anulado = personal.Where(r => r.id_reserva != null && r.nombre_estado == "ANULADO")
                                                        .Select(r => new
                                                        {
                                                            id_reserva = r.id_reserva,
                                                            nombre_reserva = r.nombre_reserva,
                                                            nombre_paquete = r.nombre_paquete,
                                                            usuario_reserva = r.usuario_reserva,
                                                            fecha_reserva = r.fecha_reserva,
                                                            id_cotizacion_origen = r.id_cotizacion_origen,
                                                            costo = r.costo,
                                                            ganancia = r.ganancia,
                                                            total = r.costo + r.ganancia,
                                                           
                                                        
                                                        }).ToList()
                                                }
                                            } : null,

                                            // Datos del personal de cotizaciones
                                            personal_cotizaciones = personal.Key.id_personal_cotizacion != null ? new
                                            {
                                                id_personal_cotizacion = personal.Key.id_personal_cotizacion,
                                                nombre_completo = $"{personal.Key.nombre_personal_cotizacion} {personal.Key.apellido_personal_cotizacion}".Trim(),
                                                correo_personal_cotizacion = personal.Key.correo_personal_cotizacion,
                                                telefono_personal_cotizacion = personal.Key.telefono_personal_cotizacion,
                                                telefono_personal_cotizacionWS = personal.Key.telefono_personal_cotizacionWS,
                                                estado_actualizacion = personal.Key.cofirmar_datos_personal_cotizacion == true ? "Actualizado" :
                                                                     personal.Key.cofirmar_datos_personal_cotizacion == false ? "No Actualizado" : "No Actualizado",
                                                cargo_personal_cotizacion = personal.Key.cargo_personal_cotizacion,
                                                cotizaciones = personal.Where(c => c.id_cotizacion != null)
                                                    .Select(c => new
                                                    {
                                                        id_cotizacion = c.id_cotizacion,
                                                        nombre_cotizacion = c.nombre_cotizacion,
                                                        id_usuario_cotiza = c.id_usuario_cotiza,
                                                        usuario_cotizacion = c.usuario_cotizacion,
                                                        estado = c.nombre_estado
                                                    }).ToList()
                                            } : null
                                        })
                                        .Where(p => p.personal_reservas != null || p.personal_cotizaciones != null)
                                        .ToList()
                                    }).ToList()
                                }).ToList()
                        }).ToList();

                    var jsonresult = JsonConvert.SerializeObject(datosEstructurados, Formatting.Indented);

                    return Json(new { success = true, data = datosEstructurados }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }



        [HttpPost]
        [ValidateInput(false)] // Deshabilitar validación para permitir HTML
        public JsonResult EnviarCorreoPersonal()
        {
            try
            {
                using (var dback = new BackOffice_WebEntities())
                {
                    var (idUsuario, idEmpresa) = SessionUtils.ObtenerValoresDeSesion(Session);
                    var siav = new SIAV_prod_4Entities();
                    var emailService = new EmailService();

                    // Leer el JSON del request
                    string jsonData;
                    using (var reader = new StreamReader(Request.InputStream))
                    {
                        jsonData = reader.ReadToEnd();
                    }

                    // Deserializar los datos
                    dynamic requestData = JsonConvert.DeserializeObject(jsonData);

                    // Extraer los datos del JSON
                    string emailDestinatario = requestData.EmailDestinatario?.ToString() ?? "";
                    string asunto = requestData.Asunto?.ToString() ?? "";
                    string contenido = requestData.Contenido?.ToString() ?? "";
                    int? idPersonal = requestData.IdPersonal != null ? Convert.ToInt32(requestData.IdPersonal) : (int?)null;
                    int? idCliente = requestData.IdCliente != null ? Convert.ToInt32(requestData.IdCliente) : (int?)null;

                    // Validaciones básicas
                    if (string.IsNullOrEmpty(emailDestinatario))
                    {
                        return Json(new { success = false, message = "Email destinatario es requerido" });
                    }

                    if (string.IsNullOrEmpty(asunto))
                    {
                        return Json(new { success = false, message = "Asunto es requerido" });
                    }

                    if (string.IsNullOrEmpty(contenido))
                    {
                        return Json(new { success = false, message = "Contenido es requerido" });
                    }

                    if (!idCliente.HasValue)
                    {
                        return Json(new { success = false, message = "ID Cliente es requerido" });
                    }

                    if (!idPersonal.HasValue)
                    {
                        return Json(new { success = false, message = "ID Personal es requerido" });
                    }

                    // Obtener información del usuario
                    var usuario = dback.ObtenerElUsuarioPorId(idUsuario).FirstOrDefault();

                    if (usuario == null)
                    {
                        return Json(new { success = false, message = "Usuario no encontrado" });
                    }

                    // Procesar archivos adjuntos desde Base64
                    bool tieneAdjuntos = false;
                    List<string> nombresArchivos = new List<string>();
                    List<string> rutasArchivos = new List<string>();

                    if (requestData.ArchivosAdjuntos != null && requestData.ArchivosAdjuntos.Count > 0)
                    {
                        string carpetaAdjuntos = Server.MapPath($"~/Uploads/CorreosPersonal/{idEmpresa}/{idUsuario}/");

                        if (!Directory.Exists(carpetaAdjuntos))
                        {
                            Directory.CreateDirectory(carpetaAdjuntos);
                        }

                        foreach (var archivo in requestData.ArchivosAdjuntos)
                        {
                            try
                            {
                                string nombreArchivo = archivo.nombre?.ToString() ?? "";
                                string datosBase64 = archivo.datos?.ToString() ?? "";

                                if (!string.IsNullOrEmpty(nombreArchivo) && !string.IsNullOrEmpty(datosBase64))
                                {
                                    // Convertir Base64 a bytes
                                    byte[] archivoBytes = Convert.FromBase64String(datosBase64);

                                    // Crear nombre único
                                    string nombreUnico = $"{DateTime.Now:yyyyMMddHHmmss}_{nombreArchivo}";
                                    string rutaCompleta = Path.Combine(carpetaAdjuntos, nombreUnico);

                                    // CORRECCIÓN: Usar System.IO.File en lugar de File
                                    System.IO.File.WriteAllBytes(rutaCompleta, archivoBytes);

                                    nombresArchivos.Add(nombreArchivo);
                                    rutasArchivos.Add(rutaCompleta);
                                    tieneAdjuntos = true;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error procesando archivo: {ex.Message}");
                            }
                        }
                    }

                    // Preparar datos para guardar en BD
                    string nombresArchivosJson = tieneAdjuntos ? JsonConvert.SerializeObject(nombresArchivos) : null;
                    string rutasArchivosJsonRelativas = tieneAdjuntos ? JsonConvert.SerializeObject(rutasArchivos.Select(r => r.Replace(Server.MapPath("~/"), "/").Replace("\\", "/")).ToList()) : null;

                    // Obtener IP y User Agent
                    string ipOrigen = Request.UserHostAddress ?? "";
                    string userAgent = Request.UserAgent ?? "";

                    // Guardar en base de datos
                    var resultado = siav.sp_InsertarCorreoPersonal(
                        idEmpresa,
                        idCliente.Value,
                        idPersonal.Value,
                        emailDestinatario,
                        usuario.Correo ?? "",
                        asunto,
                        contenido,
                        tieneAdjuntos,
                        nombresArchivosJson,
                        rutasArchivosJsonRelativas,
                        "Enviado",
                        ipOrigen,
                        userAgent
                    ).FirstOrDefault();

                    // Enviar correo real con archivos adjuntos
                    emailService.EnviarCorreoDesdeUsuarioConAdjuntos(
                        usuario.Correo,
                        usuario.passCorreoUsuario,
                        usuario.Nombre,
                        emailDestinatario,
                        asunto,
                        contenido,
                        rutasArchivos
                    );

                    return Json(new
                    {
                        success = true,
                        message = "Correo enviado y guardado exitosamente",
                        idCorreo = resultado?.id_correo ?? 0,
                        data = new
                        {
                            emailDestinatario = emailDestinatario,
                            asunto = asunto,
                            contenido = contenido,
                            fechaEnvio = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                            remitente = usuario.Correo ?? "",
                            tieneAdjuntos = tieneAdjuntos,
                            cantidadAdjuntos = nombresArchivos.Count
                        }
                    });
                }
            }
            catch (JsonException ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error al procesar datos JSON: {ex.Message}"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        //[HttpPost]
        //public JsonResult EnviarCorreoPersonal()
        //{
        //    try
        //    {
        //        using (var dback = new BackOffice_WebEntities())
        //        {
        //            var (idUsuario, idEmpresa) = SessionUtils.ObtenerValoresDeSesion(Session);
        //            var siav = new SIAV_prod_4Entities();

        //            // Leer el JSON del request
        //            string jsonData;
        //            using (var reader = new StreamReader(Request.InputStream))
        //            {
        //                jsonData = reader.ReadToEnd();
        //            }

        //            var emailService = new EmailService();

        //            // Deserializar los datos
        //            dynamic requestData = JsonConvert.DeserializeObject(jsonData);

        //            // Extraer los datos del JSON
        //            string emailDestinatario = requestData.EmailDestinatario?.ToString() ?? "";
        //            string asunto = requestData.Asunto?.ToString() ?? "";
        //            string contenido = requestData.Contenido?.ToString() ?? "";
        //            int? idPersonal = requestData.IdPersonal != null ? Convert.ToInt32(requestData.IdPersonal) : (int?)null;
        //            int? idCliente = requestData.IdCliente != null ? Convert.ToInt32(requestData.IdCliente) : (int?)null;

        //            // Validaciones básicas
        //            if (string.IsNullOrEmpty(emailDestinatario))
        //            {
        //                return Json(new { success = false, message = "Email destinatario es requerido" });
        //            }

        //            if (string.IsNullOrEmpty(asunto))
        //            {
        //                return Json(new { success = false, message = "Asunto es requerido" });
        //            }

        //            if (string.IsNullOrEmpty(contenido))
        //            {
        //                return Json(new { success = false, message = "Contenido es requerido" });
        //            }

        //            if (!idCliente.HasValue)
        //            {
        //                return Json(new { success = false, message = "ID Cliente es requerido" });
        //            }

        //            if (!idPersonal.HasValue)
        //            {
        //                return Json(new { success = false, message = "ID Personal es requerido" });
        //            }

        //            // Obtener información del usuario
        //            var usuario = dback.ObtenerElUsuarioPorId(idUsuario).FirstOrDefault();

        //            if (usuario == null)
        //            {
        //                return Json(new { success = false, message = "Usuario no encontrado" });
        //            }

        //            // Procesar archivos adjuntos si existen
        //            bool tieneAdjuntos = false;
        //            List<string> nombresArchivos = new List<string>();
        //            List<string> rutasArchivos = new List<string>();

        //            if (Request.Files.Count > 0)
        //            {
        //                string carpetaAdjuntos = Server.MapPath($"~/Uploads/CorreosPersonal/{idEmpresa}/{idUsuario}/");

        //                if (!Directory.Exists(carpetaAdjuntos))
        //                {
        //                    Directory.CreateDirectory(carpetaAdjuntos);
        //                }

        //                for (int i = 0; i < Request.Files.Count; i++)
        //                {
        //                    var archivo = Request.Files[i];
        //                    if (archivo != null && archivo.ContentLength > 0)
        //                    {
        //                        string nombreUnico = $"{DateTime.Now:yyyyMMddHHmmss}_{archivo.FileName}";
        //                        string rutaCompleta = Path.Combine(carpetaAdjuntos, nombreUnico);

        //                        archivo.SaveAs(rutaCompleta);

        //                        nombresArchivos.Add(archivo.FileName);
        //                        rutasArchivos.Add($"/Uploads/CorreosPersonal/{idEmpresa}/{idUsuario}/{nombreUnico}");
        //                        tieneAdjuntos = true;
        //                    }
        //                }
        //            }

        //            // Preparar datos para guardar en BD
        //            string nombresArchivosJson = tieneAdjuntos ? JsonConvert.SerializeObject(nombresArchivos) : null;
        //            string rutasArchivosJson = tieneAdjuntos ? JsonConvert.SerializeObject(rutasArchivos) : null;

        //            // Obtener IP y User Agent
        //            string ipOrigen = Request.UserHostAddress ?? "";
        //            string userAgent = Request.UserAgent ?? "";

        //            // Guardar en base de datos con id_empresa incluido
        //            var resultado = siav.sp_InsertarCorreoPersonal(
        //                idEmpresa,          // ← AGREGADO id_empresa
        //                idCliente.Value,
        //                idPersonal.Value,
        //                emailDestinatario,
        //                usuario.Correo ?? "",
        //                asunto,
        //                contenido,
        //                tieneAdjuntos,
        //                nombresArchivosJson,
        //                rutasArchivosJson,
        //                "Enviado",
        //                ipOrigen,
        //                userAgent
        //            ).FirstOrDefault();

        //            // Aquí irá el envío real del correo cuando me digas
        //              emailService.EnviarCorreoDesdeUsuario(usuario.Correo, usuario.passCorreoUsuario, usuario.Nombre, request.correo, subject, mensajeHtml);

        //            return Json(new
        //            {
        //                success = true,
        //                message = "Correo enviado y guardado exitosamente",
        //                idCorreo = resultado?.id_correo ?? 0,
        //                data = new
        //                {
        //                    emailDestinatario = emailDestinatario,
        //                    asunto = asunto,
        //                    contenido = contenido,
        //                    fechaEnvio = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
        //                    remitente = usuario.Correo ?? "",
        //                    tieneAdjuntos = tieneAdjuntos,
        //                    cantidadAdjuntos = nombresArchivos.Count
        //                }
        //            });
        //        }
        //    }
        //    catch (JsonException ex)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = $"Error al procesar datos JSON: {ex.Message}"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new
        //        {
        //            success = false,
        //            message = $"Error: {ex.Message}"
        //        });
        //    }
        //}


        [HttpPost]
        public JsonResult ObtenerHistorialCorreos()
        {
            try
            {
                using (var dback = new BackOffice_WebEntities())
                {
                    var (idUsuario, idEmpresa) = SessionUtils.ObtenerValoresDeSesion(Session);
                    var siav = new SIAV_prod_4Entities();

                    // Leer el JSON del request
                    string jsonData;
                    using (var reader = new StreamReader(Request.InputStream))
                    {
                        jsonData = reader.ReadToEnd();
                    }

                    // Deserializar los datos
                    dynamic requestData = JsonConvert.DeserializeObject(jsonData);

                    // Extraer los datos del JSON
                    int? idCliente = requestData.IdCliente != null ? Convert.ToInt32(requestData.IdCliente) : (int?)null;
                    int? idPersonal = requestData.IdPersonal != null ? Convert.ToInt32(requestData.IdPersonal) : (int?)null;

                    // Validaciones
                    if (!idCliente.HasValue)
                    {
                        return Json(new { success = false, message = "ID Cliente es requerido" });
                    }

                    if (!idPersonal.HasValue)
                    {
                        return Json(new { success = false, message = "ID Personal es requerido" });
                    }

                    // Obtener historial de correos con id_empresa
                    var historialCorreos = siav.sp_ObtenerHistorialCorreosPersonal(
                        idEmpresa,          // ← AGREGADO id_empresa
                        idCliente.Value,
                        idPersonal.Value
                    ).ToList();

                    // Convertir a formato para el frontend
                    var correosFormateados = historialCorreos.Select(correo => new
                    {
                        id_empresa = correo.id_empresa,        // ← AGREGADO
                        id_cliente = correo.id_cliente,
                        id_personal = correo.id_personal,
                        id_correo = correo.id_correo,
                        email_destinatario = correo.email_destinatario,
                        email_remitente = correo.email_remitente,
                        asunto = correo.asunto,
                        contenido = correo.contenido,
                        tiene_adjuntos = correo.tiene_adjuntos,
                        nombres_archivos_adjuntos = correo.nombres_archivos_adjuntos,
                        rutas_archivos_adjuntos = correo.rutas_archivos_adjuntos,
                        estado = correo.estado,
                        fecha_creacion = correo.fecha_creacion?.ToString("dd/MM/yyyy HH:mm"),
                        fecha_envio = correo.fecha_envio?.ToString("dd/MM/yyyy HH:mm")
                    }).ToList();

                    return Json(new
                    {
                        success = true,
                        message = "Historial obtenido correctamente",
                        correos = correosFormateados,
                        total = correosFormateados.Count
                    });
                }
            }
            catch (JsonException ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error al procesar datos JSON: {ex.Message}"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }



        // mensajeria whassap 

        // En tu controlador MensajeriaController


        [HttpPost]
        public JsonResult EnviarMensajePersonalCliente(RequestDataModel model)
        {
            try
            {
                using (var db = new SIAV_prod_4Entities())
                {
                    var dback = new BackOffice_WebEntities();

                    var (idUsuario, idEmpresa) = SessionUtils.ObtenerValoresDeSesion(Session);

           

                    var usuario = dback.ObtenerElUsuarioPorId(idUsuario).FirstOrDefault();

                    // Verificar si ya existe una conversación entre el personal y el cliente
                    var conversacionExistente = db.ObtenerConversacionPersonalCliente(idEmpresa, model.idCliente, model.IdPersonal).FirstOrDefault();

                    // Generar el próximo ID secuencial
                    int nextId = 1;
                    if (conversacionExistente != null && !string.IsNullOrEmpty(conversacionExistente.jsonConversaciones))
                    {
                        var mensajesExistentes = JsonConvert.DeserializeObject<List<MessageViewModel>>(conversacionExistente.jsonConversaciones);
                        if (mensajesExistentes.Any())
                        {
                            // Obtener el ID más alto y sumar 1
                            var maxId = mensajesExistentes.Max(m => int.TryParse(m.id, out int id) ? id : 0);
                            nextId = maxId + 1;
                        }
                    }

                    // Completar los datos del mensaje que vienen vacíos desde el cliente
                    model.ChatMessageData.id = nextId.ToString();
                    model.ChatMessageData.idUsuario = usuario.Id_Usuario;
                    model.ChatMessageData.nombreUsuario = usuario.Nombre;
                    model.ChatMessageData.rutaImagen = usuario.Ruta_Imagen ?? "/assets/images/user-default.jpg";

                    // Enviar mensaje por WhatsApp
                   // var resultado = EnvioMensaje.SendMessage(123, "51987611070", "text", model.ChatMessageData.mensaje, "text");       //   51924615883
                     var resultado = EnvioMensaje.SendMessage(123, model.Telefono, "text", model.ChatMessageData.mensaje, "text");

                    // Verificar si el resultado fue exitoso
                    if (!resultado.Success)
                    {
                        return Json(new { success = false, message = "No se pudo enviar el mensaje...." }, JsonRequestBehavior.AllowGet);
                    }

                    // Completar el serialized con la respuesta del envío
                    model.ChatMessageData.serialized = resultado.Message;

                    // Convertir el mensaje completo a JSON
                    string jsonNuevoMensaje = JsonConvert.SerializeObject(model.ChatMessageData);

                    if (conversacionExistente != null)
                    {
                        // ACTUALIZAR conversación existente

                        // Deserializar el JSON existente en una lista de mensajes
                        var jsonConversacionesExistente = JsonConvert.DeserializeObject<List<MessageViewModel>>(conversacionExistente.jsonConversaciones);

                        // Deserializar el nuevo mensaje para añadirlo a la lista
                        var mensajeNuevo = JsonConvert.DeserializeObject<MessageViewModel>(jsonNuevoMensaje);

                        // Añadir el nuevo mensaje
                        jsonConversacionesExistente.Add(mensajeNuevo);

                        // Serializar de nuevo a JSON
                        string conversacionActualizadaJson = JsonConvert.SerializeObject(jsonConversacionesExistente);

                        // Actualizar la conversación en la base de datos
                        db.ActualizarConversacionPersonalCliente(
                            idEmpresa,
                            model.idCliente,
                            model.IdPersonal,
                            conversacionActualizadaJson,
                            usuario.Id_Usuario,
                            usuario.Nombre,
                            usuario.Ruta_Imagen,
                            DateTime.Now,
                            model.nombrePersonal
                        );

                        return Json(new { success = resultado.Success, message = resultado.Message }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        // INSERTAR nueva conversación

                        // Crear una nueva lista con el mensaje (ya tiene todos los datos completos)
                        var mensajes = new List<MessageViewModel> { JsonConvert.DeserializeObject<MessageViewModel>(jsonNuevoMensaje) };

                        string jsonConversaciones = JsonConvert.SerializeObject(mensajes);

                        // Insertar nueva conversación
                        db.InsertarConversacionPersonalCliente(
                            idEmpresa,
                            model.idCliente,
                            model.IdPersonal,
                            model.Telefono,
                            jsonConversaciones,
                            usuario.Id_Usuario,
                            usuario.Nombre,
                            usuario.Ruta_Imagen,
                            DateTime.Now,
                            model.nombrePersonal
                        );

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


        [HttpPost]
        public JsonResult ObtenerConversacionPersonalCliente(int idCliente, int idPersonal)
        {
            try
            {
                using (var db = new SIAV_prod_4Entities())
                {
                    var (idUsuario, idEmpresa) = SessionUtils.ObtenerValoresDeSesion(Session);

                    // Obtener la conversación existente
                    var conversacion = db.ObtenerConversacionPersonalCliente(idEmpresa, idCliente, idPersonal).FirstOrDefault();

                    if (conversacion != null)
                    {
                        // PRIMERO: Buscar nuevos mensajes del cliente antes de devolver la conversación
                        try
                        {
                            var busquedaModel = new RequestBusquedaConversacion
                            {
                                idCliente = idCliente,
                                IdPersonal = idPersonal
                            };

                            // Llamar al método que busca nuevos mensajes del cliente
                            var resultadoBusqueda = ObtenerMensajesClientePersonal(busquedaModel);

                            // Si se encontraron nuevos mensajes, obtener la conversación actualizada
                            if (resultadoBusqueda.Data != null)
                            {
                                var dataBusqueda = (dynamic)resultadoBusqueda.Data;
                                if (dataBusqueda.success == true)
                                {
                                    // Recargar la conversación actualizada
                                    conversacion = db.ObtenerConversacionPersonalCliente(idEmpresa, idCliente, idPersonal).FirstOrDefault();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // Si falla la búsqueda de nuevos mensajes, continuar con la conversación actual
                            Console.WriteLine($"Error al buscar nuevos mensajes: {ex.Message}");
                        }

                        // Deserializar el JSON de conversaciones (actualizado o no)
                        var mensajes = JsonConvert.DeserializeObject<List<MessageViewModel>>(conversacion.jsonConversaciones ?? "[]");

                        return Json(new
                        {
                            success = true,
                            conversacionExiste = true,
                            mensajes = mensajes,
                            totalMensajes = conversacion.total_mensajes,
                            fechaUltimaActividad = conversacion.fecha_ultima_actividad?.ToString("dd/MM/yyyy HH:mm"),
                            estadoConversacion = conversacion.estado_conversacion
                        }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        // No existe conversación previa
                        return Json(new
                        {
                            success = true,
                            conversacionExiste = false,
                            mensajes = new List<MessageViewModel>(),
                            totalMensajes = 0,
                            message = "No hay conversación previa entre este personal y cliente"
                        }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "Error al obtener la conversación: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }





        // [HttpPost]
        public JsonResult ObtenerMensajesClientePersonal(RequestBusquedaConversacion model)
        {
            try
            {
                using (var db = new SIAV_prod_4Entities())
                {
                    var (idUsuario, idEmpresa) = SessionUtils.ObtenerValoresDeSesion(Session);

                    // Obtener conversación existente
                    var conversacionExistente = db.ObtenerConversacionPersonalCliente(idEmpresa, model.idCliente, model.IdPersonal).FirstOrDefault();

                    if (conversacionExistente == null || string.IsNullOrEmpty(conversacionExistente.jsonConversaciones))
                    {
                        return Json(new { success = false, message = "No hay conversaciones disponibles." }, JsonRequestBehavior.AllowGet);
                    }

                    // Deserializar conversaciones
                    var conversaciones = JsonConvert.DeserializeObject<List<MessageViewModel>>(conversacionExistente.jsonConversaciones);
                    int ultimoId = conversaciones.Any() ? conversaciones.Max(m => int.TryParse(m.id, out int id) ? id : 0) : 0;
                    string ultimoSerialized = conversaciones.LastOrDefault()?.serialized;

                    // Consultar Maytapi por nuevos mensajes
                    var resultado = EnvioMensaje.TestMessage(ultimoSerialized); // Usa el último `serialized` como referencia
                    var response = JsonConvert.DeserializeObject<ResponseTestMaytapi>(resultado.Message);

                    if (response?.results == null || !response.results.Any())
                    {
                        return Json(new { success = true, message = "No hay nuevos mensajes." }, JsonRequestBehavior.AllowGet);
                    }

                    // Filtrar mensajes nuevos
                    var serializadosExistentes = new HashSet<string>(conversaciones.Where(c => c.serialized != null).Select(c => c.serialized));
                    var mensajesNuevos = response.results
                        .Where(r => !string.IsNullOrEmpty(r.serialized) && !serializadosExistentes.Contains(r.serialized))
                        .ToList();

                    if (!mensajesNuevos.Any()) return Json(new { success = true, message = "No hay nuevos mensajes." }, JsonRequestBehavior.AllowGet);

                    // Agregar mensajes del cliente a la conversación
                    foreach (var msg in mensajesNuevos)
                    {
                        var nuevoMensaje = new MessageViewModel
                        {
                            id = (++ultimoId).ToString(),
                            from_Id = "2", // Cliente
                            to_Id = "1",   // Personal
                            mensaje = msg.mensaje,
                            datetime = FormatearFecha(msg.fecha_llegada),
                            idUsuario = 100,
                            nombreUsuario = "Cliente",
                            rutaImagen = "/assets/images/persona.jpg",
                            serialized = msg.serialized
                        };
                        conversaciones.Add(nuevoMensaje);
                    }

                    // ✅ Serializar y GUARDAR en la base de datos
                    string jsonActualizado = JsonConvert.SerializeObject(conversaciones);
                    db.ActualizarConversacionPersonalCliente(
                        idEmpresa,
                        model.idCliente,
                        model.IdPersonal,
                        jsonActualizado,
                        idUsuario,
                        conversacionExistente.Nombre_Usuario,
                        conversacionExistente.ruta_Imagen,
                        DateTime.Now,
                        conversacionExistente.nombre_personal
                    );

                    return Json(new
                    {
                        success = true,
                        message = $"Se agregaron {mensajesNuevos.Count} mensajes nuevos",
                        nuevosmensajes = mensajesNuevos.Count
                    }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        //[HttpPost]
        //public JsonResult ObtenerMensajesClientePersonal(RequestBusquedaConversacion model)
        //{
        //    try
        //    {
        //        using (var db = new SIAV_prod_4Entities())
        //        {
        //            var (idUsuario, idEmpresa) = SessionUtils.ObtenerValoresDeSesion(Session);

        //            // Obtener la conversación existente
        //            var conversacionExistente = db.ObtenerConversacionPersonalCliente(idEmpresa, model.idCliente, model.IdPersonal).FirstOrDefault();

        //            if (conversacionExistente == null || string.IsNullOrEmpty(conversacionExistente.jsonConversaciones))
        //            {
        //                return Json(new { success = false, message = "No hay conversaciones disponibles." }, JsonRequestBehavior.AllowGet);
        //            }

        //            // Deserializar el JSON de las conversaciones existentes
        //            var conversaciones = JsonConvert.DeserializeObject<List<dynamic>>(conversacionExistente.jsonConversaciones);

        //            if (conversaciones == null || conversaciones.Count == 0)
        //            {
        //                return Json(new { success = false, message = "No se encontraron mensajes en las conversaciones." }, JsonRequestBehavior.AllowGet);
        //            }

        //            // Obtener el último ID de las conversaciones
        //            int ultimoId = conversaciones.Count > 0 ? (int)conversaciones.Last().id : 0;

        //            // Obtener el valor de `serialized` del último objeto
        //            var ultimoMensaje = conversaciones.Last();
        //            string serialized = ultimoMensaje.serialized?.ToString();

        //            if (string.IsNullOrEmpty(serialized))
        //            {
        //                return Json(new { success = false, message = "No se encontró un valor válido para 'serialized'." }, JsonRequestBehavior.AllowGet);
        //            }

        //            // Enviar el mensaje usando `serialized` para obtener nuevos mensajes del cliente
        //            var resultado = EnvioMensaje.TestMessage(serialized);
        //            var result = JsonConvert.DeserializeObject<ResponseTestMaytapi>(resultado.Message);

        //            if (result == null || result.results == null || result.results.Count == 0)
        //            {
        //                // No hay nuevos mensajes, devolver la conversación actual
        //                return Json(new
        //                {
        //                    success = true,
        //                    conversacionExiste = true,
        //                    mensajes = conversaciones,
        //                    totalMensajes = conversaciones.Count,
        //                    message = "No hay nuevos mensajes del cliente"
        //                }, JsonRequestBehavior.AllowGet);
        //            }

        //            // FILTRAR MENSAJES: Solo obtener mensajes que NO están ya en esta conversación
        //            var serializadosExistentes = new HashSet<string>();
        //            foreach (var conv in conversaciones)
        //            {
        //                if (conv.serialized != null)
        //                {
        //                    serializadosExistentes.Add(conv.serialized.ToString());
        //                }
        //            }

        //            // Filtrar solo los mensajes nuevos que no existen en esta conversación
        //            var mensajesNuevos = result.results.Where(res =>
        //                !string.IsNullOrEmpty(res.serialized) &&
        //                !serializadosExistentes.Contains(res.serialized)
        //            ).ToList();

        //            if (mensajesNuevos.Count == 0)
        //            {
        //                return Json(new
        //                {
        //                    success = true,
        //                    conversacionExiste = true,
        //                    mensajes = conversaciones,
        //                    totalMensajes = conversaciones.Count,
        //                    message = "No hay mensajes nuevos para este personal"
        //                }, JsonRequestBehavior.AllowGet);
        //            }

        //            // Procesar solo los mensajes nuevos filtrados
        //            foreach (var res in mensajesNuevos)
        //            {
        //                // Crear un nuevo objeto JSON para el mensaje del cliente
        //                var nuevoMensajeCliente = new
        //                {
        //                    id = ++ultimoId, // Incrementar ID secuencialmente
        //                    from_Id = "2",   // Cliente envía mensaje
        //                    to_Id = "1",     // Personal recibe mensaje
        //                    mensaje = res.mensaje, // Mensaje del cliente
        //                    datetime = FormatearFecha(res.fecha_llegada), // Formatear la fecha
        //                    idUsuario = 100, // ID genérico para cliente
        //                    nombreUsuario = "Cliente",
        //                    rutaImagen = "/assets/images/persona.jpg",
        //                    serialized = res.serialized // Valor serialized del cliente
        //                };

        //                // Agregar el nuevo mensaje a la lista de conversaciones
        //                conversaciones.Add(nuevoMensajeCliente);
        //            }

        //            // Serializar la lista actualizada a JSON
        //            string jsonActualizado = JsonConvert.SerializeObject(conversaciones);

        //            // Actualizar la conversación en la base de datos con los nuevos mensajes
        //            db.ActualizarConversacionPersonalCliente(
        //                idEmpresa,
        //                model.idCliente,
        //                model.IdPersonal,
        //                jsonActualizado,
        //                idUsuario, // Usuario que ejecuta la actualización
        //                conversacionExistente.Nombre_Usuario,
        //                conversacionExistente.ruta_Imagen,
        //                DateTime.Now,
        //                conversacionExistente.nombre_personal
        //            );

        //            // Obtener la conversación actualizada
        //            var conversacionActualizada = db.ObtenerConversacionPersonalCliente(idEmpresa, model.idCliente, model.IdPersonal).FirstOrDefault();

        //            return Json(new
        //            {
        //                success = true,
        //                conversacionExiste = true,
        //                mensajes = JsonConvert.DeserializeObject<List<dynamic>>(conversacionActualizada.jsonConversaciones),
        //                totalMensajes = conversaciones.Count,
        //                nuevosmensajes = mensajesNuevos.Count,
        //                message = $"Se agregaron {mensajesNuevos.Count} nuevos mensajes para este personal"
        //            }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Devolver el mensaje de error en caso de excepción
        //        return Json(new { success = false, errorMessage = ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}


        private string FormatearFecha(string fechaOriginal)
        {
            try
            {
                DateTime fecha;

                // Manejar diferentes formatos de fecha
                if (fechaOriginal.Contains("/Date("))
                {
                    // Formato /Date(timestamp)/
                    var match = System.Text.RegularExpressions.Regex.Match(fechaOriginal, @"\d+");
                    if (match.Success && long.TryParse(match.Value, out long timestamp))
                    {
                        fecha = new DateTime(1970, 1, 1).AddMilliseconds(timestamp);
                    }
                    else
                    {
                        fecha = DateTime.Now;
                    }
                }
                else if (DateTime.TryParse(fechaOriginal, out fecha))
                {
                    // Formato normal
                }
                else
                {
                    fecha = DateTime.Now;
                }

                // RETORNAR en el formato correcto: "12 ago 25, 14:30"
                return fecha.ToString("dd MMM yy, HH:mm", new CultureInfo("es-ES"));
            }
            catch
            {
                return DateTime.Now.ToString("dd MMM yy, HH:mm", new CultureInfo("es-ES"));
            }
        }




        [HttpPost]
        public JsonResult EnviarArchivoPersonalCliente(RequestDataModel model)
        {
            try
            {
                using (var db = new SIAV_prod_4Entities())
                {
                    var dback = new BackOffice_WebEntities();
                    var (idUsuario, idEmpresa) = SessionUtils.ObtenerValoresDeSesion(Session);
                    var usuario = dback.ObtenerElUsuarioPorId(idUsuario).FirstOrDefault();

                    // Verificar si ya existe una conversación
                    var conversacionExistente = db.ObtenerConversacionPersonalCliente(idEmpresa, model.idCliente, model.IdPersonal).FirstOrDefault();

                    // Validar archivo
                    if (string.IsNullOrEmpty(model.ChatMessageData.archivoBase64))
                    {
                        return Json(new { success = false, message = "No se recibió ningún archivo" }, JsonRequestBehavior.AllowGet);
                    }

                    if (model.ChatMessageData.tamanoArchivo > 25 * 1024 * 1024) // 25MB
                    {
                        return Json(new { success = false, message = "El archivo es muy grande (máx. 25MB)" }, JsonRequestBehavior.AllowGet);
                    }

                    // Generar ID secuencial
                    int nextId = 1;
                    if (conversacionExistente != null && !string.IsNullOrEmpty(conversacionExistente.jsonConversaciones))
                    {
                        var mensajesExistentes = JsonConvert.DeserializeObject<List<MessageViewModel>>(conversacionExistente.jsonConversaciones);
                        if (mensajesExistentes.Any())
                        {
                            var maxId = mensajesExistentes.Max(m => int.TryParse(m.id, out int id) ? id : 0);
                            nextId = maxId + 1;
                        }
                    }

                    // Completar datos del mensaje
                    model.ChatMessageData.id = nextId.ToString();
                    model.ChatMessageData.idUsuario = usuario.Id_Usuario;
                    model.ChatMessageData.nombreUsuario = usuario.Nombre;
                    model.ChatMessageData.rutaImagen = usuario.Ruta_Imagen ?? "/assets/images/user-default.jpg";

                    // Convertir Base64 a bytes
                    byte[] archivoBytes = Convert.FromBase64String(model.ChatMessageData.archivoBase64);

                    // Ruta física: ~/assets/uploads/conversaciones/
                    string rutaFisicaArchivos = Server.MapPath("~/assets/uploads/conversaciones/");
                    if (!Directory.Exists(rutaFisicaArchivos))
                        Directory.CreateDirectory(rutaFisicaArchivos);

                    // Nombre único del archivo
                    string nombreArchivoUnico = $"{DateTime.Now:yyyyMMddHHmmss}_{model.ChatMessageData.nombreArchivo}";
                    string rutaFisicaCompleta = Path.Combine(rutaFisicaArchivos, nombreArchivoUnico);

                    // Guardar archivo físico
                    System.IO.File.WriteAllBytes(rutaFisicaCompleta, archivoBytes);

                    // ✅ Generar URL pública
                    string baseUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}";
                    string rutaArchivoUrl = $"{baseUrl}/assets/uploads/conversaciones/{nombreArchivoUnico}";

                    // ✅ Asignar la URL al campo rutaImagenEnviada ANTES de serializar
                    model.ChatMessageData.rutaImagenEnviada = rutaArchivoUrl;

                    // ✅ Actualizar el mensaje descriptivo
                    model.ChatMessageData.mensaje = $"📎 {model.ChatMessageData.nombreArchivo}";

                    // ✅ Enviar a Maytapi usando la URL pública
                    var resultado = EnvioMensaje.SendFile(
                        123,
                         model.Telefono, // Cambia por model.Telefono en producción
                        "text",
                        rutaArchivoUrl, // ← Aquí va la URL, no el Base64
                        model.ChatMessageData.nombreArchivo
                    );

                    if (!resultado.Success)
                    {
                        return Json(new { success = false, message = "No se pudo enviar el archivo" }, JsonRequestBehavior.AllowGet);
                    }

                    // ✅ Completar el serialized
                    model.ChatMessageData.serialized = resultado.Message;

                    // ✅ Serializar el mensaje completo (ahora con rutaImagenEnviada)
                    string jsonNuevoMensaje = JsonConvert.SerializeObject(model.ChatMessageData);

                    // ✅ Agregar a la conversación
                    if (conversacionExistente != null)
                    {
                        var mensajes = JsonConvert.DeserializeObject<List<MessageViewModel>>(conversacionExistente.jsonConversaciones);
                        mensajes.Add(JsonConvert.DeserializeObject<MessageViewModel>(jsonNuevoMensaje));

                        string jsonActualizado = JsonConvert.SerializeObject(mensajes);

                        db.ActualizarConversacionPersonalCliente(
                            idEmpresa,
                            model.idCliente,
                            model.IdPersonal,
                            jsonActualizado,
                            usuario.Id_Usuario,
                            usuario.Nombre,
                            usuario.Ruta_Imagen,
                            DateTime.Now,
                            model.nombrePersonal
                        );
                    }
                    else
                    {
                        var mensajes = new List<MessageViewModel>
                {
                    JsonConvert.DeserializeObject<MessageViewModel>(jsonNuevoMensaje)
                };

                        string jsonConversaciones = JsonConvert.SerializeObject(mensajes);

                        db.InsertarConversacionPersonalCliente(
                            idEmpresa,
                            model.idCliente,
                            model.IdPersonal,
                            model.Telefono,
                            jsonConversaciones,
                            usuario.Id_Usuario,
                            usuario.Nombre,
                            usuario.Ruta_Imagen,
                            DateTime.Now,
                            model.nombrePersonal
                        );
                    }

                    return Json(new { success = true, message = "Archivo enviado correctamente" }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar archivo: {ex.Message}");
                return Json(new { success = false, message = "Error interno: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        //[HttpPost]
        //public JsonResult EnviarArchivoPersonalCliente(RequestDataModel model)
        //{
        //    try
        //    {
        //        using (var db = new SIAV_prod_4Entities())
        //        {
        //            var dback = new BackOffice_WebEntities();

        //            var (idUsuario, idEmpresa) = SessionUtils.ObtenerValoresDeSesion(Session);

        //            var usuario = dback.ObtenerElUsuarioPorId(idUsuario).FirstOrDefault();

        //            // Verificar si ya existe una conversación entre el personal y el cliente
        //            var conversacionExistente = db.ObtenerConversacionPersonalCliente(idEmpresa, model.idCliente, model.IdPersonal).FirstOrDefault();

        //            // Validar que se recibió el archivo
        //            if (string.IsNullOrEmpty(model.ChatMessageData.archivoBase64))
        //            {
        //                return Json(new { success = false, message = "No se recibió ningún archivo" }, JsonRequestBehavior.AllowGet);
        //            }

        //            // Validar tamaño
        //            if (model.ChatMessageData.tamanoArchivo > 25 * 1024 * 1024) // 25MB
        //            {
        //                return Json(new { success = false, message = "El archivo es muy grande (máx. 25MB)" }, JsonRequestBehavior.AllowGet);
        //            }

        //            // Generar el próximo ID secuencial
        //            int nextId = 1;
        //            if (conversacionExistente != null && !string.IsNullOrEmpty(conversacionExistente.jsonConversaciones))
        //            {
        //                var mensajesExistentes = JsonConvert.DeserializeObject<List<MessageViewModel>>(conversacionExistente.jsonConversaciones);
        //                if (mensajesExistentes.Any())
        //                {
        //                    // Obtener el ID más alto y sumar 1
        //                    var maxId = mensajesExistentes.Max(m => int.TryParse(m.id, out int id) ? id : 0);
        //                    nextId = maxId + 1;
        //                }
        //            }

        //            // Completar los datos del mensaje que vienen vacíos desde el cliente
        //            model.ChatMessageData.id = nextId.ToString();
        //            model.ChatMessageData.idUsuario = usuario.Id_Usuario;
        //            model.ChatMessageData.nombreUsuario = usuario.Nombre;
        //            model.ChatMessageData.rutaImagen = usuario.Ruta_Imagen ?? "/assets/images/user-default.jpg";


        //            // Convertir el archivo base64 a bytes
        //            byte[] archivoBytes = Convert.FromBase64String(model.ChatMessageData.archivoBase64);

        //            // Ruta física a la carpeta "assets/uploads/conversaciones"
        //            string rutaFisicaArchivos = Server.MapPath("~/assets/uploads/conversaciones/");

        //            // Crear el directorio si no existe
        //            if (!Directory.Exists(rutaFisicaArchivos))
        //                Directory.CreateDirectory(rutaFisicaArchivos);

        //            // Generar nombre único para el archivo
        //            string nombreArchivoUnico = $"{DateTime.Now:yyyyMMddHHmmss}_{model.ChatMessageData.nombreArchivo}";

        //            // Combinar ruta + archivo
        //            string rutaFisicaCompleta = Path.Combine(rutaFisicaArchivos, nombreArchivoUnico);


        //            // Crear ruta dinámica basada en el servidor actual
        //            //string rutaFisicaArchivos = Server.MapPath("~/uploads/conversaciones/");
        //            //if (!Directory.Exists(rutaFisicaArchivos))
        //            //    Directory.CreateDirectory(rutaFisicaArchivos);

        //            //// Generar nombre único para el archivo
        //            //string nombreArchivoUnico = $"{DateTime.Now:yyyyMMddHHmmss}_{model.ChatMessageData.nombreArchivo}";
        //            //string rutaFisicaCompleta = Path.Combine(rutaFisicaArchivos, nombreArchivoUnico);

        //            // Guardar archivo físicamente en el servidor
        //            System.IO.File.WriteAllBytes(rutaFisicaCompleta, archivoBytes);

        //            // Generar URL dinámica del archivo usando el host actual
        //            string baseUrl = $"{Request.Url.Scheme}://{Request.Url.Authority}";
        //            string rutaArchivoUrl = $"{baseUrl}/assets/uploads/conversaciones/{nombreArchivoUnico}";

        //            // Actualizar el ChatMessageData con la ruta del archivo para el envío
        //            model.ChatMessageData.archivoBase64 = rutaArchivoUrl; // Aquí va la ruta dinámica del archivo
        //            model.ChatMessageData.mensaje = $"📎 {model.ChatMessageData.nombreArchivo}"; // Mensaje descriptivo

        //            model.ChatMessageData. = rutaArchivoUrl; // ← Aquí se guarda la ruta


        //            // Enviar archivo por WhatsApp usando SendFile con la ruta dinámica
        //            var resultado = EnvioMensaje.SendFile(123, "51987611070", "text", model.ChatMessageData.archivoBase64, model.ChatMessageData.nombreArchivo);
        //            // var resultado = EnvioMensaje.SendFile(123, model.Telefono, "text", model.ChatMessageData.archivoBase64, model.ChatMessageData.nombreArchivo);

        //            // Verificar si el resultado fue exitoso
        //            if (!resultado.Success)
        //            {
        //                return Json(new { success = false, message = "No se pudo enviar el archivo" }, JsonRequestBehavior.AllowGet);
        //            }

        //            // Completar el serialized con la respuesta del envío
        //            model.ChatMessageData.serialized = resultado.Message;

        //            // Convertir el mensaje completo a JSON
        //            string jsonNuevoMensaje = JsonConvert.SerializeObject(model.ChatMessageData);

        //            if (conversacionExistente != null)
        //            {
        //                // ACTUALIZAR conversación existente

        //                // Deserializar el JSON existente en una lista de mensajes
        //                var jsonConversacionesExistente = JsonConvert.DeserializeObject<List<MessageViewModel>>(conversacionExistente.jsonConversaciones);

        //                // Deserializar el nuevo mensaje para añadirlo a la lista
        //                var mensajeNuevo = JsonConvert.DeserializeObject<MessageViewModel>(jsonNuevoMensaje);

        //                // Añadir el nuevo mensaje
        //                jsonConversacionesExistente.Add(mensajeNuevo);

        //                // Serializar de nuevo a JSON
        //                string conversacionActualizadaJson = JsonConvert.SerializeObject(jsonConversacionesExistente);

        //                // Actualizar la conversación en la base de datos
        //                db.ActualizarConversacionPersonalCliente(
        //                    idEmpresa,
        //                    model.idCliente,
        //                    model.IdPersonal,
        //                    conversacionActualizadaJson,
        //                    usuario.Id_Usuario,
        //                    usuario.Nombre,
        //                    usuario.Ruta_Imagen,
        //                    DateTime.Now,
        //                    model.nombrePersonal
        //                );

        //                return Json(new { success = resultado.Success, message = "Archivo enviado correctamente" }, JsonRequestBehavior.AllowGet);
        //            }
        //            else
        //            {
        //                // INSERTAR nueva conversación

        //                // Crear una nueva lista con el mensaje (ya tiene todos los datos completos)
        //                var mensajes = new List<MessageViewModel> { JsonConvert.DeserializeObject<MessageViewModel>(jsonNuevoMensaje) };

        //                string jsonConversaciones = JsonConvert.SerializeObject(mensajes);

        //                // Insertar nueva conversación
        //                db.InsertarConversacionPersonalCliente(
        //                    idEmpresa,
        //                    model.idCliente,
        //                    model.IdPersonal,
        //                    model.Telefono,
        //                    jsonConversaciones,
        //                    usuario.Id_Usuario,
        //                    usuario.Nombre,
        //                    usuario.Ruta_Imagen,
        //                    DateTime.Now,
        //                    model.nombrePersonal
        //                );

        //                return Json(new { success = resultado.Success, message = "Archivo enviado correctamente" }, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log del error
        //        Console.WriteLine($"Error al enviar archivo: {ex.Message}");
        //        return Json(new { success = false, message = "Error interno del servidor: " + ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}

        public class MessageViewModel
        {
            public string id { get; set; }
            public string from_Id { get; set; }
            public string to_Id { get; set; }
            public string mensaje { get; set; }
            public string datetime { get; set; }
            public int idUsuario { get; set; }
            public string nombreUsuario { get; set; }
            public string rutaImagen { get; set; }
            public string serialized { get; set; }
            public string rutaImagenEnviada { get; set; }  // aqui se guarda la ruta de la imagen q yo envia al personal del cliente 

            public string nombreArchivo { get; set; }
            public string tipoArchivo { get; set; }
            public long tamanoArchivo { get; set; }
        }

        //[HttpPost]
        //public JsonResult EnviarMensaje(RequestDataModel model)
        //{
        //    try
        //    {
        //        using (var db = new SIAV_prod_4Entities())
        //        {
        //            int idEmpresa = SessionUtils.ObtenerIdEmpresaDeSesion(Session);

        //            // Buscar conversación existente
        //            var conversacionExistente = db.ObtenerConversacionPersonalCliente
        //                .FirstOrDefault(c => c.Id_Empresa == idEmpresa &&
        //                               c.Id_Cliente == model.IdCliente &&
        //                               c.Id_Personal == model.IdPersonal);

        //            // Generar datos del mensaje
        //            model.ChatMessageData.id = Guid.NewGuid().ToString();
        //            model.ChatMessageData.datetime = DateTime.Now.ToString("dd MMM yy, hh:mm tt");
        //            model.ChatMessageData.from_Id = model.IdPersonal.ToString();
        //            model.ChatMessageData.to_Id = model.IdCliente.ToString();

        //            // Procesar archivo si existe
        //            if (model.ChatMessageData.tipoMensaje == "archivo" && model.ChatMessageData.archivo != null)
        //            {
        //                var archivoResult = ProcesarArchivo(model.ChatMessageData.archivo, idEmpresa);
        //                if (!archivoResult.Success)
        //                {
        //                    return Json(new { success = false, message = archivoResult.Message }, JsonRequestBehavior.AllowGet);
        //                }
        //                model.ChatMessageData.archivo.rutaArchivo = archivoResult.RutaArchivo;
        //                model.ChatMessageData.archivo.archivoBase64 = null;
        //            }

        //            // Enviar mensaje por WhatsApp
        //            string mensajeParaEnviar = model.ChatMessageData.tipoMensaje == "archivo"
        //                ? $"{model.ChatMessageData.mensaje} - Archivo: {model.ChatMessageData.archivo?.nombreArchivo}"
        //                : model.ChatMessageData.mensaje;

        //            var resultado = EnvioMensaje.SendMessage(123, model.Telefono, "text", mensajeParaEnviar, "text");

        //            if (!resultado.Success)
        //            {
        //                return Json(new { success = false, message = "No se pudo enviar el mensaje: " + resultado.Message }, JsonRequestBehavior.AllowGet);
        //            }

        //            model.ChatMessageData.serialized = resultado.Message;

        //            List<ChatMessageData> conversaciones;
        //            int totalMensajes = 0;

        //            if (conversacionExistente != null)
        //            {
        //                // Actualizar conversación existente
        //                conversaciones = string.IsNullOrEmpty(conversacionExistente.jsonConversaciones)
        //                    ? new List<ChatMessageData>()
        //                    : JsonConvert.DeserializeObject<List<ChatMessageData>>(conversacionExistente.jsonConversaciones);

        //                conversaciones.Add(model.ChatMessageData);
        //                string conversacionActualizadaJson = JsonConvert.SerializeObject(conversaciones, Formatting.Indented);

        //                // Calcular total en C# (más eficiente)
        //                totalMensajes = conversaciones.Count;

        //                // Llamar SP simplificado (sin cálculo JSON)
        //                db.ActualizarConversacionPersonalCliente(
        //                    idEmpresa,
        //                    model.IdCliente,
        //                    model.IdPersonal,
        //                    conversacionActualizadaJson,
        //                    totalMensajes, // ← Pasar total calculado en C#
        //                    model.ChatMessageData.idUsuario,
        //                    model.ChatMessageData.nombreUsuario,
        //                    model.ChatMessageData.rutaImagen,
        //                    DateTime.Now,
        //                    model.NombreCotizacionMensaje
        //                );
        //            }
        //            else
        //            {
        //                // Crear nueva conversación
        //                conversaciones = new List<ChatMessageData> { model.ChatMessageData };
        //                string jsonConversaciones = JsonConvert.SerializeObject(conversaciones, Formatting.Indented);
        //                totalMensajes = 1;

        //                db.InsertarConversacionPersonalCliente(
        //                    idEmpresa,
        //                    model.IdCliente,
        //                    model.IdPersonal,
        //                    model.Telefono,
        //                    jsonConversaciones,
        //                    totalMensajes, // ← Total calculado en C#
        //                    model.ChatMessageData.idUsuario,
        //                    model.ChatMessageData.nombreUsuario,
        //                    model.ChatMessageData.rutaImagen,
        //                    DateTime.Now,
        //                    model.NombreCotizacionMensaje
        //                );
        //            }

        //            return Json(new
        //            {
        //                success = true,
        //                message = "Mensaje enviado correctamente",
        //                messageId = model.ChatMessageData.id,
        //                totalMensajes = totalMensajes,
        //                serialized = resultado.Message
        //            }, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
        //    }
        //}


        //private ArchivoResult ProcesarArchivo(ArchivoMensaje archivo, int idEmpresa)
        //{
        //    try
        //    {
        //        if (string.IsNullOrEmpty(archivo.archivoBase64))
        //        {
        //            return new ArchivoResult { Success = false, Message = "Archivo base64 vacío" };
        //        }

        //        // Convertir base64 a bytes
        //        byte[] fileBytes = Convert.FromBase64String(archivo.archivoBase64);

        //        // Crear directorio según tipo de archivo
        //        string year = DateTime.Now.Year.ToString();
        //        string month = DateTime.Now.Month.ToString("00");
        //        string uploadDir = "";

        //        if (archivo.esImagen)
        //            uploadDir = $"~/uploads/imagenes/{year}/{month}/";
        //        else if (archivo.esVideo)
        //            uploadDir = $"~/uploads/videos/{year}/{month}/";
        //        else
        //            uploadDir = $"~/uploads/archivos/{year}/{month}/";

        //        string physicalPath = Server.MapPath(uploadDir);
        //        if (!Directory.Exists(physicalPath))
        //            Directory.CreateDirectory(physicalPath);

        //        // Generar nombre único
        //        string fileName = $"{Path.GetFileNameWithoutExtension(archivo.nombreArchivo)}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(archivo.nombreArchivo)}";
        //        string filePath = Path.Combine(physicalPath, fileName);

        //        // Guardar archivo
        //       System.IO.File.WriteAllBytes(filePath, fileBytes);

        //        // Retornar ruta relativa
        //        string rutaRelativa = uploadDir.Replace("~/", "/") + fileName;

        //        return new ArchivoResult
        //        {
        //            Success = true,
        //            Message = "Archivo guardado correctamente",
        //            RutaArchivo = rutaRelativa
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ArchivoResult { Success = false, Message = "Error al procesar archivo: " + ex.Message };
        //    }
        //}

        //// CLASE AUXILIAR PARA RESULTADO DE ARCHIVO
        //public class ArchivoResult
        //{
        //    public bool Success { get; set; }
        //    public string Message { get; set; }
        //    public string RutaArchivo { get; set; }
        //}
        //[HttpPost]
        //public JsonResult EnviarMensaje(MensajeDto datos)
        //{
        //    try
        //    {
        //        // Mostrar en consola del servidor (útil para depuración)
        //        Console.WriteLine("=== Datos recibidos ===");
        //        Console.WriteLine($"Mensaje: {datos.Mensaje}");
        //        Console.WriteLine($"TipoMensaje: {datos.TipoMensaje}");
        //        Console.WriteLine($"TieneEmojis: {datos.TieneEmojis}");
        //        Console.WriteLine($"PersonalNombre: {datos.PersonalNombre}");
        //        Console.WriteLine($"PersonalEmail: {datos.PersonalEmail}");
        //        Console.WriteLine($"PersonalTelefono: {datos.PersonalTelefono}");
        //        Console.WriteLine($"IdCliente: {datos.IdCliente}");
        //        Console.WriteLine($"ClientName: {datos.ClientName}");
        //        Console.WriteLine($"IdParaMensajeria: {datos.IdParaMensajeria}");
        //        Console.WriteLine($"FechaEnvio: {datos.FechaEnvio}");
        //        Console.WriteLine($"HoraEnvio: {datos.HoraEnvio}");
        //        Console.WriteLine("=======================");

        //        // Aquí podrías guardar el mensaje en base de datos
        //        return Json(new { success = true, message = "Mensaje enviado correctamente" });
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { success = false, message = ex.Message });
        //    }
        //}


      
        // MODELOS C# PARA MENSAJERÍA
        // Modelo básico para recibir mensajes de texto
        public class RequestDataModel
        {
            public int idCliente { get; set; }
            public int IdPersonal { get; set; }
            public string Telefono { get; set; }
            public string nombrePersonal { get; set; }
            public ChatMessageDataModel ChatMessageData { get; set; }
        }

        public class ChatMessageDataModel
        {
            public string id { get; set; }
            public string from_Id { get; set; }
            public string to_Id { get; set; }
            public string mensaje { get; set; }
            public string datetime { get; set; }
            public int idUsuario { get; set; }
            public string nombreUsuario { get; set; }
            public string rutaImagen { get; set; }
            public string serialized { get; set; }

            // Propiedades específicas para archivos
            public string archivoBase64 { get; set; }
            public string nombreArchivo { get; set; }
            public string tipoArchivo { get; set; }
            public long tamanoArchivo { get; set; }
            public string rutaImagenEnviada { get; set; }

        }

        public class RequestBusquedaConversacion
        {
            public int idCliente { get; set; }
            public int IdPersonal { get; set; }
        }

        // Modelo para la respuesta de Maytapi (si no lo tienes ya)
        public class ResponseTestMaytapi
        {
            public List<MensajeClienteResult> results { get; set; }
        }


        public class MensajeClienteResult
        {
            public string mensaje { get; set; }
            public string fecha_llegada { get; set; }
            public string serialized { get; set; }
        }

        }
}