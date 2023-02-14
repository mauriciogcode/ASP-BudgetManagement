using Dapper;
using ManejoPresupuesto.Models;
using ManejoPresupuesto.Servicios;
using ManejoPresupuesto.Servicios.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Controllers
{
    public class TiposCuentasController : Controller
    {
        private readonly IRepositorioTiposCuentas repositorioTIposCuentas;
        private readonly IServicioUsuarios servicioUsuarios;

        public TiposCuentasController(IRepositorioTiposCuentas repositorioTIposCuentas, IServicioUsuarios servicioUsuarios)
        {
            this.repositorioTIposCuentas = repositorioTIposCuentas;
            this.servicioUsuarios = servicioUsuarios;
        }

        public async Task<IActionResult> Index ()
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuentas = await repositorioTIposCuentas.ObtenerTodos(usuarioId);
            return View(tipoCuentas);
        }

        public IActionResult Crear()
        {
            return View();
        }
        #region Editar


        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tiposCuenta = await repositorioTIposCuentas.ObtenerCuentaPorId(id, usuarioId);

            if (tiposCuenta is null)
            {
                return RedirectToAction("NoEncontrado","Home");
            }
            return View(tiposCuenta);
        }


        [HttpPost]
        public async Task<IActionResult> Editar(TipoCuenta tipoCuenta)
        {
            var UsuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuentasExiste = repositorioTIposCuentas.ObtenerCuentaPorId(tipoCuenta.Id, UsuarioId);
            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioTIposCuentas.Actualizar(tipoCuenta);
            return RedirectToAction("Index");
        }
        #endregion
        #region Crear
        [HttpPost]
        public async Task<IActionResult> Crear(TipoCuenta tipocuenta)
        {
            if (!ModelState.IsValid)
            {
                return View(tipocuenta);
            }
            tipocuenta.UsuarioId = servicioUsuarios.ObtenerUsuarioId();

            var yaExisteTiposCuenta = await repositorioTIposCuentas.Existe(tipocuenta.Nombre, tipocuenta.UsuarioId);

            if (yaExisteTiposCuenta)
            {
                ModelState.AddModelError(nameof(tipocuenta.Nombre), $"El nombre {tipocuenta.Nombre} ya existe.");
                return View(tipocuenta);
            }
            await repositorioTIposCuentas.Crear(tipocuenta);
            return RedirectToAction("Index");
        }
        #endregion
        #region Borrar
        [HttpGet]
        public async Task<IActionResult> Borrar(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTIposCuentas.ObtenerCuentaPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            return View(tipoCuenta);
        }


        [HttpPost]
        public async Task<IActionResult> BorrarTipoCuenta(int id)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tipoCuenta = await repositorioTIposCuentas.ObtenerCuentaPorId(id, usuarioId);

            if (tipoCuenta is null)
            {
                return RedirectToAction("NoEncontrado", "Home");
            }
            await repositorioTIposCuentas.Borrar(id);
            return RedirectToAction("Index");
        }
        #endregion

        #region Verificacion Front
        [HttpGet]
         public async Task<IActionResult> VerificarExisteTipoCuenta(string nombre)
        {
            var usuarioId  = servicioUsuarios.ObtenerUsuarioId();
            var yaExisteTipocuenta = await repositorioTIposCuentas.Existe(nombre, usuarioId);

            if(yaExisteTipocuenta)
            {
                return Json($"El nombre {nombre} ya existe");
            }

            return Json(true);
        }
        #endregion

        [HttpPost]
        public async Task<IActionResult> Ordenar([FromBody] int[] ids)
        {
            var usuarioId = servicioUsuarios.ObtenerUsuarioId();
            var tiposCuentas = await repositorioTIposCuentas.ObtenerTodos(usuarioId);
            var idsTiposCuentas = tiposCuentas.Select(tiposCuentas=> tiposCuentas.Id);

            var idsTiposCuentasNoPertenecenAlUsuario = ids.Except(idsTiposCuentas).ToList();

            if (idsTiposCuentasNoPertenecenAlUsuario.Count > 0)
            {
                return Forbid();
            }

            var idsTiposCuentasOrdenados = ids.Select( (valor,indice) => new TipoCuenta() { Id = valor, Orden = indice + 1 }).AsEnumerable();

            await repositorioTIposCuentas.Ordenar(idsTiposCuentasOrdenados);

            return Ok();
        }


    }
}
